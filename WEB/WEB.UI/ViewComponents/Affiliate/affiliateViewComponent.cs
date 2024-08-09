using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

using System.Linq;
using System.Threading.Tasks;
using Utilities;
using WEB.UI.Controllers.Client;
using WEB.UI.Service;

namespace WEB.UI.ViewComponents
{
    public class affiliateViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;

        public readonly IViewRenderService ViewRenderService;
        public affiliateViewComponent(IViewRenderService _ViewRenderService, IConfiguration _Configuration)
        {
            ViewRenderService = _ViewRenderService;

            configuration = _Configuration;
        }

        /// <summary>
        ///Load ds link aff       
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(string sview, string domain)
        {
            long client_id = -1;
            try
            {
                client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                string REFERRAL_ID = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "REFERRAL_ID").Value;
                if (string.IsNullOrEmpty(REFERRAL_ID))
                {
                    var client = new ClientService(configuration);
                    var detail = await client.getClientDetail(client_id);
                    REFERRAL_ID = detail.ReferralId;
                }
                ViewBag.referral_id = REFERRAL_ID;
                ViewBag.domain = domain;

                return View(sview);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FE]--> - linkAffiliateViewComponent client_id = " + client_id + " " + ex);
                return Content("");
            }
        }
    }
}
