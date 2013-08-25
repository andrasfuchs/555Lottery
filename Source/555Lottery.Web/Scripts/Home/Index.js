"use strict";

var currentLang = 'eng';
var ticketMode = 'blue';
var minimumSelectedNumbers = 5;
var maximumSelectedNumbers = 5;
var secondsToNextDraw = -1;
var selectedTicketIndex = -1;
var processingRandom = false;

$(document).ready(function () {
    nextDrawTimerEvent();

    $('#checkbutton').click(function (e) {
        e.preventDefault();

        $('#checkModal').reveal({
            animation: 'fade'
        });
    });
    
    $("input#checkticketlotcode").keyup(function (e) {
        if ($("input#checkticketlotcode")[0].value.length !== 11)
        {
            $("div#gobutton").addClass("disabled");
        } else {
            $("div#gobutton").removeClass("disabled");
        }

        $("div#gobutton a")[0].href = "/check/" + $("input#checkticketlotcode")[0].value;
    });

    $("input#notescheckbox").click(function () {
        if ($(this).is(':checked')) {
            $("div#paybutton").removeClass("disabled");
        } else {
            $("div#paybutton").addClass("disabled");
        }
    });
});

function nextDrawTimerEvent() {
    if (secondsToNextDraw <= 0) {
        if (secondsToNextDraw < 0) {
            setInterval(nextDrawTimerEvent, 1000);
        }

        // initialize the counter
        $.ajax({
            url: urlGetTimeLeftToNextDraw,
            type: "POST",
            dataType: 'html',
            data: {},
            success: function (data) {
                secondsToNextDraw = data;
            }
        });

    } else {
        secondsToNextDraw--;
    }

    if (secondsToNextDraw >= 0) {

        var sec = -1;

        if ($("img#clocksec10").length > 0) {
            sec = parseInt($("img#clocksec10")[0].alt + $("img#clocksec1")[0].alt, 10);
            sec -= 1;
        }

        if (sec < 0) {
            $.ajax({
                url: urlTimeLeft,
                type: "POST",
                dataType: 'html',
                data: {
                    secondsToNextDraw: secondsToNextDraw
                },
                success: function (data) {
                    $('div#clocknumber').html(data);
                }
            });
        } else {
            var newSec10 = Math.floor(sec / 10);
            var newSec1 = sec % 10;

            $("img#clocksec10")[0].src = $("img#clocksec10")[0].src.replace("_" + $("img#clocksec10")[0].alt + ".png", "_" + newSec10 + ".png");
            $("img#clocksec1")[0].src = $("img#clocksec1")[0].src.replace("_" + $("img#clocksec1")[0].alt + ".png", "_" + newSec1 + ".png");

            $("img#clocksec10")[0].alt = newSec10;
            $("img#clocksec1")[0].alt = newSec1;
        }
        
    }
}

function isToggled(t) {
    return (t.className.match(/(?:^|\s)toggled(?!\S)/)) !== null;
}

function toggle(t, state) {
    var toggled = isToggled(t);

    if (!toggled && ((state === undefined) || (state))) {
        $(t).addClass("toggled");

        $(t).find("img").each(function (index, img) {

            if (img.src.indexOf(ticketMode) >= 0) {
                img.src = img.src.replace(ticketMode, 'black');
            } else {
                img.src = img.src.replace(".png", "_toggled.png");
            }
        });
    }

    if (toggled && ((state === undefined) || (!state))) {
        // remove it
        $(t).removeClass("toggled");

        $(t).find("img").each(function (index, img) {

            if (img.src.indexOf('black') >= 0) {
                img.src = img.src.replace('black', ticketMode);
            } else {
                img.src = img.src.replace('_toggled', '');
            }
        });
    }
}

function selectedNumbersCount() {
    var selectedNumbersCountResult = 0;
    $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectedNumbersCountResult++;
    });

    return selectedNumbersCountResult;
}

function selectedJokersCount() {
    var selectedJokersCountResult = 0;
    $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div) && (div.id !== "jokerall")) selectedJokersCountResult++;
    });

    return selectedJokersCountResult;
}

function selectedTypesCount() {
    var selectedTypesCountResult = 0;

    if ($("div#tickettype").hasClass("hidden")) return null;

    $("div#tickettype").find("div.typebutton").each(function (index, div) {
        if (isToggled(div)) selectedTypesCountResult++;
    });

    return selectedTypesCountResult;
}

