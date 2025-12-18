$(document).ready(function () {

    const deleteUrl = $("#pages-list").data("delete-url");

    $(".delete-page").click(function () {

        if (!confirm("Czy na pewno chcesz usunąć ten wpis i wszystkie zdjęcia?"))
            return;

        let btn = $(this);
        let id = btn.data("id");

        // znajdź kartę do usunięcia
        let card = btn.closest(".page-item");

        let token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: deleteUrl,
            type: "POST",
            data: {
                id: id,
                __RequestVerificationToken: token
            },
            success: function (result) {
                if (result.success) {
                    card.fadeOut(300, function () {
                        $(this).remove();
                    });
                } else {
                    alert("Nie udało się usunąć wpisu.");
                }
            },
            error: function () {
                alert("Błąd połączenia z serwerem.");
            }
        });

    });

});
