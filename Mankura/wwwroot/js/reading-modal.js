document.addEventListener("DOMContentLoaded", () => {
    const openBtn = document.getElementById("bookmarkBtn");
    const modal = document.getElementById("readingModal");
    const closeBtn = modal?.querySelector(".modal-close");

    if (!openBtn || !modal) return;

    const mangaId = openBtn.dataset.mangaId;

    openBtn.addEventListener("click", () => {
        modal.style.display = "flex";
    });

    closeBtn?.addEventListener("click", () => {
        modal.style.display = "none";
    });

    modal.querySelectorAll("button[data-type]").forEach(btn => {
        btn.addEventListener("click", async () => {
            const type = btn.dataset.type;

            const res = await fetch("/ReadingProcess/Add", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    mangaId: parseInt(mangaId),
                    type: type
                })
            });

            if (res.ok) {
                openBtn.textContent = type;
                modal.style.display = "none";
            } else {
                alert("Failed to add manga");
            }
        });
    });
});

const removeBtn = document.getElementById('removeFromReading');
const modal = document.getElementById('readingModal');
const bookmarkBtn = document.getElementById('bookmarkBtn');

removeBtn.addEventListener('click', async () => {
    const mangaId = bookmarkBtn.dataset.mangaId;

    await fetch('/ReadingProcess/Remove', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken':
                document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify(mangaId)
    });

    bookmarkBtn.textContent = 'Add to bookmarks';
    modal.classList.add('hidden');
});
