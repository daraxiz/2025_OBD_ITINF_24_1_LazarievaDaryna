document.addEventListener("DOMContentLoaded", () => {

    const sortSelect = document.getElementById("catalogSort");
    if (sortSelect) {
        let grid = document.querySelector(".manga-grid");
        if (!grid) {
            const firstCard = document.querySelector(".manga-card");
            grid = firstCard ? firstCard.parentElement : null;
        }

        const getCards = () => Array.from(grid.querySelectorAll(".manga-card"));

        const applySort = () => {
            const cards = getCards();
            const mode = sortSelect.value;

            const sorted = cards.sort((a, b) => {
                const aTitle = (a.dataset.title || "").toLowerCase();
                const bTitle = (b.dataset.title || "").toLowerCase();

                const aDate = Number(a.dataset.date || 0);
                const bDate = Number(b.dataset.date || 0);

                if (mode === "name") return aTitle.localeCompare(bTitle);
                return bDate - aDate;
            });

            sorted.forEach(c => grid.appendChild(c));
        };

        sortSelect.addEventListener("change", applySort);
    }


console.log("COMMENT.JS — READY");

let deleteId = null;
let deleteMangaId = null;

const modal = document.getElementById("deleteConfirmModal");
const cancelBtn = document.getElementById("cancelDelete");
const confirmBtn = document.getElementById("confirmDelete");

document.addEventListener("click", e => {

    const btn = e.target.closest(".comment-delete-btn");
    if (!btn) return;

    e.preventDefault();

    deleteId = btn.dataset.commentId;
    deleteMangaId = btn.dataset.mangaId;

    console.log("DELETE CLICK:", deleteId, deleteMangaId);

    modal.style.display = "flex";
    modal.classList.add("active");
});

cancelBtn?.addEventListener("click", () => {
    modal.classList.remove("active");
    modal.style.display = "none";
});

confirmBtn?.addEventListener("click", async () => {

    const token = document.querySelector("input[name=__RequestVerificationToken]")?.value;

    console.log("SEND DELETE", deleteId, deleteMangaId);

    const res = await fetch("/Comment/Delete", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token
        },
        body: JSON.stringify({
            id: Number(deleteId),
            mangaId: Number(deleteMangaId)
        })
    });

    if (res.ok) {
        console.log("DELETE OK");
        location.reload();
    } else {
        console.log("DELETE FAIL");
        alert("Failed to delete comment");
    }
});

    let deleteChapterId = null;
    let deleteChapterMangaId = null;

    const chapterModal = document.getElementById("deleteChapterModal");

    document.addEventListener("click", e => {

        const btn = e.target.closest(".chapter-delete-btn");
        if (!btn) return;

        e.preventDefault();

        deleteChapterId = btn.dataset.chapterId;
        deleteChapterMangaId = btn.dataset.mangaId;

        chapterModal.style.display = "flex";
    });

    document.getElementById("cancelChapterDelete")
        ?.addEventListener("click", () => {
            chapterModal.style.display = "none";
        });

    document.getElementById("confirmChapterDelete")
        ?.addEventListener("click", async () => {

            const token = document
                .querySelector("input[name=__RequestVerificationToken]")
                ?.value;

            const res = await fetch("/Chapter/Delete", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify({
                    id: Number(deleteChapterId),
                    mangaId: Number(deleteChapterMangaId)
                })
            });

            if (res.ok) location.reload();
            else alert("Failed to delete chapter");
        });
});
