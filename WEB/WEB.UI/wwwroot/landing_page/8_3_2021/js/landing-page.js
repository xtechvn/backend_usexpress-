var swiper = new Swiper('.list-product .swiper-container', {
    slidesPerView: 4,
    slidesPerColumn: 2,
    spaceBetween: 20,
    pagination: {
        el: '.swiper-pagination',
        clickable: true,
    },
    navigation: {
        nextEl: '.swiper-button-next',
        prevEl: '.swiper-button-prev',
    },
    breakpoints: {
        640: {
            slidesPerView: 1,
            slidesPerColumn: 1,
            spaceBetween: 20,
        },
        991: {
            slidesPerView: 2,
            slidesPerColumn: 1,
        },
        1200: {
            slidesPerView: 3,
        },
    }
});

$(window).scroll(function () {
    if ($(window).scrollTop() >= 200) {
        $('#to_top').fadeIn();
    } else {
        $('#to_top').fadeOut();
    }
});

$("#to_top").click(function () {
    $("html, body").animate({
        scrollTop: 0
    });
    return false;
});

function getRandomDeal(min, max) {
    return Math.round(Math.random() * (max - min) + min);
}

function shuffle(array) {
    array.sort(() => Math.random() - 0.5);
}

var Data_Group_1000 = [];
var Data_Group_2000 = [];
var Data_Group_3000 = [];

var Data_Group_293 = [];
var Data_Group_294 = [];
var Data_Group_295 = [];

var Data_Group_296 = [];
var Data_Group_298 = [];
var Data_Group_297 = [];

function DataReady() {
    RequestApiData(1000, function (data) {
        RenderViewTopBlocked(data.slice(0, 16));
    });

    var DomFirst = "panel-product-cate-first";
    RequestApiData(2000, function (data) {
        RenderCateBlocked(data.slice(0, 20), 0, DomFirst);
    });

    var DomSecond = "panel-product-cate-second";
    RequestApiData(3000, function (data) {
        RenderCateBlocked(data.slice(0, 20), 0, DomSecond);
    });
}

function ShowBlockView(cateid, callBack) {
    var IsMemory = false;
    switch (cateid) {
        case 293:
            if (Data_Group_293 != null && Data_Group_293.length > 0) {
                IsMemory = true;
                callBack(Data_Group_293);
            }
            break;
        case 294:
            if (Data_Group_294 != null && Data_Group_294.length > 0) {
                IsMemory = true;
                callBack(Data_Group_294);
            }
            break;
        case 295:
            if (Data_Group_295 != null && Data_Group_295.length > 0) {
                IsMemory = true;
                callBack(Data_Group_295);
            }
            break;
        case 296:
            if (Data_Group_296 != null && Data_Group_296.length > 0) {
                IsMemory = true;
                callBack(Data_Group_296);
            }
            break;
        case 297:
            if (Data_Group_297 != null && Data_Group_297.length > 0) {
                IsMemory = true;
                callBack(Data_Group_297);
            }
            break;
        case 298:
            if (Data_Group_298 != null && Data_Group_298.length > 0) {
                IsMemory = true;
                callBack(Data_Group_298);
            }
        case 2000:
            if (Data_Group_2000 != null && Data_Group_2000.length > 0) {
                IsMemory = true;
                callBack(Data_Group_2000);
            }
        case 3000:
            if (Data_Group_3000 != null && Data_Group_3000.length > 0) {
                IsMemory = true;
                callBack(Data_Group_3000);
            }
            break;
    }

    if (!IsMemory) {
        RequestApiData(cateid, function (data) {
            callBack(data);
        });
    }
}

function RequestApiData(cateid, callBack) {
    var API_URL = "http://103.121.91.231:6588/api/LandingPage/campaign/product_list.json";
    var Param = "group_product_" + cateid;
    $.ajax({
        type: "POST",
        url: API_URL,
        data: { "key_cache": Param },
        success: function (data) {
            if (data.msg == "SUCCESS") {
                var anter = JSON.parse(data.product_data);
                switch (cateid) {
                    case 293:
                        Data_Group_293 = anter;
                        break;
                    case 294:
                        Data_Group_294 = anter;
                        break;
                    case 295:
                        Data_Group_295 = anter;
                        break;
                    case 296:
                        Data_Group_296 = anter;
                        break;
                    case 297:
                        Data_Group_297 = anter;
                        break;
                    case 298:
                        Data_Group_298 = anter;
                    case 1000:
                        Data_Group_1000 = shuffle(anter);
                    case 2000:
                        Data_Group_2000 = shuffle(anter);
                    case 3000:
                        Data_Group_3000 = shuffle(anter);
                        break;
                }

                if (callBack != undefined) {
                    callBack(anter);
                }
            }
        },
    });
}

function RequestApiRate(callBack) {
    var API_URL = "http://103.121.91.231:6588/api/servicepublic/rate.json";
    $.ajax({
        type: "GET",
        url: API_URL,
        success: function (data) {
            callBack(data);
        },
    });
}

function RenderViewTopBlocked(dataList) {
    RequestApiRate(function (rate) {
        console.log(rate);
        var StrHtml = "";
        dataList.forEach(itemData => {
            var _percentDeal = getRandomDeal(40, 100);
            StrHtml += '<div class="product-item col-4">'
                + '<div class="wrap">'
                + '<a class="ava" href="' + itemData.link_redirect + '">'
                + '<img src="' + itemData.image + '">'
                + '</a>'
                + '<div class="content">'
                + '<h3 class="title" title="' + itemData.product_name + '">' + itemData.product_name + '</h3>'
                + '<div class="price">'
                + '<strong>' + Math.round(itemData.price_sale * parseFloat(rate)).toLocaleString().replaceAll(',', '.') + ' đ</strong>'
                + '<div class="old">' + Math.round(itemData.price * parseFloat(rate)).toLocaleString().replaceAll(',', '.') + ' đ</div>'
                + '</div>'
                + '<div class="exist">'
                + '<div class="rest" style="width:' + _percentDeal + '%"></div>'
                + '<div class="note">Deal còn lại : <span class="number">' + _percentDeal + '%</span></div>'
                + '</div>'
                + '<div class="center mt20">'
                + '<a href="' + itemData.link_redirect + '" class="btn">Mua ngay</a>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>';
        });
        $('#panel-product-top').append(StrHtml);
    });
}

