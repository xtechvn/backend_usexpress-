
jQuery(document).ready(function () {
    costco.render_media_top(); // live stream
    costco.render_product_top_right(394, 0, 8, COSTCO, "Store-Costco-product_top_right");
    costco.render_product_slide(396, 0, 20, COSTCO, "Store-Costco-product_slide");
    costco.render_product_all(-1, 0, 15, COSTCO, "Store-Costco-product_all");
})

$(document.body).on('click', '.btn_view_more_product', function (e) {
    var skip = $(".append-data").find('.product-item').length;
    costco.render_product_all(-1, skip, 20, COSTCO, "Store-Costco-product_detail");
});



$(document.body).on('click', '.btn_fast_buy', function (e) {
    if (userAuthorized) {        
        var product_code = $(this).data("productcode");       
        cart_summery.add_to_cart(product_code, "", COSTCO, true);
    } else {
        $(".load-login").click();
    }
});


var costco = {
    render_media_top: function () {
        //$(".media_top").html('<iframe src="https://www.facebook.com/plugins/video.php?height=314&href=https%3A%2F%2Fwww.facebook.com%2FUS.ExpressVietnam%2Fvideos%2F1032197234264273%2F&show_text=false&width=560&t=0" data-autoplay="true" width="560" height="314" style="border:none;overflow:hidden" scrolling="no" frameborder="0" allowfullscreen="true" allow="autoplay; clipboard-write; encrypted-media; picture-in-picture; web-share" allowFullScreen="true"></iframe>');
       // setTimeout("$('.media_top').removeClass('placeholder');", 900);
    },
    render_product_top_right: function (group_product_id, skip, take, label_id, location_display) {

        $.ajax({
            url: "/group_product/render-product-by-group-id.json",
            type: 'POST',
            data: { group_product_id: group_product_id, skip: skip, take: take, label_id: label_id, partial_view: location_display },
            dataType: "json",
            success: function (response) {
                if (response.status == SUCCESS) {
                    $("." + location_display).html(response.data);
                }
            }
        })
    },
    render_product_slide: function (group_product_id, skip, take, label_id, location_display) {
        $.ajax({
            url: "/group_product/render-product-by-group-id.json",
            type: 'POST',
            data: { group_product_id: group_product_id, skip: skip, take: take, label_id: label_id, partial_view: location_display },
            dataType: "json",
            success: function (response) {
                if (response.status == SUCCESS) {
                    $("." + location_display).html(response.data);
                    //slide product
                    var slide_sale = new Swiper('.slide_sale .swiper-container', {
                        slidesPerView: 5,
                        slidesPerGroup: 5,
                        spaceBetween: 10,
                        simulateTouch: false,
                        navigation: {
                            nextEl: '.slide_sale .swiper-button-next',
                            prevEl: '.slide_sale .swiper-button-prev',
                        },
                        scrollbar: {
                            el: '.swiper-scrollbar',
                            draggable: true,
                        },
                        breakpoints: {
                            1190: {
                                slidesPerView: 4,
                                slidesPerGroup: 4,
                                spaceBetween: 10,
                            },
                            768: {
                                slidesPerView: 3,
                                slidesPerGroup: 3,
                                spaceBetween: 10,
                            },
                            767: {
                                slidesPerView: 2,
                                slidesPerGroup: 2,
                                spaceBetween: 10,
                            },
                            576: {
                                slidesPerView: 1,
                                slidesPerGroup: 1,
                                spaceBetween: 10,
                            }
                        }
                    });
                }
            }
        })
    },
    render_product_all: function (group_product_id, skip, take, label_id, location_display) {
        $.ajax({
            url: "/group_product/render-product-by-group-id.json",
            type: 'POST',
            data: { group_product_id: group_product_id, skip: skip, take: take, label_id: label_id, partial_view: location_display },
            dataType: "json",
            success: function (response) {
                if (response.status == SUCCESS) {

                    if (skip > 0) {

                        $(".append-data").append(response.data); // view more
                        skip = $(".append-data").find('.product-item').length;
                        if (skip >= response.total_item_store) {
                            $(".btn_view_more_product").remove();
                        }
                    } else {
                        $("." + location_display).html(response.data); // onload
                    }
                }
            }
        })
    }
}