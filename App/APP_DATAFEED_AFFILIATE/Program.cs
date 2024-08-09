using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
namespace APP_DATAFEED_AFFILIATE
{
    class Program
    {
        private static string schedule_time_minute = System.Configuration.ConfigurationManager.AppSettings["schedule_time_minute"];
        static void Main(string[] args)
        {
            //#region LISTENER 
            // construct a scheduler factory
            var schedFact = new StdSchedulerFactory();

            // get a scheduler, start the schedular before triggers or anything else
            IScheduler sched = schedFact.GetScheduler();
            sched.Start();

            // create job crawl today deal's
            IJobDetail job = JobBuilder.Create<MainSchedulingJob>()
                        .WithIdentity("jobCrawlTodayDealAmazon", "groupJob")
                        .Build();

            // create trigger
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "groupJob")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(Convert.ToInt32(schedule_time_minute) <= 0 ? 5 : Convert.ToInt32(schedule_time_minute)).RepeatForever())
                .Build();

            // Schedule the job using the job and trigger 
            sched.ScheduleJob(job, trigger);

            var myJobListener = new MyJobListener();
            myJobListener.Name = "CrawlListener";

            sched.ListenerManager.AddJobListener(myJobListener, KeyMatcher<JobKey>.KeyEquals(new JobKey("jobCrawlTodayDealAmazon", "groupJob")));
            //#endregion  
        }
    }
}
