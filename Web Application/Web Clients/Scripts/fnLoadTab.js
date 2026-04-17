function loadAnnouncement(evt) {

    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontentA");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tablinksS");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }
    document.getElementById("announce_0").style.display = "block";
    evt.currentTarget.className += " active";
}

if (document.getElementById("headerAnnouncement") != null)
    document.getElementById("headerAnnouncement").click();