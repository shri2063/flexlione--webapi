

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using flexli_erp_webapi.DataLayer;
using flexli_erp_webapi.DataLayer.Interface;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch;
using flexli_erp_webapi.Policy.SearchPolicy.TemplateSearch.Interfaces;
using flexli_erp_webapi.Repository;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.Services.Interfaces;
using flexli_erp_webapi.Services.Scoring;
using flexli_erp_webapi.Services.SearchPolicy;
using flexli_erp_webapi.Services.TaskSearch;
using flexli_erp_webapi.Utility;
using m;
using m_sort_server;
using m_sort_server.Repository;
using m_sort_server.Repository.Interfaces;
using mflexli_erp_webapi.Repository.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

namespace flexli_erp_webapi
{
    
    using Swashbuckle.AspNetCore.SwaggerUI;
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        
        public Startup(IConfiguration configuration)
        {
            ErpContext.SetConnectionString(configuration["dbConnectionString"]);

        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddScoped<ITaskSearchResultRepository, TaskSearchResultRepository>();
            services.AddScoped<TaskManagementService, TaskManagementService>();
            services.AddScoped<CheckListManagementService, CheckListManagementService>();
            services.AddScoped<DependencyManagementService, DependencyManagementService>();
            services.AddScoped<ProfileManagementService, ProfileManagementService>();
            services.AddScoped<SprintManagementService, SprintManagementService>();
            services.AddScoped<TaskScheduleManagementService, TaskScheduleManagementService>();
            services.AddScoped<TaskSummaryManagementService, TaskSummaryManagementService>();
            services.AddScoped<TaskHierarchyManagementService, TaskHierarchyManagementService>();
          
            
            services.AddScoped<SprintReportManagementService, SprintReportManagementService>();
            services.AddScoped<ISearchPriorityPolicy, SearchPriorityByCommonalityPolicy>();
            services.AddScoped<ITemplateSearchResultRepository, TemplateSearchResultRepository>();
            services.AddScoped<ITemplateTagContext, TemplateTagContext>();
            services.AddScoped<ITagContext, TagContext>();
   
            services.AddScoped<IScoreAllocationPolicy, BinaryScoreAllocationPolicy>();
            services.AddScoped<IScoreAllocationPolicy, IncrementalScoreAllocationPolicy>();

           // services.AddSingleton<ICalculateScoreForTaskPolicyService, BinaryScoreAllocationPolicy>();
           // services.AddSingleton<ICalculateScoreForTaskPolicyService, IncrementalScoreAllocationPolicy>();

            services.AddScoped<ScoreAllocationPolicySelectorPolicy, ScoreAllocationPolicySelectorPolicy>();
            services.AddScoped<ISprintUnplannedTaskManagementService, SprintUnplannedTaskManagementService>();
            services.AddScoped<ISprintUnplannedTaskRepository, SprintUnplannedTaskRepository>();
            services.AddScoped<ScoreAllocationPolicySelectorPolicy, ScoreAllocationPolicySelectorPolicy>();
            services.AddScoped<SprintManagementService, SprintManagementService>();
            services.AddScoped<ITagContext, TagContext>();
            // services.AddScoped<ITagTaskListRepository, TagTaskListRepository>();
            // services.AddScoped<ITagRepository, TagRepository>();

            services.AddScoped<ITaskRepository, TaskRepository>();
             services.AddScoped<ITaskHierarchyRelationRepository, TaskHierarchyRelationRepository>();
             services.AddScoped<ITaskSummaryRepository, TaskSummaryRepository>();
             services.AddScoped<ITaskAnchorRepository, TaskAnchorRepository>();
             services.AddScoped<ITaskRelationRepository, TaskRelationRepository>();
             services.AddScoped<ITaskScheduleRepository, TaskScheduleRepository>();
             services.AddScoped<ITaskScheduleRelationRepository, TaskScheduleRelationRepository>();
             services.AddScoped<ISprintRepository, SprintRepository>();
             services.AddScoped<ISprintReportRepository, SprintReportRepository>();
             services.AddScoped<ISprintReportRelationRepository, SprintReportRelationRepository>();
             services.AddScoped<ISprintRelationRepository, SprintRelationRepository>();
             services.AddScoped<ILabelRelationRepository, LabelRelationRepository>();
             services.AddScoped<ICheckListRepository, CheckListRepository>(); 
             services.AddScoped<IDependencyRepository, DependencyRepository>();
             services.AddScoped<ITaskHourCalculatorHandler, TaskSummaryManagementService>();
             services.AddScoped<IProfileRepository, ProfileRepository>();
            
             services.AddScoped<ISprintUnplannedTaskRepository, SprintUnplannedTaskRepository>();
             services.AddScoped<ITemplateRepository, TemplateRepository>();
             services.AddScoped<ITemplateRelationRepository, TemplateRelationRepository>();
             services.AddScoped<TaskSearchManagementService, TaskSearchManagementService>();
             services.AddScoped<ITemplateManagementService, TemplateManagementService>();
             services.AddScoped<TemplateMainService, TemplateMainService>();
             services.AddScoped<TagSearchManagementService, TagSearchManagementService>();
             services.AddScoped<SearchByLabelManagementService, SearchByLabelManagementService>();
             services.AddScoped<TaskSearchResultRelationRepository, TaskSearchResultRelationRepository>();
             services.AddScoped<IIgnoreSearchWordRepository, IgnoreSearchWordRepository>();
             services.AddScoped<ITaskSummaryRelationRepository, TaskSummaryRelationRepository>();
             services.AddScoped<ISprintLifeCycleManagementService, SprintLifeCycleManagementService>();
             services.AddScoped<ICheckListRelationRepository, CheckListRelationRepository>();
             services.AddScoped<ITaskRankingManagementService, TaskRankingManagementService>();
             services.AddScoped<ITaskValidatorService, TaskValidatorService>();
             
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
            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = Int32.MaxValue;
                o.MultipartBodyLengthLimit = Int32.MaxValue;
                o.MemoryBufferThreshold = Int32.MaxValue;
            });
            
            services.AddSwaggerGenNewtonsoftSupport();

            services.AddSwaggerGen(c =>
            {
              
                c.SwaggerDoc("v1.0",new OpenApiInfo()
                {
                    Title = "ERP Server",
                    Description = "API for ERP server",
                    Version = "v1.9"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                //https://stackoverflow.com/questions/36452468/swagger-ui-web-api-documentation-present-enums-as-strings
                c.DescribeAllEnumsAsStrings();

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
               // c.OperationFilter<ApiVersionOperationFilter>();
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
            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {

             FileProvider = new PhysicalFileProvider("C:\\inetpub\\wwwroot"),

                // RequestPath = new PathString("/Resources")
                 // FileProvider = new PhysicalFileProvider("/Users/rahulbahuguna/Data/Flexli/OM/inetpub/wwwroot"),
                
            });
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1.0/swagger.json", "Versioned Api v1.9");
                c.DocExpansion(DocExpansion.None);
                c.DefaultModelExpandDepth(0);
                c.DefaultModelsExpandDepth(-1);
            });
        }
    }
}