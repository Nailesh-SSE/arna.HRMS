let histogramChart;
let statusChart;

/* =========================================
   HELPER FUNCTIONS
========================================= */

function destroyChart(chart) {
    if (chart)
        chart.destroy();
}

function timeToHours(time) {

    if (!time)
        return 0;

    let parts = time.split(":");

    return parseInt(parts[0]) +
        parseInt(parts[1]) / 60;
}


/* =========================================
   1️⃣ HISTOGRAM CHART
   Employee vs WorkHours / Break / Total
========================================= */

/* ================================
   DESTROY CHART
================================ */

window.destroyHistogramChart = function () {

    if (histogramChart) {
        histogramChart.destroy();
        histogramChart = null;
    }

};

/* ================================
   HISTOGRAM CHART
================================ */

window.drawHistogramChart = function (data) {

    const canvas = document.getElementById("histogramChart");
    if (!canvas) return;

    const ctx = canvas.getContext("2d");

    if (histogramChart) {
        histogramChart.destroy();
        histogramChart = null;
    }

    if (!data || data.length === 0) return;


    /* ===============================
       GROUP DATA
    =============================== */

    const grouped = {};

    data.forEach(x => {

        const name = x.employeeName || "Unknown";

        if (!grouped[name]) {
            grouped[name] = {
                working: 0,
                break: 0,
                total: 0
            };
        }

        grouped[name].working += timeToHours(x.workingHours);
        grouped[name].break += timeToHours(x.breakDuration);
        grouped[name].total += timeToHours(x.totalHours);

    });


    const labels = Object.keys(grouped);

    const working = labels.map(e => grouped[e].working);
    const breakData = labels.map(e => grouped[e].break);
    const total = labels.map(e => grouped[e].total);

    const maxValue = Math.max(...working, ...breakData, ...total);


    /* ===============================
       CREATE CHART
    =============================== */

    histogramChart = new Chart(ctx, {

        type: "bar",

        data: {

            labels: labels,

            datasets: [

                {
                    label: "Working Hours",
                    data: working,
                    backgroundColor: "#2ecc71",
                    borderRadius: 4,
                    barPercentage: 0.55,
                    categoryPercentage: 0.6
                },

                {
                    label: "Break Duration",
                    data: breakData,
                    backgroundColor: "#f39c12",
                    borderRadius: 4,
                    barPercentage: 0.55,
                    categoryPercentage: 0.6
                },

                {
                    label: "Total Hours",
                    data: total,
                    backgroundColor: "#3498db",
                    borderRadius: 4,
                    barPercentage: 0.55,
                    categoryPercentage: 0.6
                }

            ]

        },

        options: {

            responsive: true,
            maintainAspectRatio: false,

            plugins: {

                legend: {
                    position: "top",
                    labels: {
                        boxWidth: 14,
                        font: {
                            size: 13
                        }
                    }
                },

                tooltip: {

                    callbacks: {

                        label: function (context) {

                            const time = hoursToHMS(context.raw);

                            return context.dataset.label + ": " + time;

                        }

                    }

                }

            },

            scales: {

                x: {

                    title: {
                        display: true,
                        text: "Employees",
                        font: {
                            weight: "bold"
                        }
                    },

                    grid: {
                        display: false
                    }

                },

                y: {

                    beginAtZero: true,
                    max: maxValue + 1,

                    title: {
                        display: true,
                        text: "Hours"
                    },

                    ticks: {
                        callback: function (value) {
                            return hoursToHMS(value);
                        }
                    }

                }

            }

        }

    });

};



/* ================================
   TIME PARSER
================================ */

function timeToHours(time) {

    if (!time) return 0;

    if (typeof time === "number") return time;

    let parts = time.split(":");

    return parseInt(parts[0]) + (parseInt(parts[1]) / 60) + (parseInt(parts[2]) / 3600);

}



/* ================================
   HOURS → HH:mm:ss FORMAT
================================ */

function hoursToHMS(hours) {

    const totalSeconds = Math.round(hours * 3600);

    const h = Math.floor(totalSeconds / 3600);
    const m = Math.floor((totalSeconds % 3600) / 60);
    const s = totalSeconds % 60;

    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;

}
/* =========================================
   STATUS BAR CHART
   Employee vs Attendance Status Count
========================================= */

window.drawStatusBarChart = function (data) {

    const canvas = document.getElementById("statusChart");
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    if (statusChart) {
        statusChart.destroy();
        statusChart = null;
    }

    if (!data || data.length === 0) return;


    /* ===============================
       GROUP DATA BY EMPLOYEE
    =============================== */

    const grouped = {};

    data.forEach(x => {

        const name = x.employeeName || "Unknown";

        if (!grouped[name]) {
            grouped[name] = {
                present: 0,
                absent: 0,
                late: 0,
                halfday: 0,
                leave: 0
            };
        }

        const status = Number(x.attendanceStatus);

        switch (status) {
            case 1: grouped[name].present++; break;
            case 2: grouped[name].absent++; break;
            case 3: grouped[name].late++; break;
            case 4: grouped[name].halfday++; break;
            case 5: grouped[name].leave++; break;
        }

    });


    const labels = Object.keys(grouped);

    const present = labels.map(e => grouped[e].present);
    const absent = labels.map(e => grouped[e].absent);
    const late = labels.map(e => grouped[e].late);
    const halfday = labels.map(e => grouped[e].halfday);
    const leave = labels.map(e => grouped[e].leave);

    const maxData = Math.max(...present, ...absent, ...late, ...halfday, ...leave);


    /* ===============================
       CREATE CHART
    =============================== */

    statusChart = new Chart(ctx, {

        type: "bar",

        data: {

            labels: labels,

            datasets: [

                {
                    label: "Present",
                    data: present,
                    backgroundColor: "#2ecc71",
                    borderRadius: 4
                },

                {
                    label: "Absent",
                    data: absent,
                    backgroundColor: "#e74c3c",
                    borderRadius: 4
                },

                {
                    label: "Late",
                    data: late,
                    backgroundColor: "#f39c12",
                    borderRadius: 4
                },

                {
                    label: "Half Day",
                    data: halfday,
                    backgroundColor: "#3498db",
                    borderRadius: 4
                },

                {
                    label: "On Leave",
                    data: leave,
                    backgroundColor: "#9b59b6",
                    borderRadius: 4
                }

            ]

        },

        options: {

            responsive: true,
            maintainAspectRatio: false,

            layout: {
                padding: {
                    top: 10,
                    bottom: 10
                }
            },

            plugins: {

                legend: {
                    position: "top",
                    labels: {
                        boxWidth: 18,
                        font: {
                            size: 13,
                            weight: "500"
                        }
                    }
                },

                tooltip: {
                    backgroundColor: "#2c3e50",
                    titleFont: {
                        size: 14
                    },
                    bodyFont: {
                        size: 13
                    }
                }

            },

            scales: {

                x: {

                    title: {
                        display: true,
                        text: "Employees",
                        font: {
                            size: 14,
                            weight: "bold"
                        }
                    },

                    grid: {
                        display: false
                    }

                },

                y: {

                    beginAtZero: true,
                    max: maxData + 1,

                    title: {
                        display: true,
                        text: "Attendance Count",
                        font: {
                            size: 14,
                            weight: "bold"
                        }
                    },

                    ticks: {
                        precision: 0
                    },

                    grid: {
                        color: "#ecf0f1"
                    }

                }

            }

        }

    });

};


/* ===============================
   DESTROY CHART
=============================== */

window.destroyStatusChart = function () {

    if (statusChart) {
        statusChart.destroy();
        statusChart = null;
    }

};