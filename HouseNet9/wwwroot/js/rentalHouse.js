document.addEventListener("DOMContentLoaded", function () {
    const selects = document.querySelectorAll(".status-select");

    selects.forEach(select => {
        select.addEventListener("change", async function () {
            const rentalId = this.dataset.id;
            const newStatusId = this.value;

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

                const response = await fetch(`/RentalHouse/ChangeStatusSelect`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token
                    },
                    body: JSON.stringify({ id: rentalId, statusId: newStatusId })
                });

                if (!response.ok) throw new Error(`Błąd HTTP: ${response.status}`);

                const data = await response.json();

                // Zaktualizuj kolor koła
                const td = this.closest("td");
                const circle = td.querySelector("span");
                if (circle) circle.style.backgroundColor = data.badgeColor;

            } catch (err) {
                console.error("Błąd podczas zmiany statusu:", err);
                alert("Nie udało się zmienić statusu.");
            }
        });
    });
});
