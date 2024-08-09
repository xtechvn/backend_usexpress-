
using Microsoft.AspNetCore.Mvc;

namespace WEB.UI.Controllers.HandleErrors
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult httpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 1:
                    ViewBag.ErrorMessage = "Rất tiếc. Hiện tại chúng tôi không tìm thấy sản phẩm này. Xin vui lòng liên hệ với bộ phận CSKH hoặc chat với chúng tôi để được báo giá thủ công";
                    statusCode = 404;
                    break;
                case 2:
                    ViewBag.ErrorMessage = "Rất tiếc. Hiện tại chúng tôi chưa tổng hợp được các sản phẩm thuộc ngành hàng này. Xin vui lòng quay lại sau";
                    statusCode = 404;
                    break;
                case 404:
                    ViewBag.ErrorMessage = "Có vẻ như trang mà quý khách đang tìm kiếm không tồn tại hoặc không tìm thấy nội dung";
                    break;                
                default:
                    ViewBag.ErrorMessage = "Hệ thống gặp sự cố. Xin vui lòng liên hệ bộ phận chăm sóc khách hàng";
                    break;
            }
            ViewBag.statusCode = statusCode;
            return View("NotFound");
        }
    }
}