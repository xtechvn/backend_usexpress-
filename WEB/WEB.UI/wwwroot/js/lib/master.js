jQuery(document).ready(function () {

    var url_check = document.URL.toLowerCase();
    /*
    if (url_check.toLowerCase().indexOf("beta.usexpress") >= 0) {
        window.location.href = "https://usexpress.vn/";
    }*/
   
    banner.show_popup_home("01-04-2022", "https://usexpress.vn/", "/images/graphics/Popup_home.jpg","https://usexpress.vn/store/costco");

})
var survey_time = null;

$(".btn_send_feedback").click(function (e) {
    var function_id = $("#function_id").val();
    var feedback = $("#feedback_version_new").val();
    if (feedback == "" || feedback == null || feedback.trim() == "") {
        $('#error_survey_input').html("Vui lòng không để trống phần Nội dung góp ý cải thiện.");
        $('#error_survey_input').css("display", "");
        return;
    }
    survey.add_new(function_id, feedback)
});

$(".btn_copy").click(function (e) {
    var id_value = $(this).data("valueid");

    var value_txt = $("#" + id_value).val();

    if (value_txt.length > 20 && value_txt.indexOf("usexpress.vn") >= 0) {

        /* Get the text field */
        var copyText = document.getElementById(id_value);

        /* Select the text field */
        copyText.select();
        copyText.setSelectionRange(0, 99999); /* For mobile devices */

        /* Copy the text inside the text field */
        navigator.clipboard.writeText(copyText.value);

        /* Alert the copied text */
        $(this).append('<span class="tip top btn_link">Copy link thành công</span>');
        setTimeout("$('.btn_link').remove();", 600);
    } else {
        obj_general.alert("Link không hợp lệ");
    }
});


$(".btn_undo_version").click(function (e) {
    $.magnificPopup.close();
});

$('.cart_view').click(function () {
    if (userAuthorized) {
        window.location.href = "/Carts/view";
    } else {
        $(".load-login").click();
    }
});

$(document.body).on('click', '.btn_destroy', function (e) {
    $.magnificPopup.close();
});

$(document.body).on('click', '.mfp-img', function (e) {
    window.location.href = "/";
});
var banner = {
    get_expired: function (expired_date) {
        //var myDate = "26-02-2012";
        var myDate = expired_date.split("-");
        var newDate = new Date(myDate[2], myDate[1] - 1, myDate[0]);
        return newDate.getTime();
    },
    show_popup_home: function (expired_date, link_display, link_image_banner, link_to_click="#") {
        // Show HOME
        var url_current = document.URL.toLowerCase();
        if (url_current != link_display) {
            return;
        }

        // Check value
        var now = parseInt(new Date().getTime());
        var expired_date = this.get_expired(expired_date);

        if (expired_date < now) { // check da het han chua
            localStorage.removeItem("POPUP_HOME");
            return;
        }

        var popup = JSON.parse(localStorage.getItem("POPUP_HOME"));
        if (popup != null) {

            // check chu kỳ hiển thị
            var expired_display = parseInt(popup.expired_display);
            if (now < expired_display) { // check da het 1 chu ky khong hien thi chua
                return;
            }
        }

        var object = { expired_display: new Date().getTime() + (30 * 60 * 1000) } // 30' /display
        localStorage.setItem("POPUP_HOME", JSON.stringify(object));
        if (link_to_click != "#") {
            var html = '<div style="display:block; text-align:center;" href="javascripts:;" ><img class="mfp-img" src="/images/graphics/Popup_home.jpg" style="max-height: 561px; cursor: pointer;" onclick="window.open(\'' + link_to_click + '\', \'_blank\');"></a>';
        }
        else {
            var html = '<div style="display:block; text-align:center;"><img class="mfp-img" src="/images/graphics/Popup_home.jpg" style="max-height: 561px; cursor: pointer;"></div>';
        }
       //  $(".mfp-img").css("cursor", "pointer");
       // $('.mfp-img').css("cursor", "pointer");
        $.magnificPopup.open({
            items: {
               // src: link_image_banner,
               // type: 'image', // this is default type 
                src: html,
                type: 'inline'
            }
        });
        $('.mfp-content').css("width", "auto");
    }
}

