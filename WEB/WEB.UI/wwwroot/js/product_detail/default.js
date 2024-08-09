
jQuery(document).ready(function () {
    product.get_price(); // crawl Price    
    product.get_product_related(); // show product realated
    product.save_local(); // product view

    product_general.get_product_history(JSON.parse(product_param).product_code); // show product history

    // product.get_seller(); // crawl seller list
   
    switch (parseInt(JSON.parse(product_param).label_id)) {
        case COSTCO:            
            product.costco_role();
            break;
    } 
})

var lastScrollTop = 0;
var is_active_scroll_down = false; // Chưa kéo xuống lần nào
$(window).scroll(function (event) {
    var st = $(this).scrollTop();
    if (st > lastScrollTop && !this.is_active_scroll_down) {
        is_active_scroll_down = true;
        product.render_product_price_related();
    }
    lastScrollTop = st;
});

$('.variation_color a').click(function () {
    product.scroll_to_top();
    var asin_current = $(this).data('asin');
    product.clear_dom();
    $(".ajax_waiting").addClass("placeholder");
    product.active_crawl(asin_current, true);

});

//$('.product-item').on('click', '.img_loading', function () {
function img_waiting(obj) {
    $(obj).addClass("placeholder");
}


$('.add_to_cart').click(function () {
    if (userAuthorized) {
        $(".add_to_cart").addClass('disable-click');
        //$(".add_to_cart").addClass("placeholder");
        $('.add-product-success').addClass('open');
        var product_detail = JSON.parse(product_param);
        var product_code = $(this).data("productcode");
        var seller_id = $(this).data("sellerid");
        var label_id = product_detail.label_id;
        cart_summery.add_to_cart(product_code, seller_id, label_id, false);
    } else {
        $(".load-login").click();
    }
});


$('.btn_fast_buy').click(function () {
    if (userAuthorized) {
        var product_detail = JSON.parse(product_param);
        var product_code = $(this).data("productcode");
        var seller_id = $(this).data("sellerid");
        var label_id = product_detail.label_id;

        cart_summery.add_to_cart(product_code, seller_id, label_id, true);
    } else {
        $(".load-login").click();
    }
});

$('.size a').click(function () {

    $('.size a').removeClass('active').addClass('disable');
    $(this).removeClass('disable').addClass('active');
    //crawl background
    var asin_current = $(this).data('asin');
    // var is_varition_has_image = $(this).data('image') == "True";
    $(".ajax_waiting").addClass("placeholder");
    product.active_crawl(asin_current, true);
});

//$('.add_to_cart').click(function () {
//    $('.add-product-success').addClass('open');
//    setTimeout(function () {
//        $('.add-product-success').removeClass('open');
//    }, 1500);
//});


