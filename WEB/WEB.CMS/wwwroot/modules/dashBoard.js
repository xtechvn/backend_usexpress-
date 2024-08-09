var totalCustomer = 0, totalReturningCustomer = 0, totalPaymentClient = 0, totalOrder = 0, totalOrderTemp = 0;
var totalRevenuToday = 0, totalRevenuTodayTemp = 0, totalRevenu = 0;

var totalRevenuTodayStr = '', totalRevenuTodayTempStr = '';

var currentType = 0, currentChartTab = 0, currentChartType = 0, revenuType = 0;
var listChartRevenuWeek = [], listDataChartRevenu = [], listDataChartLabel = [];
var recent_activity_page = 1;
var chartRevenu = null, chartLabel = null;
var logActivityToday_page = 1;

const constChart_Label_Type_Revenu = 1, constChart_Label_Type_Quantity = 2;
const constRevenu_Week = 1, constRevenu_Month = 2;
const constChart_Type_Today = 1, constChart_Type_Yesterday = 2, constChart_Type_Week = 3, constChart_Type_Month = 4;
var pageIndexLog = 1, pageSizeLog = 5, pageIndexLogToday = 1, pageSizeLogToday = 5;

var firstload = true;


$(document).ready(function () {
    menu.Init();

    //setTimeout(function () {
    //    menu.GetLogActivityToday();
    //}, 100)

    setTimeout(function () {
        menu.GetTotalCustomerInday();
    }, 200)

    setTimeout(function () {
        menu.GetTotalCustomerInday();
    }, 200)

    setTimeout(function () {
        menu.GetRevenuToday();
    }, 400)

    setTimeout(function () {
        menu.GetRevenuTemp();
    }, 500)

    setTimeout(function () {
        menu.GetRevenuDay();
    }, 600)

    setTimeout(function () {
        menu.GetTotalErrorOrder();
    }, 500)

    setTimeout(function () {
        menu.GetCrawlPercentToDay();
    }, 700)

    setTimeout(function () {
        menu.GetChartRevenu();
    }, 800)

    setTimeout(function () {
        menu.GetChartLabelRevenu();
    }, 1200)
   

    setTimeout(function () {
        menu.LoadRecentActivityLog();
    }, 2500)

    setTimeout(function () {
        menu.LoadLogActivityToday();
    }, 3000)

    //setInterval(function () {
    //    menu.GetLogActivityToday();
    //}, 10000)
});

//Dynamic Binding:
$('body').on('click', '.popup_order_history', function () {
    let _orderNo = $(this).attr('data-orderno');
    let title = 'Lịch sử đơn hàng ' + _orderNo;
    let url = '/Order/OrderHistory';
    let param = { OrderNo: _orderNo };
    _magnific.OpenSmallPopupWithHeader(title, url, param);
});
$('body').on('change', '.shipping_log_filter', function () {
    $('#order_shipping_log_loading').css("display", "");
    $('#order_shipping_log').css("display", "none");
    $('#order_shipping_log').html("");
    menu.LoadLogActivityToday();
});

