
//document.addEventListener("DOMContentLoaded", function () {

//    const nav = document.getElementById("mainNav");
//    const collapse = document.getElementById("navbarResponsive");

//    collapse.addEventListener("show.bs.collapse", function () {
//        nav.classList.add("menu-open");
//    });

//    collapse.addEventListener("hide.bs.collapse", function () {
//        nav.classList.remove("menu-open");
//    });

//});

document.addEventListener("DOMContentLoaded", function () {

    const nav = document.getElementById("mainNav");
    const collapse = document.getElementById("navbarResponsive");
    const toggler = document.querySelector(".custom-toggler");

    // OPEN MENU
    collapse.addEventListener("show.bs.collapse", function () {
        nav.classList.add("menu-open");
        toggler.classList.add("active");
    });

    // CLOSE MENU
    collapse.addEventListener("hide.bs.collapse", function () {
        nav.classList.remove("menu-open");
        toggler.classList.remove("active");
    });

    // SCROLL EFFECT
    window.addEventListener("scroll", function () {
        if (window.scrollY > 10) {
            nav.classList.add("scrolled");
        } else {
            nav.classList.remove("scrolled");
        }
    });

});
