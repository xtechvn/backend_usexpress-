using Entities.ViewModels;
using Entities.ViewModels.News;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.Service.News;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class NewsController : Controller
    {
        private const int NEWS_CATEGORY_ID = 91;
        private readonly IArticleRepository _ArticleRepository;
        private readonly IUserRepository _UserRepository;
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly ICommonRepository _CommonRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IConfiguration _configuration;

        public NewsController(IConfiguration configuration, IArticleRepository articleRepository, IUserRepository userRepository, IGroupProductRepository groupProductRepository, ICommonRepository commonRepository, IWebHostEnvironment hostEnvironment)
        {
            _ArticleRepository = articleRepository;
            _CommonRepository = commonRepository;
            _GroupProductRepository = groupProductRepository;
            _UserRepository = userRepository;
            _WebHostEnvironment = hostEnvironment;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ListArticleStatus = await _CommonRepository.GetAllCodeByType(AllCodeType.ARTICLE_STATUS);
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1);
            ViewBag.ListAuthor = await _UserRepository.GetUserSuggestionList(string.Empty);
            return View();
        }

        /// <summary>
        /// Search News
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Search(ArticleSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<ArticleViewModel>();
            try
            {
                model = _ArticleRepository.GetPagingList(searchModel, currentPage, pageSize);
                ViewBag.ListID = (model!=null&&model.ListData!=null&& model.ListData.Select(x => x.Id).ToList() != null && model.ListData.Select(x => x.Id).ToList().Count>0) ? JsonConvert.SerializeObject(model.ListData.Select(x => x.Id).ToList()) : "";
            }
            catch
            {

            }
            return PartialView(model);
        }

        public async Task<IActionResult> Detail(long Id)
        {
            var model = new ArticleModel();
            if (Id > 0)
            {
                model = await _ArticleRepository.GetArticleDetail(Id);
            }
            else
            {
                model.Status = ArticleStatus.SAVE;
            }
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1, model.Categories);
            return View(model);
        }

        public async Task<string> GetSuggestionTag(string name)
        {
            try
            {
                var tagList = await _ArticleRepository.GetSuggestionTag(name);
                return JsonConvert.SerializeObject(tagList);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IActionResult> RelationArticle(long Id)
        {
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1);
            ViewBag.ListAuthor = await _UserRepository.GetUserSuggestionList(string.Empty);
            return PartialView();
        }

        [HttpPost]
        public IActionResult RelationSearch(ArticleSearchModel searchModel, int currentPage = 1, int pageSize = 10)
        {
            var model = new GenericViewModel<ArticleViewModel>();
            try
            {
                model = _ArticleRepository.GetPagingList(searchModel, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpSert([FromBody] object data)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var model = JsonConvert.DeserializeObject<ArticleModel>(data.ToString(), settings);

                #region upload image to local folder
                //string _UploadFolder = @"uploads/images";
                //string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);
                //if (!Directory.Exists(_UploadDirectory))
                //{
                //    Directory.CreateDirectory(_UploadDirectory);
                //}

                //model.Image11 = StringHelpers.SaveBase64StringAsFile(model.Image11, _UploadFolder, _UploadDirectory);
                //model.Image43 = StringHelpers.SaveBase64StringAsFile(model.Image43, _UploadFolder, _UploadDirectory);
                //model.Image169 = StringHelpers.SaveBase64StringAsFile(model.Image169, _UploadFolder, _UploadDirectory);
                #endregion

                if (model != null && HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    model.AuthorId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                model.Body = ArticleHelper.HighLightLinkTag(model.Body);

                var articleId = await _ArticleRepository.SaveArticle(model);

                if (articleId > 0)
                {
                    // clear cache article
                    var strCategories = string.Empty;
                    if (model.Categories != null && model.Categories.Count > 0)
                        strCategories = string.Join(",", model.Categories);

                    await ClearCacheArticle(articleId, strCategories);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        dataId = articleId
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> ChangeArticleStatus(long Id, int articleStatus)
        {
            try
            {
                var _ActionName = string.Empty;

                switch (articleStatus)
                {
                    case ArticleStatus.PUBLISH:
                        _ActionName = "Đăng bài viết";
                        break;

                    case ArticleStatus.REMOVE:
                        _ActionName = "Hạ bài viết";
                        break;
                }

                var rs = await _ArticleRepository.ChangeArticleStatus(Id, articleStatus);

                if (rs > 0)
                {
                    //  clear cache article
                    var Categories = await _ArticleRepository.GetArticleCategoryIdList(Id);
                    await ClearCacheArticle(Id, string.Join(",", Categories));

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = _ActionName + " thành công",
                        dataId = Id
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = _ActionName + " thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> DeleteArticle(long Id)
        {
            try
            {
                var Categories = await _ArticleRepository.GetArticleCategoryIdList(Id);
                var rs = await _ArticleRepository.DeleteArticle(Id);

                if (rs > 0)
                {
                    //  clear cache article
                    await ClearCacheArticle(Id, string.Join(",", Categories));

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa bài viết thành công",
                        dataId = Id
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa bài viết thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task ClearCacheArticle(long articleId, string ArrCategoryId)
        {
            string token = string.Empty;
            try
            {
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_SYNC_ARTICLE;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API;
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string> {
                    { "article_id", articleId.ToString() },
                    { "category_id",ArrCategoryId }
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                await httpClient.PostAsync(apiPrefix, content);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClearCacheArticle - " + ex.Message.ToString() + " Token:" + token);
            }
        }
        [HttpPost]
        public  async Task<List<NewsViewCount>> GetPageViewByList(List<long> article_id)
        {
            try
            {
                NewsMongoService news_services = new NewsMongoService(_configuration);
                return await news_services.GetListViewedArticle(article_id);
            }
            catch
            {

            }
            return null;        
        }
    }
}
