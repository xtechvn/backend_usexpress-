
var savePost = false; // auto save quantity
var updateCart = false;
jQuery(document).ready(function () {
    setInterval('cart.autoSave()', 750);
    setInterval('cart.async_cart()', 1200);
    $('.txtQuantity').keyup(function () {
        if (!savePost) {
            savePost = true;
        }
    })
    $('.chk_cart_product').click(function () {
        
        var label_id = $(this).data("labelid");
        $(".store_" + label_id).prop("checked", $(this).is(':checked') ? 1 : 0);
        if (!updateCart) {
            updateCart = true;
        }
    });    
    
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
        //voucher.calulateVoucherSale();
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
        $(".total_discount_amount").html("0 <em>đ</em>");
    }
});



$('.txtQuantity').change(function () {
    var key_id = $(this).data('keyid');
    var quantity = parseInt(obj_general.replace_all($(this).val(), ",", ""));
    if (quantity <= 0) $(this).val(1);
    cart.calulator_total_cart();
    $(this).addClass("active_qty_change");
    $(".amount_last_vnd_" + key_id).html('<svg class="icon-us refresh"><use xlink:href="/images/icons/icon.svg#refresh"></use></svg>');
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

        cart.calulator_total_cart();
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
    cart_summery.remove_cart_info();
});

var cart = {
    async_cart: function () {
        
        if (updateCart) {
            updateCart = false;
            var chk_name = $('.chk_cart_product').attr("name");
            var chk_label = $('.chk_cart_product').data("chklabel");

            this.calculator_total_amount(chk_name, chk_label);
        }
    },
    calulator_total_cart: function () {
        var total_carts = 0;
        $(".cart_list").find(".txtQuantity").each(function () {
            total_carts += parseInt($(this).val());
        });
        $(".total_carts").html(total_carts);
    },
    bindCartSummery: function (total_amount_cart, total_discount_amount, total_amount_last) {
        $(".total_amount_vnd").html(total_amount_cart + " <em>đ</em>");
        $(".total_discount_amount").html(voucher.getTotalVoucher().toLocaleString() + " <em>đ</em>");
        $(".total_amount_last_vnd").html(total_amount_last + " <em>đ</em>");
    },
    calculator_total_amount: function (chk_name_item_cart_child, chk_label) {
        
        $(".summery_total_price").html('<svg class="icon-us refresh"><use xlink:href="/images/icons/icon.svg#refresh"></use></svg>');
        var total_amount_product_vnd = 0;
        var item_check = $("input[name='" + chk_name_item_cart_child + "']:checked").length;

        if (item_check == 0) {
            $("." + chk_label).prop("checked", false);
        }

        if (voucher.getTotalVoucher() > 0) { // has voucher
            var total_prod_choic = cart_summery.countTotalProd();
            if (total_prod_choic > 0) {
                // calculator voucher sale
                voucher.update_product_by_rule_voucher();
                var voucher_save = voucher.getVoucherSave();                
                voucher.calulateVoucherSale(voucher_save,true);
            } else {
                $(".summery_total_price").html("0 <em>đ</em>");
            }
        } else { // no voucher
            // tong tien sau giam
            var total_discount_amount_vnd = voucher.getTotalVoucher();

            for (var i = 0; i <= item_check - 1; i++) {

                var item_cart = $("input[name='" + chk_name_item_cart_child + "']:checked")[i];
                total_amount_product_vnd += parseFloat(obj_general.replace_all(item_cart.getAttribute("data-amount"), ",", ""));
            }

            $(".total_amount_vnd").html(total_amount_product_vnd.toLocaleString() + " <em>đ</em>");
            if (total_amount_product_vnd > 0) {
                $(".total_discount_amount").html(total_discount_amount_vnd.toLocaleString() + " <em>đ</em>");
                $(".total_amount_last_vnd").html((total_amount_product_vnd - total_discount_amount_vnd).toLocaleString() + " <em>đ</em>");
            } else {
                $(".summery_total_price").html("0 <em>đ</em>");
            }
        }
    },
    uncheck_all: function () {
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
            data: { key_id: key_id },
            success: function (data) {

                if (data.status == SUCCESS) {

                    $(".row_" + key_id).remove();
                    // Recalculator cart
                    cart.calculator_total_amount(chk_name_child, chklabel);
                    //Update Total product
                    var total_product_cart = $('.chk_cart_product').length;
                    if (total_product_cart == 0) {
                        localStorage.removeItem(VOUCHER_KEY);                        
                    } else {
                        //$('.total_product_cart').html("(" + total_product_cart + " sản phẩm)");                        
                    }
                    window.location.reload();

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