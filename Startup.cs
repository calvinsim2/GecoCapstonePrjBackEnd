using CapstoneProjectBlog.Data.Context;
using CapstoneProjectBlog.Models;
using CapstoneProjectBlog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProjectBlog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CapstoneProjectBlog", Version = "v1" });
            });
            // we will do this step AFTER we created our DbContext
            // GameContext is the db context that we created.
            services.AddDbContext<BlogContext>(option =>
            {

                // Need to import UseSqlServer
                // Configuration is already provided by Startup, so we use this to input our sql connection string
                option.UseSqlServer(Configuration.GetConnectionString("SqlConnection"));
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddCors(options =>
            {
                options.AddPolicy(name: "MyPolicy", builder =>
                {
                    builder.AllowAnyOrigin();
                    // builder.WithOrigins(<insert ur frontend url here>, it accepts a string of array.)
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });
            // add authentication, AFTER we set up JWT token. 
            // For AddAuthentication, we need to declare which authentication scheme we are using,
            // in this case, it is JwtBearerDefaults.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey:Key"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                });
            services.AddControllersWithViews().AddNewtonsoftJson(OptionsBuilderConfigurationExtensions =>
            OptionsBuilderConfigurationExtensions.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            // actual implementation of dependency injection
            // we add this after setting up our MailService Model and Interface. 
            services.AddTransient<IMailService, MailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CapstoneProjectBlog v1"));
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            // when implementing CORS, we need this, and make sure this is ABOVE UseRouting();
            app.UseCors("MyPolicy");

            app.UseRouting();

            // when implementing authentication such as jwt, we have to call this,
            // make sure this is BEFORE UseAuthorization().
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
