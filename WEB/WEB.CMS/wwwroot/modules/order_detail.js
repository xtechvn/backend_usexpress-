$(document).ready(function () {
    var orderid = $('#param-order-id').val();
    _cashback.LoadGrid(orderid);
    _payment.LoadGrid(orderid);
    $('#btn_updateaff').prop('disabled', true);
});

$("#ip-kup-orderno").autocomplete({
    minLength: 4,
    source: function (request, response) {
        $.ajax({
            url: "/Order/GetSuggestionOrder",
            data: {
                orderNo: request.term
            },
            success: function (data) {
                response(JSON.parse(data));
            }
        });
    },
    focus: function (event, ui) {
        event.preventDefault();
        this.value = ui.item.OrderNo;
        return false;
    },
    select: function (event, ui) {
        event.preventDefault();
        this.value = ui.item.OrderNo;
        location.href = "/order/detail/" + ui.item.Id;
    },
    keyup: function (event, ui) {
        event.preventDefault();
        this.value = ui.item.OrderNo;
        return false;
    },
    keydown: function (event, ui) {
        event.preventDefault();
        this.value = ui.item.OrderNo;
        return false;
    }
}).data("ui-autocomplete")._renderItem = function (ul, item) {
    $item = $("<li></li>").data("ui-autocomplete-item", item)
        .append("<strong style='font-size:14px;'>" + item.OrderNo + "</strong>"
            + "<br />"
            + "<strong style='color:#BABABA;font-size:13px;'>Khách hàng: " + item.ClientName + "</strong>"
            + "<br />"
            + "<strong style='color:#BABABA;font-size:13px;'>Địa chỉ: " + item.Address + "</strong>");
    return $item.appendTo(ul);
};

$('#ip-kup-orderno').keyup(function (e) {
    if (e.which === 13) {
        _orderDetail.OnMoveOrder();
    }
});

