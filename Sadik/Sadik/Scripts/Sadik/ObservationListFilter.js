$(function () {
    $('.js_filterObservationsAll').click(function () {
        $('.js_ObservationItem').show();
    });
    $('.js_filterObservationsActivities').click(function () {
        $('.js_ObservationItem').each(function () {
            if ($(this).hasClass('js_ObservationItemActivity')) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
    $('.js_filterObservationsCameInClass').click(function () {
        $('.js_ObservationItem').each(function () {
            if ($(this).hasClass('js_ObservationItemCameInClass')) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
    $('.js_filterObservationsEmotion').click(function () {
        $('.js_ObservationItem').each(function () {
            if ($(this).hasClass('js_ObservationItemEmotion')) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
    $('.js_filterObservationsComments').click(function () {
        $('.js_ObservationItem').each(function () {
            if ($(this).find('.js_comment_container').length > 0) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
})