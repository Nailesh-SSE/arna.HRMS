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