var _orderDetail = {
    OnMoveOrder: function () {
        let _orderNo = $('#ip-kup-orderno').val().trim();
        $.ajax({
            url: "/Order/FindOrderIdByOrderNo",
            data: {
                orderNo: _orderNo
            },
            success: function (data) {
                if (data != 0) {
                    location.href = "/order/detail/" + data;
                } else {
                    _msgalert.error("Đơn hàng không tồn tại trong hệ thống");
                }
            }
        });
    },

    GetOrderAmount: function (_orderId) {
        $.ajax({
            url: "/Order/GetOrderTotalAmount",
            data: {
                Id: _orderId
            },
            success: function (data) {
                $('#data-order-amount').val(formatNumber(data.toString()));
            }
        });
    },

    OnShowHistory: function (_orderNo) {
        let title = 'Lịch sử đơn hàng ' + _orderNo;
        let url = '/Order/OrderHistory';
        let param = { OrderNo: _orderNo };
        _magnific.OpenSmallPopupWithHeader(title, url, param);
    },

    PushOrderToAffilliate: function (_orderId, aff_name) {
        $.ajax({
            url: "/Order/PushOrderToAffilliate",
            type: "post",
            data: {
                orderId: _orderId,
                aff_network: aff_name
            },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    $('#btn_updateaff').prop('disabled', false);
                    $('#btn_pushaff').prop('disabled', true);
                }
                else {
                    _msgalert.error(result.msg);
                }
            }
        });
    },

    UpdateOrderAffilliate: function (_orderId, aff_name) {
        $.ajax({
            url: "/Order/UpdateOrderAffilliate",
            type: "post",
            data: {
                orderId: _orderId,
                aff_network: aff_name
            },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    $('#btn_updateaff').prop('disabled', true);

                }
                else {
                    _msgalert.error(result.msg);
                }
            }
        });
    },

    OnEditAddress: function (address_id, order_id) {
        let title = 'Thông tin giao hàng';
        let url = '/Order/OrderAddress';
        let param = { addressId: address_id, orderId: order_id };
        _magnific.OpenSmallPopupWithHeader(title, url, param, function () {
            $('#cbo-province').trigger('change');
        });
    },

    UpdateOrder: function () {
        var Payment_Date;

        var Str_PaymentDate = $('#PaymentDate').val();
        if (Str_PaymentDate != "" && Str_PaymentDate != null) {
            Payment_Date = ConvertToJSONDateTime(Str_PaymentDate);
        }

        var objParam = {
            OrderId: $('#param-order-id').val(),
            OrderStatus: $('#OrderStatus').val(),
            PaymentType: $('#PaymentType').val(),
            PaymentDate: Payment_Date
        }

        $.ajax({
            url: '/Order/UpdateOrder',
            type: 'POST',
            data: objParam,
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                } else {
                    _msgalert.error(result.message);
                }
            },
            error: function (jqXHR) {
            }
        });
    },

    OnChangeProvince: function (value, selected_district) {
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
                        DistrictHtml += '<option value="' + obj.Value + '" ' + (selected_district == obj.Value ? "selected" : "") + '>' + obj.Text + '</option>';
                    });
                }
                $('#cbo-district').html(DistrictHtml);
                $('#cbo-district').trigger('change');
                $('#cbo-ward').html(WardHtml);
            }
        });
    },

    OnChangeDistrict: function (value, selected_ward) {
        $.ajax({
            url: "/client/getWardlist",
            type: "post",
            data: { districtId: value },
            success: function (result) {
                var data = JSON.parse(result);
                var WardHtml = '<option value="">Phường / Xã</option>';
                if (data != null && data.length > 0) {
                    data.map(function (obj) {
                        WardHtml += '<option value="' + obj.Value + '" ' + (selected_ward == obj.Value ? "selected" : "") + ' >' + obj.Text + '</option>';
                    });
                }
                $('#cbo-ward').html(WardHtml);
            }
        });
    },

    UpdateAddress: function () {
        let FromCreate = $('#form-data-address');
        FromCreate.validate({
            rules: {
                ReceiverName: "required",
                ProvinceId: "required",
                DistrictId: "required",
                WardId: "required",
                Address: "required",
                Phone: "required"
            },
            messages: {
                ReceiverName: "Vui lòng nhập tên khách hàng",
                ProvinceId: "Vui lòng chọn tỉnh thành",
                DistrictId: "Vui lòng chọn quận huyện",
                WardId: "Vui lòng chọn phường xã",
                Address: "Vui lòng nhập địa chỉ",
                Phone: "Vui lòng nhập số điện thoại"
            }
        });

        if (FromCreate.valid()) {
            let form = document.getElementById('form-data-address');
            var formData = new FormData(form);

            $.ajax({
                url: '/Order/UpdateOrderAddress',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success('Cập nhật thành công');
                        $.magnificPopup.close();
                        setTimeout(function () {
                            window.location.reload();
                        }, 500);
                    } else {
                        _msgalert.error('Cập nhật thất bại');
                        console.log(result.message);
                    }
                },
                error: function (jqXHR) {
                },
                complete: function (jqXHR, status) {
                }
            });
        }
    }
};

