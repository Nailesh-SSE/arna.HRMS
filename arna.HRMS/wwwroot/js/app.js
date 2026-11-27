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

window.hrmsSyncAutofill = function () {
    try {
        var forms = document.querySelectorAll('form');
        forms.forEach(function (form) {
            var inputs = form.querySelectorAll('input:not([type=hidden]):not([type=submit]):not([type=button])');
            inputs.forEach(function (input) {
                if (input.value && input.value.trim() !== '') {
                    input.dispatchEvent(new Event('input', { bubbles: true }));
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                }
            });
        });
    } catch (e) {
        console.error('hrmsSyncAutofill error:', e);
    }
};

window.hrmsGetLoginCredentials = function () {
    try {
        var u = document.getElementById('login-username');
        var p = document.getElementById('login-password');
        return {
            username: u ? u.value : '',
            password: p ? p.value : ''
        };
    } catch (e) {
        return { username: '', password: '' };
    }
};

// Auto-detect browser autofill across all forms
(function () {
    function runAutofillCheck() {
        if (typeof window.hrmsSyncAutofill === 'function') {
            window.hrmsSyncAutofill();
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            setTimeout(runAutofillCheck, 50);
            setTimeout(runAutofillCheck, 200);
            setTimeout(runAutofillCheck, 600);
            setTimeout(runAutofillCheck, 1200);
        });
    } else {
        setTimeout(runAutofillCheck, 50);
        setTimeout(runAutofillCheck, 200);
        setTimeout(runAutofillCheck, 600);
        setTimeout(runAutofillCheck, 1200);
    }

    // Intercept user interactions in capture phase to sync inputs before blur or submit handlers run
    ['pointerdown', 'click', 'keydown', 'focusin', 'focusout'].forEach(function (evt) {
        document.addEventListener(evt, function (e) {
            var target = e.target;
            if (target && target.tagName === 'INPUT') {
                if (target.value && target.value.trim() !== '') {
                    target.dispatchEvent(new Event('input', { bubbles: true }));
                    target.dispatchEvent(new Event('change', { bubbles: true }));
                }
            } else if (target && (target.type === 'submit' || target.closest('button') || target.tagName === 'BUTTON')) {
                runAutofillCheck();
            }
        }, { capture: true, passive: true });
    });
})();
