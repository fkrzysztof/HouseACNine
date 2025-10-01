$(document).ready(function () {

    var check1 = document.getElementById('check1');
    var check2 = document.getElementById('check2');
    var check3 = document.getElementById('check3');

    let checkTab = [check1, check2, check3];

    var btnClose = document.getElementById('btnClose');
    var btnCreate = document.getElementById('btnCreate');

    //Resetuj wszystkie checkboxy na niezaznaczone
    checkTab.forEach(cb => cb.checked = false);


    // Close -> Home/Index
    //btnClose.addEventListener('click', function() {
    //    window.location.href = '@Url.Action("Index", "Home")';
    //});

    // Create -> tylko jeśli wszystkie checkboxy zaznaczone
    btnCreate.addEventListener('click', function (e) {

        if (!check1.checked || !check2.checked || !check3.checked) {
            e.preventDefault();
            checkTab.forEach(cb => {
                if (!cb.checked) {
                    cb.style.outline = "3px solid red";
                } else {
                    cb.style.removeProperty('outline');
                }
            });
        }
    });


    function checkTest(e) {
        if (!e.target.checked) {
            e.target.style.outline = "3px solid red";
        } else {
            e.target.style.removeProperty('outline');
        }
    }


    checkTab.forEach(cb => {
        cb.addEventListener("change", checkTest);
    });

});