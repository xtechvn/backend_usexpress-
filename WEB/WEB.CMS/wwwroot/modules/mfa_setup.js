var _2fa_setup_model = {
    OnMFATestGG: function () {
        $("#error-text-ontest-gg").addClass("mfp-hide");
        $("#error-text-ontest-gg").html("");
        var input_otp = $('#i_gg_otp_test').val();
        var today = new Date();
        var data = {
            MFA_type: 0,
            MFA_token: token,
            MFA_Code: input_otp,
            MFA_timenow: today,
            ReturnUrl: "/"
        };
        $.ajax({
            url: "/Account/OTPTest",
            method: "POST",
            data: {
                record: data
            },
            success: function (result) {
                $("#error-text-ontest-gg").html(result.msg);
                $("#error-text-ontest-gg").removeClass("mfp-hide");
            },
        });
    },
    OnMFATestBackupCode: function () {
        $("#error-text-ontest-bcode").addClass("mfp-hide");
        $("#error-text-ontest-bcode").html("");
        var input_otp = $('#i_bcode_test').val();
        var today = new Date();
        var data = {
            MFA_type: 1,
            MFA_token: token,
            MFA_Code: input_otp,
            MFA_timenow: today,
            ReturnUrl: "/"
        };
        $.ajax({
            url: "/Account/OTPTest",
            method: "POST",
            data: {
                record: data
            },
            success: function (result) {
                $("#error-text-ontest-bcode").html(result.msg);
                $("#error-text-ontest-bcode").removeClass("mfp-hide");
            },
        });
    },
    OnMFAConfirm: function () {
        $("#error-text-onconfirm").addClass("mfp-hide");
        $("#error-text-onconfirm").html("");
        $.ajax({
            url: "/Account/ConfirmMFA",
            method: "POST",
            data: {
                token: token
            },
            success: function (result) {
                if (result.status == "SUCCESS") {
                    window.location = "/";
                }
                else {
                    $("#error-text-onconfirm").html(result.msg);
                    $("#error-text-onconfirm").removeClass("mfp-hide");
                }
            },
        });
    },
};