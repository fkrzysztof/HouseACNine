//    document.addEventListener("DOMContentLoaded", function () {

//        function showCommentsAnimation(containerId = "commentsContainer") {
//            const items = document.querySelectorAll(`#${containerId} .comment-item`);
//            items.forEach((el, idx) => {
//                setTimeout(() => {
//                    el.style.opacity = "1";
//                    el.style.transform = "translateY(0)";
//                }, idx * 100); // stagger efekt
//            });
//        }

//    // pokaż animację przy pierwszym renderze
//    showCommentsAnimation();

//    const btn = document.getElementById("loadMoreBtn");
//    if (!btn) return;

//    btn.addEventListener("click", async function () {
//        const houseId = this.dataset.house;
//    let skip = parseInt(this.dataset.skip);

//    const response = await fetch(`/Comments/LoadMore?houseId=${houseId}&skip=${skip}&take=6`);
//    const html = await response.text();

//    // zamiast nadpisywać innerHTML -> dopisz nowe komentarze
//    const container = document.getElementById("commentsContainer");
//    container.insertAdjacentHTML("beforeend", html);

//    // pokaż animację tylko dla nowych elementów
//    showCommentsAnimation();

//    // zwiększ skip
//    this.dataset.skip = skip + 6;
//    });
//});











    document.addEventListener("DOMContentLoaded", function () {

    const btn = document.getElementById("loadMoreBtn");
    if (!btn) return;

    const spinner = btn.querySelector(".spinner-border");
    const text = btn.querySelector(".btn-text");

    let isLoading = false; // 🔒 blokada kliknięć

    function showCommentsAnimation() {
        const items = document.querySelectorAll('#commentsContainer .comment-item');
        items.forEach((el, idx) => {
        setTimeout(() => {
            el.style.opacity = "1";
            el.style.transform = "translateY(0)";
        }, idx * 100);
        });
    }

    // animacja pierwszego renderu
    showCommentsAnimation();

    btn.addEventListener("click", async function () {

        // 🚫 blokada podwójnego kliknięcia
        if (isLoading) return;
    isLoading = true;

    // 🔄 pokaż spinner
    spinner.classList.remove("d-none");
    text.classList.add("d-none");

    const houseId = this.dataset.house;

    const container = document.getElementById("commentsContainer");
    const skip = container.querySelectorAll(".comment-item").length;

    try {
            const response = await fetch(`/Comments/LoadMore?houseId=${houseId}&skip=${skip}&take=6`);
    const html = await response.text();

    // ❌ brak nowych komentarzy → ukryj przycisk
    if (!html.trim()) {
        btn.style.display = "none";
    return;
            }

    const temp = document.createElement("div");
    temp.innerHTML = html;

    const items = temp.querySelectorAll(".comment-item");

    // jeśli backend zwrócił 0 elementów
    if (items.length === 0) {
        btn.style.display = "none";
    return;
            }

            // dodaj nowe komentarze
            items.forEach(item => {
        item.style.opacity = 0;
    item.style.transform = "translateY(20px)";
    container.appendChild(item);
            });

    // animacja
    showCommentsAnimation();

        } catch (err) {
        console.error("Błąd ładowania komentarzy:", err);
        }
    finally {
        // 🔓 odblokuj kliknięcie
        isLoading = false;

    // 🔄 ukryj spinner
    spinner.classList.add("d-none");
    text.classList.remove("d-none");
        }

    });
});

