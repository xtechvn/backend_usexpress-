using Entities.ViewModels.Log;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.API.Service;
using WEB.API.Service.Log;

namespace WEB.API.Controllers
{
    [Route("log")]
    [ApiController]
    public class LogActivityController : BaseController
    {
        public IConfiguration _Configuration;
        public LogActivityController(IConfiguration Configuration)
        {
            _Configuration = Configuration;
        }
        [HttpPost("insert-log")]
        public async Task<IActionResult> AddLogActivity(string token)
        {
            JArray objParr = null;
            int _status = (int)ResponseType.FAILED;
            string _msg = "Token Invalid";
            var _token = token;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    int user_type= Convert.ToInt32(objParr[0]["user_type"].ToString());
                    long user_id=Convert.ToInt64(objParr[0]["user_id"].ToString());
                    string user_name = objParr[0]["user_name"].ToString();
                    int log_type = Convert.ToInt32(objParr[0]["log_type"].ToString());
                    string j_data_log = objParr[0]["j_data_log"].ToString();
                    string key_word_search = objParr[0]["key_word_search"].ToString();
                    if (user_id < 0 || user_name == null || user_name.Trim() == "" ||  log_type < -1 || j_data_log == null || j_data_log.Trim() == "")
                    {
                        _status = (int)ResponseType.FAILED;
                        _msg = "Token Invalid";
                    }
                    else
                    {
                        LogUsersActivityModel log = new LogUsersActivityModel()
                        {
                            user_type = user_type,
                            user_id = user_id,
                            user_name = user_name,
                            log_type = log_type,
                            log_date = DateTime.Now,
                            j_data_log = j_data_log,
                        };
                        string document_name = LogActivityBSONDocuments.CMS; ;
                        switch (log.log_type)
                        {
                            case (int)LogActivityType.CHANGE_ORDER_CMS:
                                {
                                    log.action_log = LogActivityName.CHANGE_ORDER_CMS;
                                    log.key_word_search = LogActivityKeyWord.CHANGE_ORDER_CMS + key_word_search;
                                    document_name = LogActivityBSONDocuments.CMS;
                                }
                                break;
                            case (int)LogActivityType.LOGIN_CMS:
                                {
                                    log.action_log = LogActivityName.LOGIN_CMS;
                                    log.key_word_search = LogActivityKeyWord.LOGIN_CMS + "," + log.user_name + key_word_search;
                                    document_name = LogActivityBSONDocuments.CMS;

                                }
                                break;
                            default:
                                {
                                    log.action_log = LogActivityName.ERROR;
                                    log.key_word_search = LogActivityKeyWord.ERROR;
                                    document_name = LogActivityBSONDocuments.CMS;
                                }
                                break;
                        }
                        string result_logging = await UsersLoggingService.InsertLog(_Configuration, log, document_name);
                        if (result_logging == "")
                        {
                            _status = (int)ResponseType.SUCCESS;
                            _msg = "Sucessful.";
                            _token = null;
                        }
                        else
                        {
                            _status = (int)ResponseType.FAILED;
                            _msg = result_logging;
                            _token = null;
                        }

                    }
                }
                else
                {
                    _status = (int)ResponseType.FAILED;
                    _msg = "Token Invalid";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("API - log/insert-log - token: " + token+"\n Error: "+ex.ToString());
                _status = (int)ResponseType.FAILED;
                _msg = "Token Invalid";
            }
            return Ok(new
            {
                status = _status,
                msg = _msg,
                token = _token,
            });
        }
        [HttpPost("get-log-by-id")]
        public async Task<IActionResult> GetLogActivityByID(string token)
        {
            JArray objParr = null;
            int _status = (int)ResponseType.FAILED;
            string _msg = "Token Invalid";
            var _token = token;
            dynamic _data=null;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string id = objParr[0]["id"].ToString();
                    if (id == null || id.Trim() == "")
                    {
                        _status = (int)ResponseType.FAILED;
                        _msg = "Token Invalid";
                    }
                    else
                    {
                        var client = new MongoClient("mongodb://" + _Configuration["DataBaseConfig:MongoServer:Host"] + "");
                        IMongoDatabase db = client.GetDatabase(_Configuration["DataBaseConfig:MongoServer:catalog"]);
                        IMongoCollection<LogUsersActivityModel> affCollection = db.GetCollection<LogUsersActivityModel>("UsersLogActivity");
                        var filter = Builders<LogUsersActivityModel>.Filter.Where(x => x.id == id);
                        var result_document = affCollection.Find(filter).ToList();
                        if (result_document != null && result_document.Count > 0)
                        {
                            _data = result_document;
                            _status = (int)ResponseType.SUCCESS;
                            _msg = "Sucessful.";
                            _token = null;
                        }
                        else
                        {
                            _data = null;
                            _status = (int)ResponseType.EMPTY;
                            _msg = "Empty.";
                        }
                        
                    }
                }
                else
                {
                    _status = (int)ResponseType.FAILED;
                    _msg = "Token Invalid";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("API - log/get-log-by-id - token: " + token + "\n Error: " + ex.ToString());
                _status = (int)ResponseType.FAILED;
                _msg = "Token Invalid";
            }
            return Ok(new
            {
                status = _status,
                msg = _msg,
                data = _data,
                token = _token
            });
        }
    }
}
