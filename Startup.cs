using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using m_sort_server.Interfaces;
using m_sort_server.Services;
using m_sort_server.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;

namespace m_sort_server
{
    
    using Swashbuckle.AspNetCore.SwaggerUI;
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        
        public Startup(IConfiguration configuration)
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogFileService, LogFileService>();
            services.AddSingleton<IBotOperator, BotOperator>();
            services.AddSingleton<IBotManager, BotManagers>();
            services.AddSingleton<IBotConfig,BotConfig>();
            services.AddSingleton<IBotHolder,BotHolder>();
            services.AddSingleton<ITransportOperator,TransportOperator>();
            services.AddSingleton<IBotCommand,BotCommandAutoMode>();
            services.AddSingleton<ISimulatorService,SimulatorService>();
            
             services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddMvc(options =>
            {
                // for idempotency. TODO: Enable it after this is implemented in hht and angular
                // options.Filters.Add(new IdempotencyFilter());
                
                options.Filters.Add(new ApiExceptionFilter());

                // Request processing when model is invalid. For this filter to work, the automatic response
                // processing must be disabled.
                //options.Filters.Add(new ValidateModelAttribute());
                
                // remove formatter that turns nulls into 204 - No Content responses
                // this formatter breaks Angular's Http response JSON parsing
                options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
            }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());    // Serialize enum as string instead of integer

                    // We would like to serialize a null field if value is actually null
                    // We do want to serailize a null field if the value is not populated
                    //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // Do not serialize null values
                });

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });


            services.AddRouting(options => options.LowercaseUrls = true);

            //Logging
            services.AddLogging();

            services.AddSwaggerGen(c =>
            {
              
                c.SwaggerDoc("v1.0",new Info()
                {
                    Title = "M-Sort Server",
                    Description = "API for M-Sort server",
                    Version = "v1.0"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }

                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }

                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });
                c.OperationFilter<ApiVersionOperationFilter>();
            });

            // Disable automatic response when model is invalid
            // Ref: https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-2.1#automatic-http-400-responses
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("AllowOrigin");
            //loggerFactory.add
            if (env.IsDevelopment())
            {
               // app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1.0/swagger.json", "Versioned Api v1.0");
                c.DocExpansion(DocExpansion.None);
                c.DefaultModelExpandDepth(0);
                c.DefaultModelsExpandDepth(-1);
            });
        }
    }
}