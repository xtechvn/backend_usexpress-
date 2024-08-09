
$(".btn_edit_address").click(function (e) {
    order.detail($(this).data('addressid'));
});

var order = {    
    detail: function (address_id) {
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/client/address/' + address_id + '.html',
            data: { address_id: address_id },
            success: function (data) {

                if (data.status == SUCCESS) {
                    $('#address-popup').html(data.render_address_detail);
                    $(".title_form").html("ĐỔI ĐỊA CHỈ GIAO HÀNG");
                    $(".btn_add_new_address").html("Cập nhật địa chỉ" );
                    $.magnificPopup.open({
                        items: {
                            src: '.address-popup',
                            type: 'inline'
                        }
                    });
                    $(".mfp-close").remove();
                } else {
                    alert('Địa chỉ này không tồn tại');
                    console.log(data.msg);
                }
                return;
            },
            failure: function (response) {
                console.log(response);
            }
        });
    }    
};