var survey = {
    add_new: function (function_id, feedback) {
        if (survey_time != null) {
            var summit_time = new Date();
            var dif = summit_time.getTime() - survey_time.getTime();
            var Seconds_from_T1_to_T2 = dif / 1000;
            var Seconds_Between_Dates = Math.abs(Seconds_from_T1_to_T2);
            if (Seconds_Between_Dates < 30) {
                $('#error_survey_input').html("Vui lòng chờ " + (30 - parseInt(Seconds_Between_Dates)) + "s trước khi gửi góp ý tiếp theo");
                $('#error_survey_input').css("display", "");
                return;
            }
            else {
                survey_time = summit_time;
                $('#feedback_version_new').val('');
                $('#function_id').val('-1');
            }
        }
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: 'client/survey/add-new',
            data: { function_id: function_id, feedback: feedback },
            success: function (data) {

                if (data.status == SUCCESS) {
                    obj_general.open_lightbox_msg("thanks_feedback");
                    var summit_time = new Date();
                    survey_time = summit_time;
                }
                else {
                    $('#error_survey_input').html(data.msg);
                    $('#error_survey_input').css("display", "");
                    $('#function_id').val('-1');
                }

                $('#feedback_version_new').val('');
                $('#function_id').val('-1');
            },
            failure: function (response) {
                console.log(response);
            }
        });
    }
}

var obj_general = {
    send_mail_change_pass_success: function () {
        var html = '<div class="notifi-popup white-popup" style="max-width: 750px;"><div class="content_poup center"><p class="mb15"><img src="images/icons/tam-khoa.png">';
        html += '</p><h3 class="txt_20 mb15 medium color_ylow">Lấy lại mật khẩu</h3>';
        html += '<p style="color:#585858">Thông tin thay đổi mật khẩu đã được gửi tới Email của bạn. Vui lòng chọn vào đường dẫn và tiến hành thay đổi mật khẩu tại website UsExpress<br></p>';
        html += '<a href="/" class="btn medium flex" style="width:200px">Quay về trang chủ</button></div></div>';
        $.magnificPopup.open({
            items: {
                src: html,
                type: 'inline'
            },
            mainClass: 'mfp-with-zoom'
        });
        $(".mfp-close").remove();
    },
    alert_success_aff: function () {
        var html = '<div class="notifi-popup white-popup"><div class="content_poup center"><h2 class="color_green2 txt_16 medium mb20">Đăng ký affiliate thành công!</h2><p>Bạn đã đăng ký thành công chương trình US Affiliate'
        html += ' .Hãy bắt đầu tạo link giới thiệu và chia sẻ cùng bạn bè để hưởng hoa hồng.</p><button class="btn txt_14 btn_create_aff">Tạo link giới thiệu</button><p class="mt15"style="color:#6F6F6F">';
        html += 'Bạn cần tìm hiểu thêm ? <a class="color_green2 underline" href="/chinh-sach-us-express-affiliate-363">Giới thiệu chương trình</a></p ></div ></div > ';
        $.magnificPopup.open({
            items: {
                src: html,
                type: 'inline'
            },
            mainClass: 'mfp-with-zoom'
        });
        $(".mfp-close").remove();
    },
    alert: function (msg_content) {
        $.magnificPopup.open({
            items: {
                src: (popup_ok.replace("{message_question}", msg_content)),
                type: 'inline'
            },
            mainClass: 'mfp-with-zoom'
        });
        $(".mfp-close").remove();
    },
    show_lightbox_success: function (msg) {
        var html = '<div id="notifi-popup" class="notifi-popup white-popup">';
        html += '<div class="content_poup center">';
        html += '<p class="mb15"><img src="images/graphics/verifiler.png"></p>';
        html += '<p>' + msg + '</p>';
        html += '</div>';
        html += '<button title="Close (Esc)" type="button" class="mfp-close">×</button>';
        html += '</div>';
        $('#pop-up-location').html(html);
        obj_general.open_lightbox_msg('notifi-popup');
    },
    open_lightbox_msg: function (id_popup) {
        $.magnificPopup.open({
            items: {
                src: '#' + id_popup
            },
            type: 'inline'
        });
    },
    close_lightbox: function () {
        $.magnificPopup.close();
    },
    replace_all: function (iStr, v1, v2) {
        if (iStr != undefined) {
            var i = 0, oStr = '', j = v1.length;
            while (i < iStr.length) {
                if (iStr.substr(i, j) == v1) {
                    oStr += v2;
                    i += j
                }
                else {
                    oStr += iStr.charAt(i);
                    i++;
                }
            }
            return oStr;
        }
    },
    update_aff: function () {
        var url_string = window.location.href;
        var url = new URL(url_string);
        var utm_source = url.searchParams.get("utm_source");
        if (utm_source == null || utm_source.trim() == '') {
            var trackingID = this.readCookie('_aff_sid');
            var network = this.readCookie("_aff_network");
            if (network != null)
            {
                switch (network) {
                    case "accesstrade": {
                        window.localStorage.setItem("aff", trackingID + ",454," + network + "," + network);
                    } break;
                    case "adpia": {
                        window.localStorage.setItem("aff", trackingID + ",0," + network + "," + network);
                    } break;
                    case "usexpress": {
                        window.localStorage.setItem("aff", trackingID + ",0," + network + "," + network);
                    } break;
                    default: break;
                }
            }
            else {
                window.localStorage.removeItem("aff");
            }
        }
        else {
            switch (utm_source) {
                case "accesstrade": {
                    var trackingID = this.readCookie('_aff_sid');
                    window.localStorage.setItem("aff", trackingID + ",454," + utm_source + "," + utm_source);
                } break;
                case "adpia": {
                    this.setCookie('_aff_sid', url.searchParams.get("utm_medium"), 30);
                    this.setCookie('_aff_network', utm_source, 30);
                    var trackingID = this.readCookie('_aff_sid');
                    window.localStorage.setItem("aff", trackingID + ",0," + utm_source + "," + utm_source);
                } break;
                case "usexpress": {
                    this.setCookie('_aff_sid', url.searchParams.get("utm_medium"), 30);
                    this.setCookie('_aff_network', utm_source, 30);
                    var trackingID = this.readCookie('_aff_sid');
                    window.localStorage.setItem("aff", trackingID + ",0," + utm_source + "," + utm_source);
                } break;
                default: break;
            }
        }
    },
    readCookie: function (name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },
    setCookie: function (name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + (value || "") + expires + "; path=/";
    }
}

