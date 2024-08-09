window.padopro = window.padopro || {};
padopro.Viewfull = function () {
    if ($('.view-full').length) {
        $('.view-full').click(function () {
            if (!$(this).hasClass("view-less")) {
                $(this).addClass("view-less");
                $(this).prev('div').addClass("has-toggle");
                $(this).html('<a href="javascript:;">Thu gọn</a>');
            } else {
                $(this).removeClass("view-less");
                $(this).prev('div').removeClass("has-toggle");
                $(this).html('<a href="javascript:;">Xem tất cả</a>');
            }
        })
    }
}
$(document).ready(function () {
    padopro.Viewfull();
    var galleryThumbs2 = new Swiper('.PD_Media .gallery-thumbs', {
        spaceBetween: 8,
        slidesPerView: 5,
        // loop: true,
        freeMode: true,
        loopedSlides: 7,
        watchSlidesVisibility: true,
        watchSlidesProgress: true,
        breakpoints: {
            1025: {
                slidesPerView: 4,
            }
        }
    });
    var galleryTop2 = new Swiper('.PD_Media .gallery-top', {
        // loop: true,
        loopedSlides: 7, //looped slides should be the same
        thumbs: {
            swiper: galleryThumbs2,
        },
        navigation: {
            nextEl: '.swiper-button-next',
            prevEl: '.swiper-button-prev',
        },
    });

    $('#lightgallery').lightGallery({
        selector: '.item',
    });


    // END SLIDE PRODUCT        
    $(".product_detail").find(".txt_down").click(function () {
        if (!$(this).hasClass("active")) {
            $(this).addClass("active");
            $(this).next('div').slideToggle("slow", function () {});
        } else {
            $(this).removeClass("active");
            $(this).next('div').slideToggle("hide", function () {});
        }
    });

    // Show thumb gallery
    var swiper = new Swiper('#slide-evaluate', {
        slidesPerView: 6,
        spaceBetween: 24,
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
            576: {
                slidesPerView: 3,
                spaceBetween: 15,
            }
        }
    });

    // Show comment
    $(".pd-tab-block__comment-panel__view-more-btn").click(function () {
        $('.pd-tab-block__comment-panel__content-field').toggleClass('is-expand');
        $(this).text($(this).text() == '- Hide' ? '+ More' : '- Hide');
    });

    //Video Play
    var videoPlayer = document.getElementById('videoPlayer');
    $('.video').parent().click(function () {
        if ($(this).children(".video").get(0).paused) {
            $(this).children(".video").get(0).play();
            $(this).children(".playpause").fadeOut();
        } else {
            $(this).children(".video").get(0).pause();
            $(this).children(".playpause").fadeIn();
        }
    });

});

$(window).scroll(function () {
    var top_start = $(".product-info2").position().top + 0;
    if ($(window).scrollTop() >= top_start) {
        $('.pin_detail').addClass('active');
    } else if ($(window).scrollTop() <= top_start) {
        $('.pin_detail').removeClass('active');
    }
});