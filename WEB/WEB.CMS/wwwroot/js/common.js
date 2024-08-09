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

    //scrollBar
    function scrollBar() {
        var scrollContainer = $(".scrollbar-inner");
        if (scrollContainer.length > 0) {
            scrollContainer.scrollbar();
        }
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

    //magnificPopup
    function magnificPopup() {
        $('.open-popup').magnificPopup({
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

    //OnCLick
    function onClickTongle() {

        $('.btn_show').click(function () {
            $('body').addClass('show-menu');
        });

        $('.close-menu').click(function () {
            $('body').removeClass('show-menu');
        });

        $(".all-menu-tablet").click(function () {
            $(this).toggleClass("close-menu-tablet");
        });

        $(".all-menu").click(function () {
         /*   $(".mask-content").toggle();*/
            $(".head-top").toggleClass("show-all-menu");
        });

        $(".mask-content").click(function () {
            $(this).hide();
            $(".all-menu").removeClass("close-menu-tablet");
            $(".head-top").removeClass("show-all-menu");
        });

        $(".onclick-togle").click(function () {
            if (!$(this).hasClass("active")) {
                $(this).addClass("active");
                $(this).next('.expand').slideDown();
            } else {
                $(this).removeClass("active");
                $(this).next('.expand').slideUp();
            }
        });
        $('.user-hierarchy-ul').find('.onclick').click(function () {
            if (!$(this).parent('li').hasClass("active")) {
                $(this).parent('li').addClass("active");
                $(this).parent('li').find('.lever2').slideDown('fast');
            } else {
                $(this).parent('li').removeClass("active");
            }
        });
        $(".down-up .onclick").click(function () {
            if (!$(this).hasClass("active")) {
                $(this).addClass("active");
                $(this).next('.form-down').slideDown();
                $('.form-down input').focus();

            } else {
                $(this).removeClass("active");
                $(this).next('.form-down').slideUp();
            }
        });

        $(".dropdown").find(".dropbtn").click(function () {
            $(".dropdown").find('.dropdown-content').slideUp();
            if ($(this).next().css('display') == 'none') {
                $('.dropdown-content').slideUp();
                $(this).next().stop(true, true).slideDown();
                $('.dropdown').removeClass('active');
                $(this).parent().addClass('active');
                $(".form_suggest .input_search").focus();
            } else {
                $(this).parent().find('.dropdown-content').slideUp();
                $('.dropdown').removeClass('active');
            }
        });

        $(".dropdown-content").find("li").click(function () {
            var html = $(this).html();
            $(".dropbtn").html(html);
        });

        $(document).click(function (event) {
            $(".onclick-togle, .dropdown .dropbtn,.down-up .onclick").removeClass('active');
            $(".dropdown.active").find('.dropdown-content').slideUp();
            $(".select--v2__content").slideUp();
            if ($(event.target).closest('.form-down').length == 0) {
                $(".form-down").slideUp();
            }
        });

        $(".onclick-togle, .dropdown .dropbtn, .down-up .onclick, .news-dropdown.dropdown-content, .dropdown-content .input_search,.select--v2__content").click(function (event) {
            event.stopPropagation();
        });
    }

    function onClickTab() {
        $(".tab-default a").click(function (event) {
            $(".tab-default a").removeClass("active")
            if (!$(this).hasClass("active")) {
                $(this).addClass("active");
            } else {
                $(this).removeClass("active");
            }
            event.preventDefault();
            var tab = $(this).attr("href");
            $(".tab-content > div").not(tab).css("display", "none");
            $(tab).fadeIn();
        });

    }

    function datepicker() {
        $('.datepicker-input').Zebra_DatePicker({
            format: 'd/m/Y'
        });
    }

    // function select2() {
    // 	$('.select2').select2();
    // 	$('.select2--nosearch').select2({
    // 		minimumResultsForSearch: -1
    // 	});
    // }

    $(function () {
        scrollBar();
        // magnificPopup();
        fixStickyIE();
        onClickTongle();
        // onClickTab();
        datepicker();
        // select2();
        $('[data-toggle="tooltip"]').tooltip({
            html: true
        });
    });

    $(window).on('load resize', function () {
        resizeSite();
    });

})(jQuery);