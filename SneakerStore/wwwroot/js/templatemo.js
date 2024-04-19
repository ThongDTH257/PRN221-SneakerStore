/*

TemplateMo 559 Zay Shop

https://templatemo.com/tm-559-zay-shop

*/

'use strict';
$(document).ready(function () {

    // Accordion
    var all_panels = $('.templatemo-accordion > li > ul').show();

    $('.templatemo-accordion > li > a').click(function () {
        var target = $(this).next();

        if (!target.hasClass('active')) {
            target.addClass('active').slideDown();
            var icon = $(this).children()[0];
            $(icon).removeClass('fa-chevron-circle-down');
            $(icon).addClass('fa-chevron-circle-up')
        } else {
            target.removeClass('active').slideUp();
            var icon = $(this).children()[0];
            $(icon).removeClass('fa-chevron-circle-up');
            $(icon).addClass('fa-chevron-circle-down')
        }
        return false;
    });
    // End accordion

    // Product detail
    $('.product-links-wap a').click(function () {
        var this_src = $(this).children('img').attr('src');
        $('#product-detail').attr('src', this_src);
        return false;
    });
    $('#btn-minus').click(function () {
        var val = $("#var-value").html();
        val = (val == '1') ? val : val - 1;
        $("#var-value").html(val);
        $("#product-quanity").val(val);
        return false;
    });
    $('#btn-plus').click(function () {
        var val = $("#var-value").html();
        val++;
        $("#var-value").html(val);
        $("#product-quanity").val(val);
        return false;
    });
    $('.btn-size').click(function () {
        var this_val = $(this).attr("data-id");
        $("#size-warning").hide();
        $("#product-size").val(this_val);
        $(".btn-size").removeClass('btn-success');
        $(".btn-size").addClass('btn-secondary');
        $(this).removeClass('btn-secondary');
        $(this).addClass('btn-success');
        return false;
    });
    // End product detail

});
