using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.SSM;
using Bookstore.Common;
using Constructs;
using AssetOptions = Amazon.CDK.AWS.S3.Assets.AssetOptions;

namespace Bookstore.Cdk;

public class LambdaStackProps : StackProps
{
    public IVpc Vpc { get; set; }
    public DatabaseInstance Database { get; set; }
    public Bucket ImageBucket { get; set; }
    public UserPool WebAppUserPool { get; set; }
}

public class LambdaStack : Stack
{
    private const int DatabasePort = 1433;
    
    public Function LambdaFunction { get; private set; }
    public HttpApi ApiGateway { get; private set; }
    public DatabaseProxy RdsProxy { get; private set; }

    internal LambdaStack(Construct scope, string id, LambdaStackProps props) : base(scope, id, props)
    {
        // Create security groups
        var lambdaSecurityGroup = CreateLambdaSecurityGroup(props);
        var rdsProxySecurityGroup = CreateRdsProxySecurityGroup(props);

        // Create IAM role for Lambda function
        var lambdaRole = CreateLambdaRole(props);

        // Create RDS Proxy
        RdsProxy = CreateRdsProxy(props, rdsProxySecurityGroup, lambdaRole);

        // Create Lambda function
        LambdaFunction = CreateLambdaFunction(props, lambdaRole, lambdaSecurityGroup);

        // Create API Gateway HTTP API
        ApiGateway = CreateApiGateway();

        // Configure security group rules
        ConfigureSecurityGroupRules(lambdaSecurityGroup, rdsProxySecurityGroup, props);

        // Create Cognito User Pool Client for Lambda
        CreateCognitoUserPoolClient(props);

        // Output important values
        CreateOutputs();
    }

    private SecurityGroup CreateLambdaSecurityGroup(LambdaStackProps props)
    {
        return new SecurityGroup(this, "LambdaSecurityGroup", new SecurityGroupProps
        {
            Vpc = props.Vpc,
            Description = "Security group for Lambda function accessing RDS Proxy",
            AllowAllOutbound = true
        });
    }

    private SecurityGroup CreateRdsProxySecurityGroup(LambdaStackProps props)
    {
        return new SecurityGroup(this, "RdsProxySecurityGroup", new SecurityGroupProps
        {
            Vpc = props.Vpc,
            Description = "Security group for RDS Proxy",
            AllowAllOutbound = true
        });
    }

    private Role CreateLambdaRole(LambdaStackProps props)
    {
        var role = new Role(this, "LambdaExecutionRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
            Description = "Execution role for Bob's Bookstore Lambda function",
            ManagedPolicies = new[]
            {
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"),
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole")
            }
        });

