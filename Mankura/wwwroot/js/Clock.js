document.addEventListener("DOMContentLoaded", () => {
    const clock = document.getElementById("clock");

    if (!clock) return;

    setInterval(() => {
        clock.textContent = new Date().toLocaleTimeString();
    }, 1000);
});
