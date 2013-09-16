"use strict";

var currentLang = 'eng';
var ticketMode = 'blue';
var minimumSelectedNumbers = 5;
var maximumSelectedNumbers = 5;
var selectedTicketIndex = -1;
var processingRandom = false;

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

function replaceAll(find, replace, str) {
    return str.replace(new RegExp(find, 'g'), replace);
}

function changeColor(element, color) {
    if (element === undefined) return;

    if (element.className === 'ticketleftside') return;
    if (element.className === 'tabs') return;

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


function selectedNumbersCount(tlId) {
    var selectedNumbersCountResult = 0;
    $("div.ticketarea#" + tlId + " div.ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectedNumbersCountResult++;
    });

    return selectedNumbersCountResult;
}

function selectedJokersCount(tlId) {
    var selectedJokersCountResult = 0;
    $("div.ticketarea#" + tlId + " div.ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div) && (div.className.indexOf("jokerall") == -1)) selectedJokersCountResult++;
    });

    return selectedJokersCountResult;
}

function selectedTypesCount(tlId) {
    var selectedTypesCountResult = 0;

    if ($("div.ticketarea#" + tlId + " div#tickettype").hasClass("hidden")) return null;

    $("div.ticketarea#" + tlId + " div#tickettype").find("div.typebutton").each(function (index, div) {
        if (isToggled(div)) selectedTypesCountResult++;
    });

    return selectedTypesCountResult;
}

function selectNumber(tlId, t, donotrefreshgui) {
    var snc = selectedNumbersCount(tlId);

    if ((t !== undefined) && ((snc < maximumSelectedNumbers) || (isToggled(t)))) {
        toggle(t);
    }

    if (!donotrefreshgui) {
        refreshButtonStates(tlId);
    }
}

function refreshButtonStates(tlId) {
    var snc = selectedNumbersCount(tlId);

    if ((snc === 0) && (selectedJokersCount(tlId) === 0)) {
        $("div.ticketarea#" + tlId + " div#clearbutton").addClass("disabled");
    } else {
        $("div.ticketarea#" + tlId + " div#clearbutton").removeClass("disabled");
    }

    if (((snc < minimumSelectedNumbers) || (snc > maximumSelectedNumbers) || ((selectedTypesCount(tlId) !== null) && (selectedTypesCount(tlId) === 0)))
        || ((selectedJokersCount(tlId) === 0) && ((ticketMode !== 'green') || (selectedTicketIndex !== -1)))) {
        $("div.ticketarea#" + tlId + " div#acceptbutton").addClass("disabled");
    } else {
        $("div.ticketarea#" + tlId + " div#acceptbutton").removeClass("disabled");
    }

    if (ticketMode === 'green')
    {
        $("div.ticketarea#" + tlId + " div#randombutton").addClass("disabled");
    } else {
        $("div.ticketarea#" + tlId + " div#randombutton").removeClass("disabled");
    }
}

function generateTicketType(tlId) {
    var type = "";

    if (ticketMode === "orange") {
        type = "S";
    } else if (ticketMode === "green") {
        type = "R";
    } else {
        type = "N";
    }

    if ((type === "S") || (type === "R")) {
        $("div.ticketarea#" + tlId + " div#tickettype").find("div.typebutton").each(function (index, div) {
            if (isToggled(div)) type += (index + 1);
        });
    }

    if (type.length === 1)
    {
        type += "0";
    }

    return type;
}

