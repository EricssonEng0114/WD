
 
function initSpinner() {
 
  $(".sub-menu").bind("click", function ()
  {
      isLoadSpinner = true;
      loadSpinner();
  });
  
  // Attach the event submit for all forms in the page
  $("form").bind("submit", function () {
     // loadSpinner();
  });
 
  // Attach the event click for all inputs in the page
  $("button[type=submit]").bind("click", function () {
      loadSpinner();
  });

    // Attach the event click for all inputs in the page
  $("input[type=submit]:not(.viewRpt)").bind("click", function () {
      loadSpinner();
  });

}

function loadTimeout()
{
    $(".timeoutui-bar").css("display", "none");
}

function unloadTimeout()
{
    $(".timeoutui-bar").css("display", "block");
}

function loadSpinner()
{
    
    if (isLoadSpinner) {
        $(".submit-progress").removeClass("hidden");
        $("body").addClass("submit-progress-bg");
    }
    else
    {
        unloadSpinner();
    }
}

function unloadSpinner()
{
    $(".submit-progress").addClass("hidden");
    $("body").removeClass("submit-progress-bg");
}
 
$(document).ready(function() {
    initSpinner();
});