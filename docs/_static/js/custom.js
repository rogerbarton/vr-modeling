$(document).ready(function () {
    $('a[href^="http://"], a[href^="https://"]').not('a[class*=internal]').attr('target', '_blank');
    $("p.breathe-sectiondef-title.rubric:contains('Private')").css('background-color', '#93b2c5');
});