function generateTicketSequence(tlId) {
    // display the selected number on the bottom of the ticket
    var text = "";
    var selectedCount = 0;
    $("div.ticketarea#" + tlId + " div.ticketcontent").find("div.ticketnumber").each(function (index, div) {
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

    $("div.ticketarea#" + tlId + " div.ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) text += div.firstElementChild.alt + ",";
    });

    // remove the last comma
    while (text[text.length - 1] === ',') {
        text = text.substring(0, text.length - 1);
    }

    return text;
}

function refreshTicketBottom(tlId) {
    $.ajax({
        url: urlTicketBottom,
        type: "POST",
        dataType: 'html',
        async: false,
        data: {
            ticketLotId: tlId,
            text: generateTicketSequence(tlId)
        },
        success: function (data) {
            $("div.ticketarea#" + tlId + " div.ticketfooterlist").html(data);
        }
    });
}

function refreshTicketPrice(tlId) {
    $.ajax({
        url: urlTicketPrice,
        type: "POST",
        dataType: 'html',
        async: false,
        data: {
            ticketLotId: tlId,
            ticketType: generateTicketType(tlId),
            ticketSequence: generateTicketSequence(tlId)
        },
        success: function (data) {
            $("div.ticketarea#" + tlId + " div.ticketfooterbtc").html(data);
        }
    });
}

function clearButtonClick(tlId, t) {
    if (selectedTicketIndex !== -1) {
        $.ajax({
            url: urlDeleteTicket,
            type: "POST",
            dataType: 'json',
            data: {
                ticketLotId: tlId,
                ticketIndex: selectedTicketIndex
            },
            success: function (data) {
                selectTicket(tlId, data[0]);
                refreshTotalGames();
                refreshTotalPrice();
                refreshLetPlayButtonState(data[1]);
            }
        });
    }

    clearTicket(tlId);

    refreshTicketBottom(tlId);
}

function clearTicket(tlId) {
    selectedTicketIndex = -1;

    $("div.ticketarea#" + tlId + " div.ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectNumber(tlId, div);
    });

    $("div.ticketarea#" + tlId + " div.ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if (isToggled(div)) selectNumber(tlId, div);
    });
}

function randomButtonClick(tlId, t) {
    $.ajax({
        url: urlRandomClicked,
        type: "POST"
    });

    if (processingRandom) return;

    processingRandom = true;
    $("div.ticketarea#" + tlId + " div#randombutton").addClass("disabled");

    clearTicket(tlId);

    var number = 0;

    while (true) {
        if (selectedNumbersCount(tlId) >= minimumSelectedNumbers) {
            break;
        }

        number = (Math.random() * 55);

        $("div.ticketarea#" + tlId + " div.ticketcontent").find("div.ticketnumber").each(function (index, div) {
            if ((index <= number) && (index + 1 >= number)) selectNumber(tlId, div);
        });
    }

    number = (Math.random() * 5);

    while (selectedJokersCount(tlId) === 0) {
        $("div.ticketarea#" + tlId + " div.ticketjoker").find("div.ticketnumber").each(function (index, div) {
            if ((index <= number) && (index + 1 >= number)) jokerClicked(tlId, div);
        });
    }

    refreshTicketBottom(tlId);

    setTimeout(function(){
        $("div.ticketarea#" + tlId + " div#randombutton").removeClass("disabled");
        processingRandom = false;
    }, 500);
}

function acceptButtonClick(tlId, t) {
    $.ajax({
        url: urlAcceptTicket,
        type: "POST",
        dataType: 'json',
        data: {
            ticketLotId: tlId,
            ticketType: generateTicketType(tlId),
            ticketSequence: generateTicketSequence(tlId),
            overwriteTicketIndex: selectedTicketIndex
        },
        success: function (data) {
            clearTicket(tlId);
            selectTicket(tlId, data[0]);
            refreshTotalGames();
            refreshTotalPrice();
            refreshLetPlayButtonState(data[1]);
        }
    });
}


function allClicked(tlId, t) {
    $(t).parent().find("div.ticketnumber").each(function (index, div) {
        toggle(div, true);
    });

    refreshTicketBottom(tlId);
    refreshTicketPrice(tlId);
    refreshButtonStates(tlId);
}

function jokerClicked(tlId, t, donotrefreshgui) {
    toggle(t);

    var allToggled = true;

    $(t).parent().find("div.ticketnumber:not(.jokerall)").each(function (index, div) {
        if (!isToggled(div)) allToggled = false;
    });

    toggle($(t).parent().find("div.jokerall")[0], allToggled);

    if (!donotrefreshgui) {
        refreshTicketBottom(tlId);
        refreshTicketPrice(tlId);
        refreshButtonStates(tlId);
    }
}

