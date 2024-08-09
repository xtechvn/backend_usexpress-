(function ($) {
  "use strict";
  var iOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
  var isMobile = {
    Android: function () {
      return navigator.userAgent.match(/Android/i);
    },
    BlackBerry: function () {
      return navigator.userAgent.match(/BlackBerry/i);
    },
    iOS: function () {
      return navigator.userAgent.match(/iPhone|iPad|iPod/i);
    },
    Opera: function () {
      return navigator.userAgent.match(/Opera Mini/i);
    },
    Windows: function () {
      return navigator.userAgent.match(/IEMobile/i);
    },
    any: function () {
      return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
    }
  }


  //backToTop
  function backToTop() {
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
  }
  //resizeSite
  function resizeSite() {
    var checkPoint = 1200,
      windowWidth = $(window).innerWidth();
    // mobile
    if (checkPoint > windowWidth) {

    } else {

    }
  }
  //scrollBar
  function scrollBar() {
    var scrollContainer = $(".scrollbar-inner");
    if (scrollContainer.length > 0) {
      scrollContainer.scrollbar();
    }
  }
  //magnificPopup
  function magnificPopup() {
    $('.open-popup-link').magnificPopup({
      type: 'inline',
      midClick: true,
      mainClass: 'mfp-with-zoom',
      fixedContentPos: false,
      fixedBgPos: true,
      overflowY: 'auto',
      closeBtnInside: true,
      preloader: false,
      removalDelay: 300,
    });
  }
  //fixSticky
  function fixStickyIE() {
    var stickyElements = $('.sticky');
    if (stickyElements.length > 0) {
      Stickyfill.add(stickyElements);
    }
  }

  function searchHome() {
    var $wid = $('#search').width();
    var $wid1 = ($('.op_thuonghieu').width() + 25);
    var $wid2 = $wid - $wid1;
    $('.op_thuonghieu').css('width', $wid1 + 'px');
    $('#search .warp-search').css('width', $wid2 + 'px');
    $(".op_thuonghieu .options").find("a").click(function () {
      var html = $(this).html();
      $(".op_thuonghieu .txt_hang span").html(html);
      $('.op_thuonghieu').css('width', 'auto');
      var $wid = $('#search').width();
      var $wid1 = ($('.op_thuonghieu').width() + 70);
      var $wid2 = $wid - $wid1;
      $('.op_thuonghieu').css('width', $wid1 + 'px');
      $('#search .warp-search').css('width', $wid2 + 'px');
    });
    $(".warp-search").find(".input_search").click(function () {
      $(".warp-search").find('.suggest-search').hide();
      $(".dropdown-content").slideUp();
      if (!$(this).hasClass("active")) {
        $(this).addClass("active");
        $(this).next('.suggest-search').show();
      } else {
        $(this).removeClass("active");
        $(this).next('.suggest-search').hide();
      }
    });
    $(document).click(function () {
      $(".warp-search").find(".input_search").removeClass('active');
      $(".warp-search").find('.suggest-search').hide();
    });
    $(".warp-search .input_search").click(function (event) {
      event.stopPropagation();
    });
  }

  function onClick() {
    $(".dropdown").find(".dropbtn").click(function () {
      $(".dropdown").find('.dropdown-content').slideUp();
      $(".suggest-search").hide();
      if ($(this).next().css('display') == 'none') {
        $('.dropdown-content').slideUp();
        $(this).next().stop(true, true).slideDown();
        $('.dropdown').removeClass('active');
        $(this).parent().addClass('active');
      } else {
        $(this).parent().find('.dropdown-content').slideUp();
        $('.dropdown').removeClass('active');
      }
    });

    $('.onclick >a').click(function () {
      $('.onclick >div').slideUp();
      if ($(this).next().css('display') == 'none') {
        $(this).next().slideDown();
      } else {
        $(this).next().slideUp();
      }
    });



    $(document).click(function () {
      $(".dropdown .dropbtn, .onclick >a").removeClass('active');
      $(".dropdown").find('.dropdown-content').slideUp();
      $(".onclick >a").next().slideUp();
    });
    $(".dropdown .dropbtn, .onclick >a").click(function (event) {
      event.stopPropagation();
    });


    $(".nav_setting a").click(function (event) {
      $(".nav_setting a").removeClass("active")
      if (!$(this).hasClass("active")) {
        $(this).addClass("active");
      } else {
        $(this).removeClass("active");
      }
      event.preventDefault();
      var tab = $(this).attr("href");
      $(".content_setting .tab-content").not(tab).css("display", "none");
      $(tab).css("display", "block");
    });
    $(".nav-bar").click(function () {
      if (!$(this).hasClass("active")) {
        $(this).addClass("active");
        $('.warp_menu ').removeClass("active");
      } else {
        $(this).removeClass("active");
        $('.warp_menu ').addClass("active");
      }
    });
    // Select ATM
    $('.b3_nhtt ul li').click(function () {
      $(this).addClass('tc_nhtt');
      $(this).siblings().removeClass('tc_nhtt');
    });
  }

  jQuery(function (a) {
    a(".warp_menu").mouseover(function () {
      a(".nav-bar").addClass("active")
      a(this).addClass("active")
    });
    a(".nav-bar").mouseleave(function () {
      a(this).removeClass("active")
      a(".warp_menu").removeClass("active")
    });
    a(".warp_menu").mouseleave(function () {
      a(".nav-bar").removeClass("active")
      a(".warp_menu").removeClass("active")
    });

    a(".warp_menu a.link").mouseover(function () {
      var c = a(this);
      var d = c.attr("rel");
      var b = a("#" + d);
      b.show().siblings(".child-box").hide()
    });
    a(".warp_menu .col-1 li").mouseover(function () {
      a(this).addClass("active").siblings(".warp_menu .col-1 li").removeClass("active")
    });
    a(".nav-bar").mouseover(function () {
      a(".warp_menu").addClass("active");
      a(this).addClass("active")
    });

  });

  function tabContainer() {
    $(".tab-default a").click(function (event) {
      $(".tab-default a").removeClass("active")
      if (!$(this).hasClass("active")) {
        $(this).addClass("active");
      } else {
        $(this).removeClass("active");
      }
      event.preventDefault();
      var tab = $(this).attr("href");
      $(".tab-container > div").not(tab).css("display", "none");
      $(tab).fadeIn();
    });
  }

  function slideSwiper() {
    //Slide baner
    var slider_baner = new Swiper('.slide_banner .swiper-container', {
      slidesPerView: 1,
      loop: true,
      effect: 'fade',
      autoplay: {
        delay: 3000,
      },
      pagination: {
        el: '.swiper-pagination',
        clickable: true,
      },
    });

    //slide nhãn hàng
    var slide_category = new Swiper('.slide_category .swiper-container', {
      slidesPerView: 6,
      loop: true,
      autoplay: {
        delay: 5000,
      },
      navigation: {
        nextEl: '.swiper-button-next',
        prevEl: '.swiper-button-prev',
      },
      breakpoints: {
        1190: {
          slidesPerView: 4,
        },
        768: {
          slidesPerView: 3,
        }
      }
    });

    //slide thương hiệu
    var slide_th = new Swiper('.box-thuong-hieu .swiper-container', {
      slidesPerView: 9,
      spaceBetween: 15,
      loop: true,
      autoplay: {
        delay: 5000,
      },
      breakpoints: {
        1190: {
          slidesPerView: 6
        },
        768: {
          slidesPerView: 5
        },
        767: {
          slidesPerView: 4
        },
        576: {
          slidesPerView: 3
        }
      }
    });


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
    // var swiper = new Swiper('.xu-huong .swiper-container', {
    //   slidesPerView: 6,
    //   slidesPerColumn: 2,
    //   spaceBetween: 0,
    //   navigation: {
    //     nextEl: '.swiper-button-next',
    //     prevEl: '.swiper-button-prev',
    //   },
    //   breakpoints: {
    //     1190: {
    //       slidesPerView: 5,
    //     },
    //     768: {
    //       slidesPerView: 4,
    //     },
    //     767: {
    //       slidesPerView: 3,
    //     },
    //     576: {
    //       slidesPerView: 2,
    //     }
    //   }
    // });
    var slider_video = new Swiper('.slide_box_video .swiper-container', {
      slidesPerView: 2,
      spaceBetween: 28,
      loop: true,
      navigation: {
        nextEl: '.swiper-button-next',
        prevEl: '.swiper-button-prev',
      },
      pagination: {
        el: '.swiper-pagination',
        clickable: true,
      },
      breakpoints: {
        576: {
          slidesPerView: 1,
        }
      }
    });

    var slide_sale2 = new Swiper('.slide_sale2 .swiper-container', {
      slidesPerView: 5,
      spaceBetween: 18,
      navigation: {
          nextEl: '.slide_sale2 .swiper-button-next',
          prevEl: '.slide_sale2 .swiper-button-prev',
      },
      breakpoints: {
          1190: {
              slidesPerView: 4,
          },
          768: {
              slidesPerView: 3,
          },
          576: {
              slidesPerView: 2,
          }
      }
  });


  }

  function loginForm() {
    $("#login-popup .form .tab").find("a").click(function (event) {
      $("#login-popup .form .tab").find("a").removeClass("active")
      if (!$(this).hasClass("active")) {
        $(this).addClass("active");
      } else {
        $(this).removeClass("active");
      }
      event.preventDefault();
      var tab = $(this).attr("href");
      $("#login-popup .form .tab-content").not(tab).css("display", "none");
      $(tab).css("display", "block");
    });
  }

  function datepicker() {
    $('.datepicker-input').Zebra_DatePicker({
      format: 'd/m/Y'
    });

  }


  $(function () {
    backToTop();
    scrollBar();
    magnificPopup();
    fixStickyIE();
    searchHome();
    onClick();
    tabContainer();
    slideSwiper();
    loginForm();
    datepicker();
    $('[data-toggle="tooltip"]').tooltip({
      html: true
    });
  });

  $(window).on('load resize', function () {
    resizeSite()
  });
})(jQuery);


$(document).ready(function () {


});