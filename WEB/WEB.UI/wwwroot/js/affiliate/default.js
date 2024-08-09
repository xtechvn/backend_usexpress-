
jQuery(document).ready(function () {

})

$("#btn_register_aff").click(function (e) {
    aff.register();
});

$(".btn_add_link_aff").click(function (e) {
    var link = $("#txt_link_aff").val().trim();
    if (link.length < 20) {
        obj_general.alert("Link nhập không hợp lệ. Bạn hãy copy 1 link sản phẩm rồi dán vào đây nhé");
    } else {
        if (!(link.indexOf("utm_medium") >= 0 && link.indexOf("aff_sid") >= 0 && link.indexOf("utm_source") >= 0)) {
            aff.add_link(link);
        }
    }
});

$(document.body).on('click', '.btn_create_aff', function (e) {
    location.reload();
});

var aff = {
    get_affilliate_type: function () {
        return window.localStorage.getItem("aff");
    },
    register: function () {
        $.ajax({
            url: '/client/aff/register.json',
            type: 'POST',
            dataType: "json",
            success: function (response) {

                if (response.status == SUCCESS) {
                    // set local
                
                    localStorage.setItem(REFERRAL_ID_FIRST, response.referral_id);
                    obj_general.alert_success_aff();
                }
            }
        });
    },
    add_link: function (link) {
        var referral_id = localStorage.getItem(REFERRAL_ID_FIRST);
        link = `${encodeURIComponent(link)}`;
        $.ajax({
            url: '/client/aff/add-link',
            type: 'POST',
            data: { link_aff: link, referral_first_id: referral_id },
            dataType: "json",
            success: function (response) {
              
                if (response.status == SUCCESS) {
                    $("#txt_link_aff_full").val(response.link_aff);
                    $("#txt_link_aff").val("");
                } else if (response.status == FAILED) {
                    obj_general.alert(response.msg);
                }
            }
        });
    }
}




