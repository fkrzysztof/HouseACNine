document.addEventListener('DOMContentLoaded', () => {

    const overlay = document.getElementById('ratingOverlay');
    const inline = document.getElementById('ratingInline');

    window.addEventListener('scroll', () => {

        const trigger = window.innerHeight * 0.5;

        if (window.scrollY > trigger) {
            overlay.style.opacity = "0";
            overlay.style.pointerEvents = "none";

            inline.classList.remove('hidden');
        } else {
            overlay.style.opacity = "1";
            overlay.style.pointerEvents = "auto";

            inline.classList.add('hidden');
        }
    });

});