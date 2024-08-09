//login fb
var fb = {

    ShowChatAuto: function() {
        FB.CustomerChat.showDialog();
        $('.fb-customerchat').show();
        $(".fb_cus").remove();
    },
    fbLogin: function () { // Facebook login with JavaScript SDK
        FB.login(function (response) {
            if (response.authResponse) {
                // Get and display the user profile data
                getFbUserData();
                $('#popupdky_dnhap').modal('hide');
            } else {
                AddAlert("Thông báo", "Bạn cần đăng nhập tài khoản Facebook để kết nối!", 6000, 1);
            }
        }, { scope: 'public_profile,email' });
    },
    getFbUserData: function () { // Fetch the user profile data from facebook
        FB.api('/me', { locale: 'en_US', fields: 'id,name,email,picture' }, function (response) {
            // Chuỗi json chứa thông tin login từ facebook
            var strJsonFbPost = "{"
                + "Email:'" + response.email + "',"
                + "Id:'" + response.id + "',"
                + "Name:'" + response.name + "',"
                + "AvartarUrl:'" + response.picture.data.url + "',"
                + "SourceLogin:'fb'"
                + "}";
            $.post('/Account/LoginGoogleOrFacebook', { jsonPost: strJsonFbPost }, function (result) {
                if (result.bResult) {
                    // Set cookie client use Auto login
                    localStorage.setItem("USEXPRESS_LOGIN_AUTO", response.email);
                    $('#ajax_loading').css('display', 'none');
                    if ($('#ipReturUrl').val() === 'myorder') {
                        $('#ipReturUrl').val('');
                        location.href = '/Account/myorder';
                    } else {
                        ReloadMenu();
                    }
                } else {
                    $('#ajax_loading').css('display', 'none');
                    AddAlert("Thông báo", result.msg, 10000, 0);
                }
            });
        });
    }
}

