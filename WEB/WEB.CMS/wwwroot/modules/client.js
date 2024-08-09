$(document).ready(function () {
    _client.Init();
});

$(document).on('click', '.Zebra_DatePicker .dp_clear', function () {
    $('#from-payment-date').trigger('change');
});

var _IntervalEmail = null;
$("#ip-kup-client-email").keyup(function (e) {
    if (e.which === 13) {
        _client.OnChangeEmail(e.target.value);
    } else {
        clearInterval(_IntervalEmail);
        _IntervalEmail = setInterval(function () {
            _client.OnChangeEmail(e.target.value);
            clearInterval(_IntervalEmail);
        }, 500);
    }
});

var _IntervalAddress = null;
$("#ip-client-address").keyup(function (e) {
    if (e.which === 13) {
        _client.OnChangeAddress(e.target.value);
    } else {
        clearInterval(_IntervalAddress);
        _IntervalAddress = setInterval(function () {
            _client.OnChangeAddress(e.target.value);
            clearInterval(_IntervalAddress);
        }, 500);
    }
});

var _IntervalMinAmount = null;
$("#ip-client-amount-min").keyup(function (e) {
    if (e.which === 13) {
        _client.OnSearchByMinRevenue(e.target.value.replace(/,/g, ""));
    } else {
        clearInterval(_IntervalMinAmount);
        _IntervalMinAmount = setInterval(function () {
            _client.OnSearchByMinRevenue(e.target.value.replace(/,/g, ""));
            clearInterval(_IntervalMinAmount);
        }, 600);
    }
});

var _IntervalMaxAmount = null;
$("#ip-client-amount-max").keyup(function (e) {
    if (e.which === 13) {
        _client.OnSearchByMaxRevenue(e.target.value.replace(/,/g, ""));
    } else {
        clearInterval(_IntervalMaxAmount);
        _IntervalMaxAmount = setInterval(function () {
            _client.OnSearchByMaxRevenue(e.target.value.replace(/,/g, ""));
            clearInterval(_IntervalMaxAmount);
        }, 600);
    }
});

$('input[name="radio-registered-date"]').on('click', function () {
    let value = parseInt($(this).val());
    let _dateNow = new Date();
    let _fromDate = '';
    let _toDate = '';

    if (value === 4) {
        $('.registered-date-option').removeClass('mfp-hide');
        $('#from-join-date').val("");
        $('#to-join-date').val("");
    } else {
        $('.registered-date-option').addClass('mfp-hide');
        switch (value) {
            case 0:
                _fromDate = null;
                _toDate = null;
                break;
            case 1:
                var fromDate = new Date(new Date().setDate(new Date().getDate() - 7));
                _fromDate = fromDate.toJSON();
                _toDate = _dateNow.toJSON();
                break;
            case 2:
                var fromDate = new Date(new Date().setMonth(new Date().getMonth() - 1));
                _fromDate = fromDate.toJSON();
                _toDate = _dateNow.toJSON();
                break;
            case 3:
                var fromDate = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
                _fromDate = fromDate.toJSON();
                _toDate = _dateNow.toJSON();
                break;
        }
        _client.OnChangeDateRange(_fromDate, _toDate);
    }
});

$('#grid-data').on('click', 'tr.line-item td', function () {
    let seft = $(this);
    let _tabActive = 1;
    let seftparent = seft.closest('tr.line-item');

    if (seft.hasClass('order-counting')) {
        _tabActive = 2;
    }


    if (seft.hasClass('referral-order-counting')) {
        _tabActive = 4;
    }

    let id = seftparent.data('id');
    let isloadajax = JSON.parse(seftparent.data('ajaxdetail'));

    if (seftparent.hasClass('active')) {
        seftparent.removeClass('active');
        seftparent.next().addClass('mfp-hide');
    } else {
        seftparent.siblings('tr.line-item').removeClass('active');
        seftparent.addClass('active');
        if (!isloadajax) {
            _client.OnGetDetail(id, _tabActive, function (result) {
                seftparent.data('ajaxdetail', "true");
                seftparent.siblings('.info-detail').addClass('mfp-hide');
                seftparent.after(result);
            });
        } else {
            seftparent.siblings('.info-detail').addClass('mfp-hide');
            seftparent.next().find('.tab-default a[data-tab="' + _tabActive + '"]').trigger('click');
            seftparent.next().removeClass('mfp-hide');
        }
    }
});

