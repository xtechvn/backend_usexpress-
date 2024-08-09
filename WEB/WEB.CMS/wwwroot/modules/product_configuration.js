$('body').on('focusout', '.form_block_keyword', function () {
    _product_configuration.ChangeButtonStatus();
});
var _product_configuration = {
    Initialization: function () {
        $('#form_btn_save').attr("disabled", true);
        _product_configuration.OnSearch();
    },
    ChangeButtonStatus: function () {
        var keywords = $('#form_block_keyword').val();
        if (keywords.trim() == '' || keywords == null || keywords == undefined) {

        } else {
            $('#form_btn_save').removeAttr("disabled");
        }
    },
    SelectTab: function (id) {
        switch (id) {
            case 0: {
                $('#tl-dom').css('display', '');
                $('#block-tk').css('display', 'none');
                $('#list-tk').css('display', 'none');
                $('#block_table').css('display', 'none');

                $('#tl-dom-select').addClass('active');
                $('#block-tk-select').removeClass('active');
                $('#list-tk-select').removeClass('active');
                $("input[name=form_product_status][value='0']").prop("checked", true);
            } break;
            case 1: {
                $('#tl-dom').css('display', 'none');
                $('#block-tk').css('display', '');
                $('#list-tk').css('display', 'none');
                $('#block_table').css('display', '');
                this.OnSearch();
                $('#tl-dom-select').removeClass('active');
                $('#block-tk-select').addClass('active');
                $('#list-tk-select').removeClass('active');
                //$('input:radio[name=form_product_status][value=0]').click();
                $("input[name=form_product_status][value='0']").prop("checked", true);


            } break;
            case 2: {
                $('#tl-dom').css('display', 'none');
                $('#block-tk').css('display', 'none');
                $('#list-tk').css('display', '');
                $('#block_table').css('display', '');
                this.OnSearch();

                $('#tl-dom-select').removeClass('active');
                $('#block-tk-select').removeClass('active');
                $('#list-tk-select').addClass('active');
               // $('input:radio[name=form_product_status][value=0]').click();
                $("input[name=form_product_status][value='0']").prop("checked", true);


            } break;
            default: break;
        }
    },
    OnSearch: function () {
        $('#block_table').html(this.Loading());
        var objData = {
            keywords: $('#form_block_keyword').val() == undefined || $('#form_block_keyword').val() == null || $('#form_block_keyword').val().trim() == "" ? "" : $('#form_block_keyword').val(),
            product_status: $('input[name="form_product_status"]:radio:checked').val() == undefined || $('input[name="form_product_status"]:radio:checked').val().trim() == '0' ? '-2' : $('input[name="form_product_status"]:radio:checked').val(),
            keyword_type: $('#form_block_keyword_type :checked').val(),
            label_id: $('#form_block_label :checked').val() == undefined ? "-1" : $('#form_block_label :checked').val()
        };
        $.ajax({
            url: "/product/ProductConfigurationTable",
            type: "POST",
            data: objData,
            success: function (result) {
                $('#block_table').html(result);
            },
        });
    },
    OnDelete: function () {
        let FromCreate = $('#form_block');
        FromCreate.validate({
            rules: {
                form_block_keyword: "required",
                form_product_status: "required",
            },
            messages: {
                form_block_keyword: "Vui lòng nhập từ khóa / mã sản phẩm",
                form_product_status: "Vui lòng chọn trạng thái",
            }
        });
        if (FromCreate.valid()) {
            var product_status = $('input[name="form_product_status"]:radio:checked').val() == undefined ? "0" : $('input[name="form_product_status"]:radio:checked').val();
            var keywords = $('#form_block_keyword').val();
            var keyword_type = $('#form_block_keyword_type :checked').val();
            var label_id = $('#form_block_label :checked').val() == undefined ? "-1" : $('#form_block_label :checked').val();
            this.Delete(product_status, keywords, keyword_type, label_id);
        }
    },
    ClearKeywordLockForm: function () {
        $('#form_block_keyword').val('');
        $('#form_block_keyword_type').val('2');
        $('#form_block_label').val('0');
        $("input[name=form_product_status]").removeAttr("checked");

        _product_configuration.OnSearch();

    },
    Loading: function () {
        var html = '<div class=" placeholder table-responsive table-default">'
            + '<table class="table table-nowrap"><thead class="placeholder"><tr class="bg-main2 placeholder"></tr></thead><tbody class="placeholder"></tbody></table></div>';
        return html;
    },
    SummitKeywordLock: function () {
        let FromCreate = $('#form_block');
        FromCreate.validate({
            rules: {
                form_block_keyword: "required",
                form_product_status: "required",
            },
            messages: {
                form_block_keyword: "Vui lòng nhập từ khóa / mã sản phẩm",
                form_product_status: "Vui lòng chọn trạng thái",
            }
        });
        if (FromCreate.valid()) {
            var keyword_type = $('#form_block_keyword_type :checked').val();
            if (keyword_type == '2') {
                var obj = {
                    product_status: $('input[name="form_product_status"]:radio:checked').val(),
                    keywords: $('#form_block_keyword').val(),
                    keyword_type: 0,
                    label_id: $('#form_block_label :checked').val() == undefined ? "-1" : $('#form_block_label :checked').val()
                }
                $.ajax({
                    url: "/product/SummitLockProduct",
                    type: "POST",
                    data: obj,
                    success: function (result) {
                        if (result.status == 0) {
                            _msgalert.success(result.msg);
                        }
                        else {
                            _msgalert.error(result.msg);
                        }
                    },
                });
                obj = {
                    product_status: $('input[name="form_product_status"]:radio:checked').val(),
                    keywords: $('#form_block_keyword').val(),
                    keyword_type: 1,
                    label_id: $('#form_block_label :checked').val() == undefined ? "-1" : $('#form_block_label :checked').val()
                }
                $.ajax({
                    url: "/product/SummitLockProduct",
                    type: "POST",
                    data: obj,
                    success: function (result) {
                        if (result.status == 0) {
                            _msgalert.success(result.msg);
                            _product_configuration.ClearKeywordLockForm();
                        }
                        else {
                            _msgalert.error(result.msg);
                        }
                    },
                });
            }
            else {
                var obj = {
                    product_status: $('input[name="form_product_status"]:radio:checked').val(),
                    keywords: $('#form_block_keyword').val(),
                    keyword_type: $('#form_block_keyword_type :checked').val(),
                    label_id: $('#form_block_label :checked').val() == undefined ? "-1" : $('#form_block_label :checked').val()
                }
                $.ajax({
                    url: "/product/SummitLockProduct",
                    type: "POST",
                    data: obj,
                    success: function (result) {
                        if (result.status == 0) {
                            _msgalert.success(result.msg);
                            _product_configuration.ClearKeywordLockForm();

                        }
                        else {
                            _msgalert.error(result.msg);
                        }
                    },
                });
            }
        }
        $('#form_btn_save').attr("disabled", true);
    },
    Edit: function (product_status, keywords, keyword_type, label_id, product_status) {
        $("input[name=form_product_status][value='" + product_status+ "']").prop("checked", true);

       // $('input[name="form_product_status"]').val(product_status);
       // $('input:radio[name=form_product_status][value=' + product_status+']').click();

        $('#form_block_keyword').val(keywords);
        _product_configuration.ChangeButtonStatus();
        $('#form_block_keyword_type').val(keyword_type);
        $('#form_block_label').val(label_id);
        $('html, body').animate({
            scrollTop: $("#form_block_keyword").offset().top
        }, 1000);
    },

    Delete: function (product_status, keywords, keyword_type, label_id) {
        let title = 'Thông báo xác nhận';
        let description = 'Dữ liệu về từ khóa " '+ keywords +' ". Bạn có chắc chắn không?';
        _msgconfirm.openDialog(title, description, function () {
            var obj = {
                product_status: product_status,
                keywords: keywords,
                keyword_type: keyword_type,
                label_id: label_id
            }
            $.ajax({
                url: "/product/DeleteLockProduct",
                type: "POST",
                data: obj,
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                        _product_configuration.OnSearch();
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                },
            });
        });
    }
}