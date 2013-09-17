"use strict";

$(document).ready(function () {
    setInterval(function () {
        $.ajax({
            url: urlGetUtcTimeStr,
            type: "POST",
            dataType: 'html',
            success: function (data) {
                $("div#utcclock").html(data);
            }
        });
    }, 60000);
});