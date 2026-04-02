    document.addEventListener("DOMContentLoaded", function () {

        function showCommentsAnimation(containerId = "commentsContainer") {
            const items = document.querySelectorAll(`#${containerId} .comment-item`);
            items.forEach((el, idx) => {
                setTimeout(() => {
                    el.style.opacity = "1";
                    el.style.transform = "translateY(0)";
                }, idx * 100); // stagger efekt
            });
        }

    // pokaż animację przy pierwszym renderze
    showCommentsAnimation();

    const btn = document.getElementById("loadMoreBtn");
    if (!btn) return;

    btn.addEventListener("click", async function () {
        const houseId = this.dataset.house;
    let skip = parseInt(this.dataset.skip);

    const response = await fetch(`/Comments/LoadMore?houseId=${houseId}&skip=${skip}&take=6`);
    const html = await response.text();

    // zamiast nadpisywać innerHTML -> dopisz nowe komentarze
    const container = document.getElementById("commentsContainer");
    container.insertAdjacentHTML("beforeend", html);

    // pokaż animację tylko dla nowych elementów
    showCommentsAnimation();

    // zwiększ skip
    this.dataset.skip = skip + 6;
    });
});
