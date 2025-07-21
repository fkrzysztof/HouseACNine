$(document).ready(function () {

    /* Wybieranie dni*/
    const freeDayTab = document.getElementsByClassName("calandarDayFree");
    let selectedDayTabCalendarOne = new Array();
    let selectedDayTabCalendarTwo = new Array();
    let howManyDays = 6;
    var selectHowManyDays = document.getElementById("HowManyDaysSelect");

    selectHowManyDays.addEventListener('click', function () {
        howManyDays = selectHowManyDays.value;
    });

    for (var i = 0; i < freeDayTab.length; i++) {
        freeDayTab[i].addEventListener('mouseover', showSelectedDays);
        freeDayTab[i].addEventListener('mousedown', selectedDays);
        freeDayTab[i].addEventListener('mouseout', hideSelectedDays);
    }

    function showSelectedDays() {

        var checkNextDays = new Array();
        var checkNextDays = $(this).nextAll(".btn-group-toggle").not(".invisible").slice(0, howManyDays);

        if (checkNextDays.length < howManyDays) {
            let cal2 = $(".cal2").nextAll(".btn-group-toggle").not(".invisible").slice(0, howManyDays - checkNextDays.length);
            cal2.each(function (i) {
                //checkNextDays.push($(this));
                checkNextDays.push(cal2[i]);
            });
        }


        if (checkNextDays.hasClass("rented") != true) {
            selectedDayTabCalendarOne = $(this).nextAll(".calandarDayFree").slice(0, howManyDays);
            selectedDayTabCalendarOne.push($(this));
            if (selectedDayTabCalendarOne.length <= howManyDays  && $(this).hasClass("calendarOne")) {
                selectedDayTabCalendarTwo = $(".calendarTwo").slice(0, howManyDays - selectedDayTabCalendarOne.length + 1);
                selectedDayTabCalendarTwo.each(function (i) {
                    selectedDayTabCalendarOne.push(selectedDayTabCalendarTwo[i]);
                });
            }
            selectedDayTabCalendarOne.each(function (i) {
                $(this).addClass("bg-warning");
            });
        }
        
    }

    function selectedDays() {

        //zaznacz wybrane
        $(".bg-success").removeClass("bg-success");
        $(".bg-warning").addClass("bg-success");
        //selectedDayTabCalendarOne.each(function (i) {
        //    $(this).addClass("bg-success");
        //});
    }

    $('input[type=radio][name=From]').change(function (e) {
        //alert(e.target.value);

        if ($(".bg-warning").length > 0) {

            let selectedDay = e.target.value;
            let HouseId = $('input:hidden[name=HouseId]').val()
            $.ajax({
                type: "POST",
                url: "./GetCalendar/Info",
                //data: { 'year': selectedDay, 'howManyDays': howManyDays, 'HouseId': HouseId },
                //data: { 'From': selectedDay, 'HowManyDaysFromSelect': howManyDays, 'HouseId': HouseId },
                data: { 'From': selectedDay, 'HouseId': HouseId, 'HowManyDaysFromSelect': howManyDays },
                //data: { 'howManyDays': howManyDays, 'id': id },
                //data: { 'from' : e.target.value.toUTCStrin() },
                //data: $('.RentalHouseForm').serialize(),
                // data: e.target.serialize(),
                //data: $('select[input[name=From]:checked').toUTCString(),
                //data: { 'from' : $('input[name="From"]:checked').val() },
                success: function (result) {
                    $("#info").fadeOut('fast', function () {
                        $("#info").html("");
                    });
                    
                    $("#info").fadeIn('fast', function () {
                        $("#info").append(result);
                    });
                }
            });

        }
    });


    function hideSelectedDays() {

        selectedDayTabCalendarOne.each(function (i) {
            $(this).removeClass("bg-warning");
        });
    }



    /*nawigacja kalendarz JS*/
    let prevLink = document.getElementById("previous");
    let nextLink = document.getElementById("next");

    prevLink.addEventListener("click", function (e) {
        e.preventDefault();
        navi("previous");
    });
    nextLink.addEventListener("click", function (e) {
        e.preventDefault();
        navi("next");
    });

    function navi(prevOrNext) {
        $.ajax({
            url: "./GetCalendar/Index/1?navigation=" + prevOrNext, data: { 'navigation': "previous" }, success: function (result) {
                $("#calendar").html(result);
            }
        });
    }

    //submit form
    document.getElementById("calendarSubmit").addEventListener("click", function () {
        document.getElementById("calendarForm").submit();
    });


});