        // Add permissions for RDS Proxy IAM authentication
        role.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "rds-db:connect" },
            Resources = new[]
            {
                Arn.Format(new ArnComponents
                {
                    Service = "rds-db",
                    Resource = "dbuser",
                    ResourceName = $"*/*"
                }, this)
            }
        }));

        // Add permissions for Rekognition
        role.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "rekognition:DetectModerationLabels" },
            Resources = new[] { "*" }
        }));

        // Add permissions for SSM Parameter Store
        role.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "ssm:GetParametersByPath", "ssm:GetParameter" },
            Resources = new[]
            {
                Arn.Format(new ArnComponents
                {
                    Service = "ssm",
                    Resource = "parameter",
                    ResourceName = $"{Constants.AppName}/*"
                }, this)
            }
        }));

        // Add permissions for CloudWatch Logs
        role.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[]
            {
                "logs:CreateLogGroup",
                "logs:CreateLogStream",
                "logs:PutLogEvents"
            },
            Resources = new[] { "*" }
        }));

        // Grant S3 permissions for image bucket
        props.ImageBucket.GrantReadWrite(role);

        return role;
    }

    private DatabaseProxy CreateRdsProxy(LambdaStackProps props, SecurityGroup securityGroup, Role lambdaRole)
    {
        // Create RDS Proxy
        var proxy = new DatabaseProxy(this, "RdsProxy", new DatabaseProxyProps
        {
            ProxyTarget = ProxyTarget.FromInstance(props.Database),
            Secrets = new[] { props.Database.Secret },
            Vpc = props.Vpc,
            VpcSubnets = new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            },
            SecurityGroups = new[] { securityGroup },
            DbProxyName = $"{Constants.AppName}-RdsProxy",
            IamAuth = true, // Enable IAM authentication
            RequireTLS = true,
            IdleClientTimeout = Duration.Minutes(30),
            MaxConnectionsPercent = 100,
            MaxIdleConnectionsPercent = 50
        });

        // Grant Lambda role permission to connect via IAM auth
        proxy.GrantConnect(lambdaRole, "admin");

        // Store RDS Proxy endpoint in SSM Parameter Store
        _ = new StringParameter(this, "RdsProxyEndpointSSMParameter", new StringParameterProps
        {
            ParameterName = $"/{Constants.AppName}/Database/RdsProxyEndpoint",
            StringValue = proxy.Endpoint
        });

        return proxy;
    }

    private Function CreateLambdaFunction(LambdaStackProps props, Role role, SecurityGroup securityGroup)
    {
        // Get database name from secret
        var databaseName = props.Database.Secret.SecretValueFromJson("dbInstanceIdentifier").UnsafeUnwrap();

        var function = new Function(this, "LambdaFunction", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Handler = "Bookstore.Web::Bookstore.Web.LambdaEntryPoint::FunctionHandlerAsync",
            Code = Code.FromAsset("./", new AssetOptions
            {
                Bundling = new BundlingOptions
                {
                    Image = Runtime.DOTNET_8.BundlingImage,
                    User = "root",
                    OutputType = BundlingOutput.ARCHIVED,
                    Command = new[]
                    {
                        "/bin/sh",
                        "-c",
                        "cd /asset-input/app/Bookstore.Web && " +
                        "dotnet restore && " +
                        "dotnet lambda package -o /asset-output/function.zip -c Release"
                    }
                }
            }),
            FunctionName = $"{Constants.AppName}-Lambda",
            Description = "Bob's Used Bookstore Classic - Lambda Function",
            MemorySize = 2048,
            Timeout = Duration.Seconds(30),
            Role = role,
            Vpc = props.Vpc,
            VpcSubnets = new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            },
            SecurityGroups = new[] { securityGroup },
            Environment = new Dictionary<string, string>
            {
                { "Services__Authentication", "local" },
                { "Services__Database", "aws" },
                { "Services__FileService", "aws" },
                { "Services__ImageValidationService", "aws" },
                { "Services__LoggingService", "aws" },
                { "Database__UseRdsProxy", "true" },
                { "Database__RdsProxyEndpoint", RdsProxy.Endpoint },
                { "Database__DatabaseName", databaseName },
                { "Database__Port", DatabasePort.ToString() },
                { "AWS__Region", Region }
            },
            Tracing = Tracing.ACTIVE,
            LogRetention = RetentionDays.ONE_WEEK,
            // Optional: Configure provisioned concurrency for consistent performance
            // ReservedConcurrentExecutions = 10
        });

        // Optional: Add provisioned concurrency (uncomment if needed)
        // var version = function.CurrentVersion;
        // var alias = new Alias(this, "LambdaAlias", new AliasProps
        // {
        //     AliasName = "live",
        //     Version = version
        // });
        // _ = new CfnProvisionedConcurrencyConfig(this, "ProvisionedConcurrency", new CfnProvisionedConcurrencyConfigProps
        // {
        //     FunctionName = function.FunctionName,
        //     ProvisionedConcurrentExecutions = 2,
        //     Qualifier = alias.AliasName
        // });

        return function;
    }

    private HttpApi CreateApiGateway()
    {
        // Create HTTP API (not REST API)
        var api = new HttpApi(this, "HttpApi", new HttpApiProps
        {
            ApiName = $"{Constants.AppName}-HttpApi",
            Description = "HTTP API Gateway for Bob's Used Bookstore Lambda",
            CorsPreflight = new CorsPreflightOptions
            {
                AllowHeaders = new[] { "Content-Type", "Authorization" },
                AllowMethods = new[] { CorsHttpMethod.GET, CorsHttpMethod.POST, CorsHttpMethod.PUT, CorsHttpMethod.DELETE, CorsHttpMethod.OPTIONS },
                AllowOrigins = new[] { "*" },
                MaxAge = Duration.Days(10)
            }
        });

        // Create Lambda integration
        var integration = new CfnIntegration(this, "LambdaIntegration", new CfnIntegrationProps
        {
            ApiId = api.HttpApiId,
            IntegrationType = "AWS_PROXY",
            IntegrationUri = LambdaFunction.FunctionArn,
            IntegrationMethod = "POST",
            PayloadFormatVersion = "2.0"
        });

        // Create default route
        _ = new CfnRoute(this, "DefaultRoute", new CfnRouteProps
        {
            ApiId = api.HttpApiId,
            RouteKey = "$default",
            Target = $"integrations/{integration.Ref}"
        });

        // Grant API Gateway permission to invoke Lambda
        LambdaFunction.AddPermission("ApiGatewayInvoke", new Permission
        {
            Principal = new ServicePrincipal("apigateway.amazonaws.com"),
            Action = "lambda:InvokeFunction",
            SourceArn = $"arn:aws:execute-api:{Region}:{Account}:{api.HttpApiId}/*"
        });

        return api;
    }

    private void ConfigureSecurityGroupRules(SecurityGroup lambdaSecurityGroup, SecurityGroup rdsProxySecurityGroup, LambdaStackProps props)
    {
        // Allow Lambda to connect to RDS Proxy
        rdsProxySecurityGroup.AddIngressRule(
            lambdaSecurityGroup,
            Port.Tcp(DatabasePort),
            "Allow Lambda to connect to RDS Proxy"
        );

        // Allow RDS Proxy to connect to RDS instance
        props.Database.Connections.AllowFrom(
            rdsProxySecurityGroup,
            Port.Tcp(DatabasePort),
            "Allow RDS Proxy to connect to RDS instance"
        );
    }

    private void CreateCognitoUserPoolClient(LambdaStackProps props)
    {
        var lambdaUserPoolClient = new UserPoolClient(this, "CognitoLambdaAppClient", new UserPoolClientProps
        {
            UserPool = props.WebAppUserPool,
            GenerateSecret = false,
            PreventUserExistenceErrors = true,
            SupportedIdentityProviders = new[]
            {
                UserPoolClientIdentityProvider.COGNITO
            },
            AuthFlows = new AuthFlow
            {
                UserPassword = true
            },
            OAuth = new OAuthSettings
            {
                Flows = new OAuthFlows
                {
                    AuthorizationCodeGrant = true
                },
                Scopes = new[]
                {
                    OAuthScope.OPENID,
                    OAuthScope.EMAIL,
                    OAuthScope.COGNITO_ADMIN,
                    OAuthScope.PROFILE
                },
                CallbackUrls = new[]
                {
                    $"https://{ApiGateway.HttpApiId}.execute-api.{Region}.amazonaws.com/signin-oidc"
                },
                LogoutUrls = new[]
                {
                    $"https://{ApiGateway.HttpApiId}.execute-api.{Region}.amazonaws.com/"
                }
            }
        });

        _ = new StringParameter(this, "CognitoLambdaAppClientSSMParameter", new StringParameterProps
        {
            ParameterName = $"/{Constants.AppName}/Authentication/Cognito/LambdaClientId",
            StringValue = lambdaUserPoolClient.UserPoolClientId
        });
    }

    private void CreateOutputs()
    {
        _ = new CfnOutput(this, "ApiGatewayUrl", new CfnOutputProps
        {
            Description = "API Gateway HTTP API URL",
            Value = ApiGateway.Url,
            ExportName = $"{Constants.AppName}-ApiGatewayUrl"
        });

        _ = new CfnOutput(this, "LambdaFunctionName", new CfnOutputProps
        {
            Description = "Lambda Function Name",
            Value = LambdaFunction.FunctionName,
            ExportName = $"{Constants.AppName}-LambdaFunctionName"
        });

        _ = new CfnOutput(this, "LambdaFunctionArn", new CfnOutputProps
        {
            Description = "Lambda Function ARN",
            Value = LambdaFunction.FunctionArn,
            ExportName = $"{Constants.AppName}-LambdaFunctionArn"
        });

        _ = new CfnOutput(this, "RdsProxyEndpoint", new CfnOutputProps
        {
            Description = "RDS Proxy Endpoint",
            Value = RdsProxy.Endpoint,
            ExportName = $"{Constants.AppName}-RdsProxyEndpoint"
        });
    }
}
