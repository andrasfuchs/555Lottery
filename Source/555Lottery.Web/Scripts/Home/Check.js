"use strict";

$(document).ready(function () {
    $("div.ticketarea").each(
        function (i, ta)
        {
            refreshTicketSidebar(ta.id, true);

            $(ta).find("div.ticketsidebar").find("div.sidebarticket").first().trigger("click");
        });

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