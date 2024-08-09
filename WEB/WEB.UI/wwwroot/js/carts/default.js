
var savePost = false; // auto save quantity
jQuery(document).ready(function () {
 
    setInterval('cart.autoSave()', 1000)
    $('.txtQuantity').keyup(function () {
        if (!savePost) {
            savePost = true;
        }
    })

   
})

var label_choice = -1;
$('.chk_cart_label').click(function () {
    //    cart.unCheckAll();
    //  $(".table").removeClass("checked");
    var label_id = $(this).data("labelid");
    var class_item = $(this).data("chkitem");
    var is_check = $(this).is(':checked');
    $('.' + class_item).prop("checked", is_check); // check, uncheck cha se active con ben duoi

    if (is_check) {
        var chk_name = $(this).data("chknamechild");
        //  $("#tbl_cart_" + label_id).addClass("checked");
        cart.calculator_total_amount(chk_name);


        if (label_choice == -1) {
            label_choice = label_id;
        } else {
            if (label_id != label_choice) {
                $('.chk_label_' + label_choice).prop("checked", false);
                label_choice = label_id;
                //off label cu di                
            }
        }
    } else {
        // $("#tbl_cart_" + label_id).removeClass("checked");
        $(".total_amount_vnd").html("0 <em>đ</em>");
        $(".total_amount_last_vnd").html("0 <em>đ</em>");
    }
});


$('.chk_cart_product').click(function () {
    var chk_name = $(this).attr("name");
    var chk_label = $(this).data("chklabel");
    cart.calculator_total_amount(chk_name, chk_label);
});

$('.txtQuantity').change(function () {
    var key_id = $(this).data('keyid');
    var chklabel = $(this).data('chklabel'); // xác định phần tử nào được check
    var quantity = parseInt(obj_general.replace_all($(this).val(), ",", ""));
    if (quantity <= 0) $(this).val(1);
    //cart.save_cart(key_id, quantity, chklabel);
    $(this).addClass("active_qty_change");
    $(".amount_last_vnd_"+ key_id).html('<svg class="icon-us refresh"><use xlink:href="/images/icons/icon.svg#refresh"></use></svg>');
    if (!savePost) {
        savePost = true;
    }
});


$('.up_down_quantity').click(function () {
    var key_id = $(this).data('keyid');
    var type = $(this).data('type');
    var chklabel = $(this).data('chklabel'); // xác định phần tử nào được check
    var clsquantity = $(this).data("clsquantity");
    if (key_id != undefined) {
        var quantity = parseInt(obj_general.replace_all($("." + clsquantity).val(), ",", ""));
        if (type == "down") {
            quantity -= 1;
        } else {
            quantity += 1;
        }

        if (quantity > 10000) {
            alert("Số lượng không được vượt quá 10000");
            return;
        }

        if (quantity <= 0) {
            $("." + clsquantity).val(1);
        } else {
            $("." + clsquantity).val(quantity);
        }
        $(".amount_last_vnd_" + key_id).html('<svg class="icon-us refresh"><use xlink:href="/images/icons/icon.svg#refresh"></use></svg>');
        // cart.save_cart(key_id, quantity, chklabel);
        $("." + clsquantity).addClass("active_qty_change");
        if (!savePost) {
            savePost = true;
        }
    }
});

$('.remove_cart').click(function () {
   
    var key_id = $(this).data('keyid');
    var chklabel = $(this).data("chklabel");
    var chk_name_child = $(this).data("chknamechild"); 
    cart.remove_cart(key_id, chklabel, chk_name_child);
});



