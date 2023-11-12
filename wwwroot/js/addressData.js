$(document).ready(function () {
    const city = $("#City").get(0);
    const district = $("#District").get(0);
    const commune = $("#Commune").get(0);

    const renderCity = function (data) {
        for (const x of data) {
            city.options[city.options.length] = new Option(x.Name, x.Name);
        }

        city.onchange = function () {
            district.length = 1;
            commune.length = 1;
            if (this.value != "") {
                const result = data.filter(n => n.Name === this.value);
                for (const k of result[0].Districts) {
                    district.options[district.options.length] = new Option(k.Name, k.Name);
                }
            }
        }
        district.onchange = function () {
            commune.length = 1;
            if (this.value != "") {
                const dataCity = data.filter(n => n.Name === city.value);
                const dataCommune = dataCity[0].Districts.filter(n => n.Name === this.value)[0].Wards;
                for (const ward of dataCommune) {
                    commune.options[commune.options.length] = new Option(ward.Name, ward.Name);
                }
            }
        }
    }

    $.ajax({
        type: 'get',
        url: 'https://raw.githubusercontent.com/kenzouno1/DiaGioiHanhChinhVN/master/data.json',
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            renderCity(JSON.parse(data));
        }
    })

    $(document).ready(function () {
        const onlinePayNote = $("#online-pay-note");
        $(".inpPayType").change(function () {
            if ($(this).val() == 'Online')
                onlinePayNote.show();
            else
                onlinePayNote.hide();
        });
    });

});