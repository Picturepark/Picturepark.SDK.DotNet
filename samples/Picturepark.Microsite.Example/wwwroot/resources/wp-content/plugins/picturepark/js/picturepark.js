jQuery('document').ready(function($){

	$body = $("body");

	$(document).on({
		ajaxStart: function() { $body.addClass("loading");    },
		ajaxStop: function() { $body.removeClass("loading"); }    
	});
	
	/*Show/Hide password on clicking eye icon*/
	if($("input.conf-password").length){
		if($("input.conf-password").val().length > 0)
				   $('.show-hide-password').show();
	}			   
	$(".conf-password").each(function (index, input) {
		var passinput = $(input);
		var change;
		$('.show-hide-password').mousedown(function(){
			change = "text";
			var rep = $("<input type='" + change + "' />")
				.attr("id", passinput.attr("id"))
				.attr("name", passinput.attr("name"))
				.attr('class', passinput.attr('class'))
				.val(passinput.val())
				.insertBefore(passinput);
			  passinput.remove();
			  passinput = rep;
		}).insertAfter(passinput);
		$('.show-hide-password').bind("mouseup mouseleave",function(){
			change = "password";
			var rep = $("<input type='" + change + "' />")
				.attr("id", passinput.attr("id"))
				.attr("name", passinput.attr("name"))
				.attr('class', passinput.attr('class'))
				.val(passinput.val())
				.insertBefore(passinput);
			  passinput.remove();
			  passinput = rep;
		}).insertAfter(passinput);
	});	
	$("input.conf-password").bind("change keyup",function () {
		if($('input.conf-password').val().length > 0)
			$('.show-hide-password').show();
		else{
			 $('.show-hide-password').hide();
		}
		
	});	
});
