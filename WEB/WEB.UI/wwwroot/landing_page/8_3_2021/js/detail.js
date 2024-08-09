window.padopro = window.padopro || {};
padopro.Viewfull = function() {
  if($('.view-full').length) {
    $('.view-full').click(function(){
      if(!$(this).hasClass("view-less")){
          $(this).addClass("view-less");
          $(this).prev('div').addClass("has-toggle");
          $(this).html('<a href="javascript:;">Thu gọn</a>');
      }
      else{
          $(this).removeClass("view-less");
          $(this).prev('div').removeClass("has-toggle");
          $(this).html('<a href="javascript:;">Xem tất cả</a>');
      }      
    })
  }
}
$(document).ready(function (){
    padopro.Viewfull(); 
    // var galleryTop = new Swiper('.gallery-top', {
    //   navigation: {
    //     nextEl: '.swiper-button-next',
    //     prevEl: '.swiper-button-prev',
    //   },
    //         loop: true,
    //         loopedSlides: 7
    // });
    // var galleryThumbs = new Swiper('.gallery-thumbs', {
    //   // centeredSlides: false,
    //   slidesPerView: 'auto',
    //   touchRatio: 0.2,
    //   slideToClickedSlide: true,
    //         loop: true,
    //         loopedSlides: 7
    // });
    // galleryTop.controller.control = galleryThumbs;
    // galleryThumbs.controller.control = galleryTop;    


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
    
    $('.PD_Media .gallery-thumbs .swiper-slide').click(function () {
        $('.gallery-top .swiper-slide img').removeClass('zoom_01');
        if ($('.gallery-top .swiper-slide').hasClass('swiper-slide-active')) {
            $('.gallery-top .swiper-slide-active img').addClass('zoom_01');
            $('.zoom_01').ezPlus();
        } else {
            $('.gallery-top .swiper-slide img').removeClass('zoom_01');
        }
    });
    $('.PD_Media .gallery-thumbs .swiper-button-prev').click(function () {
        $('.gallery-top .swiper-slide img').removeClass('zoom_01');
        if ($('.gallery-top .swiper-slide').hasClass('swiper-slide-active')) {
            $('.gallery-top .swiper-slide-active img').addClass('zoom_01');
            $('.zoom_01').ezPlus();
        } else {
            $('.gallery-top .swiper-slide img').removeClass('zoom_01');
        }
    });
    $('.PD_Media .gallery-thumbs .swiper-button-next').click(function () {
        $('.gallery-top .swiper-slide img').removeClass('zoom_01');
        if ($('.gallery-top .swiper-slide').hasClass('swiper-slide-active')) {
            $('.gallery-top .swiper-slide-active img').addClass('zoom_01');
            $('.zoom_01').ezPlus();
        } else {
            $('.gallery-top .swiper-slide img').removeClass('zoom_01');
        }
    });  
    $('.zoom_01').ezPlus();
    $('#lightgallery').lightGallery({
      selector: '.item',
    });


    // END SLIDE PRODUCT        
    $(".product_detail").find(".txt_down").click(function(){
        if(!$(this).hasClass("active")){
            $(this).addClass("active");
            $(this).next('div').slideToggle( "slow", function() {});
        }
        else{
            $(this).removeClass("active");
            $(this).next('div').slideToggle( "hide", function() {});
        }
    });                

    // Show comment
    $(".pd-tab-block__comment-panel__view-more-btn").click(function() {
        $('.pd-tab-block__comment-panel__content-field').toggleClass('is-expand');
        $(this).text($(this).text() == '- Hide' ? '+ More' : '- Hide');
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