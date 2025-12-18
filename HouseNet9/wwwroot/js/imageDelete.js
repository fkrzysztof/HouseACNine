$(document).ready(function () {

    const deleteUrl = $("#images-container").data("delete-url");

    $(".remove-image").click(function () {

        if (!confirm("Czy usunąć zdjęcie?")) return;

        let imageDiv = $(this).closest(".image-item");
        let imageId = imageDiv.data("id");

        let token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: deleteUrl,        // <-- używamy z data-delete-url
            type: "POST",
            data: {
                id: imageId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    imageDiv.remove();
                } else {
                    alert("Nie udało się usunąć zdjęcia.");
                }
            },
            error: function () {
                alert("Błąd połączenia z serwerem.");
            }
        });
    });
});
