window.hrmsSetHeaderHeight = function () {
    var header = document.getElementById('hrms-header');
    if (header) {
        var h = header.getBoundingClientRect().height;
        document.documentElement.style.setProperty('--hrms-header-h', h + 'px');
    }
};

window.hrmsSetHeaderHeight();
window.addEventListener('resize', function () {
    window.hrmsSetHeaderHeight();
});

    window.clockState = {
        save: function (state) {
        localStorage.setItem('hrms_clock_state', JSON.stringify(state));
        },
    load: function () {
            var raw = localStorage.getItem('hrms_clock_state');
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
        },
    clear: function () {
        localStorage.removeItem('hrms_clock_state');
        }
    };