var _2fa_authenticate_model = {
    bcodepopup: function () {
        $("#popup-gg-otp").addClass("mfp-hide");
        $("#popup-backupcode").removeClass("mfp-hide");
    },
    ggotppopup: function () {
        $("#popup-backupcode").addClass("mfp-hide");
        $("#popup-gg-otp").removeClass("mfp-hide");
    },
};