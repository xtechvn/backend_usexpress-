using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;
using Utilities.Contants;
using WEB.API.Service.Queue;
using WEB.API.ViewModels;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : BaseController
    {
        public IConfiguration _Configuration;
        public LogController(IConfiguration Configuration)
        {
            _Configuration = Configuration;
        }
        [Route("insert-log")]
        public ActionResult InsertLog(string token)
        {
            JArray objParr = null;
            var st = new Stopwatch();
            st.Start();
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string error_content = objParr[0]["error_content"].ToString();
                    string group_id = objParr[0]["group_id"].ToString();
                    string bot_token = objParr[0]["bot_token"].ToString();
                    LogQueueViewModel log = new LogQueueViewModel()
                    {
                        bot_token = bot_token,
                        error_content = error_content,
                        group_id = group_id
                    };
                    string _data_push = JsonConvert.SerializeObject(log);
                    string _queue_name = TaskQueueName.log_front_end;
                    // Execute Push Queue
                    var work_queue = new WorkQueueClient();
                    var queue_setting = new QueueSettingViewModel
                    {
                        host = _Configuration["Queue:Host"],
                        v_host = _Configuration["Queue:V_Host"],
                        port = Convert.ToInt32(_Configuration["Queue:Port"]),
                        username = _Configuration["Queue:Username"],
                        password = _Configuration["Queue:Password"]
                    };
                    bool response_queue = work_queue.InsertQueueSimple(queue_setting, _data_push, _queue_name);
                    if (response_queue)
                    {
                        st.Stop();

                        //LogHelper.InsertLogTelegram(_data_push + " publish queue success !" + "==> token = " + token);
                        return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.SUCCESS.ToString(), "Push Queue Success", token, st.ElapsedMilliseconds + " ms")));
                    }
                    else
                    {
                        st.Stop();

                        //LogHelper.InsertLogTelegram(" publish queue ERROR !" + "==> token = " + token);
                        return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Push Queue ERROR !!!", token, st.ElapsedMilliseconds + " ms")));
                    }
                }
                else
                {
                    st.Stop();
                   // LogHelper.InsertLogTelegram("Push Queue Faild: Token invalid !" + "==> token = " + token);
                    return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.EXISTS.ToString(), "Push Queue Faild: Token invalid !!!", token, st.ElapsedMilliseconds + " ms")));
                }
            }
            catch (Exception ex)
            {
                st.Stop();
                //LogHelper.InsertLogTelegram(ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + "==> error:  " + ex.Message + "==> token =" + token);
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.EXISTS.ToString(), "Token invalid !!!", token, st.ElapsedMilliseconds + " ms")));
            }
        }

    }
   
}
