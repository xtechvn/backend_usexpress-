
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using WEB.UI.Controllers.Carts;

namespace WEB.UI.ViewComponents
{
    public class cartsListViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        public cartsListViewComponent(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync(int i_selected)
        {
            try
            {
                var cart = new ShoppingCarts(configuration);
                string cart_id = cart.GetCartId(this.HttpContext);
                //get cartslist
                var cart_model = await cart.getCartListByUser(cart_id, -1, "", i_selected);

                return View(cart_model);
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web cartsListViewComponent " + ex.Message);
                return View(null);
            }
        }
    }
}
