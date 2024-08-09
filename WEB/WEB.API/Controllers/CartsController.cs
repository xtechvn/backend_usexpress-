using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.ViewModels.Carts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Utilities;
using Utilities.Contants;
using WEB.API.Model.Carts;

namespace WEB.API.Controllers
{
    /// <summary>
    ///Create by: cuonglv
    /// Các api của cart sẽ đc thao tác trong MongoDB để tăng tốc độ ghi và đọc
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : BaseController
    {
        private readonly IConfiguration configuration;
        public CartsController(IConfiguration _Configuration)
        {
            configuration = _Configuration;
        }

        [HttpPost("addnew.json")]
        public async Task<IActionResult> addToCart(string token)
        {
            string displayUrl = UriHelper.GetDisplayUrl(Request);
            try
            {
                //Muốn để lấy tất cả các bản ghi chúng ta sẽ truyền rỗng ở đó
                //var cart_item = new CartItemViewModels()
                //{
                //    cart_id = "cuonglv@gmail.com",
                //    seller_id="FGHJJKFF",
                //    product_code = "GVHHFGFFF",
                //    rate_current = 23560,
                //    price_discount_fee=20,                    
                //    quantity = 1,
                //    amount_last = 22224234,
                //    amount_last_vnd = 511444,
                //    create_date = DateTime.Now,
                //    update_last=DateTime.Now,
                //    cart_status = 0,
                //    label_id=1
                //};
                //string j_param = "{'cart_item':'" + Newtonsoft.Json.JsonConvert.SerializeObject(cart_item) + "'}";
                //token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    var cart_model = Newtonsoft.Json.JsonConvert.DeserializeObject<CartItemViewModels>(objParr[0]["cart_item"].ToString());

                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.addNew(cart_model);
                    if (cart_result != null)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = cart_result });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "add cart error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("addnew.json - addToCart: token valid !!! token =" + token);
                    return Ok(new { status = ResponseType.EXISTS.ToString(), _token = token, msg = "token valid !!!" });
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addnew.json - addToCart " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        [HttpPost("update.json")]
        public async Task<IActionResult> UpdateToCart(string token)
        {
            string displayUrl = UriHelper.GetDisplayUrl(Request);
            try
            {
                //var cart_list = new List<CartItemViewModels>();
                //var rd = new Random();
                //var cart_item1 = new CartItemViewModels()
                //{
                //    id = "5f721bafde58f86d03d220cf",
                //    cart_id = "cuongle@usexpress.vn",
                //    quantity = rd.Next(0, 1000)
                //};
                //cart_list.Add(cart_item1);
                //var cart_item2 = new CartItemViewModels()
                //{
                //    id = "5f721b8ba804488e16bf08c1", //ObjectId("5f721b8ba804488e16bf08c1"), 
                //    cart_id = "cuongle@fpt.vn",
                //    quantity = rd.Next(0, 1000)
                //};
                //cart_list.Add(cart_item2);

                //string j_param = "{'cart_list':'" + Newtonsoft.Json.JsonConvert.SerializeObject(cart_list) + "'}";
                //token = CommonHelper.Encode(j_param, EncryptApi);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    int success = 0;
                    var cart_list_model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItemViewModels>>(objParr[0]["cart_list"].ToString());
                    foreach (var cart_item in cart_list_model)
                    {
                        var shopping_cart = new ShoppingCarts(configuration);
                        var cart_result = await shopping_cart.Update(cart_item);
                        if (cart_result != string.Empty)
                        {
                            return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = cart_result });
                        }
                        else
                        {
                            Utilities.LogHelper.InsertLogTelegram("api/carts/Update.json - UpdateToCart with displayUrl " + displayUrl + " update error ");
                            return Ok(new { status = ResponseType.FAILED.ToString(), msg = "update cart error displayUrl = " + displayUrl });
                        }
                    }
                    return Ok(new { status = success == 0 ? ResponseType.FAILED.ToString() : ResponseType.SUCCESS.ToString(), msg = success + " item cart/" + cart_list_model.Count() + "  update success" });
                }
                else
                {
                    return Ok(new { status = ResponseType.EXISTS.ToString(), token = token });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("api/carts/Update.json - UpdateToCart " + ex.Message + " token=" + token.ToString() + " displayUrl=" + displayUrl);
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        [HttpPost("update-by-key-id.json")]
        public async Task<IActionResult> UpdateCartByKeyId(string token)
        {
            string displayUrl = UriHelper.GetDisplayUrl(Request);
            try
            {
                //var cart_item = new CartItemViewModels()
                //{
                //    id = "5f7c0fd6d13f270e98e19b23",                    
                //    quantity = 3
                //};                

                // string j_param = "{'cart_item':'" + Newtonsoft.Json.JsonConvert.SerializeObject(cart_item) + "'}";
                // token = CommonHelper.Encode(j_param, EncryptApi);


                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    var cart_item_model = Newtonsoft.Json.JsonConvert.DeserializeObject<CartItemViewModels>(objParr[0]["cart_item"].ToString());

                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.UpdateByKeyId(cart_item_model);
                    if (cart_result != null)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = cart_result });
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("api/carts/update-by-key-id.json - UpdateCartByKeyId with displayUrl " + displayUrl + " update error ");
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "update cart error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.EXISTS.ToString(), token = token, msg = "Key valid !!!" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("api/carts/update-by-key-id.json - UpdateCartByKeyId " + ex.Message + " token=" + token.ToString() + " displayUrl=" + displayUrl);
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        [HttpPost("delete.json")]
        public async Task<IActionResult> DeleteCart(string token)
        {
            string displayUrl = UriHelper.GetDisplayUrl(Request);
            try
            {
                //string j_param = "{'id':'5f721b8ba804488e16bf08c1'}";
                //token = CommonHelper.Encode(j_param, EncryptApi);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string key_id = objParr[0]["key_id"].ToString();
                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.DeleteItemByKeyId(key_id);
                    if (cart_result)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "delete success by token =" + token });
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("api/carts/Update.json - UpdateToCart with displayUrl " + displayUrl + " update error ");
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "update cart error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.EXISTS.ToString(), token = token, msg = "Token Failed !!!" });
                }
            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegram("api/carts/DeleteCart.json - DeleteCart " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        [HttpPost("find-by-cart-id.json")]
        public async Task<IActionResult> FindCartById(string token)
        {
            string displayUrl = UriHelper.GetDisplayUrl(Request);
            try
            {
                // string j_param = "{'cart_id':'cuongle@fpt.vn', 'label_id':'1', 'key_id':'5f77be65b63f5a1f87bf8c3d'}";
                //  token = CommonHelper.Encode(j_param, EncryptApi);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string cart_id = objParr[0]["cart_id"].ToString();
                    int label_id = Convert.ToInt32(objParr[0]["label_id"]);
                    string key_id = (objParr[0]["key_id"]).ToString(); // khoa chinh cua item cart             
                    key_id = key_id.Length < 5 ? string.Empty : key_id; // validation key cart

                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.Find(cart_id, key_id, label_id);
                    if (cart_result != null)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), list_cart = cart_result, msg = "Tim thay " + cart_result.Count + " ket qua tra ve" });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "FindCartById error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.ERROR.ToString(), token = token, msg = "token valid !!!!" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("api/carts/find-by-cart-id.json - FindCartById " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString(), token = token });
            }
        }

        /// <summary>
        /// thuc thi mapping shoppingcartid khi user dang nhap thanh cong
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("mapping-to-carts.json")]
        public async Task<IActionResult> MapingToCart(string token)
        {
            try
            {
                string displayUrl = UriHelper.GetDisplayUrl(Request);

                //string j_param = "{'cart_new_id':'cuongle@fpt.vn','cart_old_id':'4ca3b7fc-0b2f-4484-bbc7-91aa941f5302'}";
                //token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string cart_new_id = objParr[0]["cart_new_id"].ToString();
                    string cart_old_id = objParr[0]["cart_old_id"].ToString();
                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.MappingUser(cart_new_id, cart_old_id);
                    if (cart_result)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "Mapping Successfully !!!" });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "add cart error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("api/carts/mapping-to-carts - MapingToCart  Token valid !!! error token = " + token);
                    return Ok(new { status = ResponseType.ERROR.ToString(), token = token, msg = "Token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("api/carts/mapping-to-carts.json - MapingToCart " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString(), token = token });
            }
        }

        /// <summary>
        /// Lưu sp dc chọn
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("save-product-choice.json")]
        public async Task<IActionResult> saveProductChoice(string token)
        {
            try
            {
                string displayUrl = UriHelper.GetDisplayUrl(Request);

                string j_param = "{'lst_key_id':'5f8dbccb210917a4b158840e,5f8dbe3e210917a4b158840f','cart_id':'minhtamluongmsc@gmail.com','cart_id':'minhtamluongmsc@gmail.com'}";
                //token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string lst_key_id = objParr[0]["lst_key_id"].ToString();
                    string cart_id = objParr[0]["cart_id"].ToString();
                    
                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.saveProductChoice(lst_key_id, cart_id);
                    if (cart_result)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "saveProductChoice Successfully !!!" });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "saveProductChoice error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("api/carts/save-product-choice - saveProductChoice  Token valid !!! error token = " + token);
                    return Ok(new { status = ResponseType.ERROR.ToString(), token = token, msg = "Token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("api/carts/save-product-choice.json - saveProductChoice " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString(), token = token });
            }
        }

        [HttpPost("empty-cart.json")]
        public async Task<IActionResult> EmptyCart(string token)
        {
            string displayUrl = UriHelper.GetDisplayUrl(Request);
            try
            {
                //  string j_param = "{'cart_id':'minhtamluongmsc@gmail.com', 'label_id':1}";
                // token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string cart_id = objParr[0]["cart_id"].ToString();
                    int label_id = Convert.ToInt32(objParr[0]["label_id"].ToString());
                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.EmptyCart(cart_id, label_id);
                    if (cart_result)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "delete success by token =" + token });
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("api/carts/empty-cart.json - empty-cart with displayUrl " + displayUrl + " update error ");
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "update cart error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.EXISTS.ToString(), token = token, msg = "Token Failed !!!" });
                }
            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegram("api/carts/empty-cart.json - empty-cart " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        /// <summary>
        /// Lay ra thông tin đơn giá của giỏ theo listing key cart id
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-total-price-cart.json")]
        public async Task<IActionResult> getCartDetail(string token)
        {
            string displayUrl = UriHelper.GetDisplayUrl(Request);
            try
            {
                //string j_param = "{'lst_key_id':'60b5a1dd3405f9a8bab0e93d,60b89503ac1d4006df4a0d53'}";
                //token = CommonHelper.Encode(j_param, "1372498309AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k");

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string key_id = (objParr[0]["lst_key_id"]).ToString(); // khoa chinh cua item cart                              

                    var shopping_cart = new ShoppingCarts(configuration);
                    var cart_result = await shopping_cart.FindByCartList(key_id.Split(","));
                    if (cart_result != null)
                    {
                        var total_price_cart = cart_result.Sum(x => x.amount_last_vnd);

                        return Ok(new { status = (int)ResponseType.SUCCESS, total_price_cart = total_price_cart, msg = "Tổng giá trị đơn hàng được check:" + total_price_cart.ToString("N0") + " đ"});
                    }
                    else
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "getCartDetail error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.ERROR, token = token, msg = "token valid !!!!" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("api/carts/get-price-cart-detail.json  " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, msg = ex.ToString(), token = token });
            }
        }
    }
}