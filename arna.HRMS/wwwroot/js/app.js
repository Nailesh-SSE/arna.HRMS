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

window.downloadFileFromBase64 = function (fileName, base64Data, contentType) {
    try {
        contentType = contentType || 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
        var byteCharacters = atob(base64Data);
        var byteNumbers = new Array(byteCharacters.length);
        for (var i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        var byteArray = new Uint8Array(byteNumbers);
        var blob = new Blob([byteArray], { type: contentType });
        var url = URL.createObjectURL(blob);
        var link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        setTimeout(function () {
            URL.revokeObjectURL(url);
        }, 1500);
    } catch (e) {
        console.error('downloadFileFromBase64 error:', e);
    }
};