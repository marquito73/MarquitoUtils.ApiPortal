using MarquitoUtils.ApiPortal.Middleware;
using MarquitoUtils.Main.Api.Configuration;
using MarquitoUtils.Main.Files.Services;
using MarquitoUtils.Main.Sql.Context;
using MarquitoUtils.Main.Sql.Entities;
using MarquitoUtils.Main.Sql.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace MarquitoUtils.ApiPortal.Startup
{
    /// <summary>
    /// Provides a base class for configuring and initializing an application with a specified database context.
    /// </summary>
    /// <remarks>This class is responsible for managing API configuration, database context initialization,
    /// and service configuration. It includes methods for setting up JWT authentication and configuring Swagger for API
    /// documentation.</remarks>
    /// <typeparam name="DBContext">The type of the database context, which must inherit from <see cref="DefaultDbContext"/>.</typeparam>
    public abstract class DefaultStartup<DBContext>
        where DBContext : DefaultDbContext
    {
        /// <summary>
        /// The file service
        /// </summary>
        private IFileService FileService { get; set; } = new FileService();
        /// <summary>
        /// Entity service
        /// </summary>
        private IEntityService EntityService { get; set; }
        /// <summary>
        /// The database context
        /// </summary>
        protected DBContext DbContext { get; private set; }
        /// <summary>
        /// Gets the application's configuration settings.
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// Gets or sets the API configuration settings.
        /// </summary>
        private ApiConfiguration ApiConfiguration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStartup"/> class with the specified configuration.
        /// </summary>
        /// <remarks>This constructor sets up the application by managing API configuration and database
        /// context.</remarks>
        /// <param name="configuration">The configuration settings used to initialize the application.</param>
        public DefaultStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.ConfigureApiConfiguration();
            this.ConfigureDatabaseContext();
        }

        /// <summary>
        /// Loads and sets the API configuration from an XML file.
        /// </summary>
        /// <remarks>This method retrieves the API configuration data from a specified XML file and
        /// assigns it to the <see cref="ApiConfiguration"/> property. Ensure that the XML file exists and is accessible
        /// at the specified path and is an embedded resource.</remarks>
        private void ConfigureApiConfiguration()
        {
            this.ApiConfiguration = this.FileService.GetDataFromXMLFile<ApiConfiguration>(@"File\Configuration\Api.config");
        }

        /// <summary>
        /// Gets the title of the API.
        /// </summary>
        /// <returns>The title of the API.</returns>
        protected abstract string GetApiTitle();

        /// <summary>
        /// Gets the version of the API.
        /// </summary>
        /// <returns>The version of the API.</returns>
        protected abstract string GetApiVersion();

        /// <summary>
        /// Manage database
        /// </summary>
        private void ConfigureDatabaseContext()
        {
            DatabaseConfiguration databaseConfiguration =
                this.FileService.GetDefaultDatabaseConfiguration();
            // Init startup db context
            this.DbContext = DefaultDbContext
                .GetDbContext<DBContext>(databaseConfiguration);
            // Init entity service
            this.EntityService = new EntityService()
            {
                DbContext = this.DbContext,
            };
        }

        /// <summary>
        /// Configure services added to the app
        /// This method gets called by the runtime
        /// </summary>
        /// <param name="services">Services</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddControllers();
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc(this.GetApiVersion(), new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = this.GetApiTitle(),
                    Version = this.GetApiVersion(),
                });
            });
            // Add logging
            services.AddLogging();

            services.AddSingleton(this.EntityService);

            this.ConfigureJwtAuthentication(services);

            this.ConfigureDependencyInjection(services);
        }

        protected abstract void ConfigureDependencyInjection(IServiceCollection services);

        /// <summary>
        /// Manage JWT authentication
        /// </summary>
        /// <param name="services">Services</param>
        private void ConfigureJwtAuthentication(IServiceCollection services)
        {
            byte[] publicKeyBytes = Convert.FromBase64String(this.ApiConfiguration.ApiKey.PublicKey);
            ECDsa ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            // Implement JWT authentication management here if needed
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new ECDsaSecurityKey(ecdsa),
                    ValidateIssuer = true,
                    ValidIssuer = this.ApiConfiguration.Issuer,
                    ValidateAudience = false,
                    ClockSkew = this.GetClockSkew(),
                };
            });
        }

        /// <summary>
        /// Gets the clock skew for JWT authentication.
        /// </summary>
        /// <returns>The clock skew for JWT authentication.</returns>
        protected abstract TimeSpan GetClockSkew();

        /// <summary>
        /// Configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The web host environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.SwaggerEndpoint($"/swagger/{this.GetApiVersion()}/swagger.json",
                        $"{this.GetApiTitle()} {this.GetApiVersion()}");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            // Add logging middleware
            app.UseMiddleware<LoggingMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });
        }
    }
}
