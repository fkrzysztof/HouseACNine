document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("images-container");
    if (!container) return;

    new Sortable(container, {
        animation: 150,
        handle: '.drag-handle', // przeciąganie tylko po uchwycie ☰
        onEnd: function () {
            const order = [];
            document.querySelectorAll("#images-container .image-item")
                .forEach((el, index) => {
                    order.push({ id: el.dataset.id, order: index });
                });

            fetch("/DescriptionPages/UpdateImageOrder", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(order)
            });
        }
    });
});