var menu = {
    Init: function () {
        $('#revenuday_increase').hide();
        $('#revenuday_deincrease').hide();
        $('#img_deincrease').hide();
        $('#img_increase').hide();
        $('#imgLoading').show();
        $('#chartLabel1Month').hide();
        $('#revenu1Month').hide();
        $('#chartLabelMonth').hide();
        //ẩn title
        $('#titlechartLabelYesterday').hide();
        $('#chartLabelQuantity').hide();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').hide();
        //doanh thu thuần
        $('#active1month').removeClass('active')
        //doanh thu theo label
        $('#labelrevenuYesterday').removeClass('active')
        $('#labelrevenu7day').removeClass('active')
        $('#labelrevenu30day').removeClass('active')
        //so luong don theo label
        $('#labelquantityToday').removeClass('active')
        $('#labelquantityYesterday').removeClass('active')
        $('#labelquantity7day').removeClass('active')
        $('#labelquantity30day').removeClass('active')
        $('#titlechartLabelYesterday').hide();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').hide();

        $('#order_shipping_log').html('');
        $('#order_shipping_log').hide();
        $('#success_order_list').html();
    },

    GetTotalCustomerInday: function () {
        $.ajax({
            url: "/DashBoard/GetCustomerInDay",
            type: "post",
            success: function (result) {
                totalCustomer = result.total_new_client;
                totalReturningCustomer = result.total_returning_client;
                totalPaymentClient = result.total_payment_client;
                $('#customerInDay').html(result.total_new_client + " kh mới");
                $('#total_returning_client').html(result.total_returning_client + " kh quay lại");
                $('#total_payment_client').html(result.total_payment_client + " kh thanh toán");
            }
        });
    },

    GetRevenuToday: function () {
        $.ajax({
            url: "/DashBoard/GetRevenuToday",
            type: "post",
            success: function (result) {
                $('#totalOrder').html(result.data.totalOrder + " đơn đã thanh toán");
                totalOrder = result.data.totalOrder
                $('#revenu7Days').show();
                $('#revenu1Month').hide();
                $('#totalMoney').html(result.data.revenueStr);
                totalRevenuToday = result.data.revenue
                totalRevenuTodayStr = result.data.revenueStr
            }
        });
    },

    GetRevenuTemp: function () {
        $.ajax({
            url: "/DashBoard/GetRevenuTemp",
            type: "post",
            success: function (result) {
                $('#totalOrderTemp').html(result.data.totalOrder + " đơn chưa thanh toán");
                totalOrderTemp = result.data.totalOrder
                $('#revenu7Days').show();
                $('#revenu1Month').hide();
                $('#totalMoneyTemp').html(result.data.revenueStr);
                totalRevenuTodayTemp = result.data.revenue
                totalRevenuTodayTempStr = result.data.revenueStr
            }
        });
    },

    ViewCustomer: function () {
        if (totalCustomer <= 0) {
            _msgalert.error('Hiện chưa có khách hàng nào mới');
            return;
        }
        var todayTime = new Date();
        var month = ((todayTime.getMonth() + 1) < 10 ? "0" : '') + (todayTime.getMonth() + 1);
        var day = (todayTime.getDate() < 10 ? "0" : '') + todayTime.getDate();
        var year = (todayTime.getFullYear());
        window.location.href = "/Client?date=" + day + "-" + month + "-" + year;
    },

    ViewReturningCustomer: function () {
        if (totalReturningCustomer <= 0) {
            _msgalert.error('Hiện chưa có khách hàng nào quay lại');
            return;
        }
        window.location.href = "/client?IsReturningClient=1";
    },

    ViewPaymentCustomer: function () {
        if (totalReturningCustomer <= 0) {
            _msgalert.error('Chưa có khách hàng thanh toán');
            return;
        }
        window.location.href = "/client?IsPaymentInDay=1";
    },

    ViewOrder: function (paymentStatus) {
        if (totalOrder <= 0) {
            _msgalert.error('Hiện chưa có đơn hàng nào mới');
            return;
        }
        var todayTime = new Date();
        var month = ((todayTime.getMonth() + 1) < 10 ? "0" : '') + (todayTime.getMonth() + 1);
        var day = (todayTime.getDate() < 10 ? "0" : '') + todayTime.getDate();
        var year = (todayTime.getFullYear());
        window.location.href = "/Order?date=" + day + "-" + month + "-" + year + '&PaymentStatus=' + paymentStatus;
    },

    ViewOrderTemp: function (paymentStatus) {
        if (totalOrderTemp <= 0) {
            _msgalert.error('Hiện chưa có đơn hàng nào chưa thanh toán trong hôm nay');
            return;
        }
        var todayTime = new Date();
        var month = ((todayTime.getMonth() + 1) < 10 ? "0" : '') + (todayTime.getMonth() + 1);
        var day = (todayTime.getDate() < 10 ? "0" : '') + todayTime.getDate();
        var year = (todayTime.getFullYear());
        window.location.href = "/Order?date=" + day + "-" + month + "-" + year + '&PaymentStatus=' + paymentStatus;
    },

    GetRevenuDay: function () {
        $.ajax({
            url: "/DashBoard/GetRevenuDay",
            type: "post",
            success: function (result) {
                if (result.data < 0) {
                    $('#revenuday_increase').hide();
                    $('#revenuday_deincrease').show();
                    $('#img_increase').hide();
                    $('#img_deincrease').show();
                    $('#revenuday_deincrease').html(result.data + "%");
                }
                if (result.data > 0) {
                    $('#revenuday_increase').show();
                    $('#revenuday_deincrease').hide();
                    if (result.data > 100) {
                        $('#img_increase').show();
                    }
                    $('#img_deincrease').hide();
                    $('#revenuday_increase').html(result.data + "%");
                }
                if (result.data == 0) {
                    $('#revenuday_increase').show();
                    $('#revenuday_deincrease').hide();
                    $('#img_increase').hide();
                    $('#img_deincrease').hide();
                    $('#revenuday_increase').html(result.data + "%");
                }
            }
        });
    },

    GetCrawlPercentToDay: function () {
        var today = new Date();
        var date = today.getDate() + '/' + (today.getMonth() + 1) + '/' + today.getFullYear();
        var text = '<a href="/product?day=' + date + '"> result </a>';
        $.ajax({
            url: "/DashBoard/GetCrawlPercent",
            type: "post",
            success: function (result) {
                if (result.status < 0) {
                    $('#warning_crawlpercent').hide();

                }
                else {
                    $('#crawl_percent_increase').show();
                    $('#warning_crawlpercent').hide();
                    $('#crawl_percent_increase').html(text.replace(" result ", result.data));
                }
            }
        });
    },

    GetChartRevenu: function (revenuType) {
        if (revenuType == undefined || revenuType == null || revenuType == '')
            revenuType = 1
        $('#imgLoading').show();
        $('#revenuChart').hide();
        $.ajax({
            url: "/DashBoard/GetChartRevenu",
            type: "post",
            data: { revenuType: revenuType },
            success: function (result) {
                $('#imgLoading').hide();
                $('#revenuChart').show();
                totalRevenu = result.totalRevenu
                $('#doanhThuThuan').html("<i class='fa fa-arrow-circle-o-right'></i>" + totalRevenu);
                listDataChartRevenu = result.dataChart
                if (revenuType == constRevenu_Week) {
                    $('#active7day').addClass('active')
                    $('#active1month').removeClass('active')
                    $('#revenu7Days').show();
                    $('#revenu1Month').hide();
                }
                if (revenuType == constRevenu_Month) {
                    $('#active7day').removeClass('active')
                    $('#active1month').addClass('active')
                    $('#revenu7Days').hide();
                    $('#revenu1Month').show();
                }
                chart.LoadChartRevenu(revenuType);
            }
        });
    },

    GetChartLabelRevenu: function (revenuType, revenuChartType) {
        $('#imgLabelLoading').show();
        if (revenuType == undefined || revenuType == null || revenuType == ''
            || revenuType == constChart_Type_Today) {
            revenuType = constChart_Type_Today
            $('#labelrevenuToday').addClass('active')
            $('#titlechartLabelToday').show();
            $('#titlechartLabelYesterday').hide();
            $('#titlechartLabel7Days').hide();
            $('#titlechartLabel1Month').hide();
        }
        if (revenuChartType == undefined || revenuChartType == null || revenuChartType == ''
            || revenuChartType == constChart_Label_Type_Revenu) {
            revenuChartType = constChart_Label_Type_Revenu
            $('#labelQuantityToday').addClass('active')
        }
        if (revenuChartType == constChart_Label_Type_Quantity) {
            if (currentChartTab == constChart_Type_Today) {
                $('#labelQuantityToday').addClass('active')
            } else {
                $('#labelQuantityToday').removeClass('active')
            }
            if (currentChartTab == constChart_Type_Yesterday) {
                $('#labelQuantityYesterday').addClass('active')
            } else {
                $('#labelQuantityYesterday').removeClass('active')
            }
            if (currentChartTab == constChart_Type_Week) {
                $('#labelQuantity7day').addClass('active')
            } else {
                $('#labelQuantity7day').removeClass('active')
            }
            if (currentChartTab == constChart_Type_Month) {
                $('#labelQuantity30day').addClass('active')
            } else {
                $('#labelQuantity30day').removeClass('active')
            }
        }
        $.ajax({
            url: "/DashBoard/GetChartLabel",
            type: "post",
            data: { revenuType: revenuType, revenuChartType: revenuChartType },
            success: function (result) {
                listDataChartLabel = result.dataChart
                $('#imgLabelLoading').hide();
                $('#revenuLabelChart').show();
                if (revenuChartType == constChart_Label_Type_Revenu) {
                    chart.LoadChartLabelRevenu(constChart_Type_Today);
                    $('#chartLabelRevenu').show();
                    $('#chartLabelQuantity').hide();
                }
                if (revenuChartType == constChart_Label_Type_Quantity) {
                    chart.LoadChartLabelQuantity(constChart_Type_Today);
                    $('#chartLabelRevenu').hide();
                    $('#chartLabelQuantity').show();
                }
            }
        });
    },

    //loại biểu đồ doanh thu label 
    LoadChartLabelRevenuToday: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelrevenuToday').addClass('active')
        $('#labelrevenuYesterday').removeClass('active')
        $('#labelrevenu7day').removeClass('active')
        $('#labelrevenu30day').removeClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').show();
        $('#titlechartLabelYesterday').hide();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').hide();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabelRevenu').show();
        $('#chartLabelQuantity').hide();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    LoadChartLabelRevenuYesterday: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelrevenuToday').removeClass('active')
        $('#labelrevenuYesterday').addClass('active')
        $('#labelrevenu7day').removeClass('active')
        $('#labelrevenu30day').removeClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').hide();
        $('#titlechartLabelYesterday').show();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').hide();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabelRevenu').show();
        $('#chartLabelQuantity').hide();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    LoadChartLabelRevenuWeek: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelrevenuToday').removeClass('active')
        $('#labelrevenuYesterday').removeClass('active')
        $('#labelrevenu7day').addClass('active')
        $('#labelrevenu30day').removeClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').hide();
        $('#titlechartLabelYesterday').hide();
        $('#titlechartLabel7Days').show();
        $('#titlechartLabel1Month').hide();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabelRevenu').show();
        $('#chartLabelQuantity').hide();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    LoadChartLabelRevenuMonth: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelrevenuToday').removeClass('active')
        $('#labelrevenuYesterday').removeClass('active')
        $('#labelrevenu7day').removeClass('active')
        $('#labelrevenu30day').addClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').hide();
        $('#titlechartLabelYesterday').hide();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').show();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabel7Days').hide();
        $('#chartLabel1Month').show();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    //loại biểu đồ số lượng đơn theo label 
    LoadChartLabelQuantityToday: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelQuantityToday').addClass('active')
        $('#labelQuantityYesterday').removeClass('active')
        $('#labelQuantity7day').removeClass('active')
        $('#labelQuantity30day').removeClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').show();
        $('#titlechartLabelYesterday').hide();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').hide();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabel7Days').show();
        $('#chartLabel1Month').hide();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    LoadChartLabelQuantityYesterday: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelQuantityToday').removeClass('active')
        $('#labelQuantityYesterday').addClass('active')
        $('#labelQuantity7day').removeClass('active')
        $('#labelQuantity30day').removeClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').hide();
        $('#titlechartLabelYesterday').show();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').hide();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabel7Days').show();
        $('#chartLabel1Month').hide();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    LoadChartLabelQuantityWeek: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelQuantityToday').removeClass('active')
        $('#labelQuantityYesterday').removeClass('active')
        $('#labelQuantity7day').addClass('active')
        $('#labelQuantity30day').removeClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').hide();
        $('#titlechartLabelYesterday').hide();
        $('#titlechartLabel7Days').show();
        $('#titlechartLabel1Month').hide();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabel7Days').show();
        $('#chartLabel1Month').hide();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    LoadChartLabelQuantityMonth: function (revenuType, revenuChartType) {
        currentChartTab = revenuType;
        currentChartType = revenuChartType;
        //active loại biểu đồ
        $('#labelQuantityToday').removeClass('active')
        $('#labelQuantityYesterday').removeClass('active')
        $('#labelQuantity7day').removeClass('active')
        $('#labelQuantity30day').addClass('active')
        //ẩn hiện title
        $('#titlechartLabelToday').hide();
        $('#titlechartLabelYesterday').hide();
        $('#titlechartLabel7Days').hide();
        $('#titlechartLabel1Month').show();
        //ẩn hiện doanh thu hoặc số lượng
        $('#chartLabel7Days').hide();
        $('#chartLabel1Month').show();
        this.GetChartLabelRevenu(revenuType, revenuChartType);
    },

    LoadChartLabel: function (type) {
        if (currentType == 0) {
            if (type == constChart_Label_Type_Revenu) {
                $('#chartLabelRevenu').show();
                $('#chartLabelQuantity').hide();
                //doanh thu thuần
                if (currentChartTab == constChart_Type_Today) {
                    $('#labelrevenuToday').addClass('active')
                } else {
                    $('#labelrevenuToday').removeClass('active')
                }
                if (currentChartTab == constChart_Type_Yesterday) {
                    $('#labelrevenuYesterday').addClass('active')
                } else {
                    $('#labelrevenuYesterday').removeClass('active')
                }
                if (currentChartTab == constChart_Type_Week) {
                    $('#labelrevenu7day').addClass('active')
                } else {
                    $('#labelrevenu7day').removeClass('active')
                }
                if (currentChartTab == constChart_Type_Month) {
                    $('#labelrevenu30day').addClass('active')
                } else {
                    $('#labelrevenu30day').removeClass('active')
                }
                this.GetChartLabelRevenu(currentChartTab, type);
            }
            if (type == constChart_Label_Type_Quantity) {
                $('#chartLabelRevenu').hide();
                $('#chartLabelQuantity').show();
                //số lượng đơn hàng
                if (currentChartTab == constChart_Type_Today) {
                    $('#labelQuantityToday').addClass('active')
                } else {
                    $('#labelQuantityToday').removeClass('active')
                }
                if (currentChartTab == constChart_Type_Yesterday) {
                    $('#labelQuantityYesterday').addClass('active')
                } else {
                    $('#labelQuantityYesterday').removeClass('active')
                }
                if (currentChartTab == constChart_Type_Week) {
                    $('#labelQuantity7day').addClass('active')
                } else {
                    $('#labelQuantity7day').removeClass('active')
                }
                if (currentChartTab == constChart_Type_Month) {
                    $('#labelQuantity30day').addClass('active')
                } else {
                    $('#labelQuantity30day').removeClass('active')
                }
                this.GetChartLabelRevenu(currentChartTab, type);
            }
            currentType = type;
        }
        else {
            if (currentType != type) {
                if (type == constChart_Label_Type_Revenu) {
                    $('#chartLabelRevenu').show();
                    $('#chartLabelQuantity').hide();
                    //doanh thu thuần
                    if (currentChartTab == constChart_Type_Today) {
                        $('#labelrevenuToday').addClass('active')
                    } else {
                        $('#labelrevenuToday').removeClass('active')
                    }
                    if (currentChartTab == constChart_Type_Yesterday) {
                        $('#labelrevenuYesterday').addClass('active')
                    } else {
                        $('#labelrevenuYesterday').removeClass('active')
                    }
                    if (currentChartTab == constChart_Type_Week) {
                        $('#labelrevenu7day').addClass('active')
                    } else {
                        $('#labelrevenu7day').removeClass('active')
                    }
                    if (currentChartTab == constChart_Type_Month) {
                        $('#labelrevenu30day').addClass('active')
                    } else {
                        $('#labelrevenu30day').removeClass('active')
                    }
                    this.GetChartLabelRevenu(currentChartTab, type);
                }
                if (type == constChart_Label_Type_Quantity) {
                    $('#chartLabelRevenu').hide();
                    $('#chartLabelQuantity').show();
                    //số lượng đơn hàng
                    if (currentChartTab == constChart_Type_Today) {
                        $('#labelQuantityToday').addClass('active')
                    } else {
                        $('#labelQuantityToday').removeClass('active')
                    }
                    if (currentChartTab == constChart_Type_Yesterday) {
                        $('#labelQuantityYesterday').addClass('active')
                    } else {
                        $('#labelQuantityYesterday').removeClass('active')
                    }
                    if (currentChartTab == constChart_Type_Week) {
                        $('#labelQuantity7day').addClass('active')
                    } else {
                        $('#labelQuantity7day').removeClass('active')
                    }
                    if (currentChartTab == constChart_Type_Month) {
                        $('#labelQuantity30day').addClass('active')
                    } else {
                        $('#labelQuantity30day').removeClass('active')
                    }
                    this.GetChartLabelRevenu(currentChartTab, type);
                }
                currentType = type;
            }
        }

    },

    //GetLogActivityToday: function () {
    //    $.ajax({
    //        url: "/DashBoard/GetOrderLogActivityToday",
    //        data: {},
    //        type: "post",
    //        success: function (result) {
    //            if (firstload)
    //                $('#loadingImg').hide()
    //            $('#LogActivityToday').html('')
    //            if (result.data != null && result.data.length > 0) {
    //                $('#LogActivityTodayEmpty').hide()
    //                for (var i = 0; i < result.data.length; i++) {
    //                    var className = "images/icons/uslogo.jpg";
    //                    var html = " <li>" +
    //                        "<img class='icon' src='" + className + "'>" +
    //                        "   <a href='/order/detail/" + result.data[i].orderId + "'>Đơn <strong class='color-main'>"
    //                        + result.data[i].order_no + "</strong> " + result.data[i].notify_name + "</a>" +
    //                        " <div class='date'>" + result.data[i].time +
    //                        (result.data[i].status == 13 ? " - thời gian thanh toán: " : " - thời gian đổi trạng thái: ")
    //                        + (result.data[i].hour < 10 ? '0' + result.data[i].hour : result.data[i].hour) +
    //                        ":" + (result.data[i].minute < 10 ? '0' + result.data[i].minute : result.data[i].minute) + "</div>" +
    //                        "  </li>";
    //                    $('#LogActivityToday').append(html);
    //                    firstload = false
    //                }
    //            } else {
    //                $('#LogActivityToday').hide()
    //                $('#LogActivityTodayEmpty').html('Chưa có dữ liệu lịch sử đơn hàng')
    //                firstload = false
    //            }
    //            pageIndexLog++;
    //        },
    //        error: function () {
    //            if (firstload)
    //                $('#loadingImg').hide()
    //            $('#LogActivityToday').html('Chưa có dữ liệu lịch sử đơn hàng')
    //        }
    //    });
    //},

    GetTotalErrorOrder: function () {
        $.ajax({
            url: "/DashBoard/GetTotalErrorOrder",
            type: "post",
            success: function (result) {
                $('#total_error_order').html(result.data);
                if (result.data <= 0) {
                    $('#total_error_order').parent().siblings().addClass('mfp-hide');
                } else {
                    $('#total_error_order').parent().siblings().removeClass('mfp-hide');
                }
            },
            error: function () {

            }
        });
    },

    ViewErrorOrder: function () {
        var totalOrder = parseInt($('#total_error_order').text().trim());
        if (totalOrder <= 0) {
            _msgalert.error('Hiện chưa có đơn hàng nào lỗi');
            return;
        }
        window.location.href = "/Order?IsOrderError=1";
    },

    LoadRecentActivityLog: function () {
        $.ajax({
            url: "/DashBoard/GetAnswerSurvery",
            type: "post",
            data: { page_index: 1, page_size: 6 },
            success: function (result) {
                if (result != null && result != "[]") {
                    $('#recent_notifi_box').html("");
                    var obj = JSON.parse(result);
                    if (obj != null) {
                        obj.forEach(async function (answer) {
                            var html = " <li>" +
                                "<i class=\"fa fa-file-o\"></i>" +
                                "<a href=\"\"><span class=\"blue\">" + answer.user_name + "</span> góp ý tính năng \"" + answer.log_type + "\" với nội dung: " + answer.log + "</a>" +
                                "<div class=\"date\">" + answer.time_from_today + " - ngày gửi: " + answer.log_date + "</div>" +
                                "</li>";
                            $('#recent_notifi_box').append(html);

                        })
                    }

                }
                else {
                    var html = "<li>" +
                        "Hiện tại chưa có hoạt động nào mới."
                    "</li>";
                    $('#recent_notifi_box').html(html);
                    $('#recent_act_lm').css("display", "none");
                    $('#recent_act_lm').css("pointer-events", "none");
                    $('#recent_act_lm').css("cursor", "default");
                }
            }
        });

    },

    LoadMoreRecentActivity: function () {
        var next_page = recent_activity_page + 1;
        $.ajax({
            url: "/DashBoard/GetAnswerSurvery",
            type: "post",
            data: { page_index: next_page, page_size: 6 },
            success: function (result) {
                if (result != null && result != "[]") {
                    recent_activity_page += 1;
                    var obj = JSON.parse(result);
                    if (obj != null) {
                        obj.forEach(async function (answer) {
                            var html = " <li>" +
                                "<i class=\"fa fa-file-o\"></i>" +
                                "<a href=\"\"><span class=\"blue\">" + answer.user_name + "</span> góp ý tính năng \"" + answer.log_type + "\" với nội dung: " + answer.log + "</a>" +
                                "<div class=\"date\">" + answer.time_from_today + " - ngày gửi: " + answer.log_date + "</div>" +
                                "</li>";
                            $('#recent_notifi_box').append(html);

                        })
                    }

                }
                else {
                    $('#recent_act_lm').css("display", "none");
                    $('#recent_act_lm').css("pointer-events", "none");
                    $('#recent_act_lm').css("cursor", "default");
                }
            }
        });
    },
    LoadLogActivityToday: function () {
      
        var filter = $('#select_order_shipping_log_filter').find(':selected').val() == undefined ? "0" : $('#select_order_shipping_log_filter').find(':selected').val();
        $.ajax({
            url: "/DashBoard/GetOrderShippingLogToday",
            type: "post",
            data: { filter: filter },
            success: function (result) {
                if (result != null && result != "[]") {
                    logActivityToday_page += 1;
                    var obj = JSON.parse(result);
                    if (obj != null && obj.length > 0) {
                        obj.forEach(async function (item) {
                            var status_icon = 'images/icons/payment.svg';
                            switch (item.OrderStatus) {
                                case 6: {
                                    status_icon = 'images/icons/box.svg';
                                    menu.LogAcitivityDynamicHTMLBind(item, status_icon,  3, 'Số ngày xử lý từ lúc đặt mua');
                                } break;
                                case 13: {
                                    menu.LogAcitivityDynamicHTMLBind(item, status_icon, 1, 'Số ngày xử lý từ lúc thanh toán');
                                } break;
                                case 7: {
                                    status_icon = 'images/icons/shipped.svg';
                                    menu.LogAcitivityDynamicHTMLBind(item, status_icon, 3, 'Số ngày xử lý giao cho khách');
                                } break;
                                case 10: {
                                    status_icon = 'images/icons/plane.svg';
                                    menu.LogAcitivityDynamicHTMLBind(item, status_icon, 3, 'Số ngày chuyển hàng về VN');
                                } break;
                                case 11: {
                                    status_icon = 'images/icons/box.svg';
                                    menu.LogAcitivityDynamicHTMLBind(item, status_icon, 2, 'Số ngày lưu kho tại VN');
                                } break;
                                case 16: {
                                    status_icon = 'images/icons/box.svg';
                                    menu.LogAcitivityDynamicHTMLBind(item, status_icon, 3, 'Số ngày lưu kho tại VN');
                                } break;
                                default: {
                                    menu.LogAcitivityDynamicHTMLBind(item, status_icon, 3);
                                } break;
                            }
                        });
                    }
                } else {
                    $('#order_shipping_log').append('<p style="text-align: center;">Hiện tại chưa có đơn hàng nào.</p>');

                }
                $('#order_shipping_log').css("display", "");
                $('#order_shipping_log_loading').css("display", "none");
            }
        });
    },
    LogAcitivityDynamicHTMLBind: function (item, status_icon, exprire_range, total_time_label='Số ngày xử lý đơn') {
        var html = '<li> <div class="icon icon--v2"> <img src="{status_icon}"> </div> ' +
            '<a class="{isnormal}" href="/order/detail/{Order_id}"><b>{OrderCode}:</b></a> {Order_status_name} <div class="note day_ship_block">' +
            ' <p>{Total_time_label}: <a class="{isred} open-popup popup_order_history" style="{color_text_day} " data-orderno="{OrderCode}" href="javascript:;">{Total_shippingDay}</a></p> ' +
            '{OOR_date}' +
            '</div> </li>';
        var html_append = html.replace('{status_icon}', status_icon).replaceAll('{OrderCode}', item.OrderNo).replace('{Order_status_name}', item.OrderStatusName).replace('{Total_shippingDay}', item.LastestOrderProgressDay + ' ngày').replaceAll('{Order_id}', item.Id);
        html_append = html_append.replace('{Total_time_label}', total_time_label);
       
        if (parseInt(item.exprire_day_count) != undefined && parseInt(item.exprire_day_count) > 0) {
             html_append = html_append.replace('{OOR_date}', '<p>Trễ hạn: <a class="red open-popup popup_order_history" data-orderno="' + item.OrderNo + '" href="javascript:;">' + parseInt(item.exprire_day_count) + ' ngày</a></p> {Total_progress_order} <img class="feeling" style="width: 20px;" src="{icon_angry}">');
             html_append = html_append.replace('{isred}', 'red');
             html_append = html_append.replace('{isnormal}', 'red');
             html_append = html_append.replace('{color_text_day}', '')
             if (parseInt(item.exprire_day_count) <= exprire_range) {
                 html_append = html_append.replace('{icon_angry}', 'images/icons/sad.svg');
             }
             else {
                 html_append = html_append.replace('{icon_angry}', 'images/icons/angry.svg');
             }
         }
         else {
             html_append = html_append.replace('{OOR_date}', '<p>Trễ hạn (từ lúc thanh toán): <a class="red open-popup popup_order_history" data-orderno="' + item.OrderNo + '" href="javascript:;">' + (parseInt(item.TotalOrderProgressDay) - 14) + ' ngày</a></p> {Total_progress_order} <img class="feeling" style="width: 20px;" src="{icon_angry}">');
             html_append = html_append.replace('{isred}', '');
             html_append = html_append.replace('{isnormal}', 'red');
             html_append = html_append.replace('{color_text_day}', 'color: #49B391;')
             if ((parseInt(item.TotalOrderProgressDay) - 14) <= exprire_range) {
                 html_append = html_append.replace('{icon_angry}', 'images/icons/sad.svg');
             }
             else {
                 html_append = html_append.replace('{icon_angry}', 'images/icons/angry.svg');
             }

         }
        if (parseInt(item.TotalOrderProgressDay) != undefined && (parseInt(item.TotalOrderProgressDay) - 14) > 0) {
            html_append = html_append.replace('{Total_progress_order}', '<p>Tổng số ngày xử lý đơn: <a class="red open-popup popup_order_history" data-orderno="' + item.OrderNo + '" href="javascript:;">' + parseInt(item.TotalOrderProgressDay) + ' ngày</a></p>');
        }
        else {
            html_append = html_append.replace('{Total_progress_order}', '<p>Tổng số ngày xử lý đơn: <a class="open-popup popup_order_history" style="color: #49B391;" data-orderno="' + item.OrderNo + '" href="javascript:;">' + parseInt(item.TotalOrderProgressDay) + ' ngày</a></p>');
        }
        $('#order_shipping_log').append(html_append);
    },
    ExportOrderExpectDay: function () {

    }
};

