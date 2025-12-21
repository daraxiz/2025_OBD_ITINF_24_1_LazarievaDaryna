document.addEventListener('DOMContentLoaded', () => {

    const filterTitles = document.querySelectorAll('.filter-group-title');

    filterTitles.forEach(title => {
        title.addEventListener('click', () => {
            const filterGroup = title.closest('.filter-group');
            if (filterGroup) {
                filterGroup.classList.toggle('active');
            }
        });
    });

    const resetButton = document.querySelector('.filter-buttons button:not(.apply)');
    const applyButton = document.querySelector('.filter-buttons .apply');

    if (resetButton) {
        resetButton.addEventListener('click', () => {
            const sidebar = document.querySelector('.filter-sidebar');
            if (sidebar) {
                sidebar
                    .querySelectorAll('input[type="checkbox"], input[type="radio"]')
                    .forEach(input => input.checked = false);
            }
        });
    }

    if (applyButton) {
        applyButton.addEventListener('click', () => {
            console.log('Filters applied!');
        });
    }

    const tabs = document.querySelectorAll('.tab');
    const description = document.querySelector('.manga-description');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');

            if (description) {
                description.style.display =
                    tab.textContent.trim() === 'Chapters' ? 'none' : 'block';
            }
        });
    });

    const authForm = document.querySelector('.auth-form');
    if (authForm) {
        authForm.addEventListener('submit', (e) => {
            const passwordInput = authForm.querySelector('input[type="password"]');
            if (passwordInput && passwordInput.value.length < 6) {
                e.preventDefault();
                alert('Password must be at least 6 characters long!');
            }
        });
    }

    const logoutBtn = document.getElementById('logoutBtn');
    const logoutModal = document.getElementById('logoutModal');
    const cancelLogout = document.getElementById('cancelLogout');

    if (logoutBtn && logoutModal) {
        logoutBtn.addEventListener('click', () => {
            logoutModal.style.display = 'flex';
        });
    }

    if (cancelLogout) {
        cancelLogout.addEventListener('click', () => {
            logoutModal.style.display = 'none';
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        const logoutBtn = document.getElementById('logoutBtn');

        if (logoutBtn) {
            logoutBtn.addEventListener('click', () => {
                window.location.href = '/Account/Logout';
            });
        }
    });

});

document.addEventListener('DOMContentLoaded', () => {

    const openBtn = document.getElementById('openSettings');
    const closeBtn = document.getElementById('closeSettings');
    const panel = document.getElementById('profileSettings');

    if (openBtn && panel) {
        openBtn.addEventListener('click', () => {
            panel.classList.add('active');
        });
    }

    if (closeBtn && panel) {
        closeBtn.addEventListener('click', () => {
            panel.classList.remove('active');
        });
    }

});