var product_general = {

    get_product_history: function (product_code_current) {

        var j_list_hist = localStorage.getItem(PRODUCT_HISTORY);
        if (j_list_hist != null) {

            var prod_history = JSON.parse(j_list_hist);

            var list_result = product_code_current == null ? prod_history : prod_history.filter(function (el) { return el.product_code != product_code_current; }); // remove product current
            if (list_result.length >= LIMIT_PRODUCT_HIST) {
                // append view

                var list_result_limit = list_result.slice(0, LIMIT_PRODUCT_HIST)
                $.ajax({
                    url: "/Product/render-product-history.json",
                    type: 'POST',
                    data: { j_data: JSON.stringify(list_result_limit) },
                    dataType: "json",
                    success: function (response) {
                        if (response.status == SUCCESS) {
                            $(".product-history").html(response.data);
                        } else {
                            console.log(response.msg);
                        }
                    }
                })
            } else {
                $(".product-history").remove();
            }
        } else {
            $(".product-history").remove();
        }
    }
}

///////////// TIMER_COUND_COUND//////////////////
var timeinterval;
var timer = {
    getDateTimeRemaining: function (endtime) {
        var t = Date.parse(endtime) - Date.parse(new Date());
        var seconds = Math.floor((t / 1000) % 60);
        var minutes = Math.floor((t / 1000 / 60) % 60);
        var hours = Math.floor((t / (1000 * 60 * 60)) % 24);
        var days = Math.floor(t / (1000 * 60 * 60 * 24));
        return {
            'total': t,
            'days': days,
            'hours': hours,
            'minutes': minutes,
            'seconds': seconds
        };
    },
    getTimeRemaining: function (endtime) {
        var t = Date.parse(endtime) - Date.parse(new Date());
        var seconds = (Math.floor((t / 1000) % 60));

        return {
            'total': t,
            'seconds': seconds
        }
    },
    initializeClockDate: function (id, endtime) {

        var clock = document.getElementById(id);
        // var daysSpan = clock.querySelector('.days');
        var hoursSpan = clock.querySelector('.hours');
        var minutesSpan = clock.querySelector('.minutes');
        var secondsSpan = clock.querySelector('.seconds');

        function updateClock() {
            var t = timer.getDateTimeRemaining(endtime);

            // daysSpan.innerHTML = t.days;
            hoursSpan.innerHTML = ('0' + t.hours).slice(-2);
            minutesSpan.innerHTML = ('0' + t.minutes).slice(-2);
            secondsSpan.innerHTML = ('0' + t.seconds).slice(-2);

            if (t.total <= 0) {
                clearInterval(timeinterval);
            }
        }
        updateClock();
        var timeinterval = setInterval(updateClock, 1000);
    },
    initializeClock: function (id, endtime) {

        var clock = document.getElementById(id);
        var secondsSpan = clock.querySelector('.seconds');

        function updateClock() {
            var t = timer.getTimeRemaining(endtime);
            secondsSpan.innerHTML = ('0' + t.seconds).slice(-2);

            if (t.seconds <= 0) {
                clearInterval(timeinterval);
                $("#" + id).html("<svg class='icon-us'><use xlink:href='images/icons/icon.svg#vector'></use></svg>Gửi mã xác thực");
            }
        }
        updateClock();
        timeinterval = setInterval(updateClock, 1000);
    },
    clear_interval_verify: function () {
        clearInterval(timeinterval);
    }

}