$('#grid-data').on('click', '.tab-default a', function () {
    let seft = $(this);
    let tab = parseInt(seft.data('tab'));
    let id = parseInt(seft.data('id'));

    if (tab === 2) {
        //_client.OnLoadUserData(id, function (result) {
        //    seft.closest('tr').find('.grid-user-role').html(result);
        //});
    }

    seft.siblings().removeClass('active');
    seft.addClass('active');
    seft.closest('tr.info-detail').find('.tab-item').addClass('mfp-hide');
    seft.closest('tr.info-detail').find('.tab-item[data-id=' + tab + ']').removeClass('mfp-hide');
});

$('#grid-data').on('click', '.btn-update-status', function () {
    let seft = $(this);
    let id = parseInt(seft.data('id'));
    _client.OnUpdateStatus(id, function (data) {
        if (data.isSuccess) {
            _msgalert.success(data.message);
            if (data.status === 0) {
                seft.children('span').html('Khóa tài khoản');
                seft.closest('.tab-item').find('.text-client-status').html('Đang hoạt động');
            } else {
                seft.children('span').html('Mở tài khoản');
                seft.closest('.tab-item').find('.text-client-status').html('Khóa/Tạm ngừng');
            }
        } else {
            _msgalert.error(data.message);
        }
    });
});

$('#grid-data').on('click', '.btn-change-default-address', function () {
    let seft = $(this);
    let id = parseInt(seft.data('id'));
    _client.OnChangeDefaultAdress(id, function (data) {
        if (data.isSuccess) {
            _msgalert.success(data.message);
            seft.closest('.address-receive').find('.icon-active-status').remove();
            seft.closest('.address-receive').find('.btn-change-default-address.disabled').removeClass('disabled');
            seft.addClass('disabled');
            seft.closest('.item-address').find('.item-address-name').append('<span class="icon-active-status active">Đang sử dụng</span>');
        } else {
            _msgalert.error(data.message);
        }
    });
});

$('#grid-data').on('click', '.btn-not-paid-order', function () {
    var seft = $(this);
    var clientid = seft.data('clientid');
    var btnPaidOrder = $('.btn-paid-order[data-clientid="' + clientid + '"]');
    if (btnPaidOrder.hasClass('text-underline')) {
        btnPaidOrder.removeClass('text-underline')
    }
    if (seft.hasClass('text-underline')) {
        seft.removeClass('text-underline')
        $('.tbody-' + clientid + ' tr.paid-order').removeClass('mfp-hide');
    } else {
        seft.addClass('text-underline')
        $('.tbody-' + clientid + ' tr.paid-order').addClass('mfp-hide');
    }
    $('.tbody-' + clientid + ' tr.not-paid-order').removeClass('mfp-hide');
});

$('#grid-data').on('click', '.btn-paid-order', function () {
    var seft = $(this);
    var clientid = seft.data('clientid');
    var btnNotPaidOrder = $('.btn-not-paid-order[data-clientid="' + clientid + '"]');
    if (btnNotPaidOrder.hasClass('text-underline')) {
        btnNotPaidOrder.removeClass('text-underline')
    }

    if (seft.hasClass('text-underline')) {
        seft.removeClass('text-underline')
        $('.tbody-' + clientid + ' tr.not-paid-order').removeClass('mfp-hide');
    } else {
        seft.addClass('text-underline')
        $('.tbody-' + clientid + ' tr.not-paid-order').addClass('mfp-hide');
    }
    $('.tbody-' + clientid + ' tr.paid-order').removeClass('mfp-hide');
});

$('.btn-bind-revenue').on('click', function () {
    let seft = $(this);
    let value = seft.data('value');
    seft.siblings('input').val(formatNumber(value.toString()));
});

