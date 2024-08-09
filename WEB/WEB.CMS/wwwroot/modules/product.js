$(document).ready(function () {
    productManager.CheckTimeFromURL();
    productManager.OnInit();
});

$('body').on('click', '.onlocked_product', function () {
    productManager.OnLockedProduct();
});

$('body').on('click', '.lockaction_span', function (ev) {
    if (ev.target.tagName === 'SPAN') {
        if ($(ev.target).prev().is(':checked') === false) {
            $(ev.target).prev().prop('checked', true)
        }
    }
});

//$('div').on('click', function (ev) {
//    if (ev.target.tagName === 'SPAN') {
//        if ($(ev.target).prev().is(':checked') === false) {
//            $(ev.target).prev().prop('checked', true)
//        } else {
//            $(ev.target).prev().prop('checked', false)
//        }
//    }
//});

$('#ip_search_fromdate').Zebra_DatePicker({
    format: 'd/m/Y',
    direction: false,
    pair: $('#ip_search_todate'),
    onSelect: function () {
        $(this).change();
    }
}).removeAttr('readonly');

$('#ip_search_todate').Zebra_DatePicker({
    format: 'd/m/Y',
    direction: true,
    onSelect: function () {
        $(this).change();
    }
}).removeAttr('readonly');

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

var _IntervalProductName = null;
$("#ip_search_product_name").keyup(function (e) {
    if (e.which === 13) {
        productManager.OnChangeProductName(e.target.value);
    } else {
        clearInterval(_IntervalProductName);
        _IntervalProductName = setInterval(function () {
            productManager.OnChangeProductName(e.target.value);
            clearInterval(_IntervalProductName);
        }, 500);
    }
});

var _IntervalProductCode = null;
$("#ip_search_product_code").keyup(function (e) {
    if (e.which === 13) {
        productManager.OnChangeProductCode(e.target.value);
    } else {
        clearInterval(_IntervalProductCode);
        _IntervalProductCode = setInterval(function () {
            productManager.OnChangeProductCode(e.target.value);
            clearInterval(_IntervalProductCode);
        }, 500);
    }
});

$('.cbk_search_label, .rad_trade_status, .ckb-news-cate').click(function () {
    var objParam = productManager.CreateFilterObject();
    productManager.FilterObject = objParam;
    productManager.Search(objParam);
});

$('#cbo_page_size, #ip_search_fromdate, #ip_search_todate').change(function () {
    var objParam = productManager.CreateFilterObject();
    productManager.FilterObject = objParam;
    productManager.Search(objParam);
});

$('.checkbox-tb-column').click(function () {
    let seft = $(this);
    let id = seft.data('id');
    if (seft.is(':checked')) {
        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
    } else {
        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
    }
});

$('.btn-toggle-cate').click(function () {
    var seft = $(this);
    if (seft.hasClass("plus")) {
        seft.siblings('ul.lever2').show();
        seft.removeClass('plus').addClass('minus');
    } else {
        seft.siblings('ul.lever2').hide();
        seft.removeClass('minus').addClass('plus');
    }
});

$('#ip_search_category').keyup(function () {
    var seft = $(this);
    var elementValue = seft.val().trim();

    if (elementValue != "") {
        seft.siblings('.btn_reset').removeClass('mfp-hide');
    } else {
        seft.siblings('.btn_reset').addClass('mfp-hide');

        $("#panel-category-filter li").removeClass("mfp-hide");
        $("#panel-category-filter li a.btn-toggle-cate").removeClass('minus').addClass('plus');
        $("#panel-category-filter ul.lever2").hide();

        return;
    }

    $("#panel-category-filter ul.lever2").show();
    $("#panel-category-filter li").add("mfp-hide");

    $("#panel-category-filter li").each(function () {
        var seft_li = $(this);
        var reg = seft_li.find('label').text().trim();
        if (reg.search(new RegExp(elementValue, "i")) < 0) {
            seft_li.addClass('mfp-hide');
        } else {
            seft_li.removeClass('mfp-hide');
            seft_li.parent().siblings('a.btn-toggle-cate').addClass('minus').removeClass('plus');
        }
    });
});

$('#ip_reset_category').click(function () {
    var seft = $(this);
    productManager.OnResetInput(seft);
    $("#panel-category-filter li").removeClass("mfp-hide");
    $("#panel-category-filter li a.btn-toggle-cate").removeClass('minus').addClass('plus');
    $("#panel-category-filter ul.lever2").hide();
});

$('#panel-category-filter .ckb-news-cate').click(function () {
    var seft = $(this);
    var id = seft.val();

    if (seft.is(":checked")) {
        var text = seft.parent().text().trim();
        var parent_text = productManager.FindParentOfCate(seft.parent(), text);

        var strHtml = '<li class="item_cate_selected" data-id="' + id + '">'
            + '<div class="color-main" title="' + parent_text + '"style="flex:90%" >' + text + '</div>'
            + '<div>'
            + '<a class="cur-pointer btn_delete_cate" title="Xóa"><i class="red fa fa-times"></i></a>'
            + '</div>'
            + '</li>';
        $('#selected_items_panel').append(strHtml);
    } else {
        $('.item_cate_selected[data-id="' + id + '"]').remove();
    }
});

$('#selected_items_panel').on('click', '.btn_delete_cate', function () {
    var seft = $(this);
    var parent = seft.closest('li');
    var id = parent.data('id');
    parent.remove();
    $('#panel-category-filter .ckb-news-cate[value="' + id + '"]').prop("checked", false);

    var objParam = productManager.CreateFilterObject();
    productManager.FilterObject = objParam;
    productManager.Search(objParam);
});