var list_product_code = "";//phan cach dau phay
var product = {
    costco_role() {
        $(".detail_price, .kt_hang").remove();
        $(".ct_note").html('<svg class="icon-us"><use xlink:href="images/icons/icon.svg#exclamation-mark"></use></svg>Bạn đang chọn mua sản phẩm Costco từ Mỹ. Sản phẩm này dự kiến sẽ được Costco giao đến kho của US Express tại Mỹ trong khoảng <strong>6-10</strong> ngày làm việc. US Express sẽ giao hàng đến tận tay bạn trong vòng từ <strong>10-15</strong> ngày làm việc kể từ khi nhận được hàng tại kho Mỹ.');
        $(".vote").html("Tình trạng: Còn hàng");
    },
    get_product_related() {
        // append view
        var prod_current = JSON.parse(product_param);
        $.ajax({
            url: "/Product/render-product-related.json",
            type: 'POST',
            data: { product_code: prod_current.product_code },
            dataType: "json",
            success: function (response) {
                if (response.status == SUCCESS) {

                    $(".product_related").html(response.data);
                    list_product_code = response.list_product_code;

                } else {
                    $(".product_related").remove();
                }
            }
        })
    },
    render_product_price_related: function () {
        var product_code_item = list_product_code.split(",");
        product_code_item.forEach(function (product_code) {
            var get_price = function () {
                $.ajax({
                    url: "/Product/render-product-price.json",
                    type: 'POST',
                    data: { product_code: product_code, label_id: AMAZON },
                    dataType: "json",
                    success: function (data) {

                        if (data.status == SUCCESS) {
                            var amount_vnd = "<a href='" + data.link_product + "'>Xem báo giá</a>";
                            if (data.amount_vnd_raw > 0) {
                                amount_vnd = data.amount_vnd + " <em>đ</em>";
                                clearInterval(intervalId);
                            }
                            $(".price_product_waiting_" + product_code).html(amount_vnd);
                        }
                    }
                })
            }
            var intervalId = setInterval(get_price, 2000);
        });
    },
    save_local: function () {

        var prod_history = JSON.parse(product_param);

        if (prod_history.amount_vnd.replace(".", "") > 0) {
            var list_result = [];
            var j_list_hist = localStorage.getItem(PRODUCT_HISTORY);
            if (j_list_hist == null) {
                //add first list
                list_result.push(prod_history);
                localStorage.setItem(PRODUCT_HISTORY, JSON.stringify(list_result));
            } else {
                var obj_prod_hist = JSON.parse(j_list_hist);

                //add pro
                // check pro in list
                var list_result = obj_prod_hist.filter(function (el) { return el.product_code == prod_history.product_code; });
                if (list_result.length == 0) {
                    if (obj_prod_hist.length > LIMIT_PRODUCT_HIST) {
                        //remove 1 pro first
                        obj_prod_hist.splice(obj_prod_hist.length - 1, obj_prod_hist.length);
                    }
                    //add first list                    
                    obj_prod_hist.unshift(prod_history);
                    //list_result.push({ element: element });
                    //save
                    localStorage.setItem(PRODUCT_HISTORY, JSON.stringify(obj_prod_hist));
                } else {

                }
            }
        }
    },
    get_seller: function () {
        var product_detail = JSON.parse(product_param);

        if (product_detail.is_has_seller == "True") {

            $.ajax({
                url: "/product/get-seller.json",
                type: 'POST',
                data: { product_code: product_detail.product_code, label_type: AMAZON },
                dataType: "json",
                success: function (response) {
                    if (response.status == SUCCESS) {

                    } else {

                        console.log(response.msg);
                    }
                }
            })
        }
    },
    get_price: function () {
        var product_detail = JSON.parse(product_param);
        if (product_detail.regex_step == 1) {//Active crawl next step when only step 1
            $.ajax({
                url: "/product/get-detail-product-price.json",
                type: 'POST',
                data: { product_code: product_detail.product_code, label_type: AMAZON },
                dataType: "json",
                //headers: {
                //    RequestVerificationToken:
                //        $('input:hidden[name="__RequestVerificationToken"]').val()
                //},
                success: function (response) {

                    if (response.status == SUCCESS && response.amount_last != "0") {
                        if (response.price_old > 0) {                            
                            $(".price-old").html("<span>" + response.price_old + " đ</span> <strong>-" + response.discount + "%</strong>");
                        }
                        $(".price_product").after("<strong>" + response.amount_last + " đ</strong>");
                        $(".price_product").remove();
                        $(".detail_price").removeAttr("style");
                        $("#detail-price-popup").html(response.render_product_list_fee);

                    } else {

                        $(".block-price").html("Giá về tay: <a class='contact btn yellow' href='/'>Liên hệ CSKH</a>");
                        console.log(response.msg);
                    }
                }
            })
        }
    },
    active_crawl: function (asin_current, is_load_page) {
        var product_detail = JSON.parse(product_param);
        var asin_redirect = product_detail.product_code;
        var link_current = product_detail.link_product;

        var url_product_detail = link_current.replace(asin_redirect, asin_current);
        window.history.pushState(asin_redirect, "", url_product_detail);

        $.ajax({
            url: url_product_detail,
            type: 'GET',
            dataType: "json",
            success: function (response) {
                $(".ajax_waiting").removeClass("placeholder");
                if (response.status == SUCCESS) {
                    if (is_load_page) {
                        window.location.href = url_product_detail;
                    }
                } else {
                    console.log(response.msg);
                }
            }
        })
    },
    clear_dom: function () {
        $('.kt_hang').remove();
        $('.info, .ct_danhgia, .ct_star').html('');

    },
    scroll_to_top: function () {
        document.body.scrollTop = 0; // For Safari
        document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
    }
};