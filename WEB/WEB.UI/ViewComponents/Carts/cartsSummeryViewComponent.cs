using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;
using WEB.UI.Controllers.Carts;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class cartsSummeryViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        public cartsSummeryViewComponent(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var cart = new ShoppingCarts(configuration);
                string cart_id = cart.GetCartId(this.HttpContext);
                //get cartslist
                var cart_model = await cart.getCartListByUser(cart_id, -1, "", -1);

                double total_amount_cart = cart_model.Sum(x => x.total_amount_cart);
                double total_discount_amount = cart_model.Sum(x => x.total_discount_amount);
                double total_amount_last = cart_model.Sum(x => x.total_amount_last);
                var cart_filter_label = cart_model.FirstOrDefault(x => x.total_amount_cart > 0);
                var model = new CartSummeryViewModel
                {
                    total_amount_cart = total_amount_cart,
                    total_discount_amount = total_discount_amount,
                    total_amount_last = total_amount_last,
                    label_id = cart_filter_label == null ? (int)LabelType.amazon : cart_filter_label.label_id,
                };
                return View(model);
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web cartsSummeryViewComponent " + ex.Message);
                return View("Loading...");
            }
        }
    }
}
