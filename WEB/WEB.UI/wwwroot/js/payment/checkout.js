var is_force_pay = false;

jQuery(document).ready(function () {  


    checkout.check_back_page(); // check back page from PAyoo
    checkout.bind_info_delivery($("#hd_address_id").val());
    checkout.bind_list_bank_atm();

    $(".up_down_quantity").remove();
    $(".checkmark").remove();
    $(".remove_cart").remove();
    $('.txtQuantity').attr('disabled', true);
    $('.txtQuantity').removeAttr('data-keyid');
    $('.qty_input').removeClass('txtQuantity');

    var url_current = document.URL;
    if (url_current.indexOf('payment/checkout') >= 0) {
        $("#step_choice_address").addClass("succes");
        $("#step_payment").addClass("active");
        $('.btn-confirm-payment').attr("data-labelid", label_id);      
    }

    $("#cktt").click();// on tạm ngày 23-08
})

$(document.body).on('click', '.btn-confirm-us', function (e) {
    is_force_pay = true;
   // $('.btn-confirm-payment').click(); // off tạm ngày 23-08
});

$(".payment_type").click(function (e) {
    $('.btn-confirm-payment').removeAttr("data-value");
    var payment_type = parseInt($(this).data('type'));
  //  $('.btn-confirm-payment').attr("data-paykey", payment_type); // off tạm ngày 23-08
    $('.btn-confirm-payment').attr("data-paykey", 1);// on tạm ngày 23-08

    $('.tc_nhtt').removeClass('tc_nhtt');
    if (payment_type === 1) { // check cktt
        $('.btn-confirm-payment').attr("data-value", "TCB");
        $(".usexpress_banks").addClass("tc_nhtt");
        if (document.URL.indexOf('/payment/re-checkout') >= 0) {
            $(".btn-confirm-payment").html("Thông tin chuyển khoản");                        
        }
    }
});

$(".btn-change-payment").click(function (e) {
    checkout.check_back_page();
});

$(".usexpress_banks").click(function (e) {
    var code = $(this).data('code');
    $('.btn-confirm-payment').attr("data-value", code);
});

$('#tab-atm').on('click', '.bank_item', function () {
    var code = $(this).data('code');
    $('.btn-confirm-payment').attr("data-value", code);
    $(this).parent().find('.tc_nhtt').removeClass('tc_nhtt');
    $(this).addClass("tc_nhtt");
});

$('.btn-confirm-transfer').click(function (e) {
    var order_no = $(this).data("orderno");
    var amount_transfer = $(this).data("amount");
    $.ajax({
        dataType: 'json',
        type: 'POST',
        url: '/Payment/confirm-transfer',
        data: { order_no: order_no, amount_transfer: amount_transfer },
        success: function (data) {
            $('.btn-confirm-transfer').remove();

            if (data.status == SUCCESS) {                
                localStorage.removeItem(ORDER_HISTORY_LAST);
                window.location.href = "/payment/confirm-bank/" + order_no;
            }
            $.magnificPopup.close();

        },
        failure: function (response) {
            console.log(response);
        }
    });
});

