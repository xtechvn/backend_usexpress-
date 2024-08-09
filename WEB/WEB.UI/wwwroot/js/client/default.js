
jQuery(document).ready(function () {
    obj_client.check_authent_login();
})

//Register
function beginClientRegister() {

    $(".btn-create-new-client").addClass('placeholder');
    $(".btn-create-new-client").prop("disabled", true);
}

function completeClientRegister(response) {

    $(".btn-create-new-client").removeClass('placeholder');
    $(".btn-create-new-client").prop("disabled", false);
    var result = response.responseJSON;

    switch (result.status) {
        case SUCCESS:
            obj_client.reset_form();
            //obj_general.show_lightbox_success('Chúc mừng bạn đã đăng ký thành công !');    
            location.reload();
            break;
        case ERROR:
        case FAILED: completeClientRegister
            $('.error-client-summery').html(result.msg);
            break;
    }
}
//End Register

// LOGIN
function beginClientLogin() {
    $(".btn_login_client").addClass('placeholder');
    $(".btn_login_client").prop("disabled", true);
    $(".btn_login_client").html("Đang xử lý...");
}
function completeClientLogin(response) {

    $(".btn_login_client").removeClass('placeholder');
    var result = response.responseJSON;
    if (result != undefined) {
        if (result.status == SUCCESS) {
            //// clear localstorage
            localStorage.removeItem(VOUCHER_KEY);
            localStorage.removeItem(CART_INFO_KEY);
            localStorage.removeItem(ADDRESS_RECEIVER_ID);
            location.reload();
        } else {
            $(".btn_login_client").prop("disabled", false); // open btn login
            $(".error-login-client-summery").html(result.msg);
            $(".btn_login_client").html("Đăng nhập");
        }
    }
}

//fogot pass
function beginClientForgotResponse() {

}
function completeClientForgotResponse(response) {    
    $("#EmailFogot-error").remove();
    if (response.responseJSON.status == SUCCESS) {
        obj_general.send_mail_change_pass_success();
    } else {
        $(".email_forgot_error").append('<span id="EmailFogot-error" class="">' + response.responseJSON.msg + '</span>');
    }
}
// End Login

// End Login

//FORGOT
function beginClientforget() {

}
function completeClientforget(response) {

    $(".frm_client_sercurity").after(response.responseText);
    $(".frm_client_sercurity").css('display', 'none');
}

$(document.body).on('click', '.load-login', function (e) {

    $(".frm_client_sercurity").removeAttr('style');
    $(".frm_client_forgot").css('display', 'none');

    $.ajax({
        url: '/client/login-show',
        type: 'POST',
        dataType: "json",
        //headers: {
        //    RequestVerificationToken:
        //        $('input:hidden[name="__RequestVerificationToken"]').val()
        //},
        success: function (response) {

            //$(".frm_client_sercurity").html(response.view_login_client);
            if (response.status == SUCCESS) {
                $(".user-login").html(response.view_login_client);
                $(".user-register").html(response.view_register_client);

            } else {
                $(".user-login").html(response.msg);
                $(".user-register").html(response.msg);
            }
        }
    })


});

$('#EmailFogot').keyup(function () {
    $(this).removeClass("error");
    $("#EmailFogot-error").remove();
});
// END FORGOT


var obj_client = {
    check_authent_login: function () {
        var is_show_popup_login = $("#is_show_popup_login").val() == "True" ? true : false;
        if (is_show_popup_login) $(".load-login").click();
    },
    reset_form: function () {
        $("#formClientRegister").find('input:text,textarea').val('');
    },
    sendCodeVerify: function (email, btn_name) {
        $(btn_name).prop("disabled", true);
        $(".valid-email").html('');
        $(btn_name).html("<svg class='icon-us refresh'><use xlink:href='images/icons/icon.svg#refresh'></use></svg>Đang gửi...");
        $.ajax({
            url: 'client/otp-codes',
            type: 'POST',
            data: { email: email },
            dataType: "json",
            headers: {
                RequestVerificationToken:
                    $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                $(btn_name).prop("disabled", false);
                if (response.status == SUCCESS) {
                    $(".valid-email").html('<span class="text-danger field-validation-error error" style="color:#236852"  data-valmsg-replace="true"><span id="Email-error" class="">' + response.msg + '</span></span>');
                    timer.clear_interval_verify();

                    // bat countdown                     
                    $(btn_name).html("<svg class='icon-us'><use xlink:href='images/icons/icon.svg#vector'></use></svg>Gửi lại (<span id='count_down_verify' class='seconds'>...</span>s)");

                    var deadline = new Date(response.year, response.month, response.day, response.hours, response.minutes, response.seconds);
                    timer.initializeClock('btn_send_code', deadline);

                }

            }
        })
        //$.ajax({            
        //    dataType: 'json',
        //    type: 'POST',
        //    url: '/client/sendCodeVeirfy',
        //    data: '{email:' + email + '}',
        //    success: function (response) {
        //        $(btn_name).removeClass('placeholder');
        //        var result = response.responseJSON;
        //        if (result.status == SUCCESS) {
        //            $(".error-login-client-summery").html(result.msg);                    
        //        } else {
        //            $(".error-login-client-summery").html(result.msg);                    
        //        }
        //    },
        //    failure: function (response) {
        //        console.log(response);
        //    }
        //});
    }
};

