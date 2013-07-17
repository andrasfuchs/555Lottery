"use strict";

var currentLang = 'eng';
var ticketMode = 'blue';
var minimumSelectedNumbers = 5;
var maximumSelectedNumbers = 5;
var secondsToNextDraw = -1;
var drawNumber = 2;
var selectedTicketIndex = -1;

$(document).ready(function () {
    nextDrawTimerEvent();
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
    }
}

function isToggled(t) {
    return (t.className.match(/(?:^|\s)toggled(?!\S)/)) != null;
}

function toggle(t, state) {
    var toggled = isToggled(t);

    if (!toggled && ((state == undefined) || (state))) {
        $(t).addClass("toggled");

        $(t).find("img").each(function (index, img) {

            if (img.src.indexOf(ticketMode) >= 0) {
                img.src = img.src.replace(ticketMode, 'black');
            } else {
                img.src = img.src.replace(".png", "_toggled.png");
            }
        });
    }

    if (toggled && ((state == undefined) || (!state))) {
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
    var selectedNumbersCount = 0;
    $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectedNumbersCount++;
    });

    return selectedNumbersCount;
}

function selectedJokersCount() {
    var selectedJokersCount = 0;
    $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div) && (div.id != "jokerall")) selectedJokersCount++;
    });

    return selectedJokersCount;
}

function selectedTypesCount() {
    var selectedTypesCount = 0;

    if ($("div#tickettype")[0].style.opacity == 0) return null;

    $("div#tickettype").find("div.typebutton").each(function (index, div) {
        if (isToggled(div)) selectedTypesCount++;
    });

    return selectedTypesCount;
}

function selectNumber(t) {
    var snc = selectedNumbersCount();

    if ((t !== undefined) && ((snc < maximumSelectedNumbers) || (isToggled(t)))) {
        toggle(t);
    }

    refreshButtonStates();
}

function refreshButtonStates() {
    var snc = selectedNumbersCount();

    if (snc == 0) {
        $("div#clearbutton").addClass("disabled");
    } else {
        $("div#clearbutton").removeClass("disabled");
    }

    if ((snc < minimumSelectedNumbers) || (snc > maximumSelectedNumbers) || (selectedJokersCount() == 0) || ((selectedTypesCount() != null) && (selectedTypesCount() == 0))) {
        $("div#acceptbutton").addClass("disabled");
    } else {
        $("div#acceptbutton").removeClass("disabled");
    }
}

function generateTicketType() {
    var type = "";

    if (ticketMode == "orange") {
        type = "S";
    } else if (ticketMode == "green") {
        type = "R";
    } else {
        type = "N0";
    }

    if ((type == "S") || (type == "R")) {
        $("div#tickettype").find("div.typebutton").each(function (index, div) {
            if (isToggled(div)) type += (index + 1);
        });
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
    while (text[text.length - 1] == ',') {
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
    if (selectedTicketIndex != -1) {
        $.ajax({
            url: urlDeleteTicket,
            type: "POST",
            dataType: 'html',
            data: {
                ticketIndex: selectedTicketIndex
            },
            success: function (data) {
                selectTicket(data);
                refreshTotalGames();
                refreshTotalPrice();
            }
        });
    }

    clearTicket();
}

function clearTicket() {
    $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectNumber(div);
    });

    $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectNumber(div);
    });

    $("div#tickettype").find("div.typebutton").each(function (index, div) {
        toggle(div, false);
    });

    refreshTicketBottom();
}

function randomButtonClick(t) {
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

    while (selectedJokersCount() == 0) {
        $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
            if ((index <= number) && (index + 1 >= number)) jokerClicked(div);
        });
    }

    refreshTicketBottom();
}

function acceptButtonClick(t) {
    $.ajax({
        url: urlAcceptTicket,
        type: "POST",
        dataType: 'html',
        data: {
            ticketType: generateTicketType(),
            ticketSequence: generateTicketSequence(),
            overwriteTicketIndex: selectedTicketIndex
        },
        success: function (data) {
            clearTicket();
            selectTicket(data);
            refreshTotalGames();
            refreshTotalPrice();
        }
    });
}


function allClicked(t) {
    $(t).parent().find("div.ticketnumber").each(function (index, div) {
        toggle(div, true);
    });

    refreshTicketBottom();
    refreshTicketPrice();
}

