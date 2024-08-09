using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiverAnalysCrawler.Common
{
   public struct  BotType
    {
        public const int CRAWL_REALTIME = 1;
        public const int CRAWL_SCHEDULER = 2;
        public const int SYNC_PRODUCT_MANUAL = 3;
    }
}
