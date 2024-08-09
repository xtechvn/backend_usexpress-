using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using WEB.UI.Controllers.Carts;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class cartsTotalViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        public cartsTotalViewComponent(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var cart = new ShoppingCarts(configuration);

                //get cartslist
                int total_carts = await cart.getTotalCartsByUser(this.HttpContext);

                ViewBag.total_carts = total_carts;
                return View();
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web cartsViewComponent " + ex.Message);
                ViewBag.total_carts = 0;
                return View();
            }
        }
    }
}
