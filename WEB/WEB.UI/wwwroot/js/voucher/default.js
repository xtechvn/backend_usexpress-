$(document).ready(function () {
    //voucher.empty_voucher();
    cart_summery.load_cart_info();
})

$('.open-voucher').on('click', '.accept_voucher', function () {    
    if (!voucher.validation_cart(false)) return;

    var vc_selected = voucher.get_voucher_choice(false);    
    voucher.calulateVoucherSale(vc_selected, false);
})

$('.open-voucher').on('click', '.check_voucher', function () {
    $(".vc_response").remove();
})


$("#choice-voucher").click(function (e) {

    $(".vc_response").remove();
    if (!voucher.validation_cart(true)) return;

    $.magnificPopup.open({
        items: {
            src: '.open-voucher',
            type: 'inline'
        },
        mainClass: 'mfp-with-zoom'
    });
    $(".mfp-close").remove();

    $.ajax({
        url: '/voucher/get-list.json',
        type: 'POST',
        dataType: "json",
        success: function (response) {

            if (response.status == SUCCESS) {
                $(".voucher-loading").remove();
                var vc_list = response.data;
                var total_prod_choic = cart_summery.countTotalProd();

                var item = "";
                for (var i = 0; i <= vc_list.length - 1; i++) {
                    var is_active = false;
                    if (vc_list[i].rule_type == DISCOUNT_AMZ_TYPE && total_prod_choic > 1) { // 18: amz_discount_20
                        is_active = true;
                    } else if (vc_list[i].rule_type != DISCOUNT_AMZ_TYPE) {
                        is_active = true;
                    }
                    item += voucher.add_voucher(vc_list[i].voucher_name, vc_list[i].discount, vc_list[i].desc, vc_list[i].expire_date, vc_list[i].voucher_id, vc_list[i].rule_type, is_active);
                }
                $(".voucher_public").html(item);

                voucher.selected();
            }
        }
    })
});


$(".btn-apply-voucher").click(function (e) {
    $(".vc_response").remove();
    if (!userAuthorized) {
        $(".load-login").click();
    }
    var voucher_name = $('#txt_voucher').val() == undefined ? "" : $('#txt_voucher').val().toUpperCase().trim();

    if (voucher_name.length == 0) {
        $(".btn-apply-voucher").after('<span class="error vc_response">Bạn chưa nhập mã voucher</span>');
        return;
    }
    if (voucher.check_voucher_exist(voucher_name)) {

        $(".btn-apply-voucher").after('<span class="error vc_response">Mã ' + voucher_name + ' đã tồn tại rồi. Bạn hãy chọn mã bấm OK để được giảm giá nhé.</span>');
        return;
    }
    var label_id = voucher.getChoiceLabelId();
    if (label_id > 0) {
        // Get list cart check
        var lst_key_cart_id = cart_summery.getCartsChecked(); // list cart check
        voucher.apply(voucher_name, label_id, lst_key_cart_id);
        $("#txt_voucher").val("");
    } else {
        $(".btn-apply-voucher").after('<span class="error vc_response">Bạn vẫn chưa chọn sản phẩm nào để mua.</span>');
        return;
    }
});

$('#txt_voucher').keyup(function () {
    $(this).removeClass("error");
    $(".vc_response").remove();
    $(".btn-apply-voucher").removeClass("agray");
});
$('#txt_voucher').blur(function () {
    if ($(this).val().trim() == "") {
        $(".btn-apply-voucher").addClass("agray");
    }
});


