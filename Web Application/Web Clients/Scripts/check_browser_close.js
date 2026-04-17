/**
 * This javascript file checks for the brower/browser tab action.
 * It is based on the file menstioned by Daniel Melo.
 * Reference: http://stackoverflow.com/questions/1921941/close-kill-the-session-when-the-browser-or-tab-is-closed
 */
var validNavigation = false;
 
function endSession() {
  // Browser or broswer tab is closed
  // Do sth here ...
    //var flag = confirm("Please Logout properly else your account will be locked. \n Are you sure to close this page?");
    //if (flag == false) {
    //    alert("Dont Close");
    //    return false;
    //} 

    return "You have not logout properly.";

}
 
function wireUpEvents() {
  /*
  * For a list of events that triggers onbeforeunload on IE
  * check http://msdn.microsoft.com/en-us/library/ms536907(VS.85).aspx
  */


    //window.onbeforeunload = askConfirm;
    //function askConfirm() {
    //    return "You have unsaved changes.";
    //}


    function closeIt() {
        if (!validNavigation) {
            return "Please ensure to logout properly else your login will be locked \n" +
                   "Are you confirm to proceed?";
        }
    }
    window.onbeforeunload = closeIt;
     
  // Attach the event keypress to exclude the F5 refresh
  $(document).bind('keypress', function(e) {
    if (e.keyCode == 116){
      validNavigation = true;
    }
  });

  // Attach the event click for all links in the page
  $("select").bind("change", function () {
      if (typeof (Sys) != "undefined") {
          Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
             function () {
                 validNavigation = true;
             });
      }
  });

  // Attach the event click for all links in the page
  $("select").bind("click", function () {
      if (typeof (Sys) != "undefined") {
          Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(
             function () {
                 validNavigation = true;
             });
      }
  });
 
  // Attach the event click for all links in the page
  $("a").bind("click", function () {     
    validNavigation = true;
  });
 
  // Attach the event submit for all forms in the page
  $("form").bind("submit", function () {
    validNavigation = true;
  });
 
  //  // Attach the event click for all inputs in the page
  //$("button[type=button]").bind("click", function () {
  //    validNavigation = true;
  //});

  // Attach the event click for all inputs in the page
  $("button[type=submit]").bind("click", function () {
    validNavigation = true;
  });

    // Attach the event click for all inputs in the page
  $("input[type=submit]").bind("click", function () {
      validNavigation = true;
  });

  //$("input[type=button]").bind("click", function () {
  //    validNavigation = true;
  //});


  //$("button").bind("click", function () {
  //    validNavigation = true;
  //});
}

$(":submit").bind("click", function () {

    validNavigation = true;
});

 
// Wire up the events as soon as the DOM tree is ready
$(document).ready(function() {
  wireUpEvents();  
});