function RenderCateBlocked(dataList, pageIndex, domId) {
    RequestApiRate(function (rate) {
        var StrHtml = "";
        var pageSize = 10;
        var _from = 0;
        var _to = 20;

        if (pageIndex > 0) {
            _from = 20 + (pageIndex - 1) * pageSize;
            _to = _from + pageSize;
        }

        var Data2 = dataList.slice(_from, _to);
        Data2.forEach(itemData => {
            StrHtml += '<div class="product-item col-5">'
                + '<div class="wrap">'
                + '<div class="sale">-' + Math.round(itemData.discount * 100) + '%</div>'
                + '<a class="ava" href="' + itemData.link_redirect + '">'
                + '<img src="' + itemData.image + '">'
                + '</a>'
                + '<div class="content">'
                + '<h3 class="title"><a href="' + itemData.link_redirect + '" title="' + itemData.product_name + '">' + itemData.product_name + '</a></h3>'
                + '<div class="price flex">'
                + '<div class="price_new">' + Math.round(itemData.price_sale * parseFloat(rate)).toLocaleString().replaceAll(',', '.') + ' đ</div>'
                + '<div class="price_old">' + Math.round(itemData.price * parseFloat(rate)).toLocaleString().replaceAll(',', '.') + ' đ</div>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>';
        });

        $('#' + domId).append(StrHtml);
    });
}

$('.btn-show-more-cate').click(function () {
    var seft = $(this);
    var page = parseInt(seft.data('page'));

    var activeItem = seft.closest('section').find('.tab-category a.active');
    var cateId = activeItem.data('id');
    var panel = activeItem.data('panel');

    var domPanelId = "panel-product-cate-first";
    if (panel != 1) {
        domPanelId = "panel-product-cate-second";
    }

    ShowBlockView(cateId, function (data) {
        RenderCateBlocked(data, page + 1, domPanelId);
    });

    seft.data('page', page + 1);
});

$(".tab-category a").click(function (event) {
    var seft = $(this);
    var cateId = seft.data('id');
    var panel = seft.data('panel');

    seft.closest('.tab-category').find('a').removeClass("active");
    seft.closest('section').find('.btn-show-more-cate').data('page', 0);

    if (!seft.hasClass("active")) {
        seft.addClass("active");
    } else {
        seft.removeClass("active");
    }

    var domPanelId = "panel-product-cate-first";
    if (panel != 1) {
        domPanelId = "panel-product-cate-second";
    }

    ShowBlockView(cateId, function (data) {
        $('#' + domPanelId).empty();
        RenderCateBlocked(data, 0, domPanelId);
    });

});

<!--TuanDo - 4/11 - Facebook Chat-->
    window.fbAsyncInit = function () {
        FB.init({
            appId: '2317932148528143',
            autoLogAppEvents: true,
            xfbml: true,
            version: 'v3.3'
        });
       
        $(window).load(function () {
            if (window.innerWidth > 576) {
                $('#fb_chat').click();
            }
        });
    };

    // Load the SDK asynchronously
    (function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) { return; }
        js = d.createElement(s); js.id = id;
        js.src = "https://connect.facebook.net/en_US/sdk/xfbml.customerchat.js";
        fjs.parentNode.insertBefore(js, fjs);
    }(document, 'script', 'facebook-jssdk'));

    function ShowChatAuto() {
        FB.CustomerChat.showDialog();
        $('.fb-customerchat').show();
    }

    //KhoaNguyen - Đăng nhập bằng facebook - 01/06/2019

    // Facebook login with JavaScript SDK
    function fbLogin() {
        
        FB.login(function (response) {
            if (response.authResponse) {
                // Get and display the user profile data
                getFbUserData();
                $('#popupdky_dnhap').modal('hide');
            } else {
                AddAlert("Thông báo", "Bạn cần đăng nhập tài khoản Facebook để kết nối!", 6000, 1);              
            }
        }, { scope: 'public_profile,email' });
    }

    // Fetch the user profile data from facebook
    function getFbUserData() {
        FB.api('/me', { locale: 'en_US', fields: 'id,name,email,picture' }, function (response) {
            // Chuỗi json chứa thông tin login từ facebook
            var strJsonFbPost = "{"
                + "Email:'" + response.email + "',"
                + "Id:'" + response.id + "',"
                + "Name:'" + response.name + "',"
                + "AvartarUrl:'" + response.picture.data.url + "',"
                + "SourceLogin:'fb'"
                + "}";
            $.post('/Account/LoginGoogleOrFacebook', { jsonPost: strJsonFbPost }, function (result) {
                if (result.bResult) {
                    // Set cookie client use Auto login
                    localStorage.setItem("USEXPRESS_LOGIN_AUTO", response.email);
                    $('#ajax_loading').css('display', 'none');
                    if ($('#ipReturUrl').val() === 'myorder') {
                        $('#ipReturUrl').val('');
                        location.href = '/Account/myorder';
                    } else {
                        ReloadMenu();
                    }
                } else {
                    $('#ajax_loading').css('display', 'none');
                    AddAlert("Thông báo", result.msg, 10000, 0);
                }
            });
        });
}