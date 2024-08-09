using Caching.Elasticsearch;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.API.Common;
using WEB.API.Models.Product;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppDataController : BaseController
    {
        private readonly IGroupProductRepository _groupRepository;
        private readonly IGroupProductStoreRepository _groupProductStoreRepository;
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;

        private readonly IProductClassificationRepository _productClassificationRepository;
        public IConfiguration _Configuration;
        public AppDataController(IConfiguration config, IGroupProductRepository groupProductRepository,
            IGroupProductStoreRepository groupProductStoreRepository, IProductClassificationRepository ProductClassificationRepository)
        {
            _Configuration = config;
            _groupRepository = groupProductRepository;
            _groupProductStoreRepository = groupProductStoreRepository;
            _productClassificationRepository = ProductClassificationRepository;
            _productDetailMongoAccess = new ProductDetailMongoAccess(config);
        }
        [HttpPost("get-all-group-product.json")]
        public async Task<ActionResult> GetAllGroupProduct(string token)
        {
            string _msg = string.Empty;
            int status = 0;
            try
            {

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    DateTime exprire_time = DateTime.Parse(objParr[0]["time"].ToString());
                    if (exprire_time != null && exprire_time < DateTime.Now.ToUniversalTime().AddMinutes(30))
                    {
                        var group_list = await _groupRepository.GetActiveCrawlGroupProducts();
                        _msg = JsonConvert.SerializeObject(group_list);
                        status = (int)ResponseType.SUCCESS;
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        _msg = "Token khong hop le: token = " + token;
                    }

                }
                else
                {
                    status = (int)ResponseType.FAILED;
                    _msg = "Token khong hop le: token = " + token;
                }
                return Content(JsonConvert.SerializeObject(ResponseApi(status.ToString(), _msg, token, "")));

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/GroupProduct/get-all-group-product.json: " + ex);
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Error On Excution!", token, "")));
            }
        }
        [HttpPost("get-all-group-product-store.json")]
        public async Task<ActionResult> GetAllGroupProductStore(string token)
        {
            string _msg = string.Empty;
            int status = 0;
            try
            {

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    DateTime exprire_time = DateTime.Parse(objParr[0]["time"].ToString());
                    if (exprire_time != null && exprire_time < DateTime.Now.ToUniversalTime().AddMinutes(30))
                    {
                        var group_list = await _groupProductStoreRepository.GetAll();
                        _msg = JsonConvert.SerializeObject(group_list);
                        status = (int)ResponseType.SUCCESS;
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        _msg = "Token khong hop le: token = " + token;
                    }

                }
                else
                {
                    status = (int)ResponseType.FAILED;
                    _msg = "Token khong hop le: token = " + token;
                }
                return Content(JsonConvert.SerializeObject(ResponseApi(status.ToString(), _msg, token, "")));

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/GroupProduct/get-all-group-product-store: " + ex);
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Error On Excution!", token, "")));
            }
        }
        [HttpPost("get-all-product-classification.json")]
        public async Task<ActionResult> GetAllProductClassification(string token)
        {
            string _msg = string.Empty;
            int status = 0;
            try
            {

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    DateTime exprire_time = DateTime.Parse(objParr[0]["time"].ToString());
                    if (exprire_time != null && exprire_time < DateTime.Now.ToUniversalTime().AddMinutes(30))
                    {
                        var group_list = await _productClassificationRepository.GetAll();
                        _msg = JsonConvert.SerializeObject(group_list);
                        status = (int)ResponseType.SUCCESS;
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        _msg = "Token khong hop le: token = " + token;
                    }

                }
                else
                {
                    status = (int)ResponseType.FAILED;
                    _msg = "Token khong hop le: token = " + token;
                }
                return Content(JsonConvert.SerializeObject(ResponseApi(status.ToString(), _msg, token, "")));


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/Appdata/get-all-product-classification: " + ex);
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Error On Excution!", token, "")));
            }
        }
        [HttpPost("get-product-from-mongo.json")]
        public async Task<ActionResult> GetDetailFromMongo(string token)
        {
            string _msg = string.Empty;
            int status = 0;
            dynamic data = null;
            try
            {

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    string id = objParr[0]["id"].ToString();
                    if (id != null && id.Trim()!="")
                    {
                        data = (await _productDetailMongoAccess.FindByID(id)).product_detail;
                        status = (int)ResponseType.SUCCESS;
                        _msg = "Success.";
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        _msg = "Token khong hop le: token = " + token;
                    }

                }
                else
                {
                    status = (int)ResponseType.FAILED;
                    _msg = "Token khong hop le: token = " + token;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/GroupProduct/get-detail-from-mongo.json: " + ex);
            }
            return Ok(new
            {
                status = status,
                msg = _msg,
                data= data
            });
        }

    }
    
}
