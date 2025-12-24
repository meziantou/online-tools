// Initialize theme before page loads to prevent flash
(function() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
})();

function toggleTheme() {
    const root = document.documentElement;
    const currentTheme = root.getAttribute('data-theme') || 'light';
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    
    root.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateThemeIcon(newTheme);
}

function updateThemeIcon(theme) {
    const lightIcon = document.getElementById('theme-icon-light');
    const darkIcon = document.getElementById('theme-icon-dark');
    
    if (lightIcon && darkIcon) {
        if (theme === 'dark') {
            lightIcon.style.display = 'block';
            darkIcon.style.display = 'none';
        } else {
            lightIcon.style.display = 'none';
            darkIcon.style.display = 'block';
        }
    }
}

// Update icon on page load
document.addEventListener('DOMContentLoaded', function() {
    const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
    updateThemeIcon(currentTheme);
});

// Also update when Blazor re-renders
window.addEventListener('load', function() {
    const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
    updateThemeIcon(currentTheme);
});
