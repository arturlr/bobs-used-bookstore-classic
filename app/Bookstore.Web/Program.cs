using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Logging;
using System;
using Amazon.RDS.Util;
using Amazon.Rekognition;
using Amazon.S3;
using BobsBookstoreClassic.Data;
using Bookstore.Data;
using Bookstore.Data.FileServices;
using Bookstore.Data.ImageResizeService;
using Bookstore.Data.ImageValidationServices;
using Bookstore.Data.Repositories;
using Bookstore.Domain;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NLog;
using NLog.Web;

namespace Bookstore.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Configure NLog
                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                // Initialize BookstoreConfiguration
                BookstoreConfiguration.Initialize(builder.Configuration);

                // Add services to the container
                ConfigureServices(builder.Services, builder.Configuration);

                var app = builder.Build();

                // Configure the HTTP request pipeline
                Configure(app, app.Environment);

                app.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application startup failed");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add Lambda hosting support (works alongside Kestrel)
            services.AddAWSLambdaHosting(Amazon.Lambda.AspNetCoreServer.LambdaEventSource.HttpApi);

            // Add MVC with Areas support
            services.AddControllersWithViews();

            // Configure DbContext
            var connectionString = GetConnectionString(configuration);
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    // Configure connection pooling for RDS Proxy compatibility
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                    // Connection timeout for Lambda cold starts
                    sqlServerOptions.CommandTimeout(30);
                }));

            // Register application services
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IReferenceDataService, ReferenceDataService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IShoppingCartService, ShoppingCartService>();
            services.AddScoped<IImageResizeService, ImageResizeService>();

            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();

            // Register generic PaginatedList
            services.AddScoped(typeof(IPaginatedList<>), typeof(PaginatedList<>));

            // Configure file and image services based on configuration
            var fileServiceType = configuration["Services:FileService"];
            if (fileServiceType == "aws")
            {
                services.AddAWSService<IAmazonS3>();
                services.AddScoped<IFileService, S3FileService>();
            }
            else
            {
                services.AddScoped<IFileService, LocalFileService>();
            }

            var imageValidationServiceType = configuration["Services:ImageValidationService"];
            if (imageValidationServiceType == "aws")
            {
                services.AddAWSService<IAmazonRekognition>();
                services.AddScoped<IImageValidationService, RekognitionImageValidationService>();
            }
            else
            {
                services.AddScoped<IImageValidationService, LocalImageValidationService>();
            }

            // Configure authentication
            var authType = configuration["Services:Authentication"];
            if (authType == "aws")
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.MetadataAddress = configuration["Authentication:Cognito:MetadataAddress"];
                    options.ClientId = configuration["Authentication:Cognito:LocalClientId"];
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                });
            }
            else
            {
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Authentication/Login";
                        options.LogoutPath = "/Authentication/Logout";
                    });
            }

            services.AddAuthorization();

            // Add session support
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        private static void Configure(WebApplication app, IHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            
            // Add local authentication middleware for development
            var authType = app.Configuration["Services:Authentication"];
            if (authType == "local")
            {
                app.UseMiddleware<LocalAuthenticationMiddleware>();
            }
            
            app.UseAuthorization();

            app.UseSession();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }

        private static string GetConnectionString(IConfiguration configuration)
        {
            // Check if we're running in Lambda with RDS Proxy
            var useRdsProxy = configuration.GetValue<bool>("Database:UseRdsProxy", false);
            
            if (useRdsProxy)
            {
                // Build connection string for RDS Proxy with IAM authentication
                var rdsProxyEndpoint = configuration["Database:RdsProxyEndpoint"];
                var databaseName = configuration["Database:DatabaseName"];
                var region = configuration["AWS:Region"] ?? Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
                var port = configuration.GetValue<int>("Database:Port", 1433);
                
                // Generate IAM authentication token for RDS Proxy
                try
                {
                    var authToken = RDSAuthTokenGenerator.GenerateAuthToken(rdsProxyEndpoint, port, "admin");
                    
                    return $"Server={rdsProxyEndpoint},{port};Database={databaseName};User Id=admin;Password={authToken};Encrypt=True;TrustServerCertificate=True;";
                }
                catch (Exception ex)
                {
                    // Log error and fall back to standard connection string
                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Error(ex, "Failed to generate RDS Proxy IAM authentication token, falling back to standard connection string");
                }
            }
            
            // Use standard connection string (for local dev or ECS with direct RDS connection)
            return configuration.GetConnectionString("BookstoreDatabaseConnection");
        }
    }
}
