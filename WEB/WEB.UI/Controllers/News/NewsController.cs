using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.UI.Controllers.Order;
using WEB.UI.Service;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers.News
{
    // [Route("[controller]")]
    public class NewsController : Controller
    {
        private readonly IConfiguration configuration;
        public readonly IArticleRepository articleRepository;
        private readonly RedisConn redisService;
        public NewsController(IArticleRepository _articleRepository, IConfiguration _Configuration, RedisConn _redisService)
        {
            articleRepository = _articleRepository;
            configuration = _Configuration;
            redisService = _redisService;
        }

        [Route("blog")]
        public async Task<IActionResult> Home()
        {            
            return View();
        }

        [Route("menu-news.json")]
        [HttpPost]
        public async Task<IActionResult> menu()
        {
            try
            {
                var menu_sv = new GroupProductService(configuration, redisService);
                var menu_news = await menu_sv.getMenuNews();
                if (menu_news != null)
                {
                    return Json(new { status = (int)ResponseType.SUCCESS, data = await this.RenderViewToStringAsync("/Views/Shared/PartialView/News/Menu.cshtml", menu_news) });
                }
                else
                {
                    return Json(new { status = (int)ResponseType.FAILED });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[FE] menu-news.json error: " + ex.ToString());
                return Content("");
            }
        }

        [Route("top-news.json")]
        [HttpPost]
        public async Task<IActionResult> topNews(int category_id, int skip, int take, string location)
        {
            try
            {
                var article_sv = new NewsService(configuration, redisService);
                var article = await article_sv.getArticleByCategoryId(category_id, skip, take);
                if (article != null)
                {
                    string view_name = string.Empty;
                    switch (location)
                    {
                        case "top_news":
                            view_name = "/Views/Shared/PartialView/News/topNews.cshtml";
                            break;
                        case "news_category_1":
                        case "news_category_2":
                            view_name = "/Views/Shared/PartialView/News/blogNews.cshtml";
                            break;
                        default:
                            break;
                    }


                    return Json(new { status = (int)ResponseType.SUCCESS, data = await this.RenderViewToStringAsync(view_name, article.news_list), total_item = article.total_news });
                }
                else
                {
                    LogHelper.InsertLogTelegram(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[FE] top-news.json khong tim thay bai nao thuoc category_id = : " + category_id.ToString());
                    return Json(new { status = (int)ResponseType.FAILED });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[FE] top-news.json error: " + ex.ToString());
                return Json(new { status = (int)ResponseType.FAILED });
            }
        }

        [Route("{title}-{article_id}.html")]
        public async Task<IActionResult> detail(string title, long article_id)
        {
            var article_sv = new NewsService(configuration, redisService);
            var article = await article_sv.getArticleDetail(article_id);


            return View("Detail", article);
        }

        [Route("blog/{path}")]
        public async Task<IActionResult> detail(string path)
        {
            ViewBag.category_id =Convert.ToInt32( path.Split("-").Last());
            return View("Home");
        }

        [Route("top-news-pageview.json")]
        [HttpPost]
        public async Task<IActionResult> topNewsPageView()
        {
            try
            {
                var article_sv = new NewsService(configuration, redisService);
                var article = await article_sv.getNewsTopPageView();
                if (article != null)
                {
                    return Json(new { status = (int)ResponseType.SUCCESS, data = await this.RenderViewToStringAsync("/Views/Shared/PartialView/News/topPageViewNews.cshtml", article) });
                }
                else
                {
                    return Json(new { status = (int)ResponseType.FAILED });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[FE] top-news-pageview.json error: " + ex.ToString());
                return Content("");
            }
        }

        [Route("log-pageview.json")]
        [HttpPost]
        public async Task<IActionResult> logPageView(long article_id)
        {
            try
            {
                var article_sv = new NewsService(configuration, redisService);
                var rs = await article_sv.updatePageView(article_id);
                if (rs)
                {
                    return Json(new { status = (int)ResponseType.SUCCESS});
                }
                else
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "error push log" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[FE] top-news-pageview.json error: " + ex.ToString());
                return Json(new { status = (int)ResponseType.FAILED, msg = ex.ToString() });
            }
        }

    }
}
