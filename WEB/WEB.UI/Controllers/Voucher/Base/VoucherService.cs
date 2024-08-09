using Entities.ViewModels.Carts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers.Voucher.Base
{
    public partial class VoucherService
    {

        public string domain_us_api_new { get; set; }
        public string token_tele { get; set; }
        public string group_id_tele { get; set; }

        public string voucher_name { get; set; }
        public int label_id { get; set; }
        public string email { get; set; }
        public string key_api { get; set; }
        public VoucherService(string _domain_us_api_new, string _email, string _voucher, string _token_tele, string _group_id_tele, int _label_id, string _key_api)
        {
            domain_us_api_new = _domain_us_api_new;
            voucher_name = _voucher;
            email = _email;
            token_tele = _token_tele;
            group_id_tele = _group_id_tele;
            label_id = _label_id;
            key_api = _key_api;
        }

        /// <summary>
        /// Lấy ra số tiền được giảm từ Voucher
        /// Giảm giá voucher trên phí firstpound đầu cho sp lớn nhất
        /// </summary>
        /// <returns></returns>
        public async Task<VoucherEntitiesViewModel> getPriceSaleVoucher()
        {
            try
            {
                //string url_api = configuration["url_api_usexpress_new"];
                //url_api += "api/voucher/apply.json";
                string j_param = "{'voucher_name': '" + voucher_name + "', 'email_user_current': '" + email + "', 'label_id': " + label_id + "}";
                string token = CommonHelper.Encode(j_param, key_api);
                var voucher_respon = new VoucherEntitiesViewModel();
                var connect_api_us = new ConnectApi(domain_us_api_new, token_tele, group_id_tele, token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");

                string status = JsonParent[0]["status"].ToString();
                string msg = JsonParent[0]["msg"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {

                    double total_price_sale = Convert.ToDouble(JsonParent[0]["total_price_sale"].ToString());
                    int voucher_id = Convert.ToInt32(JsonParent[0]["voucher_id"].ToString());
                    string expire_date = JsonParent[0]["expire_date"].ToString();
                    string desc = JsonParent[0]["desc"].ToString();
                    string price_sale = JsonParent[0]["price_sale"].ToString();
                    string unit = JsonParent[0]["unit"].ToString();
                    int rule_type = Convert.ToInt32(JsonParent[0]["rule_type"]);
                    voucher_respon = new VoucherEntitiesViewModel
                    {
                        status = ((int)ResponseType.SUCCESS),
                        msg_response = msg,
                        total_price_sale = total_price_sale,
                        discount = unit == UnitType.PHAN_TRAM ? price_sale + "%" : (Convert.ToInt32(price_sale) / 1000) + "k",
                        voucher_id = voucher_id,
                        expire_date = expire_date,
                        desc = desc,
                        unit = unit,
                        rule_type = rule_type,
                        voucher_name = voucher_name
                    };
                }
                else
                {
                    voucher_respon = new VoucherEntitiesViewModel
                    {
                        status = Convert.ToInt32(status),
                        msg_response = msg,
                        total_price_sale = 0,
                        voucher_id = -1,
                        voucher_name = voucher_name,
                        expire_date = string.Empty,
                        desc = string.Empty
                    };
                    Utilities.LogHelper.InsertLogTelegram("[fr-voucherservice] VoucherService -- getPriceSaleVoucher with id voucher_name " + voucher_name + "  error " + JsonConvert.SerializeObject(voucher_respon));
                }
                return voucher_respon;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("VoucherService -- getPriceSaleVoucher with id voucher_name " + voucher_name + "  error " + ex.ToString());
                return null;
            }
        }

        public async Task<List<VoucherEntitiesViewModel>> getVoucherList()
        {
            try
            {
                var list_check = new List<VoucherEntitiesViewModel>();
                if (!string.IsNullOrEmpty(voucher_name))
                {
                    var arr_voucher_choice = voucher_name.Split(",");
                    for (int i = 0; i <= arr_voucher_choice.Length - 1; i++)
                    {
                        string vc_other = arr_voucher_choice[i].Trim();
                        this.voucher_name = vc_other;
                        var vc_data = await this.getPriceSaleVoucher();

                        list_check.Add(vc_data);
                    }
                    return list_check;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("VoucherService -- getTotalPriceSale with id voucher_name " + voucher_name + "  error " + ex.ToString());
                return null;
            }
        }

        public async Task<List<VoucherEntitiesViewModel>> getVoucherListPublic()
        {
            try
            {
                string j_param = "{'key_slave': 'get-list-public'}";
                string token = CommonHelper.Encode(j_param, key_api);
                var voucher_public_respon = new List<VoucherEntitiesViewModel>();
                var connect_api_us = new ConnectApi(domain_us_api_new, token_tele, group_id_tele, token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");

                string status = JsonParent[0]["status"].ToString();


                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string j_vc_list = JsonParent[0]["data"].ToString();
                    voucher_public_respon = JsonConvert.DeserializeObject<List<VoucherEntitiesViewModel>>(j_vc_list);
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    Utilities.LogHelper.InsertLogTelegram("[FR] VoucherService response API -- getVoucherListPublic with error " + msg);
                }

                return voucher_public_respon;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("VoucherService -- getVoucherListPublic with error " + ex.ToString());
                return null;
            }
        }

    }
}
