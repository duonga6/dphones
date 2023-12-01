$(".other-product-rate").each((i, e) => {
    let rate = Number($(e).data("rate"));
    if (rate == 0) rate = NaN;
    const rateHtml = generateStarRateHtml(rate);
    $(e).html(rateHtml);
});