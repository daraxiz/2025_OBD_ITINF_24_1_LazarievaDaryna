document.addEventListener("DOMContentLoaded", () => {
    const checkboxes = document.querySelectorAll('.bookmark-item input');
    const cards = document.querySelectorAll('.manga-card');
    const searchInput = document.getElementById('searchInput');

    function filter() {
        const activeTypes = [...checkboxes]
            .filter(cb => cb.checked)
            .map(cb => cb.value);

        const query = searchInput?.value.toLowerCase() || '';

        cards.forEach(card => {
            const type = (card.dataset.reading || "").trim();

            const title = card.dataset.title;

            const typeMatch =
                activeTypes.length === 0 || activeTypes.includes(type);

            const searchMatch =
                title.includes(query);

            card.style.display =
                (typeMatch && searchMatch) ? 'block' : 'none';
        });
    }

    checkboxes.forEach(cb =>
        cb.addEventListener('change', filter)
    );

    searchInput?.addEventListener('input', filter);
});