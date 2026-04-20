using System;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.Rekognition;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using BobsBookstoreClassic.Data;
using Bookstore.Common;
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using NLog.Targets;
using NLog.Web;

namespace Bookstore.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Load configuration into BookstoreConfiguration ---
            var configuration = builder.Configuration;

            // Populate BookstoreConfiguration from appsettings.json AppSettings section
            var appSettingsSection = configuration.GetSection("AppSettings");
            foreach (var setting in appSettingsSection.GetChildren())
            {
                var value = setting.Value;
                // Allow environment variables to override
                if (Environment.GetEnvironmentVariable(setting.Key) != null)
                {
                    value = Environment.GetEnvironmentVariable(setting.Key);
                }
                BookstoreConfiguration.AddSetting(setting.Key, value);
            }

            // Populate connection strings
            var connectionStringsSection = configuration.GetSection("ConnectionStrings");
            foreach (var cs in connectionStringsSection.GetChildren())
            {
                BookstoreConfiguration.AddConnectionString(cs.Key, cs.Value);
            }

            // --- SSM Parameter Loading (migrated from ConfigurationSetup) ---
            ConfigureFromSSM();

            // --- Logging (migrated from LoggingSetup) ---
            ConfigureNLog();
            builder.Host.UseNLog();

            // --- Services ---
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // EF Core DbContext
            var connectionString = BookstoreConfiguration.GetConnectionString("BookstoreDatabaseConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Domain services
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
            builder.Services.AddScoped<IImageResizeService, ImageResizeService>();

            // Repositories
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddScoped<IOfferRepository, OfferRepository>();
            builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();

            // PaginatedList generic registration
            builder.Services.AddScoped(typeof(IPaginatedList<>), typeof(PaginatedList<>));

            // File service (conditional)
            if (BookstoreConfiguration.GetSetting("Services/FileService") == "aws")
            {
                builder.Services.AddSingleton<IAmazonS3, AmazonS3Client>();
                builder.Services.AddScoped<IFileService, S3FileService>();
            }
            else
            {
                var webRootPath = Path.Combine(builder.Environment.WebRootPath ?? builder.Environment.ContentRootPath, "Content");
                builder.Services.AddSingleton<IFileService>(new LocalFileService(webRootPath));
            }

            // Image validation service (conditional)
            if (BookstoreConfiguration.GetSetting("Services/ImageValidationService") == "aws")
            {
                builder.Services.AddSingleton<IAmazonRekognition, AmazonRekognitionClient>();
                builder.Services.AddScoped<IImageValidationService, RekognitionImageValidationService>();
            }
            else
            {
                builder.Services.AddScoped<IImageValidationService, LocalImageValidationService>();
            }

            // --- Authentication (migrated from AuthenticationSetup) ---
            if (BookstoreConfiguration.GetSetting("Services/Authentication") == "aws")
            {
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.ClientId = BookstoreConfiguration.GetSetting("Authentication/Cognito/LocalClientId");
                    options.MetadataAddress = BookstoreConfiguration.GetSetting("Authentication/Cognito/MetadataAddress");
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.SaveTokens = true;
                    options.UseTokenLifetime = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "cognito:username",
                        RoleClaimType = "cognito:groups"
                    };
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            var returnUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/";
                            context.ProtocolMessage.RedirectUri = returnUrl;
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var serviceProvider = context.HttpContext.RequestServices;
                            var customerService = serviceProvider.GetRequiredService<ICustomerService>();

                            var identity = (ClaimsIdentity)context.Principal.Identity;

                            var dto = new CreateOrUpdateCustomerDto(
                                identity.GetSub(),
                                identity.Name,
                                identity.FindFirst(y => y.Type.Contains("givenname"))?.Value ?? string.Empty,
                                identity.FindFirst(y => y.Type.Contains("surname"))?.Value ?? string.Empty);

                            await customerService.CreateOrUpdateCustomerAsync(dto);
                        }
                    };
                });
            }
            else
            {
                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie();
            }

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // --- Middleware Pipeline ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            // Local authentication middleware
            if (BookstoreConfiguration.GetSetting("Services/Authentication") != "aws")
            {
                app.UseMiddleware<LocalAuthenticationMiddleware>();
            }

            // --- Routing ---
            app.MapAreaControllerRoute(
                name: "admin",
                areaName: "Admin",
                pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void ConfigureNLog()
        {
            var config = new LoggingConfiguration();

            NLog.Targets.Target loggingTarget;

            if (BookstoreConfiguration.GetSetting("Services/LoggingService") == "aws")
            {
                loggingTarget = new AWSTarget { LogGroup = Constants.AppName };
            }
            else
            {
                loggingTarget = new DebuggerTarget();
            }

            config.AddTarget("aws", loggingTarget);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, loggingTarget));

            LogManager.Configuration = config;
        }

        private static void ConfigureFromSSM()
        {
            var rootPath = "/" + Constants.AppName;
            const string databasePath = "/Database";
            const string authenticationPath = "/Authentication";
            const string fileServicePath = "/Files";

            if (BookstoreConfiguration.GetSetting("Services/Database") == "aws")
            {
                using (var client = new AmazonSimpleSystemsManagementClient())
                {
                    var request = new GetParameterRequest { Name = $"{rootPath}{databasePath}/ConnectionStrings/BookstoreDatabaseConnection" };
                    var response = client.GetParameterAsync(request).GetAwaiter().GetResult();
                    BookstoreConfiguration.AddSetting(response.Parameter.Name.Replace($"{rootPath}{databasePath}/", string.Empty), response.Parameter.Value);
                }
            }

            if (BookstoreConfiguration.GetSetting("Services/Authentication") == "aws")
            {
                using (var client = new AmazonSimpleSystemsManagementClient())
                {
                    var request = new GetParametersByPathRequest { Path = $"{rootPath}{authenticationPath}/", Recursive = true };
                    var response = client.GetParametersByPathAsync(request).GetAwaiter().GetResult();
                    foreach (var parameter in response.Parameters)
                    {
                        BookstoreConfiguration.AddSetting(parameter.Name.Replace($"{rootPath}/", string.Empty), parameter.Value);
                    }
                }
            }

            if (BookstoreConfiguration.GetSetting("Services/FileService") == "aws")
            {
                using (var client = new AmazonSimpleSystemsManagementClient())
                {
                    var request = new GetParametersByPathRequest { Path = $"{rootPath}{fileServicePath}/", Recursive = true };
                    var response = client.GetParametersByPathAsync(request).GetAwaiter().GetResult();
                    foreach (var parameter in response.Parameters)
                    {
                        BookstoreConfiguration.AddSetting(parameter.Name.Replace($"{rootPath}/", string.Empty), parameter.Value);
                    }
                }
            }
        }
    }
}
