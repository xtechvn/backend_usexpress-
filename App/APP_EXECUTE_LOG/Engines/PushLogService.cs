using APP_EXECUTE_LOG.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace APP_EXECUTE_LOG.Engines
{
    class PushLogService
    {
        public string InsertLogTelegram(LogModel msg)
        {
            try
            {
                LogHelper.InsertLogTelegram(msg.bot_token, msg.group_id, msg.error_content);
                return "";
                 
            } catch (Exception ex)
            {

                return ex.ToString();

            }
        }
        public string InsertLogElasticSearch(LogModel msg)
        {
            return "";
        }
        public string InsertLogDB(LogModel msg)
        {
            return "";
        }
    }
}
