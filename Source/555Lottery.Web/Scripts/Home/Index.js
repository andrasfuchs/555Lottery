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

    $("img.payqr").mouseover(function () {
        $(this).css('width', '370px').css('height', '370px');
    });

    $("img.payqr").mouseout(function () {
        $(this).css('width', '50px').css('height', '50px');
    });

    $("input#notificationemail").val($.cookie("#notificationemail"));

    $("input#username").val($.cookie("#username"));
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
            
            $("span#checkoutStep1TotalNormalTickets").html(ticketlot.TotalNormalTickets);
            $("span#checkoutStep1TotalNormalGames").html(ticketlot.TotalNormalGames);
            $("span#checkoutStep1TotalSystemTickets").html(ticketlot.TotalSystemTickets);
            $("span#checkoutStep1TotalSystemGames").html(ticketlot.TotalSystemGames);
            $("span#checkoutStep1TotalRandomTickets").html(ticketlot.TotalRandomTickets);
            $("span#checkoutStep1TotalRandomGames").html(ticketlot.TotalRandomGames);

            $("span#checkoutStep1GamePrice").html(ticketlot.Draw.OneGameBTC);
            $("span#checkoutStep1GameCount").html(ticketlot.TotalGames);
            $("span#checkoutStep1Draws").html(ticketlot.DrawNumber);
            $("span#checkoutStep1Discount").html(ticketlot.TotalDiscountBTC);
            $("span#checkoutStep1Total").html(amount);
            $("span#checkoutStep2Total").html(amount);

            $("span#checkoutStep1TicketLotCode").html(ticketlot.Code);

            $("span#checkoutStep2TicketLotCode").html(ticketlot.Code);
            $("span#checkoutStep2TicketTotalBTC").html(amount);
            $("span#checkoutStep2DrawBitCoinAddress").html(ticketlot.Draw.BitCoinAddress);
            $("input#lotteryaddress").val(ticketlot.Draw.BitCoinAddress);

            var paymentURI = "bitcoin:" + ticketlot.Draw.BitCoinAddress + "?amount=" + amount + "&label=555%20Lottery&message=" + ticketlot.Code;
            $("a#checkoutStep1PayLink")[0].href = paymentURI;
            $("img.payqr").attr("src", "http://qrfree.kaywa.com/?l=1&s=8&d=" + $('<div/>').text(paymentURI).html());

            $('#checkoutStep1').foundation('reveal', 'open');

            $('#checkoutStep1').bind('closed', function () {
                $.ajax({
                    url: urlCheckoutStep1Closed,
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

    $('#checkoutStep1').trigger('close');

    $('#checkoutquestion').foundation('reveal', 'open');
}

function tryoutClick(t) {
    $.ajax({
        url: urlTryoutClicked,
        type: "POST"
    });
}

function clientYesClick(t) {
    $.ajax({
        url: urlClientYesClicked,
        type: "POST"
    });

    $('#checkoutquestion').trigger('close');

    $('#yesinstructions').show();
    $('#noinstructions').hide();

    $('#checkoutStep2').foundation('reveal', 'open');
}

function clientNoClick(t) {
    $.ajax({
        url: urlClientNoClicked,
        type: "POST"
    });

    $('#checkoutquestion').trigger('close');

    $('#yesinstructions').hide();
    $('#noinstructions').show();

    $('#checkoutStep2').foundation('reveal', 'open');
}

function exportClick(t) {
    window.location.href = urlExportClicked;
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

function nameEntered(t) {
    $.ajax({
        url: urlNameEntered,
        type: "POST",
        data: {
            name: t.value
        }
    });
}

function addressEntered(t) {
    $.ajax({
        url: urlAddressEntered,
        type: "POST",
        data: {
            address: t.value
        }
    });
}
