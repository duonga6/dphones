var ctx = document.getElementById("myBarChart");
var ctx2 = document.getElementById("myBarChart2");

var myLineChart = new Chart(ctx, {
    type: 'bar',
    data: {
        labels: labels,
        datasets: [{
            label: "Doanh thu",
            backgroundColor: "rgba(2,117,216,1)",
            borderColor: "rgba(2,117,216,1)",
            data: datas,
        }],
    },
    options: {
        scales: {
            xAxes: [{
                time: {
                    unit: 'month'
                },
                gridLines: {
                    display: true
                },
                ticks: {
                    maxTicksLimit: maxLimits
                }
            }],
            yAxes: [{
                ticks: {
                    min: minColume,
                    max: maxColumn,
                    maxTicksLimit: 5
                },
                gridLines: {
                    display: true
                }
            }],
        },
        legend: {
            display: false
        }
    }
});

var myLineChart2 = new Chart(ctx2, {
    type: 'bar',
    data: {
        labels: labels2,
        datasets: [{
            label: "Lợi nhuận",
            backgroundColor: "rgba(2,117,216,1)",
            borderColor: "rgba(2,117,216,1)",
            data: datas2,
        }],
    },
    options: {
        scales: {
            xAxes: [{
                time: {
                    unit: 'month'
                },
                gridLines: {
                    display: true
                },
                ticks: {
                    maxTicksLimit: maxLimits2
                }
            }],
            yAxes: [{
                ticks: {
                    min: minColume2,
                    max: maxColumn2,
                    maxTicksLimit: 5
                },
                gridLines: {
                    display: true
                }
            }],
        },
        legend: {
            display: false
        }
    }
});


$(".product-rating").each((i, e) => {
    let rate = Number($(e).data("rate"));
    if (rate == 0) rate = NaN;
    const rateHtml = generateStarRateHtml(rate);
    $(e).html(rateHtml);
});