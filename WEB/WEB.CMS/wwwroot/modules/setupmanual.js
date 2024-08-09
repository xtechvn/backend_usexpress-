var star = 0;
var product_object = null;
var redirect_from_cms = 0;
$(document).ready(function () {
    $('.create_spin').hide();
    $('#crawl').hide();
    $('#save').hide();
    $('#getDetail').hide();
    $('#edit').prop('disabled', true);
    $('#btn_create').prop('disabled', true);

    $('.summit_form1').prop('disabled', true);
    if ($('#ASIN').val() != null && $('#ASIN').val() != "") {
        redirect_from_cms = 1;
        $('#edit').click();
    }
});

$('.s_star_select').on('click', function (event) {
    var star_point = parseInt(event.target.id);
    SelectStar(star_point);
});
$(".tab-link").click(function () {
    $(".tab-link").removeClass("active");
    if (!$(this).hasClass("active")) {
        $(this).addClass("active");
    } else {
        $(this).removeClass("active");
    }
    var tabid = $(this).data("id");
    $(".item-tab-content").css("display", "none");
    $(".item-tab-content[data-id='" + tabid + "']").fadeIn();
});
var menu = {
    GetDetailByAsin: function () {
        if ($('#product_name').val() != "" || $('#price').val() != "" || $('#shiping_fee').val() != "" || $('#image_thumb').val() != "" || $('#seller_id').val() != "" || $('#seller_name').val() != "")
        {
            let title = 'Thông báo xác nhận';
            let description = "Dữ liệu sản phẩm đang sửa / tạo mới hiện tại sẽ bị xóa. Bạn có chắc chắn không?";
            _msgconfirm.openDialog(title, description, function () {
                var ASIN = $('#ASIN').val()
                if (ASIN == undefined || ASIN == null || ASIN == '') {
                    _msgalert.error('Vui lòng nhập mã ASIN');
                    $('.create_spin').hide();
                    return;
                }
                var label_id = $('input[name="optradio"]:checked').val();

                $('.create_spin').show();
                var objData = {
                    ASIN: ASIN,
                    label_id: label_id,
                    data_type: 1,
                    redirect_from_cms: redirect_from_cms
                }
                $.ajax({
                    url: "/product/GetProductManualByASIN",
                    type: "POST",
                    data: objData,
                    success: function (result) {
                        $('.summit_form1').prop('disabled', false);
                        if (result.status == 0 && result.model != null && result.model != "") {
                            DisplayResultData(result);
                            ParseResultToForm(result.model);
                            GetGroupProductName(result.model.group_product_id);
                            ParseLockStatus(result.model.page_not_found);
                            $('#product_preview_after').show();
                            $('#product_preview_above').hide();
                        }
                        else {
                            _msgalert.error(result.msg);
                        }
                    },
                    error: function (result) {
                        _msgalert.error(result.msg);
                    }
                });
                $('#btn_create').prop('disabled', false);
                $('.create_spin').hide();
            });
        }
        else {
            var ASIN = $('#ASIN').val()
            if (ASIN == undefined || ASIN == null || ASIN == '') {
                _msgalert.error('Vui lòng nhập mã ASIN');
                $('.create_spin').hide();
                return;
            }
            var label_id = $('input[name="optradio"]:checked').val();

            $('.create_spin').show();
            var objData = {
                ASIN: ASIN,
                label_id: label_id,
                data_type: 1,
                redirect_from_cms: redirect_from_cms
            }
            $.ajax({
                url: "/product/GetProductManualByASIN",
                type: "POST",
                data: objData,
                success: function (result) {
                    $('.summit_form1').prop('disabled', false);
                    if (result.status == 0 && result.model != null && result.model != "") {
                        ParseResultToForm(result.model);
                        DisplayResultData(result);
                        GetGroupProductName(result.model.group_product_id);
                        ParseLockStatus(result.model.page_not_found);
                        $('#product_preview_after').show();
                        $('#product_preview_above').hide();
                    }
                    else {
                        _msgalert.error(result.msg);
                        
                    }
                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
            $('#btn_create').prop('disabled', false);
            $('.create_spin').hide();
        }
        redirect_from_cms = 0;
    },
    CreateNewManualProduct: function () {
        let title = 'Thông báo xác nhận';
        let description = "Dữ liệu sản phẩm đang sửa / tạo mới hiện tại sẽ bị xóa. Bạn có chắc chắn không?";
        _msgconfirm.openDialog(title, description, function () {          
            $('.summit_form1').prop('disabled', false);
            $('#product_name').val('');
            $('#page_not_found').prop('checked', false);
            $('#price').val('');
            $('#shiping_fee').val('');
            $('#discount').val('');
            $('#amount').val('');
            $('#amount_vnd').val('');
            $('#image_thumb').val('');
            $('#link_product').val('');
            $('#amount_vnd').val('');
            $('#seller_id').val('');
            $('#seller_name').val('');
            $('#item_weight').val('');
            SelectStar(0);
            $("#shipping_weight_unit").val('1');
            $('#rating').val('');
            $('#reviews_count').val('');
            $("#special_industry").val('-1');
            $('#image_size_product').val('');
            $('#product_preview_after').hide();
            $('#product_preview_above').show();
            $('#img_thumb_preview').prop('src', '');
            $('#group_name').val('');
            product_object = null;
            ParseLockStatus(false);

        });
       
    },
    DeleteProductOnES: function () {
        $('.create_spin').show();
        var ASIN = $('#ASIN').val()
        if (ASIN == undefined || ASIN == null || ASIN == '') {
            _msgalert.error('Vui lòng nhập mã ASIN');
            $('.create_spin').hide();
            return;
        }
        var label_id = $('input[name="optradio"]:checked').val();
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
        var ASIN = $('#ASIN').val()
        if (ASIN == undefined || ASIN == null || ASIN == '') {
            _msgalert.error('Vui lòng nhập mã ASIN');
            $('.create_spin').hide();
            return;
        }
        var label_id = $('input[name="optradio"]:checked').val();
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
    CheckOut: function () {
        var validate_data = ValidateAndGetValue();
        if (validate_data == "success") {
            $('.create_spin').show();
            $.ajax({
                url: '/Product/CheckOut',
                type: 'POST',
                data: {
                    json: JSON.stringify(product_object),
                },
                success: function (result) {
                    $('.create_spin').hide();
                    if (result.status == 0) {
                        $('#product_preview_after').show();
                        $('#product_preview_above').hide();
                        DisplayResultData(result);
                        ParseResultToForm(result.model);
                        ParseLockStatus(result.model.page_not_found);
                        _msgalert.success(result.msg);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (jqXHR) {
                },
                complete: function (jqXHR, status) {
                }
            });
            $('.create_spin').hide();

        }
    },
    CreateProductManual: function () {
        var validate_data = ValidateAndGetValue();
        if (validate_data == "success") {
            $('.create_spin').show();
            $.ajax({
                url: '/Product/CreateProductManual',
                type: 'POST',
                data: {
                    json: JSON.stringify(product_object),
                },
                success: function (result) {
                    $('.create_spin').hide();
                    if (result.status == 0) {
                        $('#product_preview_after').show();
                        $('#product_preview_above').hide();
                        DisplayResultData(result);
                        ParseResultToForm(result.model);
                        ParseLockStatus(result.model.page_not_found);
                        _msgalert.success(result.msg);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (jqXHR) {
                },
            });
            $('.create_spin').hide();
        }
    },
    GetDetailJSON: function (data_type) {
        if ($('#json_product').val() != "")
        {
            let title = 'Thông báo xác nhận';
            let description = "Dữ liệu sản phẩm đang sửa / tạo mới hiện tại sẽ bị xóa. Bạn có chắc chắn không?";
            _msgconfirm.openDialog(title, description, function () {
                var asin = $("#ASINCmd").val()
                if (asin == null || asin == "") {
                    _msgalert.error("Vui lòng nhập ASIN");
                    return;
                }
                $('.create_spin').show();
                var label_id = $('input[name="optradio"]:checked').val();
                var objData = {
                    ASIN: asin,
                    label_id: label_id,
                    data_type: data_type
                }
                $.ajax({
                    url: "/product/GetProductManualByASIN",
                    type: "POST",
                    data: objData,
                    success: function (result) {
                        var str = JSON.stringify(result.model, null, "\t");
                        $("#json_product").val(str);
                        DisplayResultData(result);
                        $('#product_preview_after').show();
                        $('#product_preview_above').hide();
                    },
                    error: function (result) {
                        _msgalert.error(result.msg);
                    }
                });
                $('.create_spin').hide();

            });
        }
        else {
            var asin = $("#ASINCmd").val()
            if (asin == null || asin == "") {
                _msgalert.error("Vui lòng nhập ASIN");
                return;
            }
            $('.create_spin').show();
            var label_id = $('input[name="optradio"]:checked').val();
            var objData = {
                ASIN: asin,
                label_id: label_id,
                data_type: data_type
            }
            $.ajax({
                url: "/product/GetProductManualByASIN",
                type: "POST",
                data: objData,
                success: function (result) {
                    var str = JSON.stringify(result.model, null, "\t");
                    $("#json_product").val(str);
                    DisplayResultData(result);
                    $('#product_preview_after').show();
                    $('#product_preview_above').hide();
                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
            $('.create_spin').hide();
        }
    },
    BlockProductManual: function (target_status) {
        var asin = $('#ASIN').val();
        var label_id = $('input[name="optradio"]:checked').val();
        if ((asin == null || asin == undefined || asin == '') && data_type == 1) {
            _msgalert.error("Bạn chưa nhập mã ASIN");
            return;
        }
        let title = 'Thông báo xác nhận';
        let description = "Bạn có chắc chắn muốn khoá sản phẩm " + asin + " không?";
        _msgconfirm.openDialog(title, description, function () {
            $('.create_spin').show();
            var objData = {
                product_code: product_object.product_code,
                label_id: label_id,
                target_status: target_status
            }
            $.ajax({
                url: "/product/BlockProductManual",
                type: "POST",
                data: objData,
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                        ParseLockStatus(result.current_status);
                    } else {
                        _msgalert.error(result.msg);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.msg);
                }
            });
            $('.create_spin').hide();
        });
    },
    Save: function () {
        $('.create_spin').show();
        var asin = $("#ASINCmd").val()
        var json_product = $("#json_product").val()
        if (asin == null || asin == "") {
            _msgalert.error("Vui lòng nhập ASIN");
            return;
        }
        if (json_product == null || json_product == "") {
            _msgalert.error("Vui lòng nhập chuỗi json của sản phẩm");
            return;
        }
        var objData = {
            ASIN: asin,
            json_product: json_product,
        }
        $.ajax({
            url: "/product/CreateProductManualFromJSON",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    DisplayResultData(result);
                    $('#product_preview_after').show();
                    $('#product_preview_above').hide();
                } else {
                    _msgalert.error(result.msg);
                }
            },
            error: function (result) {
                _msgalert.error(result.msg);
            }
        });
        $('.create_spin').hide();
    },

};
function ValidateAndGetValue() {
    let FromCreate = $('#form-create-product-manual');
    FromCreate.validate({
        rules: {
            product_code: "required",
            product_name: "required",
            price: "required",
            shiping_fee: "required",
            image_thumb: "required",
            seller_id: "required",
            seller_name: "required",
            item_weight: "required",
        },
        messages: {
            product_code: "Vui lòng nhập mã sản phẩm",
            product_name: "Vui lòng nhập tên sản phẩm",
            price: "Vui lòng nhập giá sản phẩm",
            shiping_fee: "Vui lòng phí vận chuyển nội địa Mỹ",
            image_thumb: "Vui lòng nhập ảnh sản phẩm",
            seller_id: "Vui lòng nhập mã người bán",
            seller_name: "Vui lòng nhập tên người bán",
            item_weight: "Vui lòng nhập đầy đủ cân nặng (bao gồm cả đơn vị)",
        }
    });
    if (FromCreate.valid()) {
        if (product_object == null) {
            product_object = {};
        }
        product_object.product_code = $('#ASIN').val();
        product_object.product_name = $('#product_name').val();
        product_object.page_not_found = $('.page_not_found:checked').val();

        product_object.seller_id = $('#seller_id').val();
        product_object.seller_name = $('#seller_name').val();
        product_object.rate = $('#rate').val();
        product_object.label_id = $('input[name="optradio"]:checked').val();
        var price = $('#price').val()
        if (price == undefined || price == null || price == '') {
            _msgalert.error('Bạn chưa nhập giá ');
            return;
        }
        if (isNaN(price) == true) {
            _msgalert.error('Giá bán phải là số');
            return;
        }
        if (parseFloat(price) <= 0) {
            _msgalert.error('Giá bán phải lớn hơn 0');
            return;
        }
        product_object.price = price;

        var ship_price = $('#shiping_fee').val()
        if (ship_price != undefined && ship_price != null && ship_price != '' && isNaN(ship_price) == true) {
            _msgalert.error('Price shipping US phải là số');
            return;
        }
        if (parseFloat(ship_price) < 0) {
            _msgalert.error('Price shipping US phải lớn hơn hoặc bằng 0');
            return;
        }
        product_object.shiping_fee = ship_price;

        var item_weight = $('#item_weight').val()
        if (item_weight != undefined && item_weight != null && item_weight != '' && isNaN(item_weight) == true) {
            _msgalert.error('Khối lượng sản phẩm phải là số');
            return;
        }
        if (parseFloat(item_weight) <= 0) {
            _msgalert.error('Khối lượng sản phẩm  phải lớn hơn 0');
            return;
        }
        var item_weight_text = $('select[name=shipping_weight_unit] option').filter(':selected').text();
        product_object.item_weight = item_weight + " " + item_weight_text;
        var unit = $('select[name=shipping_weight_unit] option').filter(':selected').val();

        var reviews_count = $('#reviews_count').val()
        if (reviews_count != undefined && reviews_count != null && reviews_count != '' && isNaN(reviews_count)) {
            _msgalert.error('Review Counts phải là số');
            return;
        }
        if (parseInt(reviews_count) < 0) {
            _msgalert.error('Review Counts phải lớn hơn 0');
            return;
        }
        product_object.reviews_count = parseInt(reviews_count);

        var rating = $('#rating').val()
        if (rating != undefined && rating != null && rating != '' && isNaN(rating)) {
            _msgalert.error('Ratings phải là số');
            return;
        }
        if (parseFloat(rating) < 0) {
            _msgalert.error('Ratings phải lớn hơn 0');
            return;
        }
        product_object.rating = parseFloat(rating);
        var image_thumb = $('#image_thumb').val();
        var listImg = image_thumb.split('.');
        var imgDuoi = listImg[listImg.length - 1];
        //JPG, PNG, JPEG, GIF, TIFF, PSD, PDF, EPS,
        if (imgDuoi != 'jpg' && imgDuoi != 'png' && imgDuoi != 'jpeg' && imgDuoi != 'gif' &&
            imgDuoi != 'tiff' && imgDuoi != 'bmp') {
            _msgalert.error('Link ảnh chỉ chấp nhận các định dạng: jpg, jpeg, png, gif, tiff, bmp');
            return;
        }
        product_object.image_thumb = image_thumb;
        var image_product = null;
        var json = '';
        if ($('#image_size_product').val() != null && $('#image_size_product').val() != "") {
            let url_list = $('#image_size_product').val().split("\n\n");

            url_list.forEach(function (element, index, array) {
                var img_size_item = element.split("\n");
                var img_size_object = {
                    Larger: '',
                    Thumb: ''
                };
                img_size_item.forEach(function (element) {
                    if (element != null && element != "" && element.includes("Larger")) {
                        img_size_object.Larger = $.trim(element.split(":-")[1]);
                    }
                    if (element != null && element != "" && element.includes("Thumb")) {
                        img_size_object.Thumb = $.trim(element.split(":-")[1]);
                    }
                });
                if (img_size_object.Larger != '' && img_size_object != '') {
                    if (json != '') {
                        json += ",";
                    }
                    json += "{\"larger\":\"" + img_size_object.Larger + "\",\"thumb\":\"" + img_size_object.Thumb + "\"}";
                }
            });
        }
        else {
            json = "{\"larger\":\"" + product_object.image_thumb + "\",\"thumb\":\"" + product_object.image_thumb + "\"}";
        }
        var image_product = JSON.parse("[" + json + "]");
        if (image_product != null)
            product_object.image_size_product = image_product;

        product_object.star = GetStar();
        product_object.is_prime_eligible = $('.is_prime_eligible:checked').val();

        var special_industry_type = $('select[name=special_industry] option').filter(':selected').val();
        product_object.industry_special_type = special_industry_type;
        return 'success';
    }
    else {
        _msgalert.error('Vui lòng kiểm tra lại thông tin đã điền.');
        return;
    }
};
function ParseResultToForm(product_detail) {
    if (product_detail != "" && product_detail != null) {
        product_object = product_detail;
        $('#product_name').val(product_object.product_name);
        if (product_object.price != null)
            $('#price').val(product_object.price);
        if (product_object.shiping_fee != null)
            $('#shiping_fee').val(product_object.shiping_fee);
        if (product_object.discount != null)
            $('#discount').val(product_object.discount);
        if (product_object.industry_special_type != null)
            $("#special_industry").val(product_object.industry_special_type);
        if (product_object.image_thumb != null) {
            $('#image_thumb').val(product_object.image_thumb);
            $('#img_thumb_preview').prop('src', product_object.image_thumb);
        }
        if (product_object.link_product != null)
            $('#link_product').val(product_object.link_product);
        if (product_object.amount_vnd != null)
            $('#amount_vnd').val(product_object.amount_vnd);
        if (product_object.seller_id != null)
            $('#seller_id').val(product_object.seller_id);
        if (product_object.seller_name != null)
            $('#seller_name').val(product_object.seller_name);
        if (product_object.image_size_product != null) {
            var image_product_html = '';
            product_object.image_size_product.forEach(function (item) {
                image_product_html += "Larger :- " + item.larger + '\nThumb :- ' + item.thumb + "\n";
                image_product_html += "\n";

            });
            $('#image_size_product').val(image_product_html);
        }
        if (product_object.item_weight != null && product_object.item_weight != "")
            $('#item_weight').val(product_object.item_weight.split(" ")[0]);
        var weight_value = 1;
        if (product_object.item_weight != null && product_object.item_weight != "") {
            switch (product_object.item_weight.split(" ")[1]) {
                case "ounces":
                    weight_value = 0;
                    break;
                case "pounds":
                    weight_value = 1;

                    break;
                case "grams":
                    weight_value = 2;

                    break;
                case "kilograms":
                    weight_value = 3;

                    break;
                case "tonne":
                    weight_value = 4;

                    break;
                case "kiloton":
                    weight_value = 5;
                    break;
            }
        }
        $("#shipping_weight_unit").val(weight_value);
        $('#reviews_count').val(product_object.reviews_count);
        $('#rating').val(product_object.rating);
        SelectStar(product_object.star);
        if (product_object.is_prime_eligible) {
            $('#is_prime_eligible').prop('checked', true);
        }
        else {
            $('#is_prime_eligible').prop('checked', false);
        }
    }
};
function DisplayResultData(result) {
    var resultProductModel = JSON.parse(result.data);
    if (resultProductModel != null && resultProductModel != "") {
        $('#title_review').html(result.model.product_name)
        $('#seller_name_review').html(result.model.seller_name)
        $('#link_amazon').attr('href', "http://dropshipping.x-tech.vn" + result.model.link_product)
        $('#price_review').html(formatMoney(result.model.price, 'USD') + ' $')
        $('#ship_price_us_review').html(result.model.shiping_fee + ' $')
        var number = parseFloat(resultProductModel.FIRST_POUND_FEE)
        if (number > 0) {
            $('#shipping_details_first_pound').html(resultProductModel.FIRST_POUND_FEE + ' $')
        }
        number = parseFloat(resultProductModel.NEXT_POUND_FEE)
        if (number > 0) {
            $('#shipping_details_next_pounds').html(resultProductModel.NEXT_POUND_FEE + ' $')
        }
        number = parseFloat(resultProductModel.LUXURY_FEE)
        if (number > 0) {
            $('#shipping_details_luxury_pound').html(resultProductModel.LUXURY_FEE + ' $')
        }

        number = parseFloat(resultProductModel.TOTAL_SHIPPING_FEE)
        if (number > 0) {
            $('#shipping_details_total_shipping_fee').html(resultProductModel.TOTAL_SHIPPING_FEE + ' $')
        }
        number = parseFloat(resultProductModel.PRICE_LAST)
        if (number > 0) {
            $('#price_last').html(formatMoney(resultProductModel.PRICE_LAST, 'USD') + ' $')
        }
        $('#price_last_vnd').html(formatMoney(result.model.amount_vnd, 'VND') + ' đ');
        number = parseFloat(result.model.rating)
        if (number > 0) {
            $('#rating').html(result.model.rating);
        }
        SetResultStar(result.model.star);
    }
};
function SelectStar(item) {
    var value = parseInt(item)
    switch (value) {
        case 0: {
            $('#1').attr('class', 'fa fa-star-o ')
            $('#2').attr('class', 'fa fa-star-o ')
            $('#3').attr('class', 'fa fa-star-o ')
            $('#4').attr('class', 'fa fa-star-o ')
            $('#5').attr('class', 'fa fa-star-o ')
            break;
        }
        case 1:
            star = 1;
            $('#1').attr('class', 'fa fa-star bg-orange')
            $('#2').attr('class', 'fa fa-star-o ')
            $('#3').attr('class', 'fa fa-star-o ')
            $('#4').attr('class', 'fa fa-star-o')
            $('#5').attr('class', 'fa fa-star-o')
            break;
        case 2:
            star = 2;
            $('#1').attr('class', 'fa fa-star bg-orange')
            $('#2').attr('class', 'fa fa-star bg-orange')
            $('#3').attr('class', 'fa fa-star-o white')
            $('#4').attr('class', 'fa fa-star-o white')
            $('#5').attr('class', 'fa fa-star-o')
            break;
        case 3:
            star = 3;
            $('#1').attr('class', 'fa fa-star bg-orange')
            $('#2').attr('class', 'fa fa-star bg-orange')
            $('#3').attr('class', 'fa fa-star bg-orange')
            $('#4').attr('class', 'fa fa-star-o')
            $('#5').attr('class', 'fa fa-star-o')
            break;
        case 4:
            star = 4;
            $('#1').attr('class', 'fa fa-star bg-orange')
            $('#2').attr('class', 'fa fa-star bg-orange')
            $('#3').attr('class', 'fa fa-star bg-orange')
            $('#4').attr('class', 'fa fa-star bg-orange')
            $('#5').attr('class', 'fa fa-star-o')
            break;
        case 5:
            star = 5;
            $('#1').attr('class', 'fa fa-star bg-orange')
            $('#2').attr('class', 'fa fa-star bg-orange')
            $('#3').attr('class', 'fa fa-star bg-orange')
            $('#4').attr('class', 'fa fa-star bg-orange')
            $('#5').attr('class', 'fa fa-star bg-orange')
            break;
    }
};
function SetResultStar (star_point) {
    switch (star_point) {
        case 1:
            $('#1star').attr('class', 'fa fa-star bg-orange')
            $('#2star').attr('class', 'fa fa-star-o ')
            $('#3star').attr('class', 'fa fa-star-o ')
            $('#4star').attr('class', 'fa fa-star-o')
            $('#5star').attr('class', 'fa fa-star-o')
            break;
        case 2:
            $('#1star').attr('class', 'fa fa-star bg-orange')
            $('#2star').attr('class', 'fa fa-star bg-orange')
            $('#3star').attr('class', 'fa fa-star-o white')
            $('#4star').attr('class', 'fa fa-star-o white')
            $('#5star').attr('class', 'fa fa-star-o')
            break;
        case 3:
            $('#1star').attr('class', 'fa fa-star bg-orange')
            $('#2star').attr('class', 'fa fa-star bg-orange')
            $('#3star').attr('class', 'fa fa-star bg-orange')
            $('#4star').attr('class', 'fa fa-star-o')
            $('#5star').attr('class', 'fa fa-star-o')
            break;
        case 4:
            $('#1star').attr('class', 'fa fa-star bg-orange')
            $('#2star').attr('class', 'fa fa-star bg-orange')
            $('#3star').attr('class', 'fa fa-star bg-orange')
            $('#4star').attr('class', 'fa fa-star bg-orange')
            $('#5star').attr('class', 'fa fa-star-o')
            break;
        case 5:
            $('#1star').attr('class', 'fa fa-star bg-orange')
            $('#2star').attr('class', 'fa fa-star bg-orange')
            $('#3star').attr('class', 'fa fa-star bg-orange')
            $('#4star').attr('class', 'fa fa-star bg-orange')
            $('#5star').attr('class', 'fa fa-star bg-orange')
            break;
    }
};
function GetStar() {
    var star = 0;
    if ($('#1').attr('class').includes('bg-orange')) star = star + 1;
    if ($('#2').attr('class').includes('bg-orange')) star = star + 1;
    if ($('#3').attr('class').includes('bg-orange')) star = star + 1;
    if ($('#4').attr('class').includes('bg-orange')) star = star + 1;
    if ($('#5').attr('class').includes('bg-orange')) star = star + 1;
    return star;
};
function formatMoney(number, currency) {

    switch (currency) {
        case 'VND': {
            return number.toLocaleString('en-US', { style: 'currency', currency: 'VND' }).replace("₫", "");
        } break;
        case 'USD': {
            return number.toLocaleString('en-US', { style: 'currency', currency: 'USD' }).replace("$", "")
        }

    }
};
function GetGroupProductName(id) {
    var objData = {
        group_id : id
    };
    if (id > 0) {
        $.ajax({
            url: "/product/GetGroupProductNameByID",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.status == 0) {
                    $('#group_name').val(result.data);
                }
            },          
        });
    }
   
};
function ParseLockStatus(is_page_not_found) {
    if (is_page_not_found == true) {
        $('#product_status').css('color', 'red');
        $('#product_status').html('Khoá tạm dừng');

        $("#btn_lock_product").attr("onclick", "menu.LockProductManual(0)");
        $('#btn_lock_product').html('Unlock');
    } else {
        $('#product_status').css('color', 'black');
        $('#product_status').html('Đang bán');
        $("#btn_lock_product").attr("onclick", "menu.LockProductManual(1)");
        $('#btn_lock_product').html('Lock');
    }
}
