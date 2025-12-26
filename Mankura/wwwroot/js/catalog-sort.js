document.addEventListener("DOMContentLoaded", () => {
    const sortSelect = document.getElementById("catalogSort");
    const grid = document.querySelector(".manga-grid");

    if (!sortSelect || !grid) return;

    sortSelect.addEventListener("change", () => {
        const cards = Array.from(grid.querySelectorAll(".manga-card"));
        const mode = sortSelect.value;

        let sortedCards;

        if (mode === "name") {
            sortedCards = cards.sort((a, b) =>
                a.dataset.title.localeCompare(b.dataset.title)
            );
        } else {
            sortedCards = cards.sort((a, b) =>
                Number(b.dataset.date) - Number(a.dataset.date)
            );
        }

        sortedCards.forEach(card => grid.appendChild(card));
    });
});
