$(document).ready(function () {

    const modal = document.getElementById("modal");
    const modalImg = document.getElementById("modalImg");
    const closeBtn = document.querySelector(".close");
    const nextBtn = document.querySelector(".next");
    const prevBtn = document.querySelector(".prev");
    const thumbnails = document.querySelectorAll(".gallery img");

    let currentIndex = 0;

    function openModal(index) {
        currentIndex = index;
        modal.style.display = "block";
        modalImg.src = thumbnails[index].src;
    }

    thumbnails.forEach(img => {
        img.addEventListener("click", () => {
            openModal(parseInt(img.dataset.index));
        });
    });

    closeBtn.onclick = () => modal.style.display = "none";

    nextBtn.onclick = () => {
        currentIndex = (currentIndex + 1) % thumbnails.length;
        modalImg.src = thumbnails[currentIndex].src;
    };

    prevBtn.onclick = () => {
        currentIndex = (currentIndex - 1 + thumbnails.length) % thumbnails.length;
        modalImg.src = thumbnails[currentIndex].src;
    };

    window.onclick = (e) => {
        if (e.target === modal) modal.style.display = "none";
    };

    document.addEventListener("keydown", e => {
        if (e.key === "Escape") modal.style.display = "none";
        if (e.key === "ArrowRight") nextBtn.onclick();
        if (e.key === "ArrowLeft") prevBtn.onclick();
    });

});