function selectNumber(t, donotrefreshgui) {
    var snc = selectedNumbersCount();

    if ((t !== undefined) && ((snc < maximumSelectedNumbers) || (isToggled(t)))) {
        toggle(t);
    }

    if (!donotrefreshgui) {
        refreshButtonStates();
    }
}

function refreshButtonStates() {
    var snc = selectedNumbersCount();

    if ((snc === 0) && (selectedJokersCount() === 0)) {
        $("div#clearbutton").addClass("disabled");
    } else {
        $("div#clearbutton").removeClass("disabled");
    }

    if (((snc < minimumSelectedNumbers) || (snc > maximumSelectedNumbers) || ((selectedTypesCount() !== null) && (selectedTypesCount() === 0)))
        || ((selectedJokersCount() === 0) && ((ticketMode !== 'green') || (selectedTicketIndex !== -1)))) {
        $("div#acceptbutton").addClass("disabled");
    } else {
        $("div#acceptbutton").removeClass("disabled");
    }

    if (ticketMode === 'green')
    {
        $("div#randombutton").addClass("disabled");
    } else {
        $("div#randombutton").removeClass("disabled");
    }
}

function refreshLetPlayButtonState(totalGames) {
    if (totalGames === 0) {
        $("div#letsplay").addClass("disabled");
    } else {
        $("div#letsplay").removeClass("disabled");
    }
}

function generateTicketType() {
    var type = "";

    if (ticketMode === "orange") {
        type = "S";
    } else if (ticketMode === "green") {
        type = "R";
    } else {
        type = "N";
    }

    if ((type === "S") || (type === "R")) {
        $("div#tickettype").find("div.typebutton").each(function (index, div) {
            if (isToggled(div)) type += (index + 1);
        });
    }

    if (type.length === 1)
    {
        type += "0";
    }

    return type;
}

function generateTicketSequence() {
    // display the selected number on the bottom of the ticket
    var text = "";
    var selectedCount = 0;
    $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) {
            text += div.firstElementChild.alt + ",";
            selectedCount++;
        }
    });

    // add missing commas to keep the bottom display of numbers consistent
    while (selectedCount < minimumSelectedNumbers - 1) {
        text += ",";
        selectedCount++;
    }

    // remove the last comma
    if (selectedCount >= minimumSelectedNumbers) {
        text = text.substring(0, text.length - 1);
    }

    text += "|";

    $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) text += div.firstElementChild.alt + ",";
    });

    // remove the last comma
    while (text[text.length - 1] === ',') {
        text = text.substring(0, text.length - 1);
    }

    return text;
}

function refreshTicketBottom() {
    $.ajax({
        url: urlTicketBottom,
        type: "POST",
        dataType: 'html',
        data: {
            text: generateTicketSequence()
        },
        success: function (data) {
            $('div#ticketfooterlist').html(data);
        }
    });
}

function refreshTicketPrice() {
    $.ajax({
        url: urlTicketPrice,
        type: "POST",
        dataType: 'html',
        data: {
            ticketType: generateTicketType(),
            ticketSequence: generateTicketSequence()
        },
        success: function (data) {
            $('div#ticketfooterbtc').html(data);
        }
    });
}

function refreshTotalGames() {
    $.ajax({
        url: urlTotalGames,
        type: "POST",
        dataType: 'html',
        data: {},
        success: function (data) {
            $('div#totalgames').html(data);
        }
    });
}

function refreshTotalPrice() {
    $.ajax({
        url: urlTotalPrice,
        type: "POST",
        dataType: 'html',
        data: {},
        success: function (data) {
            $('div#totalprice').html(data);
        }
    });
}

function clearButtonClick(t) {
    if (selectedTicketIndex !== -1) {
        $.ajax({
            url: urlDeleteTicket,
            type: "POST",
            dataType: 'json',
            data: {
                ticketIndex: selectedTicketIndex
            },
            success: function (data) {
                selectTicket(data[0]);
                refreshTotalGames();
                refreshTotalPrice();
                refreshLetPlayButtonState(data[1]);
            }
        });
    }

    clearTicket();

    refreshTicketBottom();
}

