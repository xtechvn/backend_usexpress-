using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels.GroupProducts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.API.Common;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupProductController : BaseController
    {
        private readonly IGroupProductRepository _groupRepository;
        private readonly RedisConn _redisService;
        public IConfiguration _Configuration;
        public GroupProductController(IConfiguration config, IGroupProductRepository groupProductRepository, RedisConn redisService)
        {
            _Configuration = config;
            _redisService = redisService;
            _groupRepository = groupProductRepository;
        }

        [HttpPost("get-detail-by-campaign-id.json")]
        public async Task<ActionResult> getGroupProductDetailByCampaignId(string token)
        {
            string _msg = string.Empty;
            try
            {
                //Test
                // string j_param = "{'campaign_id':'24','skip':'0','take':'20'}";
                //  token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    int campaign_id = Convert.ToInt32(objParr[0]["campaign_id"]);
                    int skip = Convert.ToInt32(objParr[0]["skip"]);
                    int take = Convert.ToInt32(objParr[0]["take"]);

                    string cache_key = CacheType.CAMPAIGN_ID_ + campaign_id;
                    var j_data = await _redisService.GetAsync(cache_key, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                    // Kiểm tra có trong cache ko
                    if (j_data != null)
                    {
                        return Content(j_data); // trả thẳng từ cache ra luôn
                    }

                    // Get dữ liệu trong DATABASE
                    var group_list = await _groupRepository.getCategoryDetailByCampaignId(campaign_id, skip, take);

                    if (group_list.Count() == 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "[getGroupProductDetailByGroupId] = Khong co chuyen muc nao thoa man Campaign_id = " + campaign_id
                        });

                    }
                    var rs = group_list.Select(n => new
                    {
                        id = n.Id,
                        name = n.Name,
                        link = n.Path,
                        image_thumb = n.ImagePath,
                        desc = n.Description
                    });
                    // Tạo Object
                    var obj_response = new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = rs.ToList()
                    };

                    // Có data set lên Redis
                    _redisService.Set(cache_key, JsonConvert.SerializeObject(obj_response), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                    return Content(JsonConvert.SerializeObject(obj_response));

                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }


                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = _msg
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/GroupProduct/get-detail-by-id.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[getGroupProductDetailByGroupId] = " + ex.ToString()
                });
            }
        }

        [HttpPost("get-all.json")]
        public async Task<ActionResult> getAllGroupProduct(string token)
        {
            string _msg = string.Empty;
            try
            {
                //Test
                //string j_param = "{'status':'0'}";
                // token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {

                    string cache_key = CacheType.MENU_GROUP_PRODUCT;
                    var j_data = await _redisService.GetAsync(cache_key, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                    // Kiểm tra có trong cache ko
                    if (j_data != null)
                    {
                        return Content(j_data); // trả thẳng từ cache ra luôn
                    }
                    else
                    {
                        // Get dữ liệu trong DATABASE
                        var group_list = await _groupRepository.getAllGroupProduct();

                        if (group_list.Count() == 0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.EMPTY,
                                msg = "[getAllGroupProduct] Khong tim thay nhom hang co token = " + token
                            });

                        }
                        var rs = group_list.Select(n => new
                        {
                            id = n.Id,
                            name = n.Name,
                            link = n.Path,
                            image_thumb = n.ImagePath,
                            desc = n.Description,
                            parent_id = n.ParentId,
                            order_no = n.OrderNo,
                            is_show_header = n.IsShowHeader,
                            is_show_footer = n.IsShowFooter
                        });
                        // Tạo Object
                        var obj_response = new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = rs.ToList()
                        };

                        // Có data set lên Redis
                        _redisService.Set(cache_key, JsonConvert.SerializeObject(obj_response), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                        return Content(JsonConvert.SerializeObject(obj_response));
                    }
                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = _msg
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/GroupProduct/get-all-group-product.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[getAllGroupProduct] = " + ex.ToString()
                });
            }
        }


        [HttpPost("get-detail-by-path.json")]
        public async Task<ActionResult> getDetailGroupProduct(string token)
        {
            string _msg = string.Empty;
            try
            {
                //Test
                // string j_param = "{'path':'michael-kors'}";
                //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string path = (objParr[0]["path"]).ToString();
                    string cache_key = CacheType.GROUP_PRODUCT_DETAIL + path;
                    var j_data = await _redisService.GetAsync(cache_key, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                    // Kiểm tra có trong cache ko
                    if (j_data != null)
                    {
                        return Content(j_data); // trả thẳng từ cache ra luôn
                    }
                    else
                    {
                        // Get dữ liệu trong DATABASE
                        var group_detail = await _groupRepository.getDetailByPath(path);

                        if (group_detail == null)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.EMPTY,
                                msg = "[getDetailGroupProduct] Khong tim thay nhom hang co token = " + token
                            });

                        }
                        var gr_detail = new Dictionary<string, string>(){
                            { "id", group_detail.Id.ToString() },
                            { "name", group_detail.Name },
                            { "link", group_detail.Path },
                            { "image_thumb" , group_detail.ImagePath },
                            { "desc" , group_detail.Description },
                            { "parent_id" , group_detail.ParentId.ToString() },
                            { "order_no" , group_detail.OrderNo.ToString() },
                            { "is_show_header" , group_detail.IsShowHeader.ToString() },
                            { "is_show_footer" , group_detail.IsShowFooter.ToString() }
                        };
                        // Tạo Object
                        var obj_response = new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = JsonConvert.SerializeObject(gr_detail)
                        };

                        // Có data set lên Redis
                        _redisService.Set(cache_key, JsonConvert.SerializeObject(obj_response), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                        return Content(JsonConvert.SerializeObject(obj_response));
                    }
                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = _msg
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/GroupProduct/get-detail-by-path.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[getAllGroupProduct] = " + ex.ToString()
                });
            }
        }

        /// <summary>
        /// api này sẽ lấy ra những mã sản phẩm khong nằm trong hệ thống
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-list-id-not-exits-product.json")]
        public async Task<ActionResult> getListEsProductCode(string token)
        {
            string _msg = string.Empty;
            try
            {
                //Test
                //string j_param = "{'product_list_target':'B07H97D57R,A2,B07RFNNDFX,B07XJZBQ1S,B084P3JYJ4,A1,A5'}";
                //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string product_list = (objParr[0]["product_list_target"]).ToString();
                    int group_id = Convert.ToInt32((objParr[0]["group_id"]).ToString());
                    string ES_HOST = _Configuration["DataBaseConfig:Elastic:Host"];
                    var ESRepository = new ESRepository<object>(ES_HOST);
                    var result_lst_product_code_not_exits = await ESRepository.getListProductCodeNotExits(_Configuration["DataBaseConfig:Elastic:index_product_search"], product_list.Split(",").ToList(), group_id);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = JsonConvert.SerializeObject(result_lst_product_code_not_exits)
                    });

                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = _msg
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api:get-list-id-not-exits-product.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[getListEsProductCode] = " + ex.ToString()
                });
            }
        }
        [HttpPost("get-group-product-featured.json")]
        public async Task<ActionResult> GetFeaturedGroupProduct(string token)
        {
            string _msg = string.Empty;
            int _status = 1;
            List<GroupProductFeaturedViewModel> data = null;
            try
            {
                JArray objParr = null;

                //  string j_param = "{'status':0,'position':'header'}";
                //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    int status = Convert.ToInt32(objParr[0]["status"].ToString());
                    string position = objParr[0]["position"] == null ? "header" : objParr[0]["position"].ToString();
                    if (status == (Int16)Status.HOAT_DONG)
                    {
                        data = await _groupRepository.GetGroupProductFeatureds(ReadFile.LoadConfig().API_IMG_UPLOAD, position);
                        _status = (int)ResponseType.SUCCESS;
                        _msg = "Success.";
                    }
                }
                else
                {
                    _status = (int)ResponseType.FAILED;
                    _msg = "Token khong hop le: token = " + token;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api:get-group-product-featured.json: " + ex);
                _status = (int)ResponseType.ERROR;
                _msg = "Error On Excution";
            }
            return Ok(new
            {
                status = _status,
                msg = _msg,
                data = data
            });
        }
        [HttpPost("get-group-product-name.json")]
        public async Task<ActionResult> GetGroupProductName(string token)
        {
            string _msg = string.Empty;
            int _status = 1;
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    int group_id = Convert.ToInt32(objParr[0]["group_id"].ToString());
                    var a = await _groupRepository.GetById(group_id);
                    data = a.Name;
                    _status = (int)ResponseType.SUCCESS;
                    _msg = "Success.";
                }
                else
                {
                    _status = (int)ResponseType.FAILED;
                    _msg = "Token khong hop le: token = " + token;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api:get-group-product-detail.json: " + ex);
                _status = (int)ResponseType.ERROR;
                _msg = "Error On Excution";
            }
            return Ok(new
            {
                status = _status,
                msg = _msg,
                data = data
            });
        }

        /// <summary>
        /// Lấy trong db trường product_code những sản phẩm thuộc các chuyên mục được SET
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-product-code-by-group-id.json")]
        public async Task<ActionResult> getProductCodeByGroupId(string token)
        {
            string _msg = string.Empty;
            try
            {
                //Test
               // string j_param = "{'group_product_id':'400'}";
               // token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    int group_product_id = Convert.ToInt32(objParr[0]["group_product_id"]);

                    string cache_key = CacheType.GROUP_PRODUCT_MANUAL + group_product_id;
                    var j_data = await _redisService.GetAsync(cache_key, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                    // Kiểm tra có trong cache ko
                    if (j_data != null)
                    {
                        return Content(j_data); // trả thẳng từ cache ra luôn
                    }

                    // Get dữ liệu trong DATABASE
                    var product_code_list = await _groupRepository.getProductCodeByGroupId(group_product_id);

                    if (product_code_list.Count() == 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "[getProductCodeByGroupId] = Khong co sản phẩm  nao thoa man group_product_id = " + group_product_id
                        });

                    }
                    var rs = product_code_list.Select(n => new
                    {
                        product_code = n.ProductCode
                    });
                    // Tạo Object
                    var obj_response = new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = rs.ToList()
                    };

                    // Có data set lên Redis
                    _redisService.Set(cache_key, JsonConvert.SerializeObject(obj_response), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                    return Content(JsonConvert.SerializeObject(obj_response));

                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }


                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = _msg
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/GroupProduct/get-product-code-by-group-id.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[get-product-code-by-group-id.json] = " + ex.ToString()
                });
            }
        }


    }
}