
var _mfa_management_model = {
    OnMFATestGG: function () {
        $("#error-text-ontest-gg").addClass("mfp-hide");
        $("#error-text-ontest-gg").html("");
        var input_otp = $('#i_gg_otp_test').val();
        var today = new Date();
        var data= {
            MFA_type: 0,
            MFA_token: "",
            MFA_Code: input_otp,
            MFA_timenow: today,
            ReturnUrl: "/"
        };
        $.ajax({
            url: "/MFAManagement/OTPTest ",
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
        var data =  {
            MFA_type: 1,
            MFA_token: "",
            MFA_Code: input_otp,
            MFA_timenow: today,
            ReturnUrl: "/"
        };
        $.ajax({
            url: "/MFAManagement/OTPTest ",
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
    
};