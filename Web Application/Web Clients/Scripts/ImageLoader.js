function GetImage(param) {
    var returnMsg = "";

    //"1" = Top image, "2" = Bottom Image
    if (param == "1") {
        returnMsg = "/ImageHandlerTop.ashx";
    }
    else
    {
        returnMsg = "/ImageHandlerBottom.ashx";
    }

  
    //$.ajax({
    //    url: '/Modules/RejectedItemDecision/ValidatePVReject.aspx/getImagePath',
    //    method: 'post',
    //    contentType: 'application/json; charset=utf-8',
    //    data: param,
    //    dataType: 'json',
    //    async: false,
    //    cache: false,
    //    success: function (data) {
    //        returnMsg = data.d;
    //    },
    //    error: function (xhr, status, error) {
    //        alert(xhr.responseText);  // to see the error message
    //        alert("Error connecting to server -> " + xhr.status + "-" + error);
    //        returnMsg = "Error";
    //    }
    //});

    return returnMsg;
}

function LoadImage(srcImg, imgID) {

    //"#viewer3"
    var iv3 = $(imgID).iviewer(
    {
        //Default as Front Tiff image
        src: srcImg
    });

    var fill = false;
    $("#fill").click(function () {
        fill = !fill;
        iv3.iviewer('fill_container', fill);
        return false;
    });

    return true;

}

function ChangeImg(srcImg, imgID)
{
    //"#viewer3"
    $(imgID).iviewer('loadImage', srcImg);
    
}

function toggleImage(module,position, divName)
{
    
    var inputId = position + "Toggle";
    var obj = { id: inputId };
    var param = JSON.stringify(obj);

    var imgSrc =  GetUpdatedImagePath(module,param);

  //  alert(srcTop);

    ChangeImg(imgSrc, divName);
}

function flipImage(module, position, divName) {

    var inputId = position + "Flip";
    var obj = { id: inputId };
    var param = JSON.stringify(obj);

    var imgSrc = GetUpdatedImagePath(module, param);
    ChangeImg(imgSrc, divName);
}
            
function GetUpdatedImagePath(module, param) {
    var returnMsg = "";
    var srcurl = "";

    //if (module == "PV")
    //{
    //    srcurl = '/Modules/RejectedItemDecision/ValidatePVReject.aspx/getImagePath';
    //}


    switch (module) {
        case "PV":
            srcurl = '/Modules/RejectedItemDecision/ValidatePVReject.aspx/getImagePath';
            break;
        case "DI":
            srcurl = '/Modules/RejectedItemDecision/ValidateDIReject.aspx/getImagePath';
            break;
        case "RLM":
            srcurl = '/Modules/RejectedItemDecision/ValidateRLRejectMultipleMode.aspx/getImagePath';
            break;
        case "RL":
            srcurl = '/Modules/RejectedItemDecision/ValidateRLReject.aspx/getImagePath';
            break;
        case "APV":
            srcurl = '/Modules/ActionedItemHistory/ViewPVHistory.aspx/getImagePath';
            break;
        case "ADI":
            srcurl = '/Modules/ActionedItemHistory/ViewDIHistory.aspx/getImagePath';
            break;
        case "ARLM":
            srcurl = '/Modules/ActionedItemHistory/ViewRLHistoryMultipleMode.aspx/getImagePath';
            break;
        case "ARL":
            srcurl = '/Modules/ActionedItemHistory/ViewRLHistory.aspx/getImagePath';
            break;
            //Added by boonchong PE - WD-24-003
        case "IAO":
            srcurl = '/Modules/ImageArchiveOutward/ImageArchiveOutwardDetail.aspx/getImagePath';
            break;
        case "IAI":
            srcurl = '/Modules/ImageArchiveInward/ImageArchiveInwardDetail.aspx/getImagePath';
            break;
    }


    $.ajax({
        url: srcurl,// '/Modules/RejectedItemDecision/ValidatePVReject.aspx/getImagePath',
        method: 'post',
        contentType: 'application/json; charset=utf-8',
        data: param,
        dataType: 'json',
        async: false,
        cache: false,
        success: function (data) {
            returnMsg = data.d;
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);  // to see the error message
            alert("Error connecting to server -> " + xhr.status + "-" + error);
            returnMsg = "Error";
        }
    });

    return returnMsg;
}