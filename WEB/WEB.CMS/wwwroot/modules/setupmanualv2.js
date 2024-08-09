
var redirect_from_cms = 0;
var product_model = null;
var product_fee = null;
var product_location = [];
var product_spec = [];
var product_fee_input = {};
var product_code_filled = '';
$('body').on('focusout', 'input', function () {
    _setupmanualv2.ChangeButtonStatus();
});
$('body').on('click', '.lockaction_span', function (ev) {
    if (ev.target.tagName === 'SPAN') {
        if ($(ev.target).prev().is(':checked') === false) {
            $(ev.target).prev().prop('checked', true)
        }
    }
});

$('body').on('click', '.img_product_del', function () {
    var img = $(this).closest('div').find('img').attr('src');
    URL.revokeObjectURL(img);
    $(this).closest('div').parent().remove();
});
$('body').on('click', '.form_btn_addpos', function () {
    _setupmanualv2.OpenFormAddProductLocation($(this));
});

$('body').on('focusout', '.form_star', function () {
    _setupmanualv2.SetStar($(this).val());
});
$('body').on('click', '.form_btn_addspecification', function () {
    _setupmanualv2.AddNewSpecification();
});
$('body').on('click', '.onlocked_product', function () {
    _setupmanualv2.OnLockedProduct();
});
$('body').on('click', '.select_lock_action', function () {
    var actionid = $(this).val();
    switch (actionid) {
        case '-1': {
            _msgalert.notify_tooltip('Sản phẩm sẽ được ẩn nút đặt mua và hiển thị button liên hệ CSKH');
        } break
        case '0': {
            _msgalert.notify_tooltip('Sản phẩm sẽ được hiển thị trở lại các nút bấm cho khách hàng mua');
        } break;
        case '1': {
            _msgalert.notify_tooltip('Link sản phẩm sẽ được điều hướng sang trang "không tìm thấy sản phẩm này"');

        } break;
        case '2': {
            _msgalert.notify_tooltip('Sản phẩm sẽ được hiển thị lại nút đặt mua cho khách hàng thanh toán');
        } break;
        default: break;
    }
});
$('body').on('click', '.remove_record', function () {
    var id = $(this).attr('data-id');
    var id_list = id.trim().split('_');
    switch (id_list[0].trim()) {
        case 'location': {
            $.each(product_location, function (index, value) {
                if (value.groupProductId == id_list[1]) {
                    product_location.splice(index - 1, 1);
                }
            });

        } break;
        case 'spec': {


        } break;
        default: break;
    }
    $(this).parent().remove();
});
$('body').on('click', '.btn-toggle-cate', function () {
    var seft = $(this);
    if (seft.hasClass("plus")) {
        seft.parent().siblings('ul.lever2').show();
        seft.removeClass('plus').addClass('minus');
    } else {
        seft.parent().siblings('ul.lever2').hide();
        seft.removeClass('minus').addClass('plus');
    }
});
$('body').on('focusout', '.form_product_code', function () {
    _setupmanualv2.GetDetailByASIN();
    _common.tinyMce('form_productinfomationHTML');
});
$('#form_label').on('change', function () {
    switch ($(this).val()) {
        case '2': {
            $('#div_amount_vnd').css('display', '');
            break;
        }
        default: {
            $('#div_amount_vnd').css('display', 'none');
            break;
        }
    };
    _setupmanualv2.GetDetailByASIN();

    _common.tinyMce('form_productinfomationHTML');

});
$('body').on('focusout', '.input_fee_change', function () {
    _setupmanualv2.OnChangeFeeInput('0');


});
$('#form_selectweightunit,#form_industry_special_type').on('change', function () {
    _setupmanualv2.OnChangeFeeInput('0');

});
$('body').on('focusout', '.input_fee_change_fixed', function () {
    if (parseInt($('#form_fixed_amount_vnd').val().replaceAll(",","")) > 0) {
        $('#form_discount').removeClass('input_fee_change');
        $('#form_productweight').removeClass('input_fee_change');
        $('#form_price').removeClass('input_fee_change');
        $('#form_shipping_price').removeClass('input_fee_change');
        _setupmanualv2.OnChangeFeeInput($('#form_fixed_amount_vnd').val().replaceAll(",",""));
        product_model.product_type = '4';
    
    }
    else {
        $('#form_discount').addClass('input_fee_change');
        $('#form_productweight').addClass('input_fee_change');
        $('#form_price').addClass('input_fee_change');
        $('#form_shipping_price').addClass('input_fee_change');
    }


});
var _setupmanualv2 = {
    Initialization: function () {
        $('input').removeClass('error');

        $('#form_label').val('1');
        _setupmanualv2.SetStar(0);
        $("#form_btn_summit").attr("disabled", true);
        $("#form_btn_new").attr("disabled", true);
        _setupmanualv2.LoadSpecificationUnit(function (spec_unit) {
            if (ASIN.trim() != '' && label_id.trim() != '') {
                $('#form_product_code').val(ASIN);
                $('#form_label').val(label_id);
                redirect_from_cms = 1;
                _setupmanualv2.GetDetailByASIN();
                _common.tinyMce('form_productinfomationHTML');
            }
            else {

                $('#form_product_code').removeAttr('disabled');
                $('#form_label').removeAttr('disabled');
                $('#form_product_code').css('background-color', '');
                $('#form_label').css('background-color', '');
                _common.tinyMce('form_productinfomationHTML');
            }
        });
    },
    ChangeButtonStatus: function () {
        if ($('#form_productname').val() == null || $('#form_product_code').val() == null || $('#form_product_code').val() == null || $('#form_star').val() == null || $('#form_productweight').val() == null || $('#form_price').val() == null
            || $('#form_productname').val().trim() == "" || $('#form_product_code').val().trim() == "" || $('#form_product_code').val().trim() == "" || $('#form_star').val().trim() == "" || $('#form_productweight').val().trim() == "" || $('#form_price').val().trim() == ""
            || $('.img_product').length <= 0 || $('#form_selectweightunit :checked').val() < 0 || $('#form_label :checked').val() < 0
        ) {

            $("#form_btn_summit").attr("disabled", true);
            //$("#form_btn_new").removeAttr("disabled");
            $("#form_btn_new").attr("disabled", true);
        }
        else {
            $("#form_btn_new").removeAttr("disabled");
            //$("#form_btn_new").attr("disabled", true);
            $("#form_btn_summit").removeAttr("disabled");
        }
    },
    NewProductInformation: function () {
        let title = 'Thông báo xác nhận';
        let description = "Dữ liệu sản phẩm đang sửa / tạo mới hiện tại sẽ bị xóa. Bạn có chắc chắn không?";
        if ($('#form_product_code').val() != undefined || $('#form_productname').val() != undefined || $('#form_price').val() != undefined || $('#form_shipping_price').val() != undefined || $('#form_manuafacter').val() != undefined ||
            $('#form_product_code').val() != "" || $('#form_productname').val() != "" || $('#form_price').val() != "" || $('#form_shipping_price').val() != "" || $('#form_manuafacter').val() != "") {
            _msgconfirm.openDialog(title, description, function () {
                $('#form_setupmanual_main')[0].reset();
                //$(this).closest('form').find("input, textarea").val('');
                $('#form_product_code').val('');
                $('#form_label').val('1');
                _setupmanualv2.SetStar(0);
                $('#form_productspecification_div').html('');
                var add_btn = '<a class="btn btn-default white form_btn_addspecification ">+ Thêm thông số</a>';
                $('#form_productspecification_div').append(add_btn);
                $('#form_groupproductposition').html('');
                add_btn = '<a class="btn btn-default white form_btn_addpos">+ Thêm / Sửa vị trí</a>';
                $('#form_groupproductposition').append(add_btn);
                $('#form_imgsize').html('');
                add_btn = '<div id="form_imgsize_divadd" class="items import"> <label class="choose choose-wrap"> <input type="file" id="form_addimage" onchange="_setupmanualv2.OnAddImage()" name="myFile"> <div class="choose-content"> <img src="/"> <span>+ Thêm</span> </div> </label> </div>';
                $('#form_imgsize').append(add_btn);
                $('#form_amountvnd').html('');
                _setupmanualv2.ChangeButtonStatus();
                product_model = {};
                product_model.product_type = '4';
            });
        }
        $('#form_product_code').removeAttr('disabled');
        $('#form_label').removeAttr('disabled');
        $('#form_product_code').css('background-color', '');
        $('#form_label').css('background-color', '');


    },
    SummitProductManual: function () {
        let FromCreate = $('#form_setupmanual_main');
        FromCreate.validate({
            rules: {
                form_productname: "required",
                form_product_code: "required",
                form_manuafacter: "required",
                form_productlifetimestart: "required",
                form_productlifetimeend: "required",
                form_productweight: "required",
                form_price: "required",
                form_shipping_price: "required",
                form_star: "required",
            },
            messages: {
                form_product_code: "Vui lòng nhập mã sản phẩm",
                form_productname: "Vui lòng nhập tên sản phẩm",
                form_manuafacter: "Vui lòng nhập tên thương hiệu",
                form_productlifetimestart: "Vui lòng nhập vào thời gian hiệu lực bắt đầu",
                form_productlifetimeend: "Vui lòng nhập vào thời gian hiệu lực kết thúc",
                form_productweight: "Vui lòng nhập đầy đủ cân nặng",
                form_shipping_price: "Vui lòng phí vận chuyển nội địa Mỹ",
                form_price: "Vui lòng nhập giá sản phẩm",
                form_star: "Vui lòng nhập vào đánh giá sao sản phẩm",
            }
        });
        if (FromCreate.valid()) {
            if ($('.img_product_img').length < 0) {
                _magnific.error('Vui lòng thêm ít nhất 1 ảnh thể hiện sản phẩm');
                return;
            }
            if (product_model == null) product_model = {};
            product_model.product_name = $('#form_productname').val();
            product_model.product_code = $('#form_product_code').val();
            product_model.label_id = $('#form_label :checked').val();
            product_model.seller_name = $('#form_manuafacter').val();
            product_model.star = $('#form_star').val();
            product_model.product_lifetime_start = _setupmanualv2.GetCorrectDateInput($('#form_productlifetimestart').val());
            product_model.product_lifetime_end = _setupmanualv2.GetCorrectDateInput($('#form_productlifetimeend').val());
            product_model.item_weight = $('#form_productweight').val() + ' ' + $('#form_selectweightunit :checked').val();
            product_model.price = $('#form_price').val();
            product_model.shiping_fee = ($('#form_shipping_price').val() == null || $('#form_shipping_price').val() == '') ? 0 : $('#form_shipping_price').val();
            product_model.product_status = $('#form_productstatus :checked').val();
            product_model.industry_special_type = $('#form_industry_special_type :checked').val();
            product_model.product_save_price = ($('#form_discount').val() == null || $('#form_discount').val().trim() == '') ? 0 : $('#form_discount').val();
            product_model.product_specification = {};

            $.each($('.specification_div'), function () {
                var key = $(this).find('select').val();
                var value = $(this).find('input').val();
                if (key != null && value != null)
                    product_model.product_specification[key.replaceAll('_', ' ')] = value;

            });
            product_model.product_infomation_HTML = tinymce.get('form_productinfomationHTML').getContent();

            product_location = [];
            $.each($('.location_product_div'), function (index, value) {
                var obj = {};
                obj.productCode = product_model.product_code
                obj.groupProductId = $(this).find('.form_productlocation_name').attr('data-groupid');
                obj.orderNo = $(this).find('select').val();
                product_location.push(obj);
            });
            var img_source = [];
            $.each($('.img_product_img'), function () {
                img_source.push($(this).attr('src'));
            });
            product_model.amount_vnd = $('#form_fixed_amount_vnd').val().replaceAll(",", "") == undefined || $('#form_fixed_amount_vnd').val().replaceAll(",", "").trim() == '' ? '0' : $('#form_fixed_amount_vnd').val().replaceAll(",", "");
            if (parseInt(product_model.amount_vnd) > 0 && (product_model.product_type == undefined || product_model.product_type.trim() == '' || product_model.product_type==null)) {
                product_model.product_type = '4';
            }
            $.ajax({
                url: '/Product/SummitProductImages',
                type: 'POST',
                data: {
                    json: JSON.stringify(img_source),
                },
                success: function (result) {
                    if (result.status == 0) {
                        //_msgalert.success(result.msg);
                        product_model.image_product = [];
                        $.each(JSON.parse(result.data), function (index, value) {
                            product_model.image_product.push(value);
                        });

                        _setupmanualv2.SummitProductDetail();
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (jqXHR) {
                },
            });

        }
        
    },
    SummitProductDetail: function () {
        let title = 'Thành công';
        let description = "Cập nhật / Thêm mới dữ liệu thành công.";
        $.ajax({
            url: '/Product/CreateProductManual',
            type: 'POST',
            data: {
                json: JSON.stringify(product_model),
                product_location: JSON.stringify(product_location)
            },
            success: function (result) {
                if (result.status == 0) {
                  // _msgalert.success(result.msg);
                    _setupmanualv2.DisplayResultData(result.model);
                    $('#product_preview_after').show();
                    $('#product_preview_above').hide();
                    $('#form_product_code').attr('disabled', 'disabled');
                    $('#form_label').attr('disabled', 'disabled');
                    $('#form_product_code').css('background-color', '#E5E5E5');
                    $('#form_label').css('background-color', '#E5E5E5');
                    
                    _msgconfirm.alertSuccess(title, description);
                }
                else {
                    //_msgalert.error(result.msg);
                     title = 'Thất bại';
                     description = result.msg;
                    _msgconfirm.alertError(title, description);

                }
            },
            error: function (jqXHR) {
            },
        });
    },
    OnChangeFeeInput: function (amount_fixed_vnd) {
        if (parseInt(amount_fixed_vnd) > 0) {
           
        }
        else {
            let FromCreate = $('#form_setupmanual_main');
            FromCreate.validate({
                rules: {
                    form_productweight: "required",
                    form_price: "required",
                    form_shipping_price: "required",
                },
                messages: {
                    form_productweight: "Vui lòng nhập đầy đủ cân nặng",
                    form_shipping_price: "Vui lòng phí vận chuyển nội địa Mỹ",
                    form_price: "Vui lòng nhập giá sản phẩm",

                }
            });
            if (FromCreate.valid()) {

            }
            else {
                return;
            }
        }
        if (parseInt(amount_fixed_vnd) > 0 || product_fee_input.labelId != $('#form_label :checked').val() || product_fee_input.price != $('#form_price').val()
            || product_fee_input.pound != $('#form_productweight').val() || product_fee_input.pound != $('#form_productweight').val() || product_fee_input.unit != $('#form_selectweightunit').find(':selected').attr('data-id')
            || product_fee_input.shippingUSFee != $('#form_shipping_price').val()) {
            product_fee_input.labelId = $('#form_label :checked').val() == undefined ? '0' : $('#form_label :checked').val();
            product_fee_input.price = $('#form_price').val() == undefined ? '0' : $('#form_price').val();
            product_fee_input.pound = $('#form_productweight').val() == undefined ? '1' : $('#form_productweight').val();
            product_fee_input.industrySpecialType = $('#form_industry_special_type :checked').val() == undefined ? '0' : $('#form_industry_special_type :checked').val();
            product_fee_input.shippingUSFee = $('#form_shipping_price').val() == undefined || $('#form_shipping_price').val().trim() == "" ? '0' : $('#form_shipping_price').val();
            product_fee_input.product_save_price = ($('#form_discount').val() == null || $('#form_discount').val().trim() == '') ? 0 : $('#form_discount').val();
            product_fee_input.unit = $('#form_selectweightunit').find(':selected').attr('data-id') == undefined ? '1' : $('#form_selectweightunit').find(':selected').attr('data-id');
            product_fee_input.rateCurrent = rate;

            $.ajax({
                url: '/Product/GetFeeFixed',
                type: 'POST',
                data: {
                    product: product_fee_input,
                    amount_vnd: amount_fixed_vnd
                },
                success: function (result) {
                    if (result != null && result.status == '0') {
                        var product_fee_list = JSON.parse(result.data);
                        product_fee = {
                            rate: _setupmanualv2.formatMoney(product_fee_list.RATE_CURRENT, 'VND'),
                            price: _setupmanualv2.formatMoney(product_fee_list.PRODUCT_PRICE, 'USD'),
                            shiping_fee: _setupmanualv2.formatMoney(product_fee_list.SHIPPING_US_FEE, 'USD'),
                            amount_vnd: _setupmanualv2.formatMoney(Math.round(product_fee_list.TOTAL_FEE), 'VND'),
                            FIRST_POUND_FEE: _setupmanualv2.formatMoney(product_fee_list.FIRST_POUND_FEE, 'USD'),
                            NEXT_POUND_FEE: _setupmanualv2.formatMoney(product_fee_list.NEXT_POUND_FEE == null ? '0' : product_fee_list.NEXT_POUND_FEE, 'USD'),
                            LUXURY_FEE: _setupmanualv2.formatMoney(product_fee_list.LUXURY_FEE == null ? '0' : product_fee_list.LUXURY_FEE, 'USD'),
                            TOTAL_SHIPPING_FEE: _setupmanualv2.formatMoney(product_fee_list.TOTAL_SHIPPING_FEE, 'USD'),
                            PRICE_LAST: _setupmanualv2.formatMoney(product_fee_list.PRICE_LAST, 'USD'),
                            product_save_price: _setupmanualv2.formatMoney($('#form_discount').val(), 'USD')
                        };
                        $('#form_amountvnd').html('<u>' + _setupmanualv2.formatMoney(Math.round(product_fee_list.TOTAL_FEE), 'VND') + ' đ </u>');
                        $('#form_price').val(parseFloat(product_fee_list.PRODUCT_PRICE).toFixed(2));
                        $('#form_productweight').val(product_fee_list.ORIGINAL_WEIGHT.split(" ")[0]);
                        $('#form_discount').val(product_fee_input.product_save_price);
                        $("#form_selectweightunit").val(product_fee_list.ORIGINAL_WEIGHT.split(" ")[1]).trigger("change.select3");
                        $('#form_shipping_price').val(product_fee_input.shippingUSFee);
                        if (parseInt($('#form_fixed_amount_vnd').val().replaceAll(",","")) > 0) {
                            _setupmanualv2.OnFixedAmountVNDProduct('1', parseInt($('#form_fixed_amount_vnd').val().replaceAll(",","")));
                        }
                        else {
                            _setupmanualv2.OnFixedAmountVNDProduct('0', '0');
                        }
                    }
                },
                error: function (jqXHR) {
                },
            });
        }
    },

    GetDetailByASIN: function () {
        var label_id = $('#form_label :checked').val();
        ASIN = $('#form_product_code').val();
        if (product_code_filled.trim() == ASIN.trim()) {
            return;
        }
        else if (ASIN == undefined || ASIN == null || ASIN == '') {
            _msgalert.error('Vui lòng nhập mã ASIN');
            return;
        }
        label_id = $('#form_label :checked').val();
        var objData = {
            ASIN: ASIN,
            label_id: label_id,
            redirect_from_cms: redirect_from_cms
        }
        $.ajax({
            url: "/product/GetProductManualByASIN",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.status == 0 && result.model != null && result.model != "") {
                    if (redirect_from_cms == 1) {
                        _setupmanualv2.DisplayResult(result);
                        _setupmanualv2.DisplayResultData(result.model);
                        $('#product_preview_after').show();
                        $('#product_preview_above').hide();

                    }
                    else if ($('#form_productname').val() != "" || $('#form_price').val() != "" || $('#form_shipping_price').val() != "" || $('#form_manuafacter').val() != "") {
                        let title = 'Thông báo xác nhận';
                        let description = "Dữ liệu sản phẩm đang sửa / tạo mới hiện tại sẽ bị xóa. Bạn có chắc chắn không?";
                        _msgconfirm.openDialog(title, description, function () {
                            _setupmanualv2.DisplayResult(result);
                            _setupmanualv2.DisplayResultData(result.model);
                            $('#product_preview_after').show();
                            $('#product_preview_above').hide();

                        });
                    }
                    else {
                        let title = 'Thông báo xác nhận';
                        let description = "Mã sản phẩm đã tồn tại trên hệ thống, bạn có muốn hiển thị thông tin đã có sẵn không?";
                        _msgconfirm.openDialog(title, description, function () {
                            _setupmanualv2.DisplayResult(result);
                            _setupmanualv2.DisplayResultData(result.model);
                            $('#product_preview_after').show();
                            $('#product_preview_above').hide();
                        });
                    }
                    redirect_from_cms = 0;
                    $('#form_product_code').attr('disabled', 'disabled');
                    $('#form_label').attr('disabled', 'disabled');
                    $('#form_product_code').css('background-color', '#E5E5E5');
                    $('#form_label').css('background-color', '#E5E5E5');
                }
                else {

                }
                product_code_filled = ASIN;
            },
            error: function (result) {
                _msgalert.error(result.msg);
            }
        });


    },

    OpenFormAddProductLocation: function () {
        let title = 'Chọn nhóm hàng / Phiên Live Stream';
        let url = '/product/AddProductLocation';
        var checked_list = [];

        $(".form_productlocation_name").each(function () {
            var group_id = $(this).attr('data-groupid').trim();
            checked_list.push(group_id);
        });
        var param = {
            checked_list: JSON.stringify(checked_list)
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    OpenFormAddProductSpecification: function () {
        let title = 'Thêm mới kiểu thông số sản phẩm';
        let url = '/product/AddProductSpecificationType';
        var param = null;
        _magnific.OpenSmallPopup(title, url, param);
    },
    AddProductSpecificationType: function () {
        var input = $('#form_add_specificationtype_description').val();
        $.ajax({
            url: "/product/AddNewProductSpecificationType",
            type: "POST",
            data: {
                type: input
            },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    $.magnificPopup.close();
                    _setupmanualv2.DislayNewSpecificationUnit(result.data);
                }
                else {
                    _msgalert.error(result.msg);
                }
            },
            error: function (result) {
                _msgalert.error(result.msg);
            }
        });
    },
    DislayNewSpecificationUnit: function (data) {
        var option_template = '<option value="{option_value}">{option_label}</option>';
        $.each($('.specification_unit'), function () {
            $(this).append(option_template.replaceAll('{option_value}', data.description.replaceAll(' ', '_')).replaceAll('{option_label}', data.description));
        });
    },
    AddNewSpecification: function () {
        var html = '';
        var html_p1 = '<div class="form-group specification_div row mb0"> <div class="col-sm-3 col-11 mb10">';
        var html_p2 = '<select class="form-control specification_unit"> {option_list} </select>';
        var html_p3 = '</div> <div class="col-sm-8 col-11 mb10"> ' +
            '<input type="text" class="form-control specification_value " value="{specification_value}" placeholder="{specification_input}"> </div> ' +
            '<a class="delete remove_record" data-id="spec_new" href="javascript:;"  style="color: #B40000;width: 20px;line-height: 35px;text-align: center;">×</a></div>';
        var option_template = '<option value="{option_value}">{option_label}</option>';
        var option_list = '';
        $.each(product_spec, function (index, value) {
            option_list = option_list + option_template.replaceAll('{option_value}', value.description).replaceAll('{option_label}', value.description);
        });
        html = html_p1 + html_p2.replaceAll('{option_list}', option_list) + html_p3.replaceAll('{specification_value}', '').replaceAll('{specification_input}', 'Điền thông số vào khung này.');
        $('#form_productspecification_div').append(html);
        $('.form_btn_addspecification').remove();
        var add_btn = '<a class="btn btn-default white form_btn_addspecification ">+ Thêm thông số</a>';
        $('#form_productspecification_div').append(add_btn);
    },
    DisplayResult: function (result) {
        product_model = result.model;
        if (product_model != null && product_model != "") {
            $('#form_label').val(product_model.label_id).trigger("change.select2");
            switch (product_model.label_id) {
                case 2: {
                    $('#div_amount_vnd').css('display', '');
                    break;
                }
                default: {
                    $('#div_amount_vnd').css('display', 'none');
                    break;
                }
            };
            $('#form_productname').val(product_model.product_name);
            $('#form_manuafacter').val(product_model.seller_name);
            //2021-08-08T17:59:43.1214259+07:00
            $("#form_productlifetimestart").val(_setupmanualv2.ParserDate(product_model.product_lifetime_start));
            $("#form_productlifetimeend").val(_setupmanualv2.ParserDate(product_model.product_lifetime_end));

            $('#form_productstatus').val(product_model.product_status);
            $('#form_star').val(product_model.star);
            _setupmanualv2.SetStar(product_model.star);
            if (product_model.image_product == null || product_model.image_product.length < 0) {
                _setupmanualv2.DisplayImage_Product(product_model.image_size_product, true);
            }
            else {
                _setupmanualv2.DisplayImage_Product(product_model.image_product, false);
            }
            _setupmanualv2.DisplayWeight(product_model.item_weight);
            $('#form_price').val(parseFloat(product_model.price).toFixed(2));
            $('#form_shipping_price').val(product_model.shiping_fee);
            $('#form_industry_special_type').val(product_model.industry_special_type);
            $('#form_discount').val(product_model.product_save_price);
            $('#form_amountvnd').html('<u>' + _setupmanualv2.formatMoney(Math.round(product_model.amount_vnd), 'VND') + ' đ</u>');
            _setupmanualv2.ParseFeeDetail(product_model.list_product_fee, product_model.rate, product_model.product_save_price);
            _setupmanualv2.DisplayProductSpecification(product_model.product_specification);
            _setupmanualv2.DisplayProductInfomationHTML(product_model.product_infomation_HTML);
            _setupmanualv2.DisplayProductLocation(product_model.product_code, product_model.group_product_id);
            if (product_model.product_type == '4') {
                _setupmanualv2.OnFixedAmountVNDProduct('1', product_model.amount_vnd);
            }
            else {
                _setupmanualv2.OnFixedAmountVNDProduct('0', product_model.amount_vnd);
            }
            $('#form_createdate').val(_setupmanualv2.GetShowTextDateInput(product_model.create_date));
            $('#form_updatelast').val(_setupmanualv2.GetShowTextDateInput(product_model.update_last));
        }
        _setupmanualv2.ChangeButtonStatus();

    },
    OpenProductFeeBox: function () {
        let title = 'Chi tiết phí mua hộ';
        let url = '/product/ProductFee';
        var param = product_fee;
        _magnific.OpenSmallPopup(title, url, param);
    },
    OnSelectedProductLocation: function () {
        var _categories = {};
        var _new = [];
        if ($('.ckb-news-cate:checked').length > 0) {
            $('.ckb-news-cate:checked').each(function () {
                _categories = {};
                _categories.groupProductId = $(this).val();
                _categories.group_product_name = $(this).parent().text();
                if (_categories.groupProductId == null || _categories.groupProductId.trim() == "" || _categories.group_product_name == null || _categories.group_product_name == "") {

                }
                else {
                    var is_added = 1;
                    for (const element of product_location) {
                        if (element.groupProductId == _categories.groupProductId) {
                            _categories = element;
                            is_added = 0;
                            break;
                        }
                    }
                    if (is_added == 1) { _categories.orderNo = 1; }
                    _new.push(_categories);
                }
            });
            product_location = _new;
            _setupmanualv2.ParseProductLocationToForm();
        }
    },
    DisplayProductInfomationHTML: function (product_infomation) {
        if (product_infomation != null && product_infomation.trim() != "") {
            tinymce.get('form_productinfomationHTML').setContent(product_infomation);
            $('#form_productinfomationHTML').html(product_infomation);
        }
    },
    DisplayProductLocation: function (product_code,group_id) {
        var objData = {
            product_code: product_code,
            group_id: group_id
        };
        $.ajax({
            url: "/product/GetProductLocation",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.status == 0) {
                    product_location = result.data;
                    _setupmanualv2.ParseProductLocationToForm();
                }
                else {
                    return null;
                }

            },
            error: function (result) {
                return null;
            }
        });
    },
    ParseProductLocationToForm: function () {
        $('#form_groupproductposition').html('');
        var html_template = '<div id="pl_{plid}" class="form-group row row_min mb0 location_product_div" style="width: 100%;"><div class="col-sm-6 mb10">' +
            ' <input id="pl_{group_id}_groupname" type="text" data-groupid="{group_id}" class="form-control disabled form_productlocation_name" value="{group_name}"> </div>' +
            '<div class="col-sm-2 col-11 mb10"><select id="pl_{plid}_pos" class="form-control">{pos_order_list}</select></div>' +
            '<a class="delete remove_record" data-id="location_{plid}" href="javascript:;" style="color: #B40000;width: 20px;line-height: 35px; text-align: center;">×</a>';
        var order_select_option_html = _setupmanualv2.LoadHTMLOrderNumberList();
        if (product_location != null && product_location.length > 0) {
            $.each(product_location, function (index, value) {
                var html = '';
                html = html_template.replaceAll('{plid}', value.groupProductId).replaceAll('{group_name}', value.group_product_name).replaceAll('{group_id}', value.groupProductId).replaceAll('{pos_order_list}', order_select_option_html);
                $('#form_groupproductposition').append(html);
                $('#pl_' + value.groupProductId + ' select').val(value.orderNo);
            });
        }
        $(".form_btn_addpos").remove();
        var add_btn = '<a class="btn btn-default white form_btn_addpos">+ Thêm / Sửa vị trí</a>';
        $('#form_groupproductposition').append(add_btn);
        $.magnificPopup.close();
    },
    LoadHTMLOrderNumberList: function () {
        var html = '';
        var template = '<option value="{place}">{place}</option>';
        for (var i = 1; i <= 20; i++) {
            html = html + template.replaceAll('{place}', i);
        }
        return html;
    },

    DisplayProductSpecification: function (product_specification) {
        $('#form_productspecification_div').html('');
        var html = '';
        var html_p1 = '<div id="{spec_key}" class="form-group specification_div row mb0"> <div class="col-sm-3 col-11 mb10">';
        var html_p2 = '<select class="form-control specification_unit"> {option_list} </select>';
        var html_p3 = '</div> <div class="col-sm-8 col-11 mb10"> ' +
            '<input type="text" class="form-control specification_value " value="{specification_value}" placeholder=""> </div> ' +
            '<a class="delete remove_record" data-id="spec_{spec_key}" href="javascript:;"  style="color: #B40000;width: 20px;line-height: 35px;text-align: center;">×</a></div>';
        var option_template = '<option value="{option_value}">{option_label}</option>';
        var option_list = '';
        _setupmanualv2.LoadSpecificationUnit(function (spec_unit) {
            $.each(spec_unit, function (index, value) {
                option_list = option_list + option_template.replaceAll('{option_value}', value.description.trim()).replaceAll('{option_label}', value.description);
            });
            if (product_specification != null)
                $.each(product_specification, function (key, value) {
                    html = html_p1.replaceAll('{spec_key}', key.trim().replaceAll(' ', '_')) + html_p2.replaceAll('{option_list}', option_list) + html_p3.replaceAll('{specification_value}', value).replaceAll('{spec_key}', key);
                    $('#form_productspecification_div').append(html);
                    $('#' + key.trim().replaceAll(' ', '_') + ' select').val(key.trim());
                });
            $('.form_btn_addspecification').remove();
            html = '<a class="btn btn-default white form_btn_addspecification">+ Thêm thông số</a>';
            $('#form_productspecification_div').append(html);
        });

    },
    LoadSpecificationUnit: function (callback) {
        $.ajax({
            url: "/product/GetSpecificationUnit",
            type: "GET",
            success: function (result) {
                if (result.status == 0) {
                    product_spec = result.data;
                    callback(result.data);
                }
                else {
                    return null;
                }

            },
        });
    },
    LoadSpecificationUnitDependency: function () {
        $.ajax({
            url: "/product/GetSpecificationUnit",
            type: "GET",
            success: function (result) {
                if (result.status == 0) {
                    product_spec = result.data;
                }
                else {
                    return null;
                }

            },
        });
    },
    ParseFeeDetail: function (fee_list, rate, save_price) {

        product_fee_input.labelId = $('#form_label :checked').val();
        product_fee_input.price = $('#form_price').val();
        product_fee_input.pound = $('#form_productweight').val();
        product_fee_input.unit = $('#form_selectweightunit').find(':selected').attr('data-id');
        product_fee_input.industrySpecialType = $('#form_industry_special_type :checked').val();
        product_fee_input.shippingUSFee = $('#form_shipping_price').val();

        product_fee = {
            rate: _setupmanualv2.formatMoney(rate, 'VND'),
            price: _setupmanualv2.formatMoney(fee_list.price, 'USD'),
            shiping_fee: _setupmanualv2.formatMoney(fee_list.shiping_fee, 'USD'),
            amount_vnd: _setupmanualv2.formatMoney(fee_list.amount_vnd, 'VND'),
            FIRST_POUND_FEE: _setupmanualv2.formatMoney(fee_list.list_product_fee.FIRST_POUND_FEE, 'USD'),
            NEXT_POUND_FEE: _setupmanualv2.formatMoney(fee_list.list_product_fee.NEXT_POUND_FEE == null ? '0' : fee_list.list_product_fee.NEXT_POUND_FEE, 'USD'),
            LUXURY_FEE: _setupmanualv2.formatMoney(fee_list.list_product_fee.LUXURY_FEE == null ? '0' : fee_list.list_product_fee.LUXURY_FEE, 'USD'),
            TOTAL_SHIPPING_FEE: _setupmanualv2.formatMoney(fee_list.list_product_fee.TOTAL_SHIPPING_FEE, 'USD'),
            PRICE_LAST: _setupmanualv2.formatMoney(fee_list.list_product_fee.PRICE_LAST, 'USD'),
            product_save_price: _setupmanualv2.formatMoney(save_price, 'USD')
        };
    },
    DisplayWeight: function (item_weight) {
        if (item_weight != null && item_weight != "") {
            var x = item_weight.split(" ");
            var weight_unit = 'pound';
            switch (x[1].trim().toLowerCase()) {
                case 'oz': weight_unit = 'ounces';
                    break;
                case 'lbs': weight_unit = 'pound';
                    break;
                default: weight_unit = x[1].trim().toLowerCase();
                    break;
            }
            $('#form_productweight').val(x[0]);
            $("#form_selectweightunit").val(weight_unit);
        }
    },
    DisplayImage_Product: function (data, is_size_image) {
        if (!is_size_image) {
            $('#form_imgsize').html('');
            var html = '<div class="col-md-3 mb10"> <div class="choose-ava"> <img class="img_product_img" src="{image_url}"> <button type="button" class="delete img_product_del">×</button> </div> </div>';
            $.each(data, function (index, value) {
                $('#form_imgsize').append(html.replaceAll('{image_url}', value));
            });
        } else {
            $('#form_imgsize').html('');
            var html = '<div class="col-md-3 mb10"> <div class="choose-ava"> <img class="img_product_img" src="{image_url}"> <button type="button" class="delete img_product_del">×</button> </div> </div>';
            $.each(data, function (index, value) {
                $('#form_imgsize').append(html.replaceAll('{image_url}', value.larger));
            });
        }
        var template = '<div id="form_imgsize_divadd" class="col-md-3 mb10"> <divclass="items import"> <label class="choose choose-wrap"> <input type="file" id="form_addimage" onchange="_setupmanualv2.OnAddImage()" name="myFile"> <div class="choose-content"> <img src="/images/icons/add_img.jpg"> <span>+ Thêm</span> </div> </label> </div> </div>';
        $('#form_imgsize').append(template);

    },
    OnAddImage: function (input) {
        var _validFileExtensions = ["jpg", "jpeg", "bmp", "gif", "png", "JPG", "JPEG", "BMP", "GIF", "PNG"];
        var file = $('#form_addimage').get(0).files[0];
        if (file) {
            var reader = new FileReader();
            if (file) {
                var fileType = file.name.split('.').pop();
                if (_validFileExtensions.includes(fileType)) {
                    reader.onload = function () {
                        var html = '<div class="col-md-3 mb10"> <div class="choose-ava"> <img class="img_product_img" src="{image_url}"> <button type="button" class="delete img_product_del">×</button> </div> </div>';
                        $('#form_imgsize').append(html.replaceAll('{image_url}', reader.result));
                        $('#form_imgsize_divadd').remove();
                        var template = '<div id="form_imgsize_divadd" class="col-md-3 mb10"> <divclass="items import"> <label class="choose choose-wrap"> <input type="file" id="form_addimage" onchange="_setupmanualv2.OnAddImage()" name="myFile"> <div class="choose-content"> <img src="/images/icons/add_img.jpg"> <span>+ Thêm</span> </div> </label> </div> </div>';
                        $('#form_imgsize').append(template);
                    }
                    reader.readAsDataURL(file);
                } else {
                    _msgalert.error('File upload phải thuộc các định dạng sau: jpg, jpeg, bmp, gif, png');
                    $('#form_addimage').val('');
                }
            }

        }
    },
    ParserDate: function (datetime_string) {
        if (datetime_string == null || datetime_string.trim() == '') {
            var dt = new Date();
            return '' + dt.getDay + '/' + dt.getMonth + '/' + dt.getFullYear + '';
        }
        var a = datetime_string.trim().split("T");
        var b = a[0].split("-");
        return b[2].trim() + "/" + b[1].trim() + "/" + b[0].trim();
    },
    SetStar: function (item) {
        var value = parseInt(item)
        switch (value) {
            case 0: {
                $('#star_1').attr('class', 'fa fa-star-o');
                $('#star_2').attr('class', 'fa fa-star-o');
                $('#star_3').attr('class', 'fa fa-star-o');
                $('#star_4').attr('class', 'fa fa-star-o');
                $('#star_5').attr('class', 'fa fa-star-o');
                if (item > 0 && item < 1) {
                    $('#star_1').attr('class', 'fa fa-star-half-o');
                    $("#star_1").css({ color: '#ffab00' });

                }
                $('#star_2').css('color', 'none');
                $('#star_3').css('color', 'none');
                $('#star_4').css('color', 'none');
                $('#star_5').css('color', 'none');
                break;
            }
            case 1:
                star = 1;
                $('#star_1').attr('class', 'fa fa-star');
                $('#star_2').attr('class', 'fa fa-star-o');
                $('#star_3').attr('class', 'fa fa-star-o');
                $('#star_4').attr('class', 'fa fa-star-o');
                $('#star_5').attr('class', 'fa fa-star-o');
                if (item > 1 && item < 2) {
                    $('#star_2').attr('class', 'fa fa-star-half-o');
                    $("#star_2").css({ color: '#ffab00' });
                }
                $('#star_1').css('color', 'none');
                $('#star_3').css('color', 'none');
                $('#star_4').css('color', 'none');
                $('#star_5').css('color', 'none');
                break;
            case 2:
                star = 2;
                $('#star_1').attr('class', 'fa fa-star');
                $('#star_2').attr('class', 'fa fa-star');
                $('#star_3').attr('class', 'fa fa-star-o');
                $('#star_4').attr('class', 'fa fa-star-o');
                $('#star_5').attr('class', 'fa fa-star-o');
                if (item > 2 && item < 3) {
                    $('#star_3').attr('class', 'fa fa-star-half-o');
                    $("#star_3").css({ color: '#ffab00' });
                }
                $('#star_2').css('color', 'none');
                $('#star_1').css('color', 'none');
                $('#star_4').css('color', 'none');
                $('#star_5').css('color', 'none');
                break;
            case 3:
                star = 3;
                $('#star_1').attr('class', 'fa fa-star');
                $('#star_2').attr('class', 'fa fa-star');
                $('#star_3').attr('class', 'fa fa-star');
                $('#star_4').attr('class', 'fa fa-star-o');
                $('#star_5').attr('class', 'fa fa-star-o');
                if (item > 3 && item < 4) {
                    $('#star_4').attr('class', 'fa fa-star-half-o');
                    $("#star_4").css({ color: '#ffab00' });

                }
                $('#star_2').css('color', 'none');
                $('#star_3').css('color', 'none');
                $('#star_1').css('color', 'none');
                $('#star_5').css('color', 'none');
                break;
            case 4:
                star = 4;
                $('#star_1').attr('class', 'fa fa-star');
                $('#star_2').attr('class', 'fa fa-star');
                $('#star_3').attr('class', 'fa fa-star');
                $('#star_4').attr('class', 'fa fa-star');
                $('#star_5').attr('class', 'fa fa-star-o');
                if (item > 4 && item < 5) {
                    $('#star_5').attr('class', 'fa fa-star-half-o');
                    $("#star_5").css({ color: '#ffab00' });
                }
                $('#star_2').css('color', 'none');
                $('#star_3').css('color', 'none');
                $('#star_4').css('color', 'none');
                $('#star_1').css('color', 'none');
                break;
            case 5:
                star = 5;
                $('#star_1').attr('class', 'fa fa-star');
                $('#star_2').attr('class', 'fa fa-star');
                $('#star_3').attr('class', 'fa fa-star');
                $('#star_4').attr('class', 'fa fa-star');
                $('#star_5').attr('class', 'fa fa-star');
                break;
                $('#star_1').css('color', 'none');
                $('#star_2').css('color', 'none');
                $('#star_3').css('color', 'none');
                $('#star_4').css('color', 'none');
                $('#star_5').css('color', 'none');
        }
    },
    formatMoney: function (number, currency) {
        if (number == null) {
            return 0;
        }
        switch (currency) {
            case 'VND': {
                return number.toLocaleString('en-US', { style: 'currency', currency: 'VND' }).replace("₫", "");
            } break;
            case 'USD': {
                return number.toLocaleString('en-US', { style: 'currency', currency: 'USD' }).replace("$", "")
            }

        }
    },
    OnDoCrawl: function () {
        var _group_id = '344';
        var _LabelId = $('#form_label :checked').val();
        var _LinkStoreMenu = '';
        switch (_LabelId) {
            case '1': {
                _LinkStoreMenu = 'https://www.amazon.com/dp/' + $('#form_product_code').val();
                if (_group_id == "" || _group_id == null || _LabelId == "" || _LabelId == null || _LinkStoreMenu == "" || _LinkStoreMenu == null) {
                    _msgalert.error("Vui lòng điền đẩy đủ thông tin, sau đó thử lại");
                    return;
                }
                var item = {
                    groupProductid: _group_id,
                    labelid: _LabelId,
                    linkdetail: _LinkStoreMenu
                };
                $.ajax({
                    url: '/Campaign/DoCrawl',
                    type: 'POST',
                    data: {
                        item: item
                    },
                    success: function (result) {
                        if (result.code == 1) {
                            _msgalert.success(result.message);
                        }
                    },
                    error: function (jqXHR) {
                    },
                    complete: function (jqXHR, status) {
                    }
                });
            } break;
            default: break;
        }
    },
    OpenLockPopup: function () {
        let title = 'Khoá / Chặn sản phẩm';
        let url = '/product/FormLockManual';
        let param = {};
        _magnific.OpenSmallPopupWithHeader(title, url, param);
    },
    OnLockedProduct: function () {
        let FromCreate = $('#form_lockproductform');
        FromCreate.validate({
            rules: {
                form_lockproductaction: "required",
                form_label: "required",
                asin_list: "required",

            },
            messages: {
                form_product_code: "Vui lòng chọn trạng thái",
                form_label: "Vui lòng chọn nhãn hàng",
                asin_list: "Vui lòng nhập 1 hoặc nhiều mã sản phẩm",
            }
        });
        if (FromCreate.valid()) {
            var action_id = $('input[name="form_lockproductaction"]:checked').val();
            var label_id = $('#form_label :checked').val();
            var asin_list = $('#asin_list').val();
            switch (action_id) {
                case '-1': this.LockProductManual(asin_list, label_id, '1'); break;
                case '0': this.BlockProductManual(asin_list, label_id, '0'); break;
                case '1': this.BlockProductManual(asin_list, label_id, '1'); break;
                case '2': this.LockProductManual(asin_list, label_id, '0'); break;
                default: break;
            }
        }
    },
    BlockProductManual: function (ASIN, label_id, isblocked) {
        if (ASIN == null || ASIN == undefined || ASIN == '' || label_id < 1 || isblocked < 0 || isblocked > 1) {
            _msgalert.error("Vui lòng nhập đầy đủ Mã sản phẩm và chọn nhãn hàng");
            return;
        }
        let title = 'Thông báo xác nhận';
        let description = "Bạn có chắc chắn muốn chặn các sản phẩm này không?";
        if (isblocked == '0') description = "Bạn có chắc chắn muốn bỏ chặn các sản phẩm này không?";
        _msgconfirm.openDialog(title, description, function () {
            var objData = {
                product_code_list: ASIN,
                label_id: label_id,
                target_status: isblocked
            }
            $.ajax({
                url: "/product/BlockProductManual",
                type: "POST",
                data: objData,
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                    } else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
        });
    },
    LockProductManual: function (ASIN, label_id, islocked) {
        if (ASIN == null || ASIN == undefined || ASIN == '' || islocked < 0 || islocked > 1) {
            _msgalert.error("Bạn chưa nhập mã ASIN");
            return;
        }
        let title = 'Thông báo xác nhận';
        let description = "Bạn có chắc chắn muốn khoá các sản phẩm này không?";
        if (islocked == '0') description = "Bạn có chắc chắn muốn mở khoá các sản phẩm này không?";
        _msgconfirm.openDialog(title, description, function () {
            var objData = {
                product_code_list: ASIN,
                label_id: label_id,
                target_status: islocked
            }
            $.ajax({
                url: "/product/LockProductManual",
                type: "POST",
                data: objData,
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                    } else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
        });
    },
    DisplayResultData: function (model) {
        var resultProductModel = model;
        if (resultProductModel != null && resultProductModel != "") {
            $('#title_review').html(model.product_name)
            $('#seller_name_review').html(model.seller_name)
            $('#link_amazon').attr('href', "http://dropshipping.x-tech.vn" + model.link_product)
            $('#price_review').html(_setupmanualv2.formatMoney(model.price, 'USD') + ' $')
            $('#ship_price_us_review').html(model.shiping_fee + ' $')
            var number = parseFloat(model.list_product_fee.list_product_fee.FIRST_POUND_FEE);
            if (number > 0) {
                $('#shipping_details_first_pound').html(model.list_product_fee.list_product_fee.FIRST_POUND_FEE + ' $')
            }
            number = parseFloat(model.list_product_fee.list_product_fee.NEXT_POUND_FEE);
            if (number > 0) {
                $('#shipping_details_next_pounds').html(model.list_product_fee.list_product_fee.NEXT_POUND_FEE + ' $')
            }
            number = parseFloat(model.list_product_fee.list_product_fee.LUXURY_FEE);
            if (number > 0) {
                $('#shipping_details_luxury_pound').html(model.list_product_fee.list_product_fee.LUXURY_FEE + ' $')
            }

            number = parseFloat(model.list_product_fee.list_product_fee.TOTAL_SHIPPING_FEE)
            if (number > 0) {
                $('#shipping_details_total_shipping_fee').html(model.list_product_fee.list_product_fee.TOTAL_SHIPPING_FEE + ' $')
            }
            number = parseFloat(model.list_product_fee.list_product_fee.PRICE_LAST)
            if (number > 0) {
                $('#price_last').html(_setupmanualv2.formatMoney(model.list_product_fee.list_product_fee.PRICE_LAST, 'USD') + ' $')
            }
            $('#price_last_vnd').html(_setupmanualv2.formatMoney(model.list_product_fee.amount_vnd, 'VND') + ' đ');
            number = parseFloat(model.list_product_fee.list_product_fee.rating)
            if (number > 0) {
                $('#rating').html(model.list_product_fee.list_product_fee.rating);
            }
            _setupmanualv2.SetResultStar(model.star);
        }
    },
    SetResultStar: function (star_point) {
        var value = parseInt(star_point);
        switch (value) {
            case 0: {
                $('#1star').attr('class', 'fa fa-star-o');
                $('#2star').attr('class', 'fa fa-star-o');
                $('#3star').attr('class', 'fa fa-star-o');
                $('#4star').attr('class', 'fa fa-star-o');
                $('#5star').attr('class', 'fa fa-star-o');
                if (star_point > 0 && star_point < 1) {
                    $('#1star').attr('class', 'fa fa-star-half-o');
                    $("#1star").css({ color: '#ffab00' });

                }
                $('#2star').css('color', 'none');
                $('#3star').css('color', 'none');
                $('#4star').css('color', 'none');
                $('#5star').css('color', 'none');
                break;
            }
            case 1:
                star = 1;
                $('#1star').attr('class', 'fa fa-star');
                $('#2star').attr('class', 'fa fa-star-o');
                $('#3star').attr('class', 'fa fa-star-o');
                $('#4star').attr('class', 'fa fa-star-o');
                $('#5star').attr('class', 'fa fa-star-o');
                if (star_point > 1 && star_point < 2) {
                    $('#2star').attr('class', 'fa fa-star-half-o');
                    $("#2star").css({ color: '#ffab00' });
                }
                $('#1star').css('color', 'none');
                $('#3star').css('color', 'none');
                $('#4star').css('color', 'none');
                $('#5star').css('color', 'none');
                break;
            case 2:
                star = 2;
                $('#1star').attr('class', 'fa fa-star');
                $('#2star').attr('class', 'fa fa-star');
                $('#3star').attr('class', 'fa fa-star-o');
                $('#4star').attr('class', 'fa fa-star-o');
                $('#5star').attr('class', 'fa fa-star-o');
                if (star_point > 2 && star_point < 3) {
                    $('#3star').attr('class', 'fa fa-star-half-o');
                    $("#3star").css({ color: '#ffab00' });
                }
                $('#2star').css('color', 'none');
                $('#1star').css('color', 'none');
                $('#4star').css('color', 'none');
                $('#5star').css('color', 'none');
                break;
            case 3:
                star = 3;
                $('#1star').attr('class', 'fa fa-star');
                $('#2star').attr('class', 'fa fa-star');
                $('#3star').attr('class', 'fa fa-star');
                $('#4star').attr('class', 'fa fa-star-o');
                $('#5star').attr('class', 'fa fa-star-o');
                if (star_point > 3 && star_point < 4) {
                    $('#4star').attr('class', 'fa fa-star-half-o');
                    $("#4star").css({ color: '#ffab00' });

                }
                $('#2star').css('color', 'none');
                $('#3star').css('color', 'none');
                $('#1star').css('color', 'none');
                $('#5star').css('color', 'none');
                break;
            case 4:
                star = 4;
                $('#1star').attr('class', 'fa fa-star');
                $('#2star').attr('class', 'fa fa-star');
                $('#3star').attr('class', 'fa fa-star');
                $('#4star').attr('class', 'fa fa-star');
                $('#5star').attr('class', 'fa fa-star-o');
                if (star_point > 4 && star_point < 5) {
                    $('#5star').attr('class', 'fa fa-star-half-o');
                    $("#5star").css({ color: '#ffab00' });
                }
                $('#2star').css('color', 'none');
                $('#3star').css('color', 'none');
                $('#4star').css('color', 'none');
                $('#1star').css('color', 'none');
                break;
            case 5:
                star = 5;
                $('#1star').attr('class', 'fa fa-star');
                $('#2star').attr('class', 'fa fa-star');
                $('#3star').attr('class', 'fa fa-star');
                $('#4star').attr('class', 'fa fa-star');
                $('#5star').attr('class', 'fa fa-star');
                $('#1star').css('color', 'none');
                $('#2star').css('color', 'none');
                $('#3star').css('color', 'none');
                $('#4star').css('color', 'none');
                $('#5star').css('color', 'none');
           break;
        }
       
    },
    DeleteProductOnES: function () {
        $('.create_spin').show();
        var ASIN = $('#form_product_code').val()
        if (ASIN == undefined || ASIN == null || ASIN == '') {
            _msgalert.error('Vui lòng nhập mã ASIN');
            $('.create_spin').hide();
            return;
        }
        var label_id = $('#form_label :checked').val();
        let title = 'Thông báo xác nhận';
        let description = "Thông tin trên ES của Product ID: " + ASIN + " sẽ bị xoá. Bạn có chắc chắn không?";
        _msgconfirm.openDialog(title, description, function () {
            var objData = {
                ASIN: ASIN,
                label_id: label_id,
            }
            $.ajax({
                url: "/product/DeleteProductOnES",
                type: "POST",
                data: objData,
                success: function (result) {
                    _msgalert.success(result.msg);

                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
        });
        $('.create_spin').hide();
    },
    DeleteProductOnRedis: function () {
        $('.create_spin').show();
        var ASIN = $('#form_product_code').val()
        if (ASIN == undefined || ASIN == null || ASIN == '') {
            _msgalert.error('Vui lòng nhập mã ASIN');
            $('.create_spin').hide();
            return;
        }
        var label_id = $('#form_label :checked').val();
        let title = 'Thông báo xác nhận';
        let description = "Thông tin trên Redis của Product ID: " + ASIN + " sẽ bị xoá. Bạn có chắc chắn không?";
        _msgconfirm.openDialog(title, description, function () {
            var objData = {
                ASIN: ASIN,
                label_id: label_id,
            }
            $.ajax({
                url: "/product/DeleteProductOnRedis",
                type: "POST",
                data: objData,
                success: function (result) {
                    _msgalert.success(result.msg);

                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
        });
        $('.create_spin').hide();
    },
    OnFixedAmountVNDProduct: function (isfixed, amount_vnd) {
        switch (isfixed) {
            case '1': {
                product_model.product_type = '4';
                $('#form_fixed_amount_vnd').val(amount_vnd).trigger("change");
                formatCurrency($('#form_fixed_amount_vnd'));
            } break;
            default: {
                product_model.product_type = '1';
                $('#form_fixed_amount_vnd').val('0').trigger("change");
                formatCurrency($('#form_fixed_amount_vnd'));
            } break;
        }
    },
    SyncManualProduct: function () {
        let title = 'Thông báo xác nhận';
        let description = "Tất cả các sản phẩm được nhập thủ công sẽ được đồng bộ, bạn có muốn thực hiện không?";
        _msgconfirm.openDialog(title, description, function () {
            var objData = {
            };
            $.ajax({
                url: "/product/SyncProductMongoDB",
                type: "POST",
                data: objData,
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                    } else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
        });
    },
    SyncPrice: function () {
        let title = 'Thông báo xác nhận';
        let description = "Tất cả các sản phẩm được nhập thủ công sẽ được cập nhật, bạn có muốn thực hiện không?";
        _msgconfirm.openDialog(title, description, function () {
            var objData = {
            };
            $.ajax({
                url: "/product/SyncFixedPrice",
                type: "POST",
                data: objData,
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                    } else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
        });
    },
    GetCorrectDateInput: function (date_str) {
        var dt = date_str.split("/");
        //"2017-09-08T19:01:55.714942+03:00"
        if (dt.length >= 3) {
            return dt[2] + "-" + dt[1] + "-" + dt[0];
        }
    },
    GetShowTextDateInput: function (date_str) {
        var date = new Date(Date.parse(date_str));
        //"2017-09-08T19:01:55.714942+03:00"
        return (date.getUTCDate() < 10 ? '0' : '') + date.getUTCDate() + "/" + ((date.getMonth() + 1) < 10 ? '0' : '') + (date.getMonth() + 1) + "/" + date.getFullYear() + " " + (date.getHours() < 10 ? '0' : '') + date.getHours() + ":" + (date.getMinutes() < 10 ? '0' : '') + date.getMinutes() + ":" + (date.getSeconds() < 10 ? '0' : '') + date.getSeconds();
    }
}

