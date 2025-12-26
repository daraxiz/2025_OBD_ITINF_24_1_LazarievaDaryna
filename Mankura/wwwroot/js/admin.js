document.addEventListener("DOMContentLoaded", () => {
    const adminBtn = document.getElementById("adminBtn");
    const adminPanel = document.getElementById("adminPanel");
    const adminWrapper = document.getElementById("adminWrapper");

    if (!adminBtn || !adminPanel) return;

    adminBtn.addEventListener("click", () => {
        window.location.href = "/Admin";
    });

    adminWrapper.addEventListener("mouseenter", () => {
        adminPanel.classList.add("open");
    });

    adminWrapper.addEventListener("mouseleave", () => {
        adminPanel.classList.remove("open");
    });
});