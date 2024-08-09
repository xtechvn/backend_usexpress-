
jQuery(document).ready(function () {

    var url_current = document.URL;

    if (url_current.indexOf('Carts/view') >= 0) {
        $('#cart_summery').append('<a class="btn btn-order btn-confirm-cart">XÁC NHẬN GIỎ HÀNG</a>');
    }
    if (url_current.indexOf('payment/checkout') >= 0 || url_current.indexOf('payment/re-checkout') >= 0) {
        $('#cart_summery').append('<a class="btn btn-order btn-confirm-payment">THANH TOÁN</a>');
    }
    if (url_current.indexOf('so-dia-chi') >= 0) {
        $('.btn_choice_address').remove();
    }
})

$(".btn_edit_address").click(function (e) {
    receiver.detail($(this).data('addressid'),false);
});


$(".btn_changepass").click(function (e) {
    $("#show_change_password").removeClass("hide");
    $(".btn_changepass").remove();

});

$(".delete_address").click(function (e) {
    $(".btn_confirm_delete_address").attr("data-addressid", $(this).data('addressid'));
    $.magnificPopup.open({
        items: {
            src: '#frm_delete_address_popup',
            type: 'inline'
        }
    });
    $(".mfp-close").remove();
});

$(".btn_confirm_delete_address").click(function (e) {
    receiver.delete($(this).data('addressid'));
    localStorage.removeItem(ADDRESS_RECEIVER_ID);
});

$(".btn_choice_address").click(function (e) {
    var address_id = $(this).data('addressid');
    var name = $(this).data('name');
    var phone = $(this).data('phone');
    var address = $(this).data('address');

    var obj = { address_id: address_id, name: name, phone: phone, address: address };
    var info_delivery = JSON.stringify(obj);

    receiver.choice_address(info_delivery);//save
});

function beginReceiverAddress() {

    $(".btn_add_new_address").addClass('placeholder');
    $(".btn_add_new_address").prop("disabled", true);
}

function completeReceiverAddress(response) {

    $(".btn_add_new_address").removeClass('placeholder');
    $(".btn_add_new_address").prop("disabled", false);
    var result = response.responseJSON;

    switch (parseInt(result.status)) {
        case SUCCESS:
            var url_current = document.URL;
            if (url_current.indexOf('payment/checkout') >= 0 || url_current.indexOf('payment/re-checkout') >= 0) {
                // clear local storage.                
                localStorage.removeItem(ADDRESS_RECEIVER_ID);
                // Autoload call from server data new
                checkout.bind_info_delivery(result.id);
                $.magnificPopup.close();
            } else {
                location.reload();
            }
            break;
        case ERROR:
        case FAILED:
            $('.error-client-summery').removeClass("hide");
            $('.error-client-summery').html(result.msg);
            break;
    }
}

var receiver = {
    choice_address: function (info_delivery) {
        localStorage.setItem(ADDRESS_RECEIVER_ID, info_delivery); //save info_delivery receiver
        if (userAuthorized) {
            window.location.href = "/payment/checkout";
        } else {
            $(".load-login").click();
        }
    },
    delete: function (address_id) {
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/client/address/delete/' + address_id + '.json',
            success: function (data) {
                $.magnificPopup.close();
                if (data.status == SUCCESS) {
                    location.reload();
                } else {
                    alert('Địa chỉ này không tồn tại');
                    console.log(data.msg);
                }
            },
            failure: function (response) {
                console.log(response);
            }
        });
    },
    detail: function (address_id, active_validation) {
        var order_id = $(".txt_order_id").val() == undefined ? -1 : parseInt($(".txt_order_id").val()) ;
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/client/address/' + address_id + '/' + order_id + '.html',
            data: { address_id: address_id },
            success: function (data) {
                
                if (data.status == SUCCESS) {
                    $('#address-popup').html(data.render_address_detail);
                    
                    if (document.URL.indexOf('/chi-tiet-don-hang') >= 0) {
                        $(".title_form").html("THÔNG TIN GIAO HÀNG");
                        $(".address_default").css("display", "none");
                    } else {
                        $(".title_form").html("THÔNG TIN THANH TOÁN");
                    }
                    $(".btn_add_new_address").html((address_id > 0 ? "Cập nhật địa chỉ" : "Thêm địa chỉ mới"));
                    $.magnificPopup.open({
                        items: {
                            src: '.address-popup',
                            type: 'inline'
                        }
                    });
                    $(".mfp-close").remove();

                    if (active_validation) {
                        $(".btn_add_new_address").click();
                    }
                    
                } else {
                    alert('Địa chỉ này không tồn tại');
                    console.log(data.msg);
                }
                return;
            },
            failure: function (response) {
                console.log(response);
            }
        });
    },
    reset_form: function () {
        $("#formAddressReceiver").find('input:text,textarea').val('');
    },
    bin_province: function () {
        receiver.get_location(0, "-1", "cbo_province");
    },
    bin_district: function (province_id) {
        this.get_location(1, province_id, "cbo_district");
    },
    bin_ward: function (district_id) {
        this.get_location(2, district_id, "cbo_ward");
    },
    reset_location: function () {
        $('#cbo_province').attr("data-provinceid", "-1");
        $('#cbo_district').attr("data-districtid", "-1");
        $('#cbo_ward').attr("data-wardid", "-1");
    },
    get_location: function (type, id, cbo_name) {
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/client/location.json',
            data: { type: type, id: id },
            success: function (data) {
                var option = "";
                if (data.status == SUCCESS) {
                    var list_data = JSON.parse(data.result);

                    switch (type) {
                        case 0:
                            option += "<option value='-1'>Tỉnh/Thành</option>";
                            for (var i = 0; i <= list_data.length - 1; i++) {
                                option += "<option value=" + list_data[i].provinceId + ">" + list_data[i].type + " " + list_data[i].name + "</option>";
                            }

                            $("#" + cbo_name).html(option);

                            var province_id = $("#cbo_province").attr("data-provinceid");
                            if (province_id != "-1") {
                                receiver.bin_district(province_id);
                            }
                            $("#cbo_province").val(province_id);
                            break;
                        case 1:
                            option += "<option value='-1'>Quận/Huyện</option>";
                            for (var i = 0; i <= list_data.length - 1; i++) {
                                option += "<option value=" + list_data[i].districtId + ">" + list_data[i].type + " " + list_data[i].name + "</option>";
                            }

                            $("#" + cbo_name).html(option);

                            var district_id = $("#cbo_district").attr("data-districtid");
                            if (district_id != "-1") {
                                // Show ward                                                                
                                receiver.bin_ward(district_id);
                            }
                            $("#cbo_district").val(district_id);
                            break;
                        case 2:
                            option += "<option value='-1'>Phường/Xã</option>";

                            for (var i = 0; i <= list_data.length - 1; i++) {
                                option += "<option value=" + list_data[i].wardId + ">" + list_data[i].type + " " + list_data[i].name + "</option>";
                            }
                            $("#" + cbo_name).html(option);

                            var ward_id = $("#cbo_ward").attr("data-wardid");
                            $("#cbo_ward").val(ward_id);
                            break;
                    }

                } else {
                    console.log(data.msg);
                }
            },
            failure: function (response) {
                console.log(response);
            }
        });
    }
};

var getDaysInMonth = function (month, year) {
    // Here January is 1 based
    //Day 0 is the last day in the previous month
    return new Date(year, month, 0).getDate();
    // Here January is 0 based
    // return new Date(year, month+1, 0).getDate();
};
