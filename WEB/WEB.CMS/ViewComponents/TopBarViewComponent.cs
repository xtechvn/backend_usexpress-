using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using WEB.CMS.Models;

namespace WEB.CMS.ViewComponents
{
    public class TopBarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var _UserName = string.Empty;
            var _UserId = string.Empty;
            double _RateCurrent = 0;

            try
            {
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                HttpClient httpClient = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_RATE_CURRENT;
                var result = await httpClient.GetAsync(apiPrefix);
                dynamic resultContent = result.Content.ReadAsStringAsync().Result;

                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                {
                    _UserName = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                    _UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                }

                if (double.TryParse(resultContent, out double rateValue))
                    _RateCurrent = rateValue;
            }
            catch
            {

            }

            ViewBag.UserId = _UserId;
            ViewBag.UserName = _UserName;
            ViewBag.RateCurrent = _RateCurrent;
            return View();
        }
    }
}
