"use strict";

var currentCurrency = 'BTC';
var secondsToNextDraw = -1;

$(document).ready(function () {
    nextDrawTimerEvent();

    $('#checkbutton').click(function (e) {
        e.preventDefault();

        $('#checkModal').reveal({
            animation: 'fade',
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

    $("div#jackpotnumber").click(function () {
        $("div#jackpotnumber").animate({ opacity: 0.0 }, 1000, "easeInOutCubic", function () {
            $.ajax({
                url: urlJackpot,
                type: "POST",
                dataType: 'html',
                data: {
                    currency: currentCurrency
                },
                success: function (data) {
                    $("div#jackpotnumber").html(data);

                    $("div#jackpotnumber").animate({ opacity: 1.0 }, 1000, "easeInOutCubic", function () {});

                    if (currentCurrency === "USD") {
                        currentCurrency = "EUR"
                    } else if (currentCurrency === "EUR") {
                        currentCurrency = "BTC"
                    } else if (currentCurrency === "BTC") {
                        currentCurrency = "USD"
                    }

                }
            });
        });
    });

    $("div#jackpotnumber").trigger("click");
    setInterval(function () {
            $("div#jackpotnumber").trigger("click");
        }, 10000);
});

function nextDrawTimerEvent() {
    if (secondsToNextDraw <= 0) {

        if (secondsToNextDraw === 0) {
            location.reload();
        }

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


function refreshLetPlayButtonState(totalGames) {
    if (totalGames === 0) {
        $("div#letsplay").addClass("disabled");
    } else {
        $("div#letsplay").removeClass("disabled");
    }
}

function refreshTotalGames() {
    $.ajax({
        url: urlTotalGames,
        type: "POST",
        dataType: 'html',
        async: false,
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
        async: false,
        data: {},
        success: function (data) {
            $('div#totalprice').html(data);
        }
    });
}


function changeDraws(drawsValueChange) {
    $.ajax({
        url: urlDraws,
        type: "POST",
        dataType: 'html',
        async: false,
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

            $('#payModal').bind('reveal:close', function () {
                $.ajax({
                    url: urlLetsPlayModalClosed,
                    type: "POST"
                });
            });

        }
    });
}

function secondChanceClick(div)
{
    var toggled = isToggled(div);

    toggle(div);

    if (!toggled) {
        $('img.secondchancesides').animate({ opacity: 0.0 }, 1000);
    } else {
        $('img.secondchancesides').animate({ opacity: 1.0 }, 500);
    }
}

function payClick(t) {
    $.ajax({
        url: urlPayClicked,
        type: "POST"
    });
}

function doneClick(t) {
    $.ajax({
        url: urlDoneClicked,
        type: "POST"
    });
    location.reload();
}

function emailEntered(t) {
    $.ajax({
        url: urlEmailEntered,
        type: "POST",
        data: {
            email: t.value
        }
    });
}
