document.addEventListener("DOMContentLoaded", function () {
    const selects = document.querySelectorAll(".status-select");
    const toast = document.getElementById("status-toast");

    function showToast(message) {
        if (!toast) return;
        toast.textContent = message;
        toast.style.opacity = "1";
        setTimeout(() => {
            toast.style.opacity = "0";
        }, 2000);
    }

    selects.forEach(select => {
        select.addEventListener("change", async function () {
            const rentalId = parseInt(this.dataset.id);
            const newStatusId = parseInt(this.value);
            this.disabled = true;

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

                const response = await fetch(changeStatusUrl, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token
                    },
                    body: JSON.stringify({ id: rentalId, statusId: newStatusId })
                });

                if (!response.ok) throw new Error(`HTTP error ${response.status}`);

                const data = await response.json();

                const td = this.closest("td");
                const circle = td.querySelector(".status-indicator");
                if (circle) {
                    circle.style.transition = "background-color 0.3s ease";
                    circle.style.backgroundColor = data.badgeColor;
                }

                if (data.emailSent) {
                    showToast("Email do klienta został wysłany");
                } else {
                    showToast("Status zapisany");
                }

                this.disabled = false;
            } catch (err) {
                console.error("Błąd zmiany statusu:", err);
                alert("Nie udało się zmienić statusu.");
                this.disabled = false;
            }
        });
    });
});