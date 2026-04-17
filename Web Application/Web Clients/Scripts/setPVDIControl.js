
function initialiseImg() {
    if (topBgColor.length > 1) {
        //top selected
        document.getElementById("PVImgFlipToggleTop").style.backgroundColor = topBgColor;
        document.getElementById("PVImgViewerTop").style.backgroundColor = topBgColor;
        document.getElementById('MainAdminContent_btnSelectTop').className = "btn btn-danger";

        //bottom seected
        document.getElementById("PVImgFlipToggleBottom").style.backgroundColor = ImgBgOriColor;
        document.getElementById("PVImgViewerBottom").style.backgroundColor = ImgBgOriColor;
        document.getElementById('MainAdminContent_btnSelectBottom').className = "btn btn-primary";

    }
    else {
        //bottom seected
        document.getElementById("PVImgFlipToggleBottom").style.backgroundColor = bottomBgColor;
        document.getElementById("PVImgViewerBottom").style.backgroundColor = bottomBgColor;
        document.getElementById('MainAdminContent_btnSelectBottom').className = "btn btn-danger";

        document.getElementById("PVImgFlipToggleTop").style.backgroundColor = ImgBgOriColor;
        document.getElementById("PVImgViewerTop").style.backgroundColor = ImgBgOriColor;
        document.getElementById('MainAdminContent_btnSelectTop').className = "btn btn-primary";
    }
}

function setBottomBg() {
    topBgColor = "";
    bottomBgColor = "lightblue"
    return true;
}

function setTopBg() {
    bottomBgColor = "";
    topBgColor = "lightblue";
    // document.getElementById("PVImgFlipToggleTop").style.backgroundColor = "lightblue";
    return true;
}

function showhideBottom(strShow) {
    if (String(strShow) == "0") {
        document.getElementById("panelBottom").style.display = "none";
    }
    else {
        document.getElementById("panelBottom").style.display = "block";
    }
}