function jokerClicked(t) {
    toggle(t);

    var allToggled = true;

    $(t).parent().find("div.ticketnumber[id!='jokerall']").each(function (index, div) {
        if (!isToggled(div)) allToggled = false;
    });

    toggle($(t).parent().find("div#jokerall")[0], allToggled);

    refreshTicketBottom();
    refreshTicketPrice();
    refreshButtonStates();
}

function changeLang(t, lang) {
    $(t).parent().find("div.langbutton").each(function (index, div) {
        toggle(div, false);
    });

    toggle(t, true);

    var oldLang = currentLang;

    $("div#rendered-content").find("img").each(function (index, img) {
        if (img.src.indexOf(oldLang) >= 0) {
            img.src = img.src.replace("_" + oldLang, "_" + lang);
        }
    });

    currentLang = lang;
}

function changeType(type) {
    $("div#tickettype")[0].style.opacity = (type == 0 ? 0 : 1);

    $("div#tickettype").find("div.typebutton").each(function (index, div) {
        toggle(div, index + 1 == type);
    });

    refreshButtonStates();
}

function changeTab(mode) {

    toggle($("div.tabblue")[0], mode == 'blue');
    toggle($("div.taborange")[0], mode == 'orange');
    toggle($("div.tabgreen")[0], mode == 'green');

    ticketMode = mode;

    changeType(0);

    if (mode == 'orange') {
        changeType(1);
        $("div#tickettype img").each(function (index, img) {
            img.src = img.src.replace("random", "system");
        });
    }

    if (mode == 'green') {
        changeType(1);
        $("div#tickettype img").each(function (index, img) {
            img.src = img.src.replace("system", "random");
        });
    }

    changeColor($("div#ticketarea")[0], mode);

    if (mode == 'orange') {
        minimumSelectedNumbers = 6;
        maximumSelectedNumbers = 10;
    } else if ((mode == 'green') && (selectedTicketIndex == -1)) {
        minimumSelectedNumbers = 0;
        maximumSelectedNumbers = 4;
    } else {
        minimumSelectedNumbers = 5;
        maximumSelectedNumbers = 5;
    }

    selectNumber();
}

function replaceAll(find, replace, str) {
    return str.replace(new RegExp(find, 'g'), replace);
}

function changeColor(element, color) {
    if (element == undefined) return;

    if (element.id == 'ticketsidebar') return;
    if (element.id == 'tabs') return;

    element.className = replaceAll('blue', color, element.className);
    element.className = replaceAll('orange', color, element.className);
    element.className = replaceAll('green', color, element.className);

    if (element.src !== undefined) {
        element.src = element.src.replace('_blue', '_' + color).replace('_orange', '_' + color).replace('_green', '_' + color);
    }

    if (element.children !== undefined) {
        var length = element.children.length;
        for (var i = 0; i < length; i++) {
            changeColor(element.children[i], color);
        }
    }

}


function pageUp() {
    moveSidebar(-1, -1, false);
}

function pageDown() {
    moveSidebar(+1, -1, false);
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
            $('div#ticketsidebar').html(data);
        }
    });
}

function reloadTicket(ticket) {
    clearTicket();

    selectedTicketIndex = ticket.Index;

    if (ticket.Color != ticketMode) {
        if (ticket.Mode == 1) { // normal
            changeTab("blue");
        }
        if (ticket.Mode == 2) { // system
            changeTab("orange");
        }
        if (ticket.Mode == 3) { // random
            changeTab("green");
        }
    }

    if (selectedTicketIndex != -1) changeType(ticket.Type);

    $("div#ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if ($.inArray(index + 1, ticket.Numbers) > -1) selectNumber(div);
    });

    $("div#ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if ($.inArray(index + 1, ticket.Jokers) > -1) jokerClicked(div);
    });

    refreshButtonStates();
}

function changeDraws(drawsValueChange) {
    drawNumber += drawsValueChange;

    $.ajax({
        url: urlDraws,
        type: "POST",
        dataType: 'html',
        data: {
            value: drawNumber
        },
        success: function (data) {
            $('div#draws').html(data);
        }
    });

    refreshTotalPrice();
}