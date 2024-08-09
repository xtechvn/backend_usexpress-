
jQuery(document).ready(function () {
   
    var url_current = document.URL.toLowerCase();
    
    if (url_current.indexOf('carts/view') >= 0) {        
        $('#cart_summery').append('<a class="btn btn-order btn-confirm-cart">XÁC NHẬN GIỎ HÀNG</a>');
      
    } else {
        $(".voucher_box").remove();
    }    
    if (url_current.indexOf('carts/view') >= 0 || url_current.indexOf('payment/') >= 0) {
        cart_summery.load_cart_info();
    }

})


//$('.btn-confirm-cart').click(function () {
$('#cart_summery').on('click', '.btn-confirm-cart', function () {

    var item_check = $(".chitiet_giohang").find("input[type='checkbox']:checked");
    if (item_check.length == 0) {
        obj_general.alert("Bạn vẫn chưa chọn sản phẩm nào để mua.");
        return;
    } else {
        if (userAuthorized) {
            localStorage.removeItem(ORDER_HISTORY_LAST); // destroy Order History
            cart_summery.submitProductChoice(); // lưu sp dc chọn                        
        } else {
            $(".load-login").click();
        }
    }
});

$('#cart_summery').on('click', '.btn-confirm-payment', function () {
    var order_id = parseInt($("#hd_order_id").val());
    var address_key = parseInt($(this).attr("data-addresskey"));
    var pay_key = parseInt($(this).attr("data-paykey"));
    var bank_code = $(this).attr("data-value");
    var label_id = parseInt($(this).attr("data-labelid"));
    var voucher_sale = cart_summery.getVoucherName();
    checkout.confirm(label_id, address_key, pay_key, bank_code, voucher_sale, is_force_pay, order_id);
});

var cart_summery = {
    add_to_cart: function (product_code, seller_id, label_id, is_fast_buy) {
        if (product_code == undefined) return;
        if (product_code == "") return;
        seller_id = seller_id == null ? "" : seller_id;

        $.ajax({
            url: "/Carts/add-to-cart",
            type: 'POST',
            data: { product_code: product_code, seller_id: seller_id, label_id: label_id },
            dataType: "json",
            //headers: {
            //    RequestVerificationToken:
            //        $('input:hidden[name="__RequestVerificationToken"]').val()
            //},
            success: function (response) {
                // dong popup

                setTimeout(function () {
                    $('.add-product-success').removeClass('open');
                }, 1400);

                $(".add_to_cart").removeClass('disable-click');
                //$(".add_to_cart").removeClass("placeholder");
                if (response.status == SUCCESS) {
                    var total_current_cart = parseInt($(".total_carts").html()) + 1;
                    $(".total_carts").html(total_current_cart);

                    // redirect To Carts
                    if (is_fast_buy) window.location.href = "/Carts/view.html";

                } else if (response.status == EMPTY) {
                    if (response.is_refresh) {
                        alert("Thông tin sản phẩm đã được thay đổi. Xin vui lòng chờ trong giây lát.");
                        location.reload();
                    } else {
                        alert("Sản phẩm đã hết hàng. Xin vui lòng liên hệ với bộ phận CSKH để được hỗ trợ");
                        console.log(response.msg);
                    }

                }
            }
        })
    },
    submitProductChoice: function () {
        var lst_key_id = cart_summery.getCartsChecked();
        var list_voucher = voucher.getVoucherSave();
        var label_id = voucher.getChoiceLabelId();
        $.ajax({
            dataType: 'json',
            type: 'POST',
            data: { lst_key_id: lst_key_id, list_voucher: list_voucher, label_id: label_id },
            url: '/Carts/save-carts-choice.json',
            success: function (data) {
                if (data.status == SUCCESS) {
                    
                    cart_summery.save(data.cart_info);
                   
                    if (data.voucher_change != undefined && data.voucher_change != "") {
                        // save voucher with datetime Expire                          
                        voucher.calulateVoucherSale(data.voucher_change, true);
                    }
                    window.location.href = data.link_next_step;
                } else {
                    //$(".vc_response").remove();
                    //$(".voucher_apply").after('<span class="error vc_response">"' + data.msg + '"</span>');
                    voucher.empty_voucher();
                    obj_general.alert(data.msg)
                }
            },
            failure: function (response) {
                console.log(response);
            }
        });
    },
    getCartsChecked: function () {
        var list_key_cart_id = "";

        var cart_checked = $(".chitiet_giohang").find("input[type='checkbox']:checked");
        var total_cart_checked = cart_checked.length;
        if (total_cart_checked > 0) {
            for (var i = 0; i <= total_cart_checked - 1; i++) {
                var data_key_id = cart_checked[i].getAttribute('data-keyid');
                if (data_key_id != null) {
                    list_key_cart_id += list_key_cart_id == "" ? "" : ",";
                    list_key_cart_id += data_key_id;
                }
            }
        }
        return list_key_cart_id;
    },
    countTotalProd: function () {
        var total_quantity = 0;
        var cart_checked = $(".chitiet_giohang").find("input[type='checkbox']:checked");
        var total_cart_checked = cart_checked.length;
        if (total_cart_checked > 0) {
            for (var i = 0; i <= total_cart_checked - 1; i++) {
                var data_key_id = cart_checked[i].getAttribute('data-keyid');
                if (data_key_id != null) {
                    total_quantity += parseInt($(".quantity_key_" + data_key_id).val());
                }
            }
        }
        return total_quantity;
    },
    getVoucherName: function () {
        var voucher = localStorage.getItem(VOUCHER_KEY);
        var obj_vc = JSON.parse(voucher);
        if (obj_vc != undefined) {
            var list = obj_vc["voucher"];
            return list.map(x => x.vc_name).join(', ');
        }
        return "";
    },
    save: function (cart_info) {
        var data = { cart_info: cart_info}
        localStorage.setItem(CART_INFO_KEY, JSON.stringify(data));
    },
    load_cart_info: function () {
        
        var url_current = document.URL.toLowerCase();
         
        var cart = localStorage.getItem(CART_INFO_KEY);
        //if (cart != null && url_current.indexOf('payment/checkout') >= 0) {
        if (cart != null) {
            var obj_vc = JSON.parse(cart)["cart_info"];
            $(".total_amount_vnd").html(parseFloat(obj_vc["total_price_cart"]).toLocaleString() + " <em>đ</em>");
            $(".total_discount_amount").html(parseFloat(obj_vc["total_price_sale_vc"]).toLocaleString() + " <em>đ</em>");
            $(".total_amount_last_vnd").html(parseFloat(obj_vc["total_amount_last"]).toLocaleString() + " <em>đ</em>");
        } else {

            var total_amount_vnd = $(".total_amount_vnd").data("totalamountcart");
            var total_discount_amount =  $(".total_discount_amount").data("totaldiscountamount");
            var total_amount_last_vnd = $(".total_amount_last_vnd").data("totalamountlast");

            $(".total_amount_vnd").html(total_amount_vnd.toLocaleString() + " <em>đ</em>");
            $(".total_discount_amount").html(total_discount_amount.toLocaleString() + " <em>đ</em>");
            $(".total_amount_last_vnd").html(total_amount_last_vnd.toLocaleString() + " <em>đ</em>");            
        }
    },
    remove_cart_info: function () {
        localStorage.removeItem(CART_INFO_KEY);
    }
}; 