using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Utilities;
using Utilities.Contants;
using WEB.UI.Controllers.Client;
using WEB.UI.Service;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers.Help
{
    public class FaqController : Controller
    {
        private readonly IConfiguration Configuration;
        public FaqController(IConfiguration _Configuration)
        {
            Configuration = _Configuration;
        }

        [HttpGet]
        [Route("{faq_type}")]
        public async Task<ActionResult> faq(string faq_type)
        {
            try
            {
                // get list faq
                var obj_help = new HelpService(Configuration);
                // validation
                var obj_listing_rs = await obj_help.getListMenuHelp(Convert.ToInt32(Configuration["News:cate_id_help"]));
                if (obj_listing_rs != null)
                {
                    var cate_detail = obj_listing_rs.FirstOrDefault(x => x.path == faq_type);
                    if (cate_detail != null)
                    {
                        var model = new FaqViewModel
                        {
                            list_faq_menu = obj_listing_rs,
                            article_list = await obj_help.getArticleListByCateId(cate_detail.cate_id),
                            path_help_active = cate_detail.path,
                            cate_name_active = cate_detail.name
                        };
                        return View(model);
                    }
                }
                else
                {
                    LogHelper.InsertLogTelegram("[FR] FaqController faq ==> faq_type = " + faq_type + ", Khong tim thay listing menu help faq_type = " + faq_type);
                }
                return Redirect("/Error/404");
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR] FaqController faq==> model = " + faq_type + " " + ex.ToString());
                return Redirect("/Error/404");
            }
        }

        [HttpPost]
        public async Task<ActionResult> search(string faq_search)
        {
            try
            {
                if (!string.IsNullOrEmpty(faq_search))
                {
                    var obj_help = new HelpService(Configuration);
                    // search
                    var result = await obj_help.FindAnserByTitle(faq_search, Convert.ToInt32(Configuration["News:cate_id_help"]));
                    if (result != null)
                    {
                        return Json(new { status = result.Count() > 0? (int)ResponseType.SUCCESS : (int)ResponseType.EMPTY, count = result.Count(), render_search = result.Count() > 0 ? await this.RenderViewToStringAsync("PartialView/Help/contentFaq", result) : string.Empty });
                    }
                    else
                    {
                        return Json(new { status = (int)ResponseType.EMPTY });
                    }
                }
                else
                {
                    return Json(new { status = (int)ResponseType.EMPTY });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR] FaqController search ==> ex = " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR });

            }
        }
    }
}