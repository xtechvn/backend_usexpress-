using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Utilities;

namespace WEB.CMS.Controllers
{
    public class ExtensionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetContent(string contentHtml)
        {
            try
            {
                LogHelper.InsertLogTelegram("GetContent. Demo content from Amazon. Nội dung " + contentHtml);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = "",
                    Message = "Thành công"
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContent - GroupProductController: " + ex.Message);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    Message = "Lỗi"
                });
            }
        }
    }
}