var _client = {

    Init: function () {
        var _searchModel = {
            ClientName: '',
            Email: '',
            Phone: '',
            OrderCode: '',
            ProvinceId: '',
            DistrictId: '',
            WardId: '',
            Address: '',
            FromAmount: '',
            ToAmount: '',
            FromDate: null,
            ToDate: null,
            QuantityRange: -1,
            LableId: '',
            GroupProductId: '',
            FromQuantity: 0,
            ReferralId: '',
            ToQuantity: 10000,
            FromJoinDate: this.ConvertToJSONDate($('#from-join-date').val()),
            ToJoinDate: this.ConvertToJSONDate($('#to-join-date').val()),
            IsBackClientInDay: parseInt($('#ip-returning-client').val()),
            IsPaymentInDay: parseInt($('#ip-payment-inday').val()),
        };

        var objSearch = {
            searchModel: _searchModel,
            currentPage: 1,
            pageSize: 20
        };

        this.SearchParam = objSearch;
        this.Search(objSearch);
    },

    Loading: function () {
        var html = '<div class="wrap_bg mb20">'
            + '<div class="placeholder mb10" style="height: 60px;"></div>'
            + '<div class="placeholder mb10" style="height: 60px; width: 300px;"></div>'
            + '<div class="box-create-api" style="padding:10px;">'
            + '<div class="placeholder mb10" style="height: 100px;"></div>'
            + '<div class="placeholder mb10" style="height: 100px;"></div>'
            + '<div class="placeholder" style="height: 100px;"></div>'
            + '</div>'
            + '</div>';
        return html;
    },

    Search: function (input) {
        $('#grid-data').html(this.Loading());
        $.ajax({
            url: "/client/search",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
            }
        });
    },

    OnPaging: function (value) {
        var objSearch = this.SearchParam;
        objSearch.currentPage = value;
        this.Search(objSearch);
    },

    ReLoad: function () {
        this.Search(this.SearchParam);
    },

    OnChangeClientName: function (value) {
        var objSearch = this.SearchParam;
        objSearch.searchModel.ClientName = value;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnChangeEmail: function (value) {
        var objSearch = this.SearchParam;
        objSearch.searchModel.Email = value.trim();
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnSearchByLabelId: function () {
        let _ArrLabel = [];
        $('.cbk-client-label:checked').each(function () {
            let dataValue = parseInt($(this).val());
            _ArrLabel.push(dataValue);
        });
        var objSearch = this.SearchParam;
        objSearch.searchModel.LableId = _ArrLabel.join();
        this.Search(objSearch);
    },

    OnChangeOrderRange: function (from, to) {
        var objSearch = this.SearchParam;
        objSearch.searchModel.FromQuantity = from;
        objSearch.searchModel.ToQuantity = to;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnChangeDateRange: function (from, to) {
        var objSearch = this.SearchParam;
        objSearch.searchModel.FromJoinDate = from;
        objSearch.searchModel.ToJoinDate = to;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnChangeInputJoinDate: function () {
        var fromdate = $('#from-join-date').val().split("/");
        var todate = $('#to-join-date').val().split("/");
        var _fromdate = new Date(fromdate[2], fromdate[1] - 1, fromdate[0]);
        var _todate = new Date(todate[2], todate[1] - 1, todate[0]);
        if (_fromdate.toJSON() !== null && _todate.toJSON() !== null) {
            if (_fromdate <= _todate) {
                this.OnChangeDateRange(_fromdate.toJSON(), _todate.toJSON());
            } else {
                _msgalert.error('Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc');
            }
        }
    },

    OnChangeInputPaymentDate: function () {
        let objSearch = this.SearchParam;
        let fromdate = $('#from-payment-date').val();
        let todate = $('#to-payment-date').val();
        let _fromdate = null;
        let _todate = null;

        if (fromdate != "") {
            let arrfromdate = fromdate.split("/");
            _fromdate = new Date(arrfromdate[2], arrfromdate[1] - 1, arrfromdate[0]);
            objSearch.searchModel.FromDate = _fromdate.toJSON();
        } else {
            objSearch.searchModel.FromDate = null;
        }

        if (todate != "") {
            let arrtodate = todate.split("/");
            _todate = new Date(arrtodate[2], arrtodate[1] - 1, arrtodate[0]);
            objSearch.searchModel.ToDate = _todate.toJSON();
        } else {
            objSearch.searchModel.ToDate = null;
        }

        if (_fromdate != null && _todate != null) {
            if (_fromdate > _todate) {
                _msgalert.error('Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc');
                return;
            }
        }

        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnSearchByMinRevenue: function (fromAmount) {
        let objSearch = this.SearchParam;
        objSearch.searchModel.FromAmount = fromAmount;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnSearchByMaxRevenue: function (toAmount) {
        let objSearch = this.SearchParam;
        objSearch.searchModel.ToAmount = toAmount;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    ConvertToJSONDate: function (strdate) {
        if (strdate == null || strdate == "") {
            return null;
        }
        let arrdate = strdate.split("/");
        var jsdate = new Date(arrdate[2], arrdate[1] - 1, arrdate[0]);
        return jsdate.toJSON();
    },

    OnChangeProvince: function (value) {
        $.ajax({
            url: "/client/getdistrictlist",
            type: "post",
            data: { provinceId: value },
            success: function (result) {
                var data = JSON.parse(result);
                var DistrictHtml = '<option value="">Quận / Huyện</option>';
                var WardHtml = '<option value="">Phường / Xã</option>';
                if (data != null && data.length > 0) {
                    data.map(function (obj) {
                        DistrictHtml += '<option value="' + obj.Value + '">' + obj.Text + '</option>';
                    });
                }
                $('#sl-client-district').html(DistrictHtml);
                $('#sl-client-ward').html(WardHtml);
            }
        });

        var objSearch = this.SearchParam;
        objSearch.searchModel.ProvinceId = value;
        if (value === "") {
            objSearch.searchModel.DistrictId = value;
            objSearch.searchModel.WardId = value;
        }
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnChangeDistrict: function (value) {
        $.ajax({
            url: "/client/getWardlist",
            type: "post",
            data: { districtId: value },
            success: function (result) {
                var data = JSON.parse(result);
                var WardHtml = '<option value="">Phường / Xã</option>';
                if (data != null && data.length > 0) {
                    data.map(function (obj) {
                        WardHtml += '<option value="' + obj.Value + '">' + obj.Text + '</option>';
                    });
                }
                $('#sl-client-ward').html(WardHtml);
            }
        });
        var objSearch = this.SearchParam;
        objSearch.searchModel.DistrictId = value;
        if (value === "") {
            objSearch.searchModel.WardId = value;
        }
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnChangeWard: function (value) {
        var objSearch = this.SearchParam;
        objSearch.searchModel.WardId = value;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnChangeAddress: function (value) {
        var objSearch = this.SearchParam;
        objSearch.searchModel.Address = value;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnGetDetail: function (id, tabactive, callback) {
        $.ajax({
            url: "/client/detail",
            type: "post",
            data: { Id: id, tabActive: tabactive },
            success: function (result) {
                callback(result);
            }
        });
    },

    OnGetHistory: function (id) {
        let title = 'Lịch sử truy cập';
        let url = '/client/history';
        let param = { Id: id };
        _magnific.OpenLargerPopup(title, url, param);
    },

    OnResetPassword: function (id) {
        let title = "Thông báo xác nhận";
        let description = "Bạn có chắc chắn muốn reset mật khẩu của khách hàng này không?";
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/client/resetpassword",
                type: "post",
                data: { clientId: id },
                success: function (data) {
                    if (data.isSuccess) {
                        _msgalert.success(data.message);
                        _magnific.OpenResetPasswordPopup(data.result);
                    } else {
                        _msgalert.error(data.message);
                    }
                }
            });
        });
    },

    OnResetPasswordDefault: function (id) {
        let title = "Thông báo xác nhận reset mật khẩu mặc định ";
        let description = "Bạn có chắc chắn muốn reset mật khẩu của khách hàng này bên hệ thống cũ không?";
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/client/ResetPasswordDefault",
                type: "post",
                data: { clientId: id },
                success: function (data) {
                    if (data.isSuccess) {
                        _msgalert.success(data.message);
                        //_magnific.OpenResetPasswordPopup(data.result);
                    } else {
                        _msgalert.error(data.message);
                    }
                }
            });
        });
    },

    OnUpdateStatus: function (id, callback) {
        let title = "Thông báo xác nhận";
        let description = "Bạn có chắc chắn muốn thay đổi trạng thái hoạt động của khách hàng này không?";
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/client/changestatus",
                type: "post",
                data: { clientId: id },
                success: function (data) {
                    callback(data);
                }
            });
        });
    },

    OnChangeDefaultAdress: function (id, callback) {
        let title = "Thông báo xác nhận";
        let description = "Bạn có chắc chắn muốn thay đổi địa chỉ mặc định của khách hàng này không?";
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/client/changedefaultaddress",
                type: "post",
                data: { addressId: id },
                success: function (data) {
                    callback(data);
                }
            });
        });
    },

    OnSearchAdvance: function () {
        let _clientNameValue = $('#ip-search-client').val().trim();
        let _phoneValue = $('#ip-client-phone').val().trim();
        let _orderCodeValue = $('#ip-client-order-code').val().trim();
        let _referralId = $('#ip-client-referralid').val().trim();

        
        let objSearch = this.SearchParam;
        objSearch.searchModel.ClientName = _clientNameValue;
        objSearch.searchModel.Phone = _phoneValue;
        objSearch.searchModel.ReferralId = _referralId;
        objSearch.searchModel.OrderCode = _orderCodeValue;
        objSearch.currentPage = 1;

        let _inputValue = "";

        if (_clientNameValue != null && _clientNameValue != "") {
            _inputValue += "[ClientName: " + _clientNameValue + "] ";
        }

        if (_phoneValue != null && _phoneValue != "") {
            _inputValue += "[Phone: " + _phoneValue + "] ";
        }

        if (_orderCodeValue != null && _orderCodeValue != "") {
            _inputValue += "[Order-Code: " + _orderCodeValue + "]";
        }

        if (_referralId != null && _referralId != "") {
            _inputValue += "[ReferralId: " + _referralId + "]";
        }

        if (_inputValue != "") {
            this.Search(objSearch);
            $('.btn_reset').removeClass('mfp-hide');
            $('#ip-kup-client-email').val(_inputValue);
        }

        $('.suggest-search2.dropdown-content').hide();
    },

    OnResetAdvance: function () {
        let objSearch = this.SearchParam;
        let _emptyvalue = "";
        objSearch.searchModel.ClientName = _emptyvalue;
        objSearch.searchModel.Email = _emptyvalue;
        objSearch.searchModel.Phone = _emptyvalue;
        objSearch.searchModel.ReferralId = _emptyvalue;
        objSearch.searchModel.OrderCode = _emptyvalue;

        this.Search(objSearch);

        $('#ip-kup-client-email').val(_emptyvalue);
        $('#ip-client-phone').val(_emptyvalue);
        $('#ip-client-order-code').val(_emptyvalue);
        $('#ip-client-referralid').val(_emptyvalue);
        $('#ip-search-client').val(_emptyvalue);

        $('.btn_reset').addClass('mfp-hide');
    },

    PushToUsexpressOld: function (clientId) {
        var objData = {
            clientId: clientId,
        }
        $.ajax({
            url: "/Client/PushToUsexpressOld",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.message);
                } else {
                    _msgalert.error(result.message);
                }
            },
            error: function (result) {
                _msgalert.error(result.message);
            }
        });
    },

    MappingClient: function () {
        $('#mapping').show();
        var lstClient = $('#lstClient').val()
        if (lstClient == null || lstClient == '') {
            _msgalert.error('Vui lòng nhập danh sách email khách hàng để đồng bộ');
            $('#mapping').hide();
            return;
        }
        lstClient = lstClient.trim()
        var listClient = lstClient.trim().split(',');
        var objData = {
            listClient: listClient,
        }
        $.ajax({
            url: "/Client/MappingClientJson",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.message);
                    _client.ReLoad();
                    $.magnificPopup.close();
                } else {
                    $('#mapping').hide();
                    _msgalert.error(result.message);
                }
            },
            error: function (result) {
                _msgalert.error(result.message);
            }
        });
    },

    OnOpenMappingClient: function () {
        let title = 'Đồng bộ khách hàng';
        let url = '/client/mappingClient';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    ExportExcel: function () {
        var input = this.SearchParam;
        $('#icon-export').removeClass('fa-file-excel-o');
        $('#icon-export').addClass('fa-spinner fa-pulse');
        $.ajax({
            url: "/Client/ExportExcel",
            type: "post",
            data: input,
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.message);
                }
                $('#icon-export').removeClass('fa-spinner fa-pulse');
                $('#icon-export').addClass('fa-file-excel-o');
            }
        });
    }
};