function changeType(tlId, type) {

    if (type === 0)
    {
        $("div.ticketarea#" + tlId + " div#tickettype").addClass("hidden");
    } else
    {
        $("div.ticketarea#" + tlId + " div#tickettype").removeClass("hidden");
    }   

    $("div.ticketarea#" + tlId + " div#tickettype").find("div.typebutton").each(function (index, div) {
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

function changeTab(tlId, mode) {
    $.ajax({
        url: urlTabChanged,
        type: "POST",
        data: {
            ticketLotId: tlId,
            changedTo: mode
        }
    });


    if (selectedTicketIndex !== -1) {
        moveSidebar(tlId, 0, -1, false, true);
    }

    toggle($("div.ticketarea#" + tlId + " div.tabblue")[0], mode === 'blue');
    toggle($("div.ticketarea#" + tlId + " div.taborange")[0], mode === 'orange');
    toggle($("div.ticketarea#" + tlId + " div.tabgreen")[0], mode === 'green');
    
    changeColor($("div.ticketarea#" + tlId)[0], mode);
    changeColor($("div.ticketarea#" + tlId + " div.ticketsidebar div.selectedticket")[0], mode);
    changeColor($("div.ticketarea#" + tlId + " div.ticketsticksleft div.selectedticket")[0], mode);
    ticketMode = mode;

    if ((mode === 'orange') || (mode === 'green')) {
        changeType(tlId, 1);
    } else {
        changeType(tlId, 0);
    }

    clearTicket(tlId);

    selectNumber(tlId);
}


function pageUp(tlId) {
    moveSidebar(tlId, -1, 0, false);
}

function pageDown(tlId) {
    moveSidebar(tlId, +1, 0, false);
}

function selectTicket(tlId, index) {
    moveSidebar(tlId, 0, index, true);
}

function moveSidebar(tlId, scrollPositionChange, selectedTicketIndex, refreshTicket, forceTab) {
    $.ajax({
        url: urlMoveSlider,
        type: "POST",
        dataType: 'json',
        data: {
            ticketLotId: tlId,
            scrollPositionChange: scrollPositionChange,
            selectedTicketIndex: selectedTicketIndex
        },
        success: function (data) {
            refreshTicketSidebar(tlId, forceTab);

            if (forceTab === true)
            {
                refreshTicketBottom(tlId);
                refreshTicketPrice(tlId);
            }

            if (refreshTicket) {
                reloadTicket(tlId, data);
            }
        }
    });
}

function refreshTicketSidebar(tlId, forceTab) {
    $.ajax({
        url: urlTicketSidebar,
        type: "POST",
        dataType: 'html',
        data: {
                TicketLotId: tlId
            },
        success: function (data) {
            $("div.ticketarea#" + tlId + " div.ticketleftside").html(data);

            if (forceTab === true) {
                changeTab(tlId, ticketMode);
            }
        }
    });
}

function reloadTicket(tlId, ticket) {
    clearTicket(tlId);

    if (ticket.Color !== ticketMode) {
        if ((ticket.Mode === 0) || (ticket.Mode === 1)) { // empty or normal
            changeTab(tlId, "blue");
        }
        if (ticket.Mode === 2) { // system
            changeTab(tlId, "orange");
        }
        if (ticket.Mode === 3) { // random
            changeTab(tlId, "green");
        }
    }

    selectedTicketIndex = ticket.Index;
    if (selectedTicketIndex !== -1) changeType(tlId, ticket.Type);

    $("div.ticketarea#" + tlId + " div.ticketcontent").find("div.ticketnumber").each(function (index, div) {
        if ($.inArray(index + 1, ticket.Numbers) > -1) selectNumber(tlId, div, true);
    });

    $("div.ticketarea#" + tlId + " div.ticketjoker").find("div.ticketnumber").each(function (index, div) {
        if ($.inArray(index + 1, ticket.Jokers) > -1) jokerClicked(tlId, div, true);
    });

    refreshTicketBottom(tlId);
    refreshTicketPrice(tlId);
    refreshButtonStates(tlId);
}
