window.deviceHelper = {

    getDeviceType: function () {

        const ua = navigator.userAgent.toLowerCase();
        const width = window.innerWidth;

        let deviceType = "UnknownDevice";

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