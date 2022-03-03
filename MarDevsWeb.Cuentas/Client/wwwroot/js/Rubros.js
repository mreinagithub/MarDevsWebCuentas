export function ScrollTop() {
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

$(window).scroll(function () { scrollFunction(); })
function scrollFunction() {
    var mybutton = document.getElementById("btnScrollTop");
    if (document.body.scrollTop > 20 || document.documentElement.scrollTop > 20) {
        mybutton.style.display = "block";
    } else {
        mybutton.style.display = "none";
    }
}
