//*********************************************************************//
// PDSA Collapser
// The following functions are for the PDSA collapser which adds
// a glyph icon to the right side of the bootstrap panel collapser.
//*********************************************************************//
$(document).ready(function () {
    //2023-07-25 new jquery version cannot execute the following line 
    //Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
    //function () {
        // Hook into glyph anchor tag so we can 
        // toggle the collapse and change the glyph
        $(".pdsa-panel-toggle").click(function () {
            // Retrieve the previous anchor tag's 'href' attribute
            // so we can toggle it.
            $($(this).prev().attr("href")).collapse("toggle");
            // Change the glyphs
            toggleGlyphs($(this));
        });

        // Hook into title anchor tag so we can toggle the glyph
        $("a[data-toggle='collapse']").click(function () {
            // Change the glyphs
            toggleGlyphs($(this).next());
        });

        // Add the down glyph to all elements that have the class 'pdsa-panel-toggle'
        $(".pdsa-panel-toggle").addClass("glyphicon glyphicon-chevron-down");

        // Find any panel's that have the class '.in',
        // remove the down glyph and add the up glyph
        var list = $(".in");
        for (var i = 0; i < list.length; i++) {
            $($("a[href='#" + $(list[i]).attr("id") + "']")).next()
              .removeClass("glyphicon glyphicon-chevron-down")
              .addClass("glyphicon glyphicon-chevron-up");
        }


//});


});

// This function toggles the glyphs
function toggleGlyphs(ctl) {
  if ($(ctl).hasClass("glyphicon glyphicon-chevron-up")) {
    $(ctl).removeClass("glyphicon glyphicon-chevron-up");
    $(ctl).addClass("glyphicon glyphicon-chevron-down");
  }
  else {
    $(ctl).removeClass("glyphicon glyphicon-chevron-down");
    $(ctl).addClass("glyphicon glyphicon-chevron-up");
  }
}
//*********************************************************************//
// END: PDSA Collapser
//*********************************************************************//
