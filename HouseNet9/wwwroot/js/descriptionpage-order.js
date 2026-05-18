$(document).ready(function () {

    const el = document.getElementById("sortable-order");
    const updateUrl = "/DescriptionPages/UpdateOrder";

    if (!el) return;

    Sortable.create(el, {
        animation: 150,
        ghostClass: "sortable-ghost",

        // 🔥 lepszy UX (opcjonalne ale polecam)
        forceFallback: true,
        fallbackTolerance: 5,

        onEnd: function () {

            let items = [];

            document.querySelectorAll("#sortable-order .order-item")
                .forEach((item, index) => {

                    items.push({
                        id: parseInt(item.dataset.id),
                        order: index + 1
                    });

                });

            $.ajax({
                url: updateUrl,
                type: "POST",
                contentType: "application/json",
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                data: JSON.stringify(items),

                // 🔥 DODANE: feedback + debug
                success: function () {
                    console.log("Order zapisany");
                },

                error: function (xhr) {
                    console.error("Błąd zapisu kolejności", xhr.responseText);
                    alert("Nie udało się zapisać kolejności");
                }
            });
        }
    });
});