document.addEventListener('DOMContentLoaded', () => {

    document.querySelectorAll('.filter-group-title')
        .forEach(title => {
            title.addEventListener('click', () => {
                title.closest('.filter-group')?.classList.toggle('active');
            });
        });

    const resetButton = document.querySelector('.filter-buttons button:not(.apply)');
    const applyButton = document.querySelector('.filter-buttons .apply');

    if (resetButton) {
        resetButton.addEventListener('click', () => {
            document.querySelectorAll('.filter-sidebar input')
                .forEach(i => i.checked = false);
        });
    }

    if (applyButton) {
        applyButton.addEventListener('click', () => {
            console.log('Filters applied!');
        });
    }

    const tabs = document.querySelectorAll(".tab-btn");
    const description = document.getElementById("tab-desc");

    tabs.forEach(btn => {
        btn.addEventListener("click", () => {

            tabs.forEach(b => b.classList.remove("active"));
            document.querySelectorAll(".tab-content")
                .forEach(c => c.classList.remove("active"));

            btn.classList.add("active");
            document.getElementById("tab-" + btn.dataset.tab).classList.add("active");
        });
    });


    const authForm = document.querySelector('.auth-form');
    if (authForm) {
        authForm.addEventListener('submit', e => {
            const pw = authForm.querySelector('input[type="password"]');
            if (pw && pw.value.length < 6) {
                e.preventDefault();
                alert('Password must be at least 6 characters long!');
            }
        });
    }


    const logoutBtn = document.getElementById('logoutBtn');
    const logoutModal = document.getElementById('logoutModal');
    const cancelLogout = document.getElementById('cancelLogout');

    if (logoutBtn && logoutModal) {
        logoutBtn.addEventListener('click', e => {
            e.preventDefault();       
            logoutModal.style.display = 'flex';
        });
    }

    if (cancelLogout) {
        cancelLogout.addEventListener('click', () => {
            logoutModal.style.display = 'none';
        });
    }


    const openBtn = document.getElementById('openSettings');
    const closeBtn = document.getElementById('closeSettings');
    const panel = document.getElementById('profileSettings');

    if (openBtn && panel) openBtn.addEventListener('click', () => panel.classList.add('active'));
    if (closeBtn && panel) closeBtn.addEventListener('click', () => panel.classList.remove('active'));

});