var cart = {
    add_to_cart: function (product_code, seller_id, label_id, is_fast_buy) {
        if (product_code == undefined) return;
        if (product_code == "") return;
        seller_id = seller_id == null ? "" : seller_id;

        $.ajax({
            url: "/Carts/add-to-cart",
            type: 'POST',
            data: { product_code: product_code, seller_id: seller_id, label_id: label_id },
            dataType: "json",
            //headers: {
            //    RequestVerificationToken:
            //        $('input:hidden[name="__RequestVerificationToken"]').val()
            //},
            success: function (response) {
                // dong popup

                setTimeout(function () {
                    $('.add-product-success').removeClass('open');
                }, 1400);

                $(".add_to_cart").removeClass('disable-click');
                //$(".add_to_cart").removeClass("placeholder");
                if (response.status == SUCCESS) {
                    var total_current_cart = parseInt($(".total_carts").html()) + 1;
                    $(".total_carts").html(total_current_cart);

                    // redirect To Carts
                    if (is_fast_buy) window.location.href = "/Carts/view.html";

                } else if (response.status == EMPTY) {
                    if (response.is_refresh) {
                        alert("Thông tin sản phẩm đã được thay đổi. Xin vui lòng chờ trong giây lát.");
                        location.reload();
                    } else {
                        alert("Sản phẩm đã hết hàng. Xin vui lòng liên hệ với bộ phận CSKH để được hỗ trợ");
                        console.log(response.msg);
                    }

                }
            }
        })
    },
    syncData: function () {
        //read data old
        var cart_history = localStorage.getItem(CART_HISTORY_KEY);
        // compare list pro_id
        // Check product --> push queue --> sync ES        
        // Check expired price cart-> show alert --> push queue update price


    },
    calculator_total_amount: function (chk_name_item_cart_child, chk_label) {
        var total_amount_product_vnd = 0;
        var total_discount_amount_vnd = 0;
        var total_amount_last_vnd = 0;
        var item_check = $("input[name='" + chk_name_item_cart_child + "']:checked").length;

        if (item_check == 0) {
            $("." + chk_label).prop("checked", false);
        }
        for (var i = 0; i <= item_check - 1; i++) {

            var item_cart = $("input[name='" + chk_name_item_cart_child + "']:checked")[i];
            total_amount_product_vnd += parseFloat(obj_general.replace_all(item_cart.getAttribute("data-amount"), ",", ""));
        }
        $(".total_amount_vnd").html(total_amount_product_vnd.toLocaleString() + " <em>đ</em>");

        total_discount_amount_vnd = parseFloat($(".total_discount_amount").html());

        // tong tien sau ck
        $(".total_amount_last_vnd").html((total_amount_product_vnd - total_discount_amount_vnd).toLocaleString() + " <em>đ</em>");
    },
    unCheckAll: function () {
        $(".chitiet_giohang").find("input[type='checkbox']:checked").prop("checked", false);
    },
    autoSave: function () {
        if (savePost) {
            savePost = false;
            cart.save_cart();
        }
    },
    save_cart: function () { //luu don hang                       
        
        var qty_change = $(".chitiet_giohang").find(".active_qty_change").eq(0);        
        var key_id = qty_change.data("keyid");
        var quantity = qty_change.val();
        var chklabel = qty_change.data("chklabel");
        var chk_name_child = qty_change.data("chknamechild");

        $(".quantity_key_" + key_id).removeClass('active_qty_change');

        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/Carts/update-to-cart.json',
            data: { key_id: key_id, quantity: parseInt(quantity) },
            success: function (data) {
                //$(".tang_sl").html('+');
                //$('#ajax_loading').css('display', 'none');

                if (data.status == SUCCESS) {
                    $(".chk_product_" + key_id).attr("data-amount", data.amount_last_vnd);
                    $(".amount_last_vnd_" + key_id).html(data.amount_last_vnd.toLocaleString() + " <em>đ</em>");

                    // Recalculator cart
                    cart.calculator_total_amount(chk_name_child, chklabel);

                } else {
                    console.log(data.msg);
                }
            },
            failure: function (response) {
                //$('#ajax_loading').css('display', 'none');
                console.log(response);
            }
        });
    },
    remove_cart: function (key_id, chklabel, chk_name_child) {           
        $.ajax({
            dataType: 'json',
            type: 'POST',
            url: '/Carts/delete-item-cart.json',
            data: { key_id: key_id},
            success: function (data) {                
                //cart_summery.remove_cart_info();
               //debugger;
                if (data.status == SUCCESS) {
                    $(".row_" + key_id).remove();
                    // Recalculator cart
                    cart.calculator_total_amount(chk_name_child, chklabel);
                    
                } else {
                    console.log(data.msg);
                }
            },
            failure: function (response) {                
                console.log(response);
            }
        });
    }
}; 