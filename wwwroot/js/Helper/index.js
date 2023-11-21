const generateStarRateHtml = function (rate) {
    let ratingHeading = "";
    const integerRate = parseInt(rate);
    const realPartRate = parseFloat(rate) % 1;
    for (let i = 1; i <= integerRate; i++) {
        ratingHeading += `<i class="fa-solid fa-star third-text"></i>`;
    }

    if (!isNaN(realPartRate)) {
        ratingHeading += `<span class="fa-solid fa-star text-muted position-relative" style="z-index: 1">
            <i class="fa-solid fa-star-half position-absolute start-0 end-0 third-text" style="z-index: 0"></i>
        </span>`;
        for (let i = integerRate + 2; i <= 5; i++) {
            ratingHeading += `<i class="fa-solid fa-star text-muted"></i>`;
        }
    }
    else {
        for (let i = integerRate + 1; i <= 5; i++) {
            ratingHeading += `<i class="fa-solid fa-star text-muted"></i>`;
        }
    }

    return ratingHeading;
}
