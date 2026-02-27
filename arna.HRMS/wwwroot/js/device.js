window.deviceHelper = {
    getDeviceType: function () {
        return this.getDeviceInfo().category;
    },

    getDeviceInfo: function () {

        const ua = navigator.userAgent.toLowerCase();
        const width = window.innerWidth;
        const hasTouch = navigator.maxTouchPoints > 0;
        const isFinePointer = window.matchMedia("(pointer: fine)").matches;

        // Detect iPad desktop mode (iPad pretending to be Mac)
        const isIPadDesktopMode =
            ua.includes("macintosh") && hasTouch;

        let deviceCategory = "Unknown";

        // =========================
        // 1️⃣ iOS Devices (Strict First)
        // =========================
        if (ua.includes("iphone") || ua.includes("ipod")) {
            deviceCategory = "iOS Mobile";
        }
        else if (ua.includes("ipad") || isIPadDesktopMode) {
            deviceCategory = "iOS Tablet";
        }

        // =========================
        // 2️⃣ Android Devices
        // =========================
        else if (ua.includes("android")) {
            if (ua.includes("mobile")) {
                deviceCategory = "Android Mobile";
            } else {
                deviceCategory = "Android Tablet";
            }
        }

        // =========================
        // 3️⃣ Desktop Operating Systems
        // =========================
        else if (ua.includes("windows") || ua.includes("macintosh") || ua.includes("linux")) {

            if (hasTouch) {
                deviceCategory = "Touch Laptop/Desktop";
            } else if (!hasTouch && isFinePointer) {
                deviceCategory = "Non-Touch Laptop/Desktop";
            } else {
                deviceCategory = "Laptop/Desktop";
            }
        }

        // =========================
        // 4️⃣ Fallback by screen size
        // =========================
        else {
            if (width <= 768) {
                deviceCategory = "Mobile";
            } else if (width <= 1024) {
                deviceCategory = "Tablet";
            } else {
                deviceCategory = hasTouch
                    ? "Touch Laptop/Desktop"
                    : "Non-Touch Laptop/Desktop";
            }
        }

        return {
            category: deviceCategory,
            width: width,
            hasTouch: hasTouch,
            userAgent: ua
        };
    }
};