var chart = {
    LoadChartRevenu: function (type) {
        if (chartRevenu != null && chartRevenu != undefined) {
            chartRevenu.destroy();
        }
        var backgroundColor = '#49B391';
        var borderColor = '#49B391';
        var backgroundColorAgo = '#ECEAEA';
        var borderColorAgo = '#ECEAEA';
        if (chartRevenu != undefined) chartRevenu.destroy();
        var ctx = document.getElementById("revenuChart").getContext('2d');

        var listRevenu = [];
        var listLabel = [];
        var listLabelAgo = [];
        var label = "";
        var listDataNow = [];
        var listDataAgo = [];
        for (var i = 0; i < listDataChartRevenu.length; i++) {
            listLabel.push(listDataChartRevenu[i].dateStr);
            listLabelAgo.push(listDataChartRevenu[i].datePassStr);
            listDataNow.push(listDataChartRevenu[i].totalShipFee);
            listDataAgo.push(listDataChartRevenu[i].totalShipFeePass);
        }
        var barChartData = {
            labels: listLabel,
            datasets: [
                {
                    type: "bar",
                    fill: false,
                    label: 'Doanh thu thuần cùng kỳ',
                    backgroundColor: backgroundColorAgo,
                    borderColor: borderColorAgo,
                    borderWidth: 1,
                    data: listDataAgo,
                    stack: 2
                },
                {
                    type: "bar",
                    fill: false,
                    label: 'Doanh thu thuần',
                    backgroundColor: backgroundColor,
                    borderColor: borderColor,
                    borderWidth: 1,
                    data: listDataNow,
                    stack: 1
                },
            ]
        };

        barPercentageCustomer = 0.9 * Math.min(6, listLabel.length) / 6;// lay ti le nho

        var ObjectChart = {
            type: 'bar',
            data: barChartData,
            options: {
                responsive: true,
                legend: {
                    cursor: "pointer",
                    position: 'top',
                },
                title: {
                    display: true,
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            fontColor: 'black',
                            beginAtZero: true,
                            callback: function (value, index, values) {
                                return value / 1000000 + " tr";
                            }
                        },
                        stacked: true,
                    }],
                    xAxes: [{
                        barPercentage: 0.5,
                    }]

                },
                tooltips: {
                    enabled: true,
                    mode: 'single',
                    callbacks: {
                        title: function () { },
                        label: function (tooltipItems, data) {
                            var indice = tooltipItems.index;
                            var dateStr = data.labels[indice].toLocaleString();
                            var date_Ago = "";
                            var order_Count = 0;
                            var order_Count_Ago = 0;
                            var revenu = 0;
                            var revenu_ago = 0;
                            for (var i = 0; i < listDataChartRevenu.length; i++) {
                                if (listDataChartRevenu[i].dateStr == dateStr) {
                                    order_Count = listDataChartRevenu[i].orderCount;
                                    order_Count_Ago = listDataChartRevenu[i].orderCountPass;
                                    revenu = listDataChartRevenu[i].totalRevenu;
                                    revenu_ago = listDataChartRevenu[i].totalRevenuPass;
                                    date_Ago = listDataChartRevenu[i].datePassStr;
                                    break;
                                }
                            }
                            var multistringText = [];
                            if (tooltipItems.datasetIndex == 0) {
                                multistringText.push(date_Ago);
                            } else {
                                multistringText.push(dateStr);
                            }
                            var revenu_exac = 0;
                            if (tooltipItems.datasetIndex == 1) {
                                revenu_exac = revenu;
                            } else {
                                revenu_exac = revenu_ago;
                            }
                            multistringText.push("Doanh thu thuần : " + tooltipItems.yLabel.toLocaleString());
                            multistringText.push("Doanh thu : " + revenu_exac.toLocaleString());
                            var count = 0;
                            //var shipfeeAmount = 0;
                            if (tooltipItems.datasetIndex == 1) {
                                count = order_Count;
                                //shipfeeAmount = ship_Fee;
                            } else {
                                count = order_Count_Ago;
                                //shipfeeAmount = ship_Fee_Ago;
                                tooltipItems.xLabel = date_Ago
                            }
                            multistringText.push("SL : " + count.toLocaleString());
                            //  multistringText.push("Ship Fee: " + shipfeeAmount.toLocaleString());
                            return multistringText;
                        }
                    }
                },
            }
        };
        chartRevenu = new Chart(ctx, ObjectChart);
        if (chartRevenu != undefined)
            chartRevenu.update();
    },

    LoadChartLabelRevenu: function (type) {
        if (chartLabel != null && chartLabel != undefined) {
            chartLabel.destroy();
        }
        var backgroundColor = '#49B391';
        var borderColor = '#49B391';
        if (chartLabel != undefined) chartLabel.destroy();
        var label = "";
        var ctx = document.getElementById("revenuLabelChart").getContext('2d');

        var listRevenu = []
        var listLabel = []
        for (var i = 0; i < listDataChartLabel.length; i++) {
            listLabel.push(listDataChartLabel[i].storeName);
            listRevenu.push(listDataChartLabel[i].totalRevenu);
        }
        var dataset = {
            label: label,
            data: listRevenu,
            backgroundColor: backgroundColor,
            borderColor: borderColor
        };

        barPercentageCustomer = 0.9 * Math.min(6, listLabel.length) / 6;// lay ti le nho
        var lstDataset = [dataset];

        var ObjectChart = {
            type: 'horizontalBar',
            data: {
                labels: listLabel,
                datasets: lstDataset
            },
            options: {
                responsive: true,
                legend: {
                    display: false,
                },
                title: {
                    display: true,
                    text: label
                },
                scales: {
                    yAxes: [{
                        position: 'left',
                        type: "category",
                        barPercentage: 0.5,
                    }],
                    xAxes: [{
                        ticks: {
                            fontColor: 'black',
                            beginAtZero: true,
                            callback: function (value, index, values) {
                                if (value <= 1) {
                                    return 0 + " tr";
                                }
                                if (value <= 100) {
                                    return value;
                                }
                                else {
                                    return value / 1000000 + " tr";
                                }
                            }
                        }
                    }]

                },
                tooltips: {
                    mode: 'label',
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var indice = tooltipItem.index;
                            return data.datasets[0].data[indice].toLocaleString();
                        }
                    }
                },
            }
        };
        chartLabel = new Chart(ctx, ObjectChart);

        if (chartLabel != undefined)
            chartLabel.update();
    },
    LoadChartLabelQuantity: function (type) {
        if (chartLabel != null && chartLabel != undefined) {
            chartLabel.destroy();
        }
        var backgroundColor = '#49B391';
        var borderColor = '#49B391';
        if (chartLabel != undefined) chartLabel.destroy();
        var label = "";
        var ctx = document.getElementById("revenuLabelChart").getContext('2d');
        var listQuantity = []
        var listLabel = []
        for (var i = 0; i < listDataChartLabel.length; i++) {
            listLabel.push(listDataChartLabel[i].storeName);
            listQuantity.push(listDataChartLabel[i].orderCount);
        }
        var dataset = {
            label: label,
            data: listQuantity,
            backgroundColor: backgroundColor,
            borderColor: borderColor
        };

        barPercentageCustomer = 0.9 * Math.min(6, listLabel.length) / 6;// lay ti le nho
        var lstDataset = [dataset];

        var ObjectChart = {
            type: 'horizontalBar',
            data: {
                labels: listLabel,
                datasets: lstDataset
            },
            options: {
                responsive: true,
                legend: {
                    display: false,
                },
                title: {
                    display: true,
                    text: label
                },
                scales: {
                    yAxes: [{
                        position: 'left',
                        type: "category",
                        barPercentage: 0.5,
                    }],
                    xAxes: [{
                        ticks: {
                            fontColor: 'black',
                            beginAtZero: true,
                            callback: function (value, index, values) {
                                if (value <= 1) {
                                    return 0;
                                }
                                return value;
                            }
                        }
                    }]

                },
                tooltips: {
                    mode: 'label',
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var indice = tooltipItem.index;
                            return data.datasets[0].data[indice].toLocaleString();
                        }
                    }
                },
            }
        };
        chartLabel = new Chart(ctx, ObjectChart);

        if (chartLabel != undefined)
            chartLabel.update();
    },
   
}

