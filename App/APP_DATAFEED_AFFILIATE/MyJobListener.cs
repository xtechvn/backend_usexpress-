using Quartz;
using System;

namespace APP_DATAFEED_AFFILIATE
{
   public class MyJobListener: IJobListener
    {
        void IJobListener.JobExecutionVetoed(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        void IJobListener.JobToBeExecuted(IJobExecutionContext context)
        {
            Console.WriteLine("\r\n Job running...");
        }

        void IJobListener.JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Console.WriteLine("Job execute done...");
        }

        // this property is REQUIRED and must be SET before you use the joblistener.
        public string Name { get; set; }
    }
}