var checkout = {
    redirectPageBack: function () {
        location.reload();
    },
    confirm: function (label_id, address_key, pay_key, bank_code, voucher_choice, is_force_pay, order_id) { // is_force_pay:accep payment with no voucher
        
        //validation address
        var j_address = localStorage.getItem(ADDRESS_RECEIVER_ID);
        if (j_address == null) {
            receiver.detail(address_info.address_id, true);         
            return false;
        } else {
            var address_info = JSON.parse(j_address);
            if (address_info.address.length < 15 || address_info.phone < 9) {
                receiver.detail(address_info.address_id, true);     
                return false;
            }
        }

        if (!(Number.isInteger($('.btn-confirm-payment').data('addresskey')))) {
            obj_general.alert("Địa chỉ nhận hàng không hợp lệ. Liên hệ bộ phận CSKH để được hỗ trợ");
            this.return;
        } else if (address_key <= 0) {
            obj_general.alert("Địa chỉ nhận hàng không hợp lệ. Liên hệ bộ phận CSKH để được hỗ trợ");
            this.return;
        }

        if (!(Number.isInteger($('.btn-confirm-payment').data('paykey')))) {
            obj_general.alert("Hình thức thanh toán không hợp lệ. Liên hệ bộ phận CSKH để được hỗ trợ");
            this.return;
        } else if (address_key <= 0) {
            obj_general.alert("Hình thức thanh toán không hợp lệ. Liên hệ bộ phận CSKH để được hỗ trợ");
            this.return;
        }

        // Check rule payment        
        switch (pay_key) {
            case ATM_PAYOO_PAY:
            case USEXPRESS_BANK:
                if (bank_code == undefined) {
                    obj_general.alert("Bạn phải chọn một ngân hàng");
                    return;
                } else if (bank_code.length < 2) {
                    obj_general.alert("Ngân hàng bạn chọn không hợp lệ. Liên hệ bộ phận CSKH để được hỗ trợ");
                    return;
                }
                break;
            case VISA_PAYOO_PAY:
                break;
            default:
                obj_general.alert("Hình thức thanh toán không hợp lệ. Liên hệ bộ phận CSKH để được hỗ trợ");
                console.log(pay_key);
                return;
                break;
        }

        //Detect Affiliate
        var affiliate_val = aff.get_affilliate_type();

        $(".btn-confirm-payment").html('<svg class="icon-us refresh"><use xlink:href="/images/icons/icon.svg#refresh"></use></svg> THANH TOÁN');
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: order_id > 0 ? '/Payment/Update' : '/Payment/Create',
            data: { label_id: label_id, address_id: address_key, pay_type: pay_key, bank_code: bank_code, voucher_choice: voucher_choice, is_force_pay: is_force_pay, order_id: order_id, affiliate: affiliate_val },
            success: function (data) {
                is_force_pay = false; // reset global param

                localStorage.removeItem(ADDRESS_RECEIVER_ID);
                localStorage.removeItem(VOUCHER_KEY);
                localStorage.removeItem(CART_INFO_KEY);

                switch (data.status) {

                    case SUCCESS:
                       
                        // save order to local
                        localStorage.setItem(ORDER_HISTORY_LAST, data.order_id); 

                        if (data.payment_type != USEXPRESS_BANK) {
                            $(".btn-confirm-payment").removeAttr('data-addresskey').removeAttr('data-labelid').removeAttr('data-paykey').removeAttr('data-value').removeClass('btn-confirm-payment');

                            window.location.href = data.url_payoo_redirect;
                        } else {
                            $(".btn-confirm-payment").remove();
                            $(".amount_payment").html(data.amount + " đ");
                            $(".order_no_payment").html(data.order_no.replace("-", "") + " CHUYEN KHOAN MUA HO");
                            $('.btn-confirm-transfer').attr('data-orderno', data.order_no.replace("-","")); 
                            $('.btn-confirm-transfer').attr('data-amount', data.amount);

                            // CKTT show lightbox                            
                            $.magnificPopup.open({
                                items: {
                                    src: '#usexpress-bank-popup',
                                    type: 'inline'
                                },
                                mainClass: 'mfp-with-zoom',
                                closeOnBgClick: false,
                                enableEscapeKey: false
                            });
                            $(".mfp-close").remove();
                        }
                        break;
                    case EMPTY:
                        window.location.href = "/Carts/view.html";
                        break;

                    case CONFIRM:
                        $(".btn-confirm-payment").html('THANH TOÁN');

                        localStorage.removeItem(VOUCHER_KEY);
                        cart_summery.load_cart_info(); // Tính lại giá tiền khi hết hạn

                        $.magnificPopup.open({
                            items: {
                                src: (popup_question.replace("{message_question}", "Rất tiếc! Mã voucher <strong>" + data.voucher_name + "</strong> đã hết hiệu lực sử dụng. Bạn có muốn tiếp tục thanh toán không ?")),
                                type: 'inline'
                            },
                            mainClass: 'mfp-with-zoom'
                        });
                        $(".mfp-close").remove();
                        break;

                    default:
                        $(".btn-confirm-payment").html("THANH TOÁN");
                        console.log(data.msg);
                        break;
                }

            },
            failure: function (response) {
                $(".btn-confirm-payment").html("THANH TOÁN");
                console.log(response);
            }
        });

    },
    bind_list_bank_atm: function () {
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/Payment/banks-list.json',
            success: function (data) {
                if (data.status == SUCCESS) {
                    var bank_list = data.bank_data;
                    var icon_bank = "";
                    for (var i = 0; i <= bank_list.length - 1; i++) {
                        icon_bank += "<li class='bank_item' data-code='" + bank_list[i].code + "'><a><img src='" + bank_list[i].url_icon + "' alt='" + bank_list[i].name + "'></a></li>"
                    }
                    $('.list_bank_atm').html(icon_bank);
                    //$('.btn-confirm-payment').attr("data-paykey", ATM_PAYOO_PAY); // set default
                } else {
                    console.log(data.msg);
                }
            },
            failure: function (response) {
                console.log(response);
            }
        });
    },
    bind_info_delivery: function (address_id) {

        var result = this.parse_address_detail();
        if (!result) {

            //call địa chỉ mặc định
            $.ajax({
                dataType: 'json',
                type: 'POST',
                url: '/client/address-receiver/detail.json',
                data: { address_id: address_id },
                success: function (data) {
                    if (data.status == SUCCESS) {
                        var obj = { address_id: data.result.id, name: data.result.receiverName, phone: data.result.phone, address: data.result.fullAddress };
                        var info_delivery = JSON.stringify(obj);
                        localStorage.setItem(ADDRESS_RECEIVER_ID, info_delivery); //save info_delivery receiver
                        // show view
                        checkout.parse_address_detail();
                        $('.btn-confirm-payment').attr("data-addresskey", data.result.id); // set address choice
                    } else {
                        console.log(data.msg);
                    }
                },
                failure: function (response) {
                    console.log(response);
                }
            });
        }
    },
    parse_address_detail: function () {
        var j_data = localStorage.getItem(ADDRESS_RECEIVER_ID);
        if (j_data != undefined) {
            $('.info_delivery .item').removeClass('placeholder');
            var receiver = JSON.parse(j_data);
            $('.btn_edit_address').attr("data-addressid", receiver.address_id);
            $('.delivery_name').html(receiver.name);
            $('.delivery_phone').html(receiver.phone);
            $('.delivery_address').html(receiver.address);
            $('.btn-confirm-payment').attr("data-addresskey", receiver.address_id); // set address choice
            return true;
        } else {
            return false;
        }
    },
    check_back_page: function () {
       
        var order_id = localStorage.getItem(ORDER_HISTORY_LAST); 
        if (order_id != null) {
            localStorage.removeItem(ORDER_HISTORY_LAST);
            window.location.href = "/payment/re-checkout/" + order_id;
            return false;
        }
    }
}