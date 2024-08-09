using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
   public class ScrapyType
    {
        public enum CrawBy
        {
            QUEUE_RPC=1, // CRAWL THEO QUEUE CHUẨN 6 -- https://www.rabbitmq.com/getstarted.html
            QUEUE_WORK, // CRAWL THEO QUEUE CHUẨN 2 
            HTTP_REQUEST, // CRAWL THEO HTTP REQUEST
            API_USEXPRESS_OLD, // crawl theo api cũ bên USEXPRESS
            SELELIUM,
            FADO
        }
    }
}
