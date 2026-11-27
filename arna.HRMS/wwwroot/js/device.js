window.deviceHelper = {

    getDeviceType: function () {

        const ua = navigator.userAgent.toLowerCase();
        const width = window.innerWidth;

        let deviceType = "LaptopDesktop";

        if (
            ua.includes("iphone") ||
            ua.includes("ipod") ||
            (ua.includes("android") && ua.includes("mobile"))
        ) {
            deviceType = "Mobile";
        }

        else if (
            ua.includes("ipad") ||
            (ua.includes("android") && !ua.includes("mobile")) ||
            ua.includes("tablet")
        ) {
            deviceType = "Tablet";
        }

        else if (
            ua.includes("windows") ||
            ua.includes("macintosh") ||
            ua.includes("linux")
        ) {
            deviceType = "LaptopDesktop";
        }

        else {
            if (width <= 768) deviceType = "Mobile";
            else if (width <= 1024) deviceType = "Tablet";
            else deviceType = "LaptopDesktop";
        }

        return deviceType;
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