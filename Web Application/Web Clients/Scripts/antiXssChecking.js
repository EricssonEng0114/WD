
// Wire up the events as soon as the DOM tree is ready
$(document).ready(function () {
    //alert("ready xsd");
    checkXss();
});

function checkXss() {

    $('input[type=text], textarea').change(function () {
       var str = this.value;//"*$&@Q*()$&#*()@&$)(@*#)(*!()#*!)9378";
        var patt1 = /^(?!(.|\n)*<[a-z!\/?])(?!(.|\n)*&#)(.|\n)*$/g;
        var result = str.match(patt1);
        if (result == null) {
            alert("Unsupported combination of characters detected. Please re-enter.");
            this.value = "";
        }
    });
}