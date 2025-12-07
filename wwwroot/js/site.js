$(document).ready(function() {
    // Smooth scroll for nav links
    $(".nav-link").on("click", function(event) {
        if (this.hash !== "") {
            event.preventDefault();
            $("html, body").animate(
                { scrollTop: $(this.hash).offset().top },
                800
            );
        }
    });

    // Highlight active nav item
    $(".navbar-nav .nav-link").on("click", function() {
        $(".navbar-nav .nav-link").removeClass("active");
        $(this).addClass("active");
    });

    // Tooltip example
    $('[data-toggle="tooltip"]').tooltip();
});
