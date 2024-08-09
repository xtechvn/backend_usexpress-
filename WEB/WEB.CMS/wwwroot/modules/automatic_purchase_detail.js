var purchase_detail = {};
var edited_enable = 0;
var cache_url = '';
$('body').on('focusout', 'input', function () {
    auto_purchase_detail.OnInputFocusOut();
});
$('body').on('focusout', 'select', function () {
    auto_purchase_detail.OnInputFocusOut();
});
$('body').on('focusout', '.form_PurchaseUrl', function () {
    var url = $(this).val();
    if (purchase_detail.PurchaseUrl == undefined || purchase_detail.PurchaseUrl != url || url != cache_url) {
        var product_code = auto_purchase_detail.GetProductCodeFromUrl(url);
        if (product_code == undefined || product_code.trim() == '') {
            _msgalert.error('URL mua tự động nhập vào không chính xác');

        } else {
            $('#form_OrderCode').val(product_code);
            cache_url = url;
        }
    }
});
$('body').on('focusout', '.form_Amount', function () {
    var value = $(this).val();

    var amount = parseFloat(value);
    if (Number.isNaN(amount) || amount == undefined || amount <= 0) {
        _msgalert.error('Vui lòng nhập lại giá (lớn hơn 0)');
    }
});
$('body').on('focusout', '.form_Quanity', function () {
    var value = $(this).val();

    var amount = parseInt(value);
    if (Number.isNaN(amount) || amount == undefined || amount <= 0) {
        _msgalert.error('Vui lòng nhập lại số lượng (lớn hơn 0)');
    }
});
var auto_purchase_detail = {
    Init: function (purchase_id) {
        $('input').removeClass('error');
        if (purchase_id != undefined && parseInt(purchase_id) > -1) {
            auto_purchase_detail.GetPurchaseDetail(purchase_id);
        }
    },


    GetPurchaseDetail(purchase_id) {
        $.ajax({
            url: "/AutomaticPurchase/GetDetail",
            type: "POST",
            data: {
                id: parseInt(purchase_id)
            },
            success: function (result) {
                if (result.status == 0) {
                    var detail = JSON.parse(result.data);
                    auto_purchase_detail.ParseDetailToForm(detail);
                }
            }
        });
    },
    ParseDetailToForm(detail) {
        purchase_detail = detail;
        
        $('#form_OrderCode').val(detail.OrderCode);
        $('#form_PurchaseUrl').val(detail.PurchaseUrl);
        $('#form_Amount').val(detail.Amount);
        $('#form_createdate').val(auto_purchase_detail.GetShowTextDateInput(detail.CreateDate));
        $('#form_ProductCode').val(detail.ProductCode);
        $('#form_Quanity').val(detail.Quanity);
        $('#form_UpdateLast').val(auto_purchase_detail.GetShowTextDateInput(detail.UpdateLast));
        $("#form_PurchaseMessage").val(detail.PurchaseMessage);
        $("#form_ManualNote").val(detail.ManualNote);
        $("#form_OrderedSuccessUrl").val(detail.OrderedSuccessUrl);
        $("#form_PurchasedOrderID").val(detail.PurchasedOrderID);
        $("#form_PurchasedSellerStoreUrl").val(detail.PurchasedSellerStoreUrl);
        $("#form_PurchasedSellerName").val(detail.PurchasedSellerName);
        $("#form_DeliveryStatus").val(detail.DeliveryStatus);
        $("#form_OrderDetailUrl").val(detail.OrderDetailUrl);
        $("#form_OrderEstimatedDeliveryDate").val(auto_purchase_detail.GetShowTextDateInput(detail.OrderEstimatedDeliveryDate));
        $("#form_DeliveryMessage").val(detail.DeliveryMessage);
        var img_html = '<div class="col-12"><div class="form-group pr10"><label class="lbl">${Label}</label><div class="wrap_input"><img src="${src}" style="width:90%;height:auto;"/></div></div></div>';
        if (detail.Screenshot != undefined && Object.entries(JSON.parse(detail.Screenshot)).length > 0) {
            for (let [key, value] of Object.entries(JSON.parse(detail.Screenshot))) {
                var img_html_edited = img_html.replace("${Label}", key).replace("${src}", value);
                $("#form_purchase_img").append(img_html_edited);
            }
        }
        $("#form_PurchaseStatus").val(detail.PurchaseStatus);

        

    },
    OnInputFocusOut: function () {
        if (edited_enable == 0) {

        }
        else {
            $("#btn_autopurchase_save").removeAttr("disabled");

        }
    },
    EnableEdit() {
        let title = 'Thông báo xác nhận';
        let description = "Thông tin mua tự động sẽ được sửa đổi, việc này ảnh hưởng trực tiếp tới đơn hàng của khách hàng. Bạn có chắc chắn không?";
        _msgconfirm.openDialog(title, description, function () {

            $("#form_PurchaseUrl").removeAttr("disabled");
            $("#form_Amount").removeAttr("disabled");
            $("#form_Quanity").removeAttr("disabled");
            $("#form_OrderedSuccessUrl").removeAttr("disabled");
            $("#form_OrderDetailUrl").removeAttr("disabled");
            $("#form_PurchasedSellerStoreUrl").removeAttr("disabled");
            $("#form_purchase_img").removeAttr("disabled");
            $("#form_OrderEstimatedDeliveryDate").removeAttr("disabled");
            $("#form_DeliveryMessage").removeAttr("disabled");
            $("#btn_autopurchase_save").removeAttr("disabled");
            $("#form_ManualNote").removeAttr("disabled");

            $('#form_PurchaseUrl').css('background-color', '');
            $('#form_Amount').css('background-color', '');
            $('#form_Quanity').css('background-color', '');
            $('#form_OrderedSuccessUrl').css('background-color', '');
            $('#form_OrderDetailUrl').css('background-color', '');
            $('#form_PurchasedSellerStoreUrl').css('background-color', '');
            $('#form_purchase_img').css('background-color', '');
            $('#form_OrderEstimatedDeliveryDate').css('background-color', '');
            $('#form_DeliveryMessage').css('background-color', '');
            $('#btn_autopurchase_save').css('background-color', '');
            $('#form_ManualNote').css('background-color', '');
            $("#btn_autopurchase_EnableEdit").attr("disabled", true);

            $("#form_PurchaseStatus").removeAttr("disabled");
            $('#form_PurchaseStatus').css('background-color', '');

            $("#form_PurchaseStatus option[value=0]").prop('disabled', false);
            $("#form_PurchaseStatus option[value=1]").prop('disabled', false);
            $("#form_PurchaseStatus option[value=3]").prop('disabled', false);
            edited_enable = 1;
        });
    },
    Save() {
        let FromCreate = $('#form_automaticpurchase_main');
        FromCreate.validate({
            rules: {
                form_PurchaseUrl: "required",
                form_Amount: "required",
                form_Quanity: "required",
                form_PurchaseStatus: "required",
            },
            messages: {
                form_PurchaseUrl: "Vui lòng nhập chính xác Url mua tự động",
                form_Amount: "Vui lòng nhập chính xác giá sản phẩm",
                form_Quanity: "Vui lòng nhập chính xác số lượng",
                form_PurchaseStatus: "Vui lòng chọn trạng thái mua hộ",
            }
        });
        if (FromCreate.valid()) {
            if (parseInt($('#form_Amount').val()) <= 0) {
                _msgalert.error("Vui lòng nhập lại giá sản phẩm ( giá sản phẩm phải lớn hơn 0)");
                return;
            }
            if (parseInt($('#form_Quanity').val()) <= 0) {
                _msgalert.error("Vui lòng nhập lại số lương ( số lượng phải lớn hơn 0)");
                return;
            }
            var Url = $("#form_PurchaseUrl").val();
            if (Url == undefined || Url.trim() == '' || !Url.includes("http")) {
                _msgalert.error("Vui lòng nhập lại chính xác Url mua tự động");
                return;
            }
            if (Url == undefined || Url.trim() == '' || !Url.includes("http")) {
                _msgalert.error("Vui lòng nhập lại chính xác Url mua tự động");
                return;
            }
            var purchase_status = parseInt($('#form_PurchaseStatus :checked').val());
            if (purchase_status == undefined || purchase_status < 0) {
                _msgalert.error("Vui lòng nhập chọn lại trạng thái mua tự động");
                return;
            }
            if (purchase_detail == undefined) {
                purchase_detail = {};
            }

            let title = 'Thông báo xác nhận';
            let description = "Thông tin mua tự động sẽ được sửa đổi. Bạn có chắc chắn không?";
            _msgconfirm.openDialog(title, description, function () {
                purchase_detail.PurchaseUrl = $("#form_PurchaseUrl").val();
                purchase_detail.Amount = parseFloat($("#form_Amount").val());
                purchase_detail.Quanity = parseInt($("#form_Quanity").val());
                purchase_detail.PurchaseStatus = parseInt($('#form_PurchaseStatus :checked').val());
                purchase_detail.OrderedSuccessUrl = $("#form_OrderedSuccessUrl").val();
                purchase_detail.OrderDetailUrl = $("#form_OrderDetailUrl").val();
                purchase_detail.PurchasedSellerStoreUrl = $("#form_PurchasedSellerStoreUrl").val();
                purchase_detail.OrderEstimatedDeliveryDate = $("#form_OrderEstimatedDeliveryDate").val();
                purchase_detail.DeliveryMessage = $("#form_DeliveryMessage").val();
                purchase_detail.ManualNote = $("#form_ManualNote").val();

                var objdata = {
                    data: purchase_detail
                };
                $.ajax({
                    url: "/AutomaticPurchase/UpdateAutoPurchaseDetail",
                    type: "POST",
                    data: objdata,
                    success: function (result) {
                        if (result.status == 0) {
                            _msgalert.success(result.msg);
                        }
                        else {
                            _msgalert.error(result.msg);
                        }
                        $("#btn_autopurchase_save").attr("disabled", true);

                    },
                });

            });
        }
    },
    GetShowTextDateInput: function (date_str) {
        var date = new Date(Date.parse(date_str));
        return (date.getUTCDate() < 10 ? '0' : '') + date.getUTCDate() + "/" + ((date.getMonth() + 1) < 10 ? '0' : '') + (date.getMonth() + 1) + "/" + date.getFullYear() + " " + (date.getHours() < 10 ? '0' : '') + date.getHours() + ":" + (date.getMinutes() < 10 ? '0' : '') + date.getMinutes() + ":" + (date.getSeconds() < 10 ? '0' : '') + date.getSeconds();
    },
    GetProductCodeFromUrl(Url) {
        var objData = {
            URL: Url
        }
        $.ajax({
            url: "/AutomaticPurchase/GetProductCodeFromUrl",
            type: "POST",
            data: objData,
            success: function (result) {
                return result;
            },
        });
        return undefined;
    }
}