function clearTicket() {
    selectedTicketIndex = -1;

    $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectNumber(div);
    });

    $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectNumber(div);
    });
}

function randomButtonClick(t) {

    if (processingRandom) return;

    processingRandom = true;
    $("div#randombutton").addClass("disabled");

    clearTicket();

    var number = 0;

    while (true) {
        if (selectedNumbersCount() >= minimumSelectedNumbers) {
            break;
        }

        number = (Math.random() * 55);

        $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
            if ((index <= number) && (index + 1 >= number)) selectNumber(div);
        });
    }

    number = (Math.random() * 5);

    while (selectedJokersCount() === 0) {
        $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
            if ((index <= number) && (index + 1 >= number)) jokerClicked(div);
        });
    }

    refreshTicketBottom();

    setTimeout(function(){
        $("div#randombutton").removeClass("disabled");
        processingRandom = false;
    }, 500);
}

function acceptButtonClick(t) {
    $.ajax({
        url: urlAcceptTicket,
        type: "POST",
        dataType: 'json',
        data: {
            ticketType: generateTicketType(),
            ticketSequence: generateTicketSequence(),
            overwriteTicketIndex: selectedTicketIndex
        },
        success: function (data) {
            clearTicket();
            selectTicket(data[0]);
            refreshTotalGames();
            refreshTotalPrice();
            refreshLetPlayButtonState(data[1]);
        }
    });
}


function allClicked(t) {
    $(t).parent().find("div.ticketnumber").each(function (index, div) {
        toggle(div, true);
    });

    refreshTicketBottom();
    refreshTicketPrice();
    refreshButtonStates();
}

function jokerClicked(t, donotrefreshgui) {
    toggle(t);

    var allToggled = true;

    $(t).parent().find("div.ticketnumber[id!='jokerall']").each(function (index, div) {
        if (!isToggled(div)) allToggled = false;
    });

    toggle($(t).parent().find("div#jokerall")[0], allToggled);

    if (!donotrefreshgui) {
        refreshTicketBottom();
        refreshTicketPrice();
        refreshButtonStates();
    }
}

function changeLang(t, lang) {
    $(t).parent().find("div.langbutton").each(function (index, div) {
        toggle(div, false);
    });

    toggle(t, true);

    var oldLang = currentLang;

    $("div#rendered-content").find("img").each(function (index, img) {
        if (img.src.indexOf(oldLang) >= 0) {
            img.src = img.src.replace("_" + oldLang + "_", "_" + lang + "_");
            img.src = img.src.replace("_" + oldLang + ".", "_" + lang + ".");
        }
    });

    currentLang = lang;
}

function changeType(type) {

    if (type === 0)
    {
        $("div#tickettype").addClass("hidden");
    } else
    {
        $("div#tickettype").removeClass("hidden");
    }   

    $("div#tickettype").find("div.typebutton").each(function (index, div) {
        toggle(div, index + 1 === type);
    });

    if (type === 0)
    {
        minimumSelectedNumbers = 5;
        maximumSelectedNumbers = 5;
    } else if (ticketMode === 'orange') {
        minimumSelectedNumbers = 5 + type;
        maximumSelectedNumbers = 5 + type;
    } else if ((ticketMode === 'green') && (selectedTicketIndex === -1)) {
        minimumSelectedNumbers = 0;
        maximumSelectedNumbers = 4;
    } else {
        minimumSelectedNumbers = 5;
        maximumSelectedNumbers = 5;
    }

    refreshButtonStates();
}

function changeTab(mode) {
    if (selectedTicketIndex !== -1) return;

    toggle($("div.tabblue")[0], mode === 'blue');
    toggle($("div.taborange")[0], mode === 'orange');
    toggle($("div.tabgreen")[0], mode === 'green');
    
    changeColor($("div#ticketarea")[0], mode);
    changeColor($("div#ticketsidebar div.selectedticket")[0], mode);
    changeColor($("div.ticketsticksleft div.selectedticket")[0], mode);
    ticketMode = mode;

    if ((mode === 'orange') || (mode === 'green')) {
        changeType(1);
    } else {
        changeType(0);
    }

    clearTicket();

    selectNumber();
}

function replaceAll(find, replace, str) {
    return str.replace(new RegExp(find, 'g'), replace);
}