var voucher = {
    validation_cart: function (is_show_popup) {
        var lst_key_cart_id = cart_summery.getCartsChecked(); // list cart check
        if (lst_key_cart_id == "") {
            var msg = "Bạn hãy chọn sản phẩm cần mua để được giảm giá nhé.";
            if (is_show_popup) {
                obj_general.alert(msg);
            } else {
                $(".btn-apply-voucher").after('<span class="error vc_response">' + msg + '</span>');
            }
            return false;
        }
        return true;
    },
    apply: function (voucher_search, label_id, lst_key_cart_id) {
        $(".btn-apply-voucher").html('<svg class="icon-us refresh" style="margin-right: 7px;"><use xlink:href="/images/icons/icon.svg#refresh"></use></svg>Áp dụng');
        $.ajax({
            url: '/voucher/apply.json',
            type: 'POST',
            dataType: "json",
            data: { voucher_search: voucher_search, label_id: label_id, lst_key_cart_id: lst_key_cart_id, voucher_choice: "" },
            success: function (response) {
                $(".btn-apply-voucher").html("Áp dụng");
                if (response.status == SUCCESS) {
                    response = response.data;
                    var item = voucher.add_voucher(voucher_search, response.discount, response.desc, response.expire_date, response.voucher_id, 0, true);
                    $(".voucher_public").prepend(item);
                } else {
                    $(".btn-apply-voucher").after('<span class="error vc_response">Rất tiếc! Không thể tìm thấy mã voucher này. Bạn vui lòng kiểm tra lại mã và hạn sử dụng nhé.</span>');
                }
            }
        })
    },
    add_voucher: function (voucher_name, discount, desc, expire_date, voucher_id, rule_type, is_active) {

        var item = "<label data-type=" + rule_type + " data-id=" + voucher_id + " class='confir_res circle " + (is_active ? "" : "expired") + " lbl_rule_type_" + rule_type + "'>";
        item += "<input data-price=" + discount + " data-name=" + voucher_name + " class='check_voucher chk_voucher_"+ voucher_name +" chk_rule_type_" + rule_type + "' type='checkbox'>" + (is_active ? "<span class='checkmark'></span>" : "") + "'<div class='code'><h4>" + voucher_name.toUpperCase() + "</h4><div>Giảm " + discount + "</div></div><div class='content'>";
        item += "<p class='txt_15'><strong>" + desc + "</strong></p>";
        item += rule_type == 18 ? "" : "<p>HSD: " + expire_date + "</p>";
        item += "</div>";
        item += "<a href=''class='if color_green'>Điều kiện<svg class='icon-us'><use xlink:href='images/icons/icon.svg#down'></use></svg></a></label>";
        return item;
    },
    getChoiceLabelId: function () {
        var item_check = $(".chitiet_giohang").find("input[type='checkbox']:checked");
        var label_id_check = item_check.length == 0 ? -1 : item_check[0].getAttribute('data-labelid');
        return label_id_check;
    },
    getTotalVoucher: function () {
        var voucher = localStorage.getItem(VOUCHER_KEY);
        if (voucher != null) {
            var obj_vc = JSON.parse(voucher);
            return voucher == undefined ? 0 : parseInt(obj_vc["total_price_sale_vc"]);
        } else {
            return 0;
        }
    },
    remove: function (obj) {
        var vc_class_type = $(obj).data('type');
        $("." + vc_class_type).remove();
        var total_price_sale_vc = this.getTotalVoucher();
        var total_amount_product_vnd = parseFloat(obj_general.replace_all($(".total_amount_vnd").html(), ",", ""));
        $(".total_discount_amount").html(total_price_sale_vc.toLocaleString() + " <em>đ</em>");
        $(".total_amount_last_vnd").html((total_amount_product_vnd - total_price_sale_vc).toLocaleString() + " <em>đ</em>");
    },
    save: function (total_price_sale_vc, voucher_detail) { // save voucher info with total price sale  
       
        
            var object = { voucher: voucher_detail, total_price_sale_vc: total_price_sale_vc }
            localStorage.setItem(VOUCHER_KEY, JSON.stringify(object));        
    },
    empty_voucher: function () {
        localStorage.removeItem(VOUCHER_KEY);
    },

    get_voucher_choice: function () {
        var vc_checked = $('.voucher_public').find("input[type='checkbox']:checked");
        var total = vc_checked.length;
        var vc_item = "";
        if (total == 0) {
            $(".btn-apply-voucher").after('<span class="error vc_response">Bạn hãy nhập hoặc chọn mã để được giảm giá nhé.</span>');
        } else if (total < 3) {
            for (var i = 0; i <= total - 1; i++) {
                var vc_choice = vc_checked[i].getAttribute('data-name').toUpperCase().trim();
                if (vc_choice != null) {
                    vc_item += vc_item == "" ? "" : ",";
                    vc_item += vc_choice;
                }
            }
            $(".vc_response").remove();
        } else {
            $(".btn-apply-voucher").after('<span class="error vc_response">Rất tiếc. Bạn chỉ được phép chọn tối đa 2 voucher.</span>');
        }
        return vc_item;
    },
    getVoucherSave: function () {

        var voucher = localStorage.getItem(VOUCHER_KEY);
        var obj_vc = JSON.parse(voucher);
        if (obj_vc != undefined) {
            var list = obj_vc["voucher"];
            return list.map(x => x.vc_name).join(', ');
        }
        return "";
    },
    check_voucher_exist: function (voucher_name) {
        var vc_checked = $('.voucher_public').find("input[type='checkbox']");
        for (var i = 0; i <= vc_checked.length - 1; i++) {
            var vc_choice = vc_checked[i].getAttribute('data-name').toUpperCase().trim();
            if (vc_choice != null) {
                if (vc_choice.toUpperCase().trim() == voucher_name) {
                    return true;
                }
            }
        }
    },
    selected: function () {
        var obj_vc = localStorage.getItem(VOUCHER_KEY);
        if (obj_vc != undefined) {
            var list_result = JSON.parse(obj_vc)["voucher"];
            for (var i = 0; i <= list_result.length - 1; i++) {
                var name = list_result[i].vc_name;
                $('.chk_voucher_' + name).prop("checked", 1);
            }
        }
    },
    update_product_by_rule_voucher: function () { 
       // debugger;
        var obj_vc = localStorage.getItem(VOUCHER_KEY);
        if (obj_vc != undefined) {
            // default
            var list_result = JSON.parse(obj_vc)["voucher"];
            var total_price_sale_vc = list_result.filter(function (el) { return 1==1 }).reduce((a, b) => a + (b["price_sale"] || 0), 0);
            
            // filter check rule
            for (var i = 0; i <= list_result.length - 1; i++) {

                var rule_type = list_result[i].rule_type;
                switch (rule_type) {
                    case DISCOUNT_AMZ_TYPE:
                        // check buy more 2
                        var total_prod_choic = cart_summery.countTotalProd();
                        if (total_prod_choic < AMZ_QUANTITY_MAX) {

                            list_result = list_result.filter(function (el) { return el.rule_type != DISCOUNT_AMZ_TYPE; });

                            total_price_sale_vc = list_result.filter(function (el) { return el.rule_type != DISCOUNT_AMZ_TYPE; }).reduce((a, b) => a + (b["price_sale"] || 0), 0);
             
                        }
                        break;
                    default:
                }
            }
            if (parseInt(total_price_sale_vc) > 0) {
                voucher.save(total_price_sale_vc, list_result);
            } else {
                this.empty_voucher();
                cart_summery.remove_cart_info();
                cart_summery.load_cart_info();               
            }
        }
    },
    calulateVoucherSale: function (vc_selected,is_call_on_cart) {  // cart and approve voucher call       
        
        var label_id = voucher.getChoiceLabelId();
      
        var lst_key_cart_id = cart_summery.getCartsChecked(); // list cart check

        if (vc_selected.length > 2) {
            $.ajax({
                url: '/voucher/approve-voucher.json',
                type: 'POST',
                dataType: "json",
                data: { voucher_selected: vc_selected, label_id: label_id, lst_key_cart_id: lst_key_cart_id },
                success: function (response) {                   
                    
                    if (response.status == SUCCESS || is_call_on_cart) {

                        var voucher_detail = response.voucher_detail;
                        var total_price_sale_vc = response.total_price_sale_vc;
                        var total_amount_last = response.total_amount_last;
                        $(".total_amount_vnd").html((total_amount_last + total_price_sale_vc).toLocaleString() + " <em>đ</em>"); //total price discount from voucher
                        $(".total_discount_amount").html(total_price_sale_vc.toLocaleString() + " <em>đ</em>"); //total price discount from voucher
                        $(".total_amount_last_vnd").html(total_amount_last.toLocaleString() + " <em>đ</em>"); //total price last

                        voucher.save(total_price_sale_vc, voucher_detail);
                        $(".btn_destroy").click();
                    } else {
                        $(".btn-apply-voucher").after('<span class="error vc_response">' + response.msg + '</span>');
                    }
                }
            })
        }

    }
}

