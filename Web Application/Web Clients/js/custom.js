;(function($){
	'use strict';
	var DH = {
		init: function(){
			
			//User Login and register account.
			this.userInit();
		},
		userInit: function(){
			
			$(document).on('click','[data-rel=registerModal]',function(e){
				e.stopPropagation();
				e.preventDefault();
				if($('#userloginModal').length){
					$('#userloginModal').modal('hide');
				}
				if($('#userlostpasswordModal').length){
					$('#userlostpasswordModal').modal('hide');
				}
				if($('#userregisterModal').length){
					$('#userregisterModal').modal('show');
				}
			});
			$(document).on('click','[data-rel=loginModal]',function(e){
				e.stopPropagation();
				e.preventDefault();
				if($('#userregisterModal').length){
					$('#userregisterModal').modal('hide');
				}
				if($('#userlostpasswordModal').length){
					$('#userlostpasswordModal').modal('hide');
				}
				if($('#userloginModal').length){
					$('#userloginModal').modal('show');
				}
			});
		},
	};
	$(document).ready(function(){
		DH.init();
	});
})(jQuery);

function validateFloatKeyPress(el, evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    var number = el.value.split('.');
    if (charCode != 46 && charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    //just one dot (thanks ddlab)
    if (number.length > 1 && charCode == 46) {
        return false;
    }
    //get the carat position
    var caratPos = getSelectionStart(el);
    var dotPos = el.value.indexOf(".");
    if (caratPos > dotPos && dotPos > -1 && (number[1].length > 1)) {
        return false;
    }
    return true;
}

//thanks: http://javascript.nwbox.com/cursor_position/
function getSelectionStart(o) {
    if (o.createTextRange) {
        var r = document.selection.createRange().duplicate()
        r.moveEnd('character', o.value.length)
        if (r.text == '') return o.value.length
        return o.value.lastIndexOf(r.text)
    } else return o.selectionStart
}
