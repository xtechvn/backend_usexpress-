var _loginModel = {
    OpenResetPasswordPopup: function () {
        $('#form-forget-password')[0].reset();
        $("#panel-login").addClass("mfp-hide");
        $("#panel-reset-password").removeClass("mfp-hide");
    },

    OpenLoginPopup: function () {
        $("#panel-reset-password").addClass("mfp-hide");
        $("#panel-login").removeClass("mfp-hide");
    },

    OnRegistry: function () {
        let elPopup = $('#magnific-popup-registry');
        elPopup.find('.magnific-body').html("Xin vui lòng liên hệ với bộ phận kỹ thuật để được cấp tài khoản");
        jQuery.magnificPopup.open({
            items: {
                src: elPopup
            },
            type: 'inline',
            midClick: true,
            mainClass: 'mfp-with-zoom',
            fixedContentPos: false,
            fixedBgPos: true,
            overflowY: 'auto',
            closeBtnInside: true,
            preloader: false,
            removalDelay: 300
        });
    },

    OnResetPassword: function () {
        let Form = $('#form-forget-password');
        Form.validate({
            rules: {
                EmailOrUserName: "required",
            },
            messages: {
                EmailOrUserName: "Vui lòng nhập Tên đăng nhập hoặc Email",
            }
        });

        if (Form.valid()) {
            $.ajax({
                url: '/Account/resetpassword',
                type: "post",
                data: { EmailOrUserName: $("#EmailOrUserName").val().trim() },
                success: function (data) {
                    let elPopup = $('#magnific-popup-registry');
                    elPopup.find('.magnific-body').html(data.message);
                    jQuery.magnificPopup.open({
                        items: {
                            src: elPopup
                        },
                        type: 'inline',
                        midClick: true,
                        mainClass: 'mfp-with-zoom',
                        fixedContentPos: false,
                        fixedBgPos: true,
                        overflowY: 'auto',
                        closeBtnInside: true,
                        preloader: false,
                        removalDelay: 300
                    });

                    if (data.isSuccess) {
                        _loginModel.OpenLoginPopup();
                    }
                }
            });
        }
    }
};