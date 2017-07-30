using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using Quartz.Impl;
using Quartz;

namespace WebDemoQuartz
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            StartScheduler();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }

        private void StartScheduler()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = factory.GetScheduler().Result;
            scheduler.Start().Wait();
            ScheduleJobs(scheduler);
        }

        private void ScheduleJobs(IScheduler scheduler)
        {
            IJobDetail job = JobBuilder.Create<DemoJob>().WithIdentity("demoJob", "demoGroup").Build();
            // Trigger para que o job seja executado imediatamente, e repetidamente a cada 15 segundos
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("demoTrigger", "demoGroup")
              .StartNow()
              .WithSimpleSchedule(x => x
                  .WithIntervalInSeconds(15)
                  .RepeatForever())
              .Build();
            scheduler.ScheduleJob(job, trigger).Wait();
        }
    }
}