function dateToString(_date) {
    var dd = _date.getDate();
    var mm = _date.getMonth() + 1;

    var yyyy = _date.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    };
    if (mm < 10) {
        mm = '0' + mm;
    };
    return dd + '/' + mm + '/' + yyyy;
}
function getMinute(datetimeString) {
    if (datetimeString == undefined || datetimeString == '') return null;
    var dateTime = datetimeString.split("T");
    var timeOnly = dateTime[1];
    var hm = timeOnly.split(':');
    return hm[1];
}
function getHour(datetimeString) {
    if (datetimeString == undefined || datetimeString == '') return null;
    var dateTime = datetimeString.split("T");
    var timeOnly = dateTime[1];
    var hm = timeOnly.split(':');
    return hm[0];
}
function stringToDate(dateStr) {
    var newDate = new Date();
    newDate.setDate(dateStr.split('/')[0]);
    newDate.setMonth(dateStr.split('/')[1] - 1);
    newDate.setFullYear(dateStr.split('/')[2]);
    newDate.setHours(0, 0, 0);
    return newDate;
}
function parseDateMinute(datetimeString) {
    if (datetimeString == undefined || datetimeString == '') return null;
    var dateTime = datetimeString.split("T");
    var dateOnly = dateTime[0];
    var dmy = dateOnly.split('-');
    var timeOnly = dateTime[1];
    var hm = timeOnly.split(':');
    return dmy[2] + '-' + dmy[1] + '-' + dmy[0];
}