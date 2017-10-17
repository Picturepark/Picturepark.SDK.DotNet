///////////////////////////////////////////////////
// ShowHide plugin                               
// Author: Ashley Ford - http://papermashup.com
// Demo: Tutorial - http://papermashup.com/jquery-show-hide-plugin
// Built: 19th August 2011                                     
///////////////////////////////////////////////////

(function ($) {
	$.fn.showHide = function (options) {

		//default vars for the plugin
		var defaults = {
			speed: 10,
			changeText: 0,
			showText: 'Show',
			hideText: 'Hide'

		};
		var options = $.extend(defaults, options);

		$(this).click(function () {
			// this var stores which button you've clicked
			var toggleClick = $(this);
			// this reads the rel attribute of the button to determine which div id to toggle
			var toggleDiv = $(this).attr('rel');
			// optionally add the class .toggleDiv to each div you want to automatically close

			if ($('.toggleDiv').is(':visible')) {
			//do nothing
			} else {
			$('.toggleDiv').slideUp(options.speed, options.easing, function () {
			// this only fires once the animation is completed
			if (options.changeText == 1) {
			$('.show_hide').text(options.showText);
			}
			});
			}

			// here we toggle show/hide the correct div at the right speed and using which easing effect
			$(toggleDiv).slideToggle(options.speed, options.easing, function () {
			// this only fires once the animation is completed
			if (options.changeText == 1) {
			$(toggleDiv).is(":visible") ? toggleClick.text(options.hideText) : toggleClick.text(options.showText);
			}
			});

			return false;

		});

	};
})(jQuery);