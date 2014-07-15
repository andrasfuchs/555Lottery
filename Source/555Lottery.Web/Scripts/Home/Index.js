"use strict";

var currentCurrency = 'BTC';
var secondsToNextDraw = -1;
var isBitcoinAddressValid = false;

var noviceAdvancedMasterMode = "novice";

$(document).ready(function () {
    nextDrawTimerEvent();

    $('#checkbutton').click(function (e) {
        //e.preventDefault();

        $('#checkModal').foundation('reveal', 'open');
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

    if ((timeToDraw != 0) && (timeToDraw < 60))
    {
        window.setTimeout(function () { location.reload(); }, 60 * 1000);
    }

    if (noviceAdvancedMasterMode === "novice")
    {
        $("div.ticket>div.tabs>div.taborange").css("opacity", 0).css("cursor", "default").css("pointer-events", "none");
        $("div.ticket>div.tabs>div.tabgreen").css("opacity", 0).css("cursor", "default").css("pointer-events", "none");
    }
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
        $("div#letsplay").addClass("fakedisabled");
    } else {
        $("div#letsplay").removeClass("fakedisabled");
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

            if (ticketlot.TotalBTC > 0) {

                var amount = ticketlot.TotalBTC - ticketlot.TotalDiscountBTC;
                amount = parseFloat(Math.round(amount * 100000000) / 100000000).toFixed(8);

                $("span#checkoutStep1TicketLotCode").html(ticketlot.Code);
                $("span#checkoutStep2TicketLotCode").html(ticketlot.Code);
                $("span#checkoutStep3TicketLotCode").html(ticketlot.Code);


                $("span#checkoutStep2TotalNormalTickets").html(ticketlot.TotalNormalTickets);
                $("span#checkoutStep2TotalNormalGames").html(ticketlot.TotalNormalGames);
                $("span#checkoutStep2TotalSystemTickets").html(ticketlot.TotalSystemTickets);
                $("span#checkoutStep2TotalSystemGames").html(ticketlot.TotalSystemGames);
                $("span#checkoutStep2TotalRandomTickets").html(ticketlot.TotalRandomTickets);
                $("span#checkoutStep2TotalRandomGames").html(ticketlot.TotalRandomGames);

                $("span#checkoutStep2GamePrice").html(ticketlot.Draw.OneGameBTC);
                $("span#checkoutStep2GameCount").html(ticketlot.TotalGames);
                $("span#checkoutStep2Draws").html(ticketlot.DrawNumber);
                $("span#checkoutStep2Discount").html(ticketlot.TotalDiscountBTC.toFixed(8));
                $("span#checkoutStep2Total").html(amount);

                $("span#checkoutStep3Total").html(amount);
                $("span#checkoutStep3TicketTotalBTC").html(amount);
                $("span#checkoutStep3DrawBitCoinAddress").html(ticketlot.Draw.BitCoinAddress);
                $("input#lotteryaddress").val(ticketlot.Draw.BitCoinAddress);

                $("span#checkoutStep3TicketLotCode").html(ticketlot.Code);

                var paymentURI = "bitcoin:" + ticketlot.Draw.BitCoinAddress + "?amount=" + amount + "&label=555%20Lottery&message=" + ticketlot.Code;
                //$("a#checkoutStep2PayLink")[0].href = paymentURI;
                $("img.payqr").attr("src", "http://qrfree.kaywa.com/?l=1&s=8&d=" + $('<div/>').text(paymentURI).html());

                $('#checkoutStep1').foundation('reveal', 'open');

                $('#checkoutStep1 a.close-reveal-modal').bind('click', function () {
                    $.ajax({
                        url: urlCheckoutStep1Closed,
                        type: "POST"
                    });
                });
            } else
            {
                var snc = selectedNumbersCount(0);

                if ((snc < minimumSelectedNumbers) || (snc > maximumSelectedNumbers)) {
                    $("div.ticketcontent div.ticketnumber").fadeTo(500, 0.5, "easeInOutCubic").fadeTo(500, 1.0, "easeInOutCubic").fadeTo(500, 0.5, "easeInOutCubic").fadeTo(500, 1.0, "easeInOutCubic");
                } else {
                    $("div#acceptbutton").fadeTo(500, 0.5, "easeInOutCubic").fadeTo(500, 1.0, "easeInOutCubic").fadeTo(500, 0.5, "easeInOutCubic").fadeTo(500, 1.0, "easeInOutCubic");
                }


                if (true) {

                }
            }
        }
    });

    isBitcoinAddressValid = false;
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

function nextClick(t) {
    $.ajax({
        url: urlNextClicked,
        type: "POST"
    });

    if ($("select#clientname")[0].value != 'selectone') {

        $.ajax({
            url: urlAddressEntered,
            type: "POST",
            data: {
                address: $("input#useraddress")[0].value
            },
            success: function (data) {
                if (data == "True") {
                    // validation ok
                    $('#checkoutStep2').foundation('reveal', 'open');

                    $('#checkoutStep2 a.close-reveal-modal').bind('click', function () {
                        $.ajax({
                            url: urlCheckoutStep2Closed,
                            type: "POST"
                        });
                    });
                } else {
                    // validation error
                    indicateValidationError($("input#useraddress")[0]);
                }
            }
        });
    } else {
        // validation error
        indicateValidationError($("select#clientname")[0]);
    }
}

function indicateValidationError(t)
{
    $(t).animate({ borderColor: "#f7d093" }, 'fast').delay(1500).animate({ borderColor: "#8dd9ff" }, 'fast');
}

function payClick(t) {
    $.ajax({
        url: urlPayClicked,
        type: "POST"
    });

    //$('#checkoutquestion').foundation('reveal', 'open');

    showStep3(false);
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

    showStep3(true);
}

function clientNoClick(t) {
    $.ajax({
        url: urlClientNoClicked,
        type: "POST"
    });

    showStep3(false);
}

function clientSkipClick(t) {
    $.ajax({
        url: urlClientSkipClicked,
        type: "POST"
    });

    showStep3(false);
}

function showStep3(showYes)
{
    if (showYes) {
        $('#noinstructions').hide();
        $('#yesinstructions').show();
    } else {
        $('#yesinstructions').hide();
        $('#noinstructions').show();
    }

    $('#checkoutStep3').foundation('reveal', 'open');
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

function closeClick(t) {
    $.ajax({
        url: urlCloseClicked,
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

function bitcoinWalletSelected(t) {
    $.ajax({
        url: urlBitcoinWalletSelected,
        type: "POST",
        data: {
            wallet: t.value
        }
    });
}