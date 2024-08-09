using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.API.Service.News;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : BaseController
    {        
        private readonly IArticleRepository articleRepository;
        private readonly IGroupProductRepository groupProductRepository;
        public IConfiguration configuration;
        private readonly RedisConn _redisService;
        private readonly ITagRepository _tagRepository;
        public ArticleController(IConfiguration config, IArticleRepository _articleRepository, RedisConn redisService, IGroupProductRepository _groupProductRepository, ITagRepository tagRepository)
        {
            configuration = config;
            articleRepository = _articleRepository;
            groupProductRepository = _groupProductRepository;
            _redisService = redisService;
            _tagRepository = tagRepository;
        }
               
        /// <summary>
        /// Lấy ra bài viết theo 1 chuyên mục
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-list-by-categoryid.json")]
        public async Task<ActionResult> getListArticleByCategoryId(string token)
        {
            try
            {
                //string j_param = "{'category_id':287}";
                //token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string db_type = string.Empty;
                    int _category_id = Convert.ToInt32(objParr[0]["category_id"]);
                    string cache_name = CacheType.ARTICLE_CATEGORY_ID + _category_id;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var list_article = new List<ArticleViewModel>();

                    if (j_data != null)
                    {
                        list_article = JsonConvert.DeserializeObject<List<ArticleViewModel>>(j_data);
                        db_type = "cache";
                    }
                    else
                    {
                        list_article = await articleRepository.getArticleListByCategoryId(_category_id);
                        if (list_article.Count() > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(list_article), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        }
                        db_type = "database";
                    }

                    //var list_article_lite = from detail in list_article
                    //                        select new ArticleViewModel
                    //                        {
                    //                            article_id = detail.Id,
                    //                            tile = detail.Title,
                    //                            publish_date = (detail.PublishDate ?? DateTime.Now).ToString("dd-MM-yyyy hh:MM"),
                    //                            lead = detail.Lead,
                    //                            content = detail.Body
                    //                        };
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data_list = list_article,
                        category_id = _category_id,
                        msg = "Get " + db_type + " Successfully !!!"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[api/article/detail]: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[api/article/detail] = " + ex.ToString(),
                    _token = token
                });
            }
        }

        [HttpPost("get-detail.json")]
        public async Task<ActionResult> GetArticleDetailLite(string token)
        {
            try
            {
                string j_param = "{'article_id':35}";
                //token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string db_type = string.Empty;
                    long article_id = Convert.ToInt64(objParr[0]["article_id"]);
                    string cache_name = CacheType.ARTICLE_ID + article_id;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var detail = new ArticleModel();

                    if (j_data != null)
                    {
                        detail = JsonConvert.DeserializeObject<ArticleModel>(j_data);
                        db_type = "cache";
                    }
                    else
                    {
                        detail = await articleRepository.GetArticleDetailLite(article_id);
                        detail.Tags = await _tagRepository.GetAllTagByArticleID(article_id);
                        if (detail != null)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(detail), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                            db_type = "database";
                        }

                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = detail,
                        msg = "Get " + db_type + " Successfully !!!",
                        _token = token
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[api/article/detail]: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[api/article/detail] = " + ex.ToString(),
                    _token = token
                });
            }
        }

        [HttpPost("find-article.json")]
        public async Task<ActionResult> FindArticleByTitle(string token)
        {
            try
            {
                string j_param = "{'title':'54544544444','parent_cate_faq_id':279}";
                // token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string db_type = "database";
                    string title = (objParr[0]["title"]).ToString().Trim();
                    int parent_cate_faq_id = Convert.ToInt32(objParr[0]["parent_cate_faq_id"]);

                    var detail = new List<ArticleViewModel>();

                    detail = await articleRepository.FindArticleByTitle(title, parent_cate_faq_id);

                    return Ok(new
                    {
                        status = detail.Count() > 0 ? (int)ResponseType.SUCCESS : (int)ResponseType.EMPTY,
                        data_list = detail.Count() > 0 ? detail : null,
                        msg = "Get " + db_type + " Successfully !!!",
                        _token = token
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("find-article.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "find-article.json = " + ex.ToString(),
                    _token = token
                });
            }
        }

        /// <summary>
        /// Lấy ra bài viết theo 1 chuyên mục, skip+take, sắp xếp theo ngày gần nhất
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-list-by-categoryid-order.json")]
        public async Task<ActionResult> getListArticleByCategoryIdOrderByDate(string token)
        {
            try
            {
                JArray objParr = null;
                string msg = "";
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string db_type = string.Empty;
                    int category_id = Convert.ToInt32(objParr[0]["category_id"]);
                    int skip = Convert.ToInt32(objParr[0]["skip"]);
                    int take = Convert.ToInt32(objParr[0]["take"]);
                    string cache_key = CacheType.CATEGORY_NEWS + category_id;
                    var j_data = await _redisService.GetAsync(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    List<ArticleFeModel> data_list;
                    int total_count = -1;
                    if (j_data == null || j_data == "")
                    {
                        var group_product = await groupProductRepository.GetGroupProductNameAsync(category_id);
                        var data_100 = await articleRepository.getArticleListByCategoryIdOrderByDate(category_id, 0, 100, group_product);
                        if (skip + take > 100)
                        {
                            var data = await articleRepository.getArticleListByCategoryIdOrderByDate(category_id, skip, take, group_product);
                            data_list = data.list_article_fe;
                            total_count = data.total_item_count;
                        }
                        else
                        {
                            data_list = data_100.list_article_fe.Skip(skip).Take(take).ToList();
                            total_count = data_100.total_item_count;

                        }
                        //-- If is home Category, Add Pinned Article:
                        if (category_id == 401) 
                        { 
                        
                        }

                        _redisService.Set(cache_key, JsonConvert.SerializeObject(data_100),DateTime.Now.AddMinutes(15), Convert.ToInt32(configuration["Redis:Database:db_common"]));

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data_list = data_list,
                            total_item = total_count
                        });

                        //return Content(JsonConvert.SerializeObject(data_list));
                    }
                    else
                    {
                        var group_product = await groupProductRepository.GetGroupProductNameAsync(category_id);

                        if (skip + take > 100)
                        {
                            var data = await articleRepository.getArticleListByCategoryIdOrderByDate(category_id, skip, take, group_product);
                            data_list = data.list_article_fe;
                            total_count = data.total_item_count;
                        }
                        else
                        {
                            var data_100 = JsonConvert.DeserializeObject<ArticleFEModelPagnition>(j_data);
                            data_list = data_100.list_article_fe.Skip(skip).Take(take).ToList();
                            total_count = data_100.total_item_count;
                        }

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data_list = data_list,
                            total_item = total_count
                        });
                       // return Content(JsonConvert.SerializeObject(data_list));
                    }

                }
                else
                {
                    msg = "Key ko hop le";
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = msg
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/article/getListArticleByCategoryIdOrderByDate: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution.",
                    _token = token
                });
            }
        }

        [HttpPost("menu.json")]
        public async Task<ActionResult> GetMenu()
        {
            try
            {
                var ParentNewsId = Convert.ToInt32(configuration["News:cate_id_news_parent"]);
                string cache_name = CacheType.MENU_NEWS;
                var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                if (j_data != null)
                {
                    return Ok(new { status = ResponseType.SUCCESS, data_list = j_data });
                }
                else
                {
                    var dataHtml = await groupProductRepository.GetHtmlHorizontalMenu(ParentNewsId);
                    if (!string.IsNullOrEmpty(dataHtml))
                    {
                        _redisService.Set(cache_name, dataHtml, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    }
                    return Ok(new { status = !string.IsNullOrEmpty(dataHtml) ? ResponseType.SUCCESS : ResponseType.ERROR, data_list = dataHtml });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[api==>>] GetMenu==> error:  " + ex.Message);
                return Ok(new { status = ResponseType.EMPTY, msg = ex.ToString() });
            }
        }

        /// <summary>
        /// Lưu pageview cho bài viết
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("post-article-pageview.json")]
        public async Task<ActionResult> PostArticlePageView(string token)
        {
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    long articleID = Convert.ToInt64(objParr[0]["articleID"].ToString());
                    long pageview = Convert.ToInt64(objParr[0]["pageview"].ToString());
                    if (articleID < 0 || pageview < 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Data không hợp lệ"
                        });
                    }
                    else
                    {
                        NewsMongoService services = new NewsMongoService(configuration);
                        var _id = await services.AddNewOrReplace(new NewsViewCount() { pageview = pageview, articleID = articleID });
                        //_redisService.clear(CacheType.MOST_VIEWED_ARTICLE, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));

                        return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data = _id });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NewsController - PostArticlePageView: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution"
                });
            }
        }

        [HttpPost("get-most-viewed-article.json")]
        public async Task<ActionResult> GetMostViewedArticle(string token)
        {
            try
            {
                int status = (int)ResponseType.FAILED;
                string msg = "No Item Found";
                var data_list = new List<ArticleFeModel>();
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string cache_name = CacheType.MOST_VIEWED_ARTICLE;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    var detail = new ArticleFeModel();

                    if (j_data != null)
                    {
                        data_list = JsonConvert.DeserializeObject<List<ArticleFeModel>>(j_data);
                        msg = "Get From Cache Success";

                    }
                    else
                    {
                        NewsMongoService services = new NewsMongoService(configuration);
                        var list = await services.GetMostViewedArticle();
                        if (list != null && list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                var article = await articleRepository.GetArticleDetailLiteFE(item.articleID);
                                if (article != null) data_list.Add(article);
                            }
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(data_list), DateTime.Now.AddMinutes(5), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                            status = (int)ResponseType.SUCCESS;
                            msg = "Get from DB Success";
                        }
                    }
                    return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Success", data = data_list });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NewsController - GetMostViewedArticle: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution"
                });
            }
        }

    }
}