function changeColor(element, color) {
    if (element === undefined) return;

    if (element.id === 'ticketleftside') return;
    if (element.id === 'tabs') return;

    element.className = replaceAll('blue', color, element.className);
    element.className = replaceAll('orange', color, element.className);
    element.className = replaceAll('green', color, element.className);

    if (element.src !== undefined) {
        var newType = (color === 'orange' ? 'systemtype' : color === 'green' ? 'randomtype' : 'normaltype');

        var newElementSrc = element.src.replace('_blue', '_' + color).replace('_orange', '_' + color).replace('_green', '_' + color).replace('systemtype', newType).replace('randomtype', newType).replace('normaltype', newType);
        element.src = newElementSrc;
    }

    if (element.children !== undefined) {
        var length = element.children.length;
        for (var i = 0; i < length; i++) {
            changeColor(element.children[i], color);
        }
    }

}


function pageUp() {
    moveSidebar(-1, 0, false);
}

function pageDown() {
    moveSidebar(+1, 0, false);
}

function selectTicket(index) {
    moveSidebar(0, index, true);
}

function moveSidebar(scrollPositionChange, selectedTicketIndex, refreshTicket) {
    $.ajax({
        url: urlMoveSlider,
        type: "POST",
        dataType: 'json',
        data: {
            scrollPositionChange: scrollPositionChange,
            selectedTicketIndex: selectedTicketIndex
        },
        success: function (data) {
            refreshTicketSidebar();

            if (refreshTicket) {
                reloadTicket(data);
            }
        }
    });
}

function refreshTicketSidebar() {
    $.ajax({
        url: urlTicketSidebar,
        type: "POST",
        dataType: 'html',
        data: {},
        success: function (data) {
            $('div#ticketleftside').html(data);
        }
    });
}

function reloadTicket(ticket) {
    clearTicket();

    if (ticket.Color !== ticketMode) {
        if ((ticket.Mode === 0) || (ticket.Mode === 1)) { // empty or normal
            changeTab("blue");
        }
        if (ticket.Mode === 2) { // system
            changeTab("orange");
        }
        if (ticket.Mode === 3) { // random
            changeTab("green");
        }
    }

    selectedTicketIndex = ticket.Index;
    if (selectedTicketIndex !== -1) changeType(ticket.Type);

    $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if ($.inArray(index + 1, ticket.Numbers) > -1) selectNumber(div, true);
    });

    $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if ($.inArray(index + 1, ticket.Jokers) > -1) jokerClicked(div, true);
    });

    refreshTicketBottom();
    refreshTicketPrice();
    refreshButtonStates();
}

function changeDraws(drawsValueChange) {
    $.ajax({
        url: urlDraws,
        type: "POST",
        dataType: 'html',
        data: {
            valueChange: drawsValueChange
        },
        success: function (data) {
            $('div#draws').html(data);
        }
    });

    refreshTotalPrice();
}

function letsplay() {
    $.ajax({
        url: urlLetsPlay,
        type: "POST",
        dataType: 'json',
        data: {},
        success: function (ticketlot) {
            var amount = ticketlot.TotalBTC - ticketlot.TotalDiscountBTC;
            amount = parseFloat(Math.round(amount * 100000000) / 100000000).toFixed(8);

            $("span#payModalTicketLotCode").html(ticketlot.Code);
            $("span#payModalTicketTotalBTC").html(amount);
            $("span#payModalDrawBitCoinAddress").html(ticketlot.Draw.BitCoinAddress);

            var paymentURI = "bitcoin:" + ticketlot.Draw.BitCoinAddress + "?amount=" + amount + "&label=555%20Lottery&message=" + ticketlot.Code;
            $("a#payModelPayLink")[0].href = paymentURI;
            $("img#payqr")[0].src = "http://qrfree.kaywa.com/?l=1&s=8&d=" + $('<div/>').text(paymentURI).html();

            $('#payModal').reveal({
                animation: 'fade',
                width: 380
            });
        }
    });
}

function secondchanceclick(div)
{
    var toggled = isToggled(div);

    toggle(div);

    if (!toggled) {
        $('img.secondchancesides').animate({ opacity: 0.0 }, 1000);
    } else {
        $('img.secondchancesides').animate({ opacity: 1.0 }, 500);
    }
}