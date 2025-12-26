document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("deleteUserModal");
    const userIdInput = document.getElementById("deleteUserId");
    const cancelBtn = document.getElementById("cancelDeleteUser");

    document.querySelectorAll(".admin-delete-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            userIdInput.value = btn.dataset.userId;
            modal.style.display = "flex";
        });
    });

    cancelBtn?.addEventListener("click", () => {
        modal.style.display = "none";
        userIdInput.value = "";
    });

    const adminWrapper = document.getElementById("adminWrapper");
    const adminPanel = document.getElementById("adminPanel");

    if (adminWrapper && adminPanel) {

        let hoverTimeout;

        adminWrapper.addEventListener("mouseenter", () => {
            clearTimeout(hoverTimeout);
            adminPanel.style.display = "flex";
        });

        adminWrapper.addEventListener("mouseleave", () => {
            hoverTimeout = setTimeout(() => {
                adminPanel.style.display = "none";
            }, 150);
        });

    }

});
