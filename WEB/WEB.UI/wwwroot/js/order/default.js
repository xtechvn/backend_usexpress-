$(document).ready(function () {
    $(".quan-ly-don-hang").addClass("active");
    order.ShowButtonMoreOrder();
})

$(document).on('keypress', function (e) {
    if (e.which == 13) {
        var input_search = $('.txt_input_search').val();
        if (input_search.length > 2) {
            $(".btn_search_order").click();
        }
    }
});

$(".tab_order_status").click(function (e) {
    var order_status = parseInt($(this).data('index'));
    $('.txt_input_search').val("");

    order.renderOrderList("tab_active", order_status, 0);
});

$(".btn_view_more_order").click(function (e) {
    var tab_index = $('.tab_order').find('.active').index();
    var order_status = $("#tab_" + tab_index).data('index');
    var total_row_current = $('.tb_order_item .tr_item_order').length;
    order.renderOrderList("view_more", order_status, total_row_current);
});

$(".btn_search_order").click(function (e) {
    var tab_index = $('.tab_order').find('.active').index();
    var order_status = $("#tab_" + tab_index).data('index');

    order.renderOrderList("search_order", order_status, 0);
});

var order = {
    renderOrderList: function (view_type, order_status, total_row_current) {
      
        var input_search = $('.txt_input_search').val() == undefined ? "" : $('.txt_input_search').val().trim();
        if (order_status != -1) {
            $(".control_search_order").css("display", "none");
        } else {
            $(".control_search_order").removeAttr("style");
        }

        if (view_type == "tab_active") {
            $('.txt_input_search').val("");
        }

        $.ajax({
            url: '/order/get-order-by-status',
            type: 'POST',
            dataType: "json",
            data: { order_status: order_status, input_search: input_search, total_row_current: total_row_current, view_type: view_type },
            success: function (response) {
           
                if (response.status == SUCCESS) {
                    $(".order_control").removeAttr("style");
                    if (response.view_type == "tab_active" || response.view_type == "search_order") {
                        $(".order_manager").html(response.order_list);

                        if (response.order_status == -1) {
                            $(".tab_0").html("(" + response.total_order + ")");
                            if (response.total_order <= response.page_size) {
                                $(".btn_view_more_order").css("display", "none");
                                return;
                            }
                        }
                    } else {

                        var order_list_more = response.order_view_more;
                        $(".tb_order_item").append(order_list_more);                       

                    }
                } else if (response.status == EMPTY) {
                    $(".order_manager").html("<table class='table'><thead><tr><td colspan='10' style='text-align:center'>Chưa có đơn hàng</td></tr></thead><tbody></tbody></table>");                    
                }
                order.ShowButtonMoreOrder();
            }
        })
    },
    ShowButtonMoreOrder: function () {
        var tab_index = $('.tab_order').find('.active').index();
        var total_order = $("#tab_" + tab_index).data('count');
        var total_row_order = total_order == 0 ? 0 : $('.tb_order_item .tr_item_order').length;
        if (total_order > total_row_order) {
            $(".btn_view_more_order").removeAttr("style");
        } else {
            $(".btn_view_more_order").css("display", "none");
        }

    }
}

