"use strict";

$(document).ready(function () {
    $("div.ticketsidebar").each(function (i, sb) { $(sb).find("div.sidebarticket").first().trigger("click") });

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