$('#grid-data').on('click', '.sortable', function () {
    var seft = $(this);
    var sortField = parseInt(seft.data('field'));
    var sortType = 1;
    if (seft.find('i').hasClass('fa-sort-down')) {
        sortType = 0;
    }
    productManager.OnSortTable(sortField, sortType);
});

var productManager = {

    FindParentOfCate: function (element, str) {
        var parentElement = $(element).closest('ul').siblings('label');
        if (parentElement.length > 0) {
            str = parentElement.text().trim() + " >> " + str;
            return this.FindParentOfCate(parentElement, str);
        } else {
            return str;
        }
    },

    OnChangeInputResetable: function (element) {
        var elementValue = $(element).val().trim();
        if (elementValue != "") {
            $(element).siblings('.btn_reset').removeClass('mfp-hide');
        } else {
            $(element).siblings('.btn_reset').addClass('mfp-hide');
        }
    },

    OnResetInput: function (element) {
        $(element).siblings('input.ip-resetable').val('');
        $(element).siblings('input.ip-resetable').trigger('keyup');
        $(element).addClass('mfp-hide');
    },

    OnSelectDate: function (select_type) {
        var today = new Date();
        if (select_type == 0) { // today
            $('#block_filter_date .datepicker-input-range').val(ConvertJsDateToString(today));
        } else if (select_type == 1) { // yesterday
            var yesterday = new Date((new Date()).valueOf() - 1000 * 60 * 60 * 24);
            $('#block_filter_date .datepicker-input-range').val(ConvertJsDateToString(yesterday));
        } else {
            $('#ip_search_fromdate, #ip_search_todate').val('');
        }

        $('#ip_search_fromdate').trigger('change');
    },

    OnInit: function () {
        var objParam = this.CreateFilterObject();
        this.FilterObject = objParam;
        this.Search(objParam);
    },

    CreateFilterObject: function () {
        let _ProductName = $('#ip_search_product_name').val().trim();
        let _ProductCode = $('#ip_search_product_code').val().trim();
        let _FromDate = ConvertToJSONDate($('#ip_search_fromdate').val());
        let _ToDate = ConvertToJSONDate($('#ip_search_todate').val());
        let _Status = parseInt($('.rad_trade_status:checked').val());
        let _PageSize = parseInt($('#cbo_page_size').val());

        let _ArrLabel = [];
        $('.cbk_search_label:checked').each(function () {
            let dataValue = parseInt($(this).val());
            _ArrLabel.push(dataValue);
        });

        var _ArrCategory = [];
        $('.ckb-news-cate').each(function () {
            if ($(this).is(":checked")) {
                _ArrCategory.push(parseInt($(this).val()));
            }
        });

        var objParam = {
            ProductName: _ProductName,
            ProductCode: _ProductCode,
            Labels: _ArrLabel,
            Categories: _ArrCategory,
            FromDate: _FromDate,
            ToDate: _ToDate,
            Status: _Status,
            SortField: 0,
            SortType: 1,
            PageIndex: 1,
            PageSize: _PageSize
        };

        return objParam;
    },

    Search: function (input) {
        $.ajax({
            url: "/product/productgrid",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id');
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    } else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
            },
            error: function (err) {

            }
        });
    },

    OnPaging: function (pageIndex) {
        var objSearch = this.FilterObject;
        objSearch.PageIndex = pageIndex;
        this.Search(objSearch);
    },

    OnChangeProductName: function (value) {
        var objSearch = this.FilterObject;
        objSearch.ProductName = value.trim();
        objSearch.PageIndex = 1;
        this.Search(objSearch);
    },

    OnChangeProductCode: function (value) {
        var objSearch = this.FilterObject;
        objSearch.ProductCode = value.trim();
        objSearch.PageIndex = 1;
        this.Search(objSearch);
    },

    CheckTimeFromURL: function () {
        var url_string = window.location.href;
        var url = new URL(url_string);
        var time_from_view = url.searchParams.get("day");
        if (time_from_view != null && time_from_view != "") {
            $('#ip_search_fromdate').val(time_from_view);
            $('#ip_search_todate').val(time_from_view);
        }
    },

    OnSortTable: function (sortField, sortType) {
        var objSearch = this.FilterObject;
        objSearch.SortField = sortField;
        objSearch.SortType = sortType;
        objSearch.PageIndex = 1;
        this.Search(objSearch);
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
    OpenDoCrawlPopup: function () {
        let title = 'Khoá / Chặn sản phẩm';
        let url = '/product/CrawlOfflinePopup';
        let param = {};
        _magnific.OpenSmallPopupWithHeader(title, url, param);
    },
    CrawlOffline: function () {
        var asin = $('#cr_asinlist').val();
        var label_id = $('#cr_label :checked').val();
        var group_id = $('#cr_groupid').val();
        if (asin == null || asin == undefined || asin == '') {
            _msgalert.error("Bạn chưa nhập mã ASIN");
            return;
        }
        var objData = {
            ASIN: asin,
            label_id: label_id,
            group_id: group_id
        }
        $.ajax({
            url: "/product/CrawlASINOffline",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    $('#cr_asinlist').val('');
                } else {
                    _msgalert.error(result.msg);
                }

            },
            error: function (result) {
                _msgalert.error(result.msg);
            }
        });
    }
};