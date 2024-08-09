using System;
using System.Collections.Generic;
using System.Text;

namespace APP_EXECUTE_LOG.Models
{
    //"{'error_content':'không tìm thấy dữ liệu','group_id':'12345abcd','bot_token':'abcbcbcbcbcbc'}";
    class LogModel
    {
        public string error_content { get; set; }
        public string group_id { get; set; }
        public string bot_token { get; set; }

    }
}
