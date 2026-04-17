$(document).ready(function () {

    var timeout = 15;//OriSessionTimeout;
    timeout = timeout * 60 * 1000;//1 minutes

    // * Must be multiple of 1000
    var interval = 5000; //5 second = 5000 milisecon( must be multiple of 1000)

    if (userId) {
        timerTimeout = setInterval(function () {
            timeout -= interval;

            if (timeout <= 0) {
                loadTimeout();
                clearInterval(timerTimeout);

              //  loadSpinner();
                var url = "/LogoutTimeout/" + userId + "/" + clientName;
                onLogOut(url,'1');


                //jQuery("#dialog-session-expired").dialog(
                //    {
                    
                //    modal: true,
                //    buttons: {
                //        "Ok": function () {
                //            validNavigation = true;
                //            $(this).dialog("close");
                //            loadSpinner();
                //            var url = "/LogoutTimeout/" + userId + "/" + clientName;
                //            onLogOut(url,'1');
                //        }
                //    }
                //});

            }
        }, interval);
    }

    //each button click will reset the session timeout again
    $('button').click(function () {
        timeout = OriSessionTimeout;
        timeout = timeout * 60 * 1000;
    });

    $('#dataTable').on('draw.dt', function () {
        //once table redrawn, bind the  button in the datatable
        $('#dataTable button').click(function () {
            timeout = OriSessionTimeout;
            timeout = timeout * 60 * 1000;
        });
    });

    //collapse menu
    //Edited AK - hide menu for left panel
    $(document).on('scroll', function (e) {
        if (!$(e.target).hasClass('page-sidebar-wrapper'))
            collapse();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('div.page-sidebar-wrapper').length)
            collapse();
    });

    $('li.quicklinks').on('shown.bs.dropdown', function () {
        collapse();
    });

    function collapse() {
        var open = $('ul#ulmenu>li.open');
        if (open.length) {
            open.children('ul.sub-menu').css("display", "none");
            open.removeClass("open");
        }
    }

});


