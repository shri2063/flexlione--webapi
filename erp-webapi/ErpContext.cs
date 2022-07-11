using flexli_erp_webapi.DataModels;
using Microsoft.EntityFrameworkCore;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;


namespace flexli_erp_webapi
{
    public class ErpContext:DbContext
    {
        public DbSet<TaskDetail> TaskDetail { get; set; }
        
        public DbSet<CheckList> CheckList { get; set; }
        
        public DbSet<Dependency> Dependency { get; set; }
        
        public DbSet<Profile> Profile { get; set; }
        
        public DbSet<Sprint> Sprint { get; set; }
        
        public DbSet<TaskSchedule> TaskSchedule { get; set; }
        
        public DbSet<TaskSummary> TaskSummary { get; set; }
        
        public DbSet<Comment> Comment { get; set; }
        
        public DbSet<TaskHierarchy> TaskHierarchy { get; set; }
        
        public DbSet<Template> Template { get; set; }
        
        
        
        public DbSet<TemplateTask> TemplateTask { get; set; }
        
        public DbSet<RegisterTimeStamp> RegisterTimeStamp { get; set; }

        // for logging sql queries
        private static readonly ILoggerFactory LoggerFactory  = new LoggerFactory().AddConsole((_,___) => true);

        private static string _connString = ""; // updated in Constructor

        /// <summary>
        /// Initialise Connection
        /// </summary>        
        public static void SetConnectionString(string defaultConnectionString)
        {
            _connString = defaultConnectionString;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLoggerFactory(LoggerFactory) //tie-up DbContext with LoggerFactory object
                .EnableSensitiveDataLogging()
                .UseNpgsql(_connString); // updated in Constructor

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // composite key for lineItems table
           // modelBuilder.Entity<StoreOrderLineItem>()
             //   .HasKey(lineItem => new { storeOrderId = lineItem.StoreOrderId, skuCode = lineItem.SkuCode });

             
            
        }
    }
}