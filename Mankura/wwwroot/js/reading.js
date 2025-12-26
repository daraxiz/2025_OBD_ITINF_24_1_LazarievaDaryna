function addToReading(btn) {
    const mangaId = btn.dataset.mangaId;

    fetch('/ReadingProcess/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `mangaId=${mangaId}`
    })
        .then(() => {
            btn.innerText = 'Added ✓';
            btn.disabled = true;
        });
}
