jQuery(document).ready(function () {
    product_top.bind_group_product_best();

    product_top.bind_product_home(".product_top", 22, 0, 15, "productTop", "product_top", 100);

    product_top.bind_product_home(".box_flash_sale", 23, 0, 8, "productTop", "product_flash_sale", 700);
    product_top.bind_product_home(".box_hunter_deal", 24, 0, 10, "groupHunterDeal", "product_hunter_deals", 1000);
    
    product_top.bind_product_costco(394, 0, 8, COSTCO, "Store-Costco-product_liston_home");
})


$(document.body).on('click', '.tab_product_slide_top', function (e) {
    var folder_id = $(this).data("id");
    var box_type = $(this).data("boxtype");
    var slidename = $(this).data("slidename");

    $('.tab_product_slide_top').removeClass('active');
    $(this).addClass('active');
    product_top.render_product_by_id(folder_id, box_type, slidename);
});

$(document.body).on('click', '.tab_product_flash_sale', function (e) {
    var folder_id = $(this).data("id");
    var box_type = $(this).data("boxtype");
    var slidename = $(this).data("slidename");
    $('.tab_product_flash_sale').removeClass('active');
    $(this).addClass('active');
    product_top.render_product_by_id(folder_id, box_type, slidename);
});

var product_top = {
    bind_group_product_best: function () {
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/group-product/choice.json',
            success: function (data) {
                if (data.status == SUCCESS) {
                    $('.best_group_groduct').html(data.gr);
                    var swiper = new Swiper('.xu-huong .swiper-container', {
                        slidesPerView: 6,
                        slidesPerColumn: 2,
                        spaceBetween: 0,
                        navigation: {
                            nextEl: '.swiper-button-next',
                            prevEl: '.swiper-button-prev',
                        },
                        breakpoints: {
                            1190: {
                                slidesPerView: 5,
                            },
                            768: {
                                slidesPerView: 4,
                            },
                            767: {
                                slidesPerView: 3,
                            },
                            576: {
                                slidesPerView: 2,
                            }
                        }
                    });
                } else {
                    console.log(data.msg);
                }
            },
            failure: function (response) {
                console.log(response);
            }
        });
    },
    render_product_by_id: function (folder_id, box_type, slidename) {
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: 'Product/get-product-tab.json',
            data: { folder_id: folder_id, partial_view: box_type },
            success: function (data) {

                if (data.status == SUCCESS) {

                    $("." + box_type).html(data.products);
                    //class chứa từ slide sẽ render lại hàm slide
                    if (slidename.indexOf('slide') >= 0) {
                        var slide_sale_render = new Swiper('.' + slidename + ' .swiper-container', {
                            slidesPerView: 5,
                            slidesPerGroup: 5,
                            spaceBetween: 10,
                            simulateTouch: false,
                            navigation: {
                                nextEl: '.swiper-button-next',
                                prevEl: '.swiper-button-prev',
                            },
                            scrollbar: {
                                el: '.swiper-scrollbar',
                                draggable: true,
                            },
                            breakpoints: {
                                1190: {
                                    slidesPerView: 4,
                                    slidesPerGroup: 4,
                                },
                                768: {
                                    slidesPerView: 3,
                                    slidesPerGroup: 3,
                                },
                                767: {
                                    slidesPerView: 2,
                                    slidesPerGroup: 2,
                                },
                                576: {
                                    slidesPerView: 1,
                                    slidesPerGroup: 1,
                                }
                            }
                        });
                    }
                }
            },
            failure: function (response) {
                console.log(response);
            }
        });
    },
    bind_product_home: function (div_append, campaign_id, skip, take, component_name, box_name, delay) {

        setTimeout(
            function () {
                $.ajax({
                    url: 'Product/get-product-home',
                    type: "post",
                    dataType: "json",
                    beforeSend: function (x) {
                        if (x && x.overrideMimeType) {
                            x.overrideMimeType("application/json;charset=UTF-8");
                        };
                    },
                    data: { _campaign_id: campaign_id, _skip: skip, _take: take, _component_name: component_name, _box_name: box_name },
                    complete: function (result) {
                        $(div_append).html(result.responseText);
                        if (campaign_id == 22) {
                            $(".tab_product_slide_top").eq(0).click();
                        }
                    }
                });
            }, delay);

    },
    bind_product_costco: function (group_product_id, skip, take, label_id, location_display) {
       
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
}