var _cashback = {
    Search: function (orderid) {
        $.ajax({
            url: "/Order/Cashback",
            data: {
                orderId: orderid
            },
            success: function (data) {
                $('#grid-cashback-order').html(data);
            }
        });
    },

    LoadGrid: function (id) {
        this.Search(id);
    },

    FillUpdateItem: function (id, date, amount, note) {
        $('#model-cashback-id').val(id);
        $('#model-cashback-date').val(date);
        $('#model-cashback-note').val(note);
        $('#model-cashback-amount').val(formatNumber(amount));
    },

    Save: function () {
        let valid = true;

        var obj = {
            Id: $('#model-cashback-id').val() == "" ? 0 : parseFloat($('#model-cashback-id').val()),
            OrderId: $('#param-order-id').val(),
            Amount: $('#model-cashback-amount').val() == "" ? 0 : parseFloat($('#model-cashback-amount').val().replace(/,/g, "")),
            CashbackDate: ConvertToJSONDateTime($('#model-cashback-date').val()),
            Note: $('#model-cashback-note').val()
        };

        if (obj.CashbackDate == null) {
            _msgalert.error("Bạn phải chọn ngày hoàn tiền");
            return;
        }

        if (obj.Amount <= 0) {
            _msgalert.error("Bạn phải nhập số tiền hoàn lại");
            return;
        }

        if (valid) {
            $.ajax({
                url: "/Order/SaveCashback",
                type: "post",
                data: { model: obj },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        _cashback.LoadGrid(obj.OrderId);
                        _orderDetail.GetOrderAmount(obj.OrderId);
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        }
    },

    Delete: function (id) {
        let orderid = $('#param-order-id').val();
        let title = 'Thông báo xác nhận';
        let description = 'Bạn có chắc chắn muốn xóa?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/Order/DeleteCashback",
                type: "post",
                data: { cashbackId: id },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        _cashback.LoadGrid(orderid);
                        _orderDetail.GetOrderAmount(orderid);
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        });
    },

    ResetForm: function () {
        $('#model-cashback-id').val(0);
        $('#model-cashback-date').val('');
        $('#model-cashback-note').val('');
        $('#model-cashback-amount').val('');
    }
};

var _payment = {
    Search: function (orderid) {
        $.ajax({
            url: "/Order/Payment",
            data: {
                orderId: orderid
            },
            success: function (data) {
                $('#grid-payment-order').html(data);
            }
        });
    },

    LoadGrid: function (id) {
        this.Search(id);
    },

    FillUpdateItem: function (id, date, type, amount, note) {
        $('#model-payment-id').val(id);
        $('#model-payment-date').val(date);
        $('#model-payment-note').val(note);
        $('#model-payment-amount').val(formatNumber(amount));
        $('#model-payment-type').val(type);
    },

    Save: function () {
        let valid = true;

        var obj = {
            Id: $('#model-payment-id').val() == "" ? 0 : parseFloat($('#model-payment-id').val()),
            OrderId: $('#param-order-id').val(),
            Amount: $('#model-payment-amount').val() == "" ? 0 : parseFloat($('#model-payment-amount').val().replace(/,/g, "")),
            PaymentType: parseInt($('#model-payment-type').val()),
            PaymentDate: ConvertToJSONDateTime($('#model-payment-date').val()),
            Note: $('#model-payment-note').val()
            // ProductId: parseFloat($('#model-payment-productid').val()),
        };

        if (obj.PaymentDate == null) {
            _msgalert.error("Bạn phải chọn ngày thanh toán");
            return;
        }

        if (obj.Amount <= 0) {
            _msgalert.error("Bạn phải nhập số tiền thanh toán");
            return;
        }

        if (obj.PaymentType <= 0) {
            _msgalert.error("Bạn phải chọn hình thức thanh toán");
            return;
        }

        if (valid) {
            $.ajax({
                url: "/Order/SavePayment",
                type: "post",
                data: { model: obj },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        _payment.LoadGrid(obj.OrderId);
                        _orderDetail.GetOrderAmount(obj.OrderId);
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        }
    },

    Delete: function (id) {
        let orderid = $('#param-order-id').val();
        let title = 'Thông báo xác nhận';
        let description = 'Bạn có chắc chắn muốn xóa?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/Order/DeletePayment",
                type: "post",
                data: { paymentId: id },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        _payment.LoadGrid(orderid);
                        _orderDetail.GetOrderAmount(orderid);
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        });
    },

    ResetForm: function () {
        $('#model-payment-id').val(0);
        $('#model-payment-date').val('');
        $('#model-payment-note').val('');
        $('#model-payment-amount').val('');
        $('#model-payment-type').val(0);
    }
};