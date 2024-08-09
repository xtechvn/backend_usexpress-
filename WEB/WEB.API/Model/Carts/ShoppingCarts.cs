using Entities.ViewModels.Carts;

using Microsoft.Extensions.Configuration;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;

namespace WEB.API.Model.Carts
{

    public class ShoppingCarts
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<CartItemViewModels> cartCollection;
        public ShoppingCarts(IConfiguration _Configuration)
        {
            configuration = _Configuration;

            //var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            //Gọi phương thức GetDatabase() trên MongoClient và chỉ định tên cơ sở dữ liệu để kết nối(FirstDatabase trong trường hợp này)
            //GetDatabase trả về đối tượng .IMongoDatabase.Tiếp theo CartsViewModels collection được truy suất sử dụng phương thức GetCollection của IMongoDatabase.
            // IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            // this.cartCollection = db.GetCollection<CartItemViewModels>("Carts");


            //-- "mongodb://user1:password1@localhost/test"
            string url = "mongodb://" + _Configuration["MongoDB:user"] + ":" + _Configuration["MongoDB:pwd"] + "@" + _Configuration["MongoDB:Host"] + ":" + _Configuration["MongoDB:Port"] + "/" + _Configuration["MongoDB:catalog_core"];
            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(_Configuration["MongoDB:catalog_core"]);
            cartCollection = db.GetCollection<CartItemViewModels>("Carts");
        }
        public async Task<CartItemViewModels> addNew(CartItemViewModels cart_item)
        {
            try
            {
                // Kiểm tra sp có trong giỏ hàng chưa
                var filter = Builders<CartItemViewModels>.Filter.Where(x => x.cart_id == cart_item.cart_id && x.product_code == cart_item.product_code && x.label_id == cart_item.label_id); //FilterDefinition chỉ định một điều kiện tìm kiếm được sử dụng trong khi cập nhật một document
                var result_document = cartCollection.Find(filter).ToList();

                if (result_document != null && result_document.Count > 0)
                {

                    //FilterDefinition chỉ định một điều kiện tìm kiếm được sử dụng trong khi cập nhật một document                        
                    // var filter_key_cart = Builders<CartItemViewModels>.Filter.Eq("id", result_document[0].id);
                    //// Số lượng
                    int total_quantity = Convert.ToInt32(result_document[0].quantity) + cart_item.quantity;
                    cart_item.quantity = total_quantity; // cap nhat so luong khi user click vao button add gio hang                   
                    cart_item.id = result_document[0].id;
                    cart_item.label_id = cart_item.label_id;
                    var cart_result = await Update(cart_item);
                    return cart_result == string.Empty ? null : cart_item;
                }
                else
                {
                    if (cart_item.product_detail.list_product_fee == null)
                    {
                        Utilities.LogHelper.InsertLogTelegram("shoppingcart-- addToCart - UpdateToCart with id cart: product not found  " + Newtonsoft.Json.JsonConvert.SerializeObject(cart_item) + "-->> Vui lòng kiểm tra trên trang gốc và tạo lại ");
                        return null;
                    }
                    else
                    {
                        double amount_last = cart_item.product_detail.list_product_fee.list_product_fee["PRICE_LAST"] * cart_item.quantity;
                        double amount_last_vnd = amount_last * cart_item.rate_current;
                        cart_item.amount_last = amount_last;
                        cart_item.amount_last_vnd = Math.Round(amount_last_vnd);

                        await cartCollection.InsertOneAsync(cart_item);
                        return cart_item;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- addToCart - UpdateToCart with id cart " + cart_item.id + " update error " + ex.ToString());
                return null;
            }
        }

        public async Task<string> Update(CartItemViewModels cart_item)
        {
            try
            {
                var filter = Builders<CartItemViewModels>.Filter.Eq("id", cart_item.id); //FilterDefinition chỉ định một điều kiện tìm kiếm được sử dụng trong khi cập nhật một document
                var update_def = Builders<CartItemViewModels>.Update.Set("quantity", cart_item.quantity); //UpdateDefinition chỉ định thuộc tính được chỉnh sửa và giá trị mới của chúng.

                if (cart_item.product_detail.list_product_fee != null)
                {
                    // thanh tien
                    double amount_last = cart_item.product_detail.list_product_fee.list_product_fee["PRICE_LAST"] * cart_item.quantity;
                    double amount_last_vnd = Math.Round(amount_last * cart_item.rate_current);

                    update_def = update_def.Set("amount_last", amount_last); // change update amount by quantity
                    update_def = update_def.Set("amount_last_vnd", amount_last_vnd); // change update amount by quantity
                    if (cart_item.cart_id.Length > 5)
                    {
                        update_def = update_def.Set("cart_id", cart_item.cart_id); // mapp account
                    }
                    update_def = update_def.Set("quantity", cart_item.quantity);// change quantity
                    update_def = update_def.Set("update_last", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());

                    var result = await cartCollection.UpdateOneAsync(filter, update_def);
                    if (result.IsAcknowledged)
                    {
                        return cart_item.id;
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("api/carts/Update.json - UpdateToCart with id cart " + cart_item.id + " update error ");
                        return string.Empty;
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("api/carts/Update.json - UpdateToCart with id cart " + cart_item.id + " khong tim thay du lieu cua san pham. San pham bi null ");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- Update with id cart " + cart_item.id + " update error " + ex.ToString());
                return string.Empty;
            }
        }

        public async Task<CartItemViewModels> UpdateByKeyId(CartItemViewModels cart_item)
        {
            try
            {
                if (!(string.IsNullOrEmpty(cart_item.id)) && cart_item.quantity > 0)
                {
                    var filter = Builders<CartItemViewModels>.Filter.Eq("id", cart_item.id); //FilterDefinition chỉ định một điều kiện tìm kiếm được sử dụng trong khi cập nhật một document                    
                    if (filter != null)
                    {
                        // thanh tien
                        var result_document = await cartCollection.FindAsync(filter).Result.ToListAsync();

                        double amount_last = result_document[0].product_detail.list_product_fee.list_product_fee["PRICE_LAST"] * cart_item.quantity;
                        double amount_last_vnd = Math.Round(amount_last * cart_item.rate_current);

                        var update_def = Builders<CartItemViewModels>.Update.Set("quantity", cart_item.quantity); //UpdateDefinition chỉ định thuộc tính được chỉnh sửa và giá trị mới của chúng.
                        update_def = update_def.Set("amount_last", amount_last); // change update amount by quantity
                        update_def = update_def.Set("amount_last_vnd", amount_last_vnd); // change update amount by quantity                                                
                        update_def = update_def.Set("update_last", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());

                        var result = await cartCollection.UpdateOneAsync(filter, update_def);
                        if (result.IsAcknowledged)
                        {
                            result_document[0].quantity = cart_item.quantity; //gan lai quantity 
                            result_document[0].amount_last_vnd = amount_last_vnd; // gan lai tong tien sau khi nhan so luong
                            return result_document[0];
                        }
                        else
                        {
                            Utilities.LogHelper.InsertLogTelegram("UpdateByKeyId with id cart " + cart_item.id + " update error ");
                            return null;
                        }
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("UpdateByKeyId with id cart " + cart_item.id + " khong tim thay du lieu cua cart ");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("UpdateByKeyId-- Update with key_id " + cart_item.id + " update error " + ex.ToString());
                return null;
            }
        }
        public async Task<List<CartItemViewModels>> Find(string cart_id, string key_id, int label_id)
        {
            try
            {
                var builder = Builders<CartItemViewModels>.Filter;
                var filter = builder.Eq("cart_id", cart_id);
                filter = filter & builder.Eq("cart_status", StatusType.BINH_THUONG);

                if (key_id != string.Empty) filter = filter & builder.Eq("id", key_id);
                if (label_id > 0) filter = filter & builder.Eq("label_id", label_id);

                var result_document = await cartCollection.FindAsync(filter).Result.ToListAsync();
                return result_document;
            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- FindCartById with id cart_id " + cart_id + " update error " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// Lấy ra ds sản phẩm được chọn trong giỏ hàng
        /// </summary>
        /// <param name="cart_id"></param>
        /// <param name="label_id"></param>
        /// <returns></returns>
        public async Task<List<CartItemViewModels>> FindCartSelectedByEmail(string cart_id,int label_id)
        {
            try
            {
                var builder = Builders<CartItemViewModels>.Filter;
                var filter = builder.Eq("cart_id", cart_id);
                filter = filter & builder.Eq("cart_status", StatusType.BINH_THUONG);
                filter = filter & builder.Eq("is_selected", true);
                if (label_id > 0) filter = filter & builder.Eq("label_id", label_id);

                var result_document = await cartCollection.FindAsync(filter).Result.ToListAsync();
                return result_document;
            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- FindCartSelectedByEmail with id cart_id " + cart_id + " update error " + ex.ToString());
                return null;
            }
        }
        public async Task<List<CartItemViewModels>> FindByCartList(string[] key_cart_id)
        {
            try
            {
                var builder = Builders<CartItemViewModels>.Filter;                
                var filter = builder.In("id", key_cart_id);  

                var result_document = await cartCollection.FindAsync(filter).Result.ToListAsync();
                return result_document;
            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- FindCartById with id cart_id " + string.Join(",",key_cart_id) + " update error " + ex.ToString());
                return null;
            }
        }
        public async Task<bool> DeleteItemByKeyId(string key_id)
        {
            try
            {
                var result = await cartCollection.DeleteOneAsync<CartItemViewModels>(e => e.id == key_id);
                if (result.IsAcknowledged)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- DeleteItemByKeyId with id key_id " + key_id + " update error " + ex.ToString());
                return false;
            }
        }
        public async Task<bool> MappingUser(string cart_new_id, string cart_old_id)
        {
            try
            {
                //FilterDefinition chỉ định một điều kiện tìm kiếm được sử dụng trong khi cập nhật một document
                var filter = Builders<CartItemViewModels>.Filter.Eq("cart_id", cart_old_id);
                var result_document = await cartCollection.FindAsync(filter).Result.ToListAsync();

                var update_def = Builders<CartItemViewModels>.Update.Set("cart_id", cart_new_id);
                update_def = update_def.Set("update_last", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());


                var result = await cartCollection.UpdateManyAsync(filter, update_def);
                if (result.IsAcknowledged)
                {
                    return true;
                    //return Ok(new { status = ResponseType.SUCCESS.ToString(), token = token, msg = "Mapping Success !!!" });
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("api/carts/Update.json - MapingToCart with id cart " + cart_new_id + " update error cart_old_id = " + cart_old_id);
                    // return Ok(new { status = ResponseType.FAILED.ToString(), token = token, msg = "Mapping FAILED !!!" });
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("MappingUser with id cart_new_id " + cart_new_id + "cart_old_id " + cart_old_id + " MappingUser error " + ex.ToString());
                return false;
            }
        }

        public async Task<bool> saveProductChoice(string lst_key_id, string cart_id)
        {
            try
            {
                // MAPPING lại toàn bộ giỏ hàng với sp được chọn
                var builders = Builders<CartItemViewModels>.Filter;
                var filter = builders.Eq("cart_id", cart_id) & builders.Eq("cart_status", StatusType.BINH_THUONG); // lay ra sp trong gio hang dang active cua 1 user
                var cart_document = await cartCollection.FindAsync(filter).Result.ToListAsync();
                if (cart_document != null)
                {
                    foreach (var item in cart_document)
                    {
                        bool is_selected = lst_key_id.IndexOf(item.id) >= 0 ? true : false;
                        var update_def = Builders<CartItemViewModels>.Update.Set("is_selected", is_selected);
                        update_def = update_def.Set("update_last", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());
                        var cart_detail = builders.Eq("id", item.id);
                        var result = await cartCollection.UpdateManyAsync(cart_detail, update_def);
                        if (!result.IsAcknowledged)
                        {
                            Utilities.LogHelper.InsertLogTelegram("api --> saveProductChoice failed cart_id = " + cart_id + "--> lst_key_id" + lst_key_id);
                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("saveProductChoice with id lst_key_id " + lst_key_id + "cart_id " + cart_id + " saveProductChoice error " + ex.ToString());
                return false;
            }
        }

        public async Task<bool> EmptyCart(string cart_id, int label_id)
        {
            try
            {
                // MAPPING lại toàn bộ giỏ hàng với sp được chọn
                var builders = Builders<CartItemViewModels>.Filter;
                var filter = builders.Eq("cart_id", cart_id) & builders.Eq("cart_status", StatusType.BINH_THUONG) & builders.Eq("label_id", label_id) & builders.Eq("is_selected", true); // lay ra sp trong gio hang dang active cua 1 user
                var cart_document = await cartCollection.FindAsync(filter).Result.ToListAsync();
                if (cart_document != null)
                {
                    foreach (var item in cart_document)
                    {
                        var result = await cartCollection.DeleteOneAsync<CartItemViewModels>(e => e.id == item.id);
                        if (!result.IsAcknowledged)
                        {
                            Utilities.LogHelper.InsertLogTelegram("Delete Cart error with cart_id = " + cart_id + " key_id=" + item.id);
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- EmptyCart with id cart_id " + cart_id + " EmptyCart error " + ex.ToString());
                return false;
            }
        }

        // Lấy ra số tiền mua hộ ko tính luxury của 1 sản phẩm có giá cao nhất trong giỏ hàng        
        // is_free_luxury: là  bao gồm luxury
        // is_max_fee: true: la lấy ra phí mua hộ cao nhất | fasle: là lấy ra giá cao nhất trong giỏ theo nhãn
        public async Task<double> getMaxPriceBuyNoluxuryInCart(int label_id, string cart_id, bool is_free_luxury, bool is_max_fee, bool is_price_first_pound_fee)
        {
            double product_price_max_vnd = 0;
            try
            {
                var cart_list = await Find(cart_id, string.Empty, label_id);

                var max_cart = new CartItemViewModels();
                cart_list = cart_list.Where(x => x.is_selected).ToList();
                if (cart_list.Count() > 0)
                {
                    // Lấy ra phí first pound fee của sản phẩm
                    if (is_price_first_pound_fee)
                    {
                        product_price_max_vnd = cart_list.Max(x => x.product_detail.list_product_fee.list_product_fee["FIRST_POUND_FEE"]);
                    }
                    else
                    {
                        // Lấy ra phí mua hộ cao nhất
                        if (is_max_fee)
                        {
                            // lay ra phí mua ho cao nhat trong gio hang
                            max_cart = cart_list.OrderByDescending(x => x.product_detail.list_product_fee.list_product_fee["TOTAL_SHIPPING_FEE"]).FirstOrDefault();
                        }
                        else
                        {
                            // lay ra gia cao nhat trong gio hang
                            max_cart = cart_list.OrderByDescending(x => x.amount_last_vnd).FirstOrDefault();
                        }

                        if (is_free_luxury)
                        {
                            // Toàn bộ phí mua hộ của sp có giá gốc cao nhất
                            product_price_max_vnd = max_cart.product_detail.list_product_fee.list_product_fee["TOTAL_SHIPPING_FEE"];
                        }
                        else
                        {
                            // Phí mua hộ trừ đi phí luxury
                            product_price_max_vnd = (max_cart.product_detail.list_product_fee.list_product_fee["TOTAL_SHIPPING_FEE"] - (max_cart.product_detail.list_product_fee.list_product_fee["LUXURY_FEE"]));
                        }
                    }
                }

                return product_price_max_vnd;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- getMaxPriceBuyNoluxuryInCart with id cart_id " + cart_id + " EmptyCart error " + ex.ToString());
                return product_price_max_vnd;
            }
        }

        // Lấy ra tổng số tiền mua hộ không tính phí Luxury của giỏ hàng        
        // is_free_luxury: là  bao gồm luxury
        public async Task<double> getTotalFeeNoluxuryInCart(int label_id, string cart_id)
        {
            double total_fee_not_luxury = 0;
            try
            {
                var cart_list = await Find(cart_id, string.Empty, label_id);

                // Phí mua hộ trừ đi phí luxury
                total_fee_not_luxury = cart_list.Where(x => x.is_selected).Sum(x => x.product_detail.list_product_fee.list_product_fee["TOTAL_SHIPPING_FEE"] - x.product_detail.list_product_fee.list_product_fee["LUXURY_FEE"]);

                return total_fee_not_luxury;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("shoppingcart-- getMaxPriceBuyNoluxuryInCart with id cart_id " + cart_id + " EmptyCart error " + ex.ToString());
                return 0;
            }
        }
    }
}
