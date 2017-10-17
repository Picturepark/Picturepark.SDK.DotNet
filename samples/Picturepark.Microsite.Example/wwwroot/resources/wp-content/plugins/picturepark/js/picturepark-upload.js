jQuery(function ($) {

  $( 'body' ).on( 'click', 'button#picturepark-media-upload-button',  function( e ) {  

	$.ajax({
			  url: pictureparkUploadData.editorPluginFile,
			  type: "post",
			  data: { picturepark_url: true },
			  
			  success: function(data) {
				  
				  console.log(data);
				  var dualScreenLeft = window.screenLeft != undefined ? window.screenLeft : screen.left;
				  var dualScreenTop = window.screenTop != undefined ? window.screenTop : screen.top;

				  var width = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
				  var height = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;	

				  var left = ((width / 2) - (900 / 2)) + dualScreenLeft;
				  var top = ((height / 2) - (500 / 2)) + dualScreenTop;
				  window.open(data, "", "width=900,height=500,left="+left+",top="+top);

			  }
			});
	  
  });

});;   