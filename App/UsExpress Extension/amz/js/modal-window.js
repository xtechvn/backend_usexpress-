/**
 * SinmpleModalWindow Plugin Version 1.0
 * License: The MIT License (MIT)
 * Created 2014 by Yamamoto Ryota
 */
(function($){
    $.fn.modalWindow = function(options){
        options = $.extend({
			"openTrigger": ".trigger",
			"closeTrigger": ".closeBtn",
			"modalContent": ".modalContent",
			"overLay" : "overLay",
			"width" : 500,
			"height": 500,
			"feadSpeed" : 500,
        },options);

		//resize
		$(window).resize(function(){
			$(options.modalContent).css({
				top:$(window).height() /2 - currentModal.outerHeight() /2 + $(window).scrollTop(),
                left:($(window).width() /2 - currentModal.outerWidth() /2 + $(window).scrollLeft()),
			});
		});

		//Get current modalwindow from data
		$(options.openTrigger).on('click',function(){
			if($(options.openTrigger).length > 1){
				//In the case of multiple modal window
				getModal = this.getAttribute('data-modal');
				currentModal = $('.' + getModal);
			} else {
				currentModal = $(options.modalContent);
			}
			openModal();
			$(window).resize();
			scrollModal();
			closeModal(options.closeTrigger);
		});

		//Display processing for modal window
		function openModal(){
			$('body').append('<div id="'+options.overLay+'"></div>');
			currentModal.fadeIn(options.fadeSpeed);
			$(options.modalContent).css({
				width: options.width,
				height: options.height,
			});
		}

		//ScrollMordalContent
		function scrollModal(){
			$(window).scroll('on',function(){
				$(options.modalContent).css('position','fixed');
			});
		}

		//End processing
		function closeModal(closeObj){
			$(closeObj).on('click',function(){
				$(options.modalContent).fadeOut(options.feadSpeed);
				$('div#' + options.overLay).fadeOut(options.fadeSpeed,function(){
					$(this).remove();
				});
			});
		}


	}
})(jQuery);