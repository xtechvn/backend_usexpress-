//-- Const
const label_id=1;
const label_name='AMAZON';
const currency_symbol ='$ ';
//--HTML Text:
var  out_stock, on_loading, btn_onloading, button_redirect_toUSExpress,
    primarybutton_readytoclick, OrderButton_readytoclick,
    note_instock, note_outstock;
//-- Variable:
var url_usexpress, asin, current_url = '';
var logo_url = chrome.runtime.getURL('/amz/images/logo.png');
var ic_cart_url = chrome.runtime.getURL('/amz/images/ic-cart.png');
var xpath = {};
//-- On Start
$(document).ready(function () {
    //---- Init HTML Text from .json config file:
    $.get(chrome.runtime.getURL('/assets/ext_data.json'), function (data_local) {
        url_usexpress = data_local.url_usexpress;
        out_stock = data_local.HTML.out_stock;
        on_loading = data_local.HTML.on_loading;
        btn_onloading = data_local.HTML.btn_onloading;
        button_redirect_toUSExpress = data_local.HTML.button_redirect_toUSExpress;
        primarybutton_readytoclick = data_local.HTML.primarybutton_readytoclick;
        OrderButton_readytoclick = data_local.HTML.OrderButton_readytoclick;
        note_instock = data_local.HTML.note_instock;
        note_outstock = data_local.HTML.note_outstock;
    });
    usexpress_amz.GetASINFromCurrentPage(function (product_code) {
        asin = product_code;
        if (product_code != null && product_code != undefined && product_code.replace(/^\s+|\s+$/gm,'') != '') {
            //-- Load and inject HTML:
            $.get(chrome.runtime.getURL('/amz/index.html'), function (data) {
                var div = document.createElement("div");
                div.className = 'extension_usexpress';
                var html = data.replace("{logo_png}", logo_url).replace("{iccart_png}", ic_cart_url).replace("{primarybutton_readytoclick}", primarybutton_readytoclick).replaceAll("{label_name}", label_name);
                div.innerHTML = html;
                document.body.appendChild(div);
                $('.box_pin').css('background', 'rgba(35, 104, 82, .9) url(' + chrome.runtime.getURL('/amz/images/bg-map.png') + ') no-repeat');
                $('#btn_baogia').text(btn_onloading);
                $("#btn_baogia").removeClass("bao-gia");
                usexpress_amz.DetectPriceRange();

            });
        }
    });
    
});
//-- Dynamic bind:
$('body').on('click', '.bao-gia', function (ev) {
    $('.bao-gia').text(btn_onloading);
    $('#product_amount').text(on_loading);
    $('#product_fee_first_pound').html(on_loading);
    $('#product_fee_after_pound').html(on_loading);
    $('#luxury_fee').html(on_loading);
    $('#product_discount_fee').html(on_loading);
    $('#product_total_shipping_fee').html(on_loading);
    $('#product_total_amount').html(on_loading);
    $('#product_fee_us_shipping_fee').html(on_loading);
    $('#product_total_amount_vnd').html(on_loading);
    $('#btn_baogia').addClass("disable-click");
    $("#more_infomation").html(note_instock);
    $(".hide-when-out-stock").removeClass('hidden_div');
    $('.modal-popup').addClass("show");
    $('body').toggleClass("open-popup");
    var xpath_obj = {
        function_name: "get_xpath",
        label_name: label_name
    };
    chrome.runtime.sendMessage(xpath_obj, function (response) {
        if(response.response!='Success'){
            $('#product_amount').text('');
            $('#product_total_amount_vnd').html('');
            //--Update Lightbox:
            $('.bao-gia').text(primarybutton_readytoclick);
            $(".hide-when-out-stock").addClass('hidden_div');
            $("#usexpress_order_confirm").html("<img src=" + ic_cart_url + ">  " + OrderButton_readytoclick);
            $("#more_infomation").html(note_outstock);
            return;
        }
        xpath = response.data;        
        //-- Get Price:
        var price = 0;
        $.each(xpath.price, function (index, value) {
            xPathResult = document.evaluate(value, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
            if (xPathResult.singleNodeValue != null) {
                var text = xPathResult.singleNodeValue.innerHTML;
                price = parseFloat(text.replace("$", "").replace(",", ""));
                if(price>0) return false;
            }
        });
        //-- Item_weight
        var item_weight = '1 pounds';
        $.each(xpath.item_weight, function (index, value) {
           var xPathResult_list = document.evaluate(value, document, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE  , null);
           var node;
           while ((node = xPathResult_list.iterateNext()) != null) {
             //  console.log(node.textContent);
                if(node.textContent.includes('Item Weight')){
                    item_weight=node.textContent.replace("Item Weight","").replace("\n","").replace(":","").replace(/^\s+|\s+$/gm,'');
                    break;
                }
                else if(node.textContent.includes('Package Dimensions')){
                    item_weight=node.textContent.replace("\n","").replace(":","").split(';')[1];
                    if(item_weight!=undefined){
                        break;
                    }
                    else{
                        item_weight = '1 pounds';
                        continue;
                    }
                }
           }
        });
        var weight = item_weight.replace(/^\s+|\s+$/gm,'').replace(/\r?\n|\r/gm,'').split(" ");
        //-- Product_name:
        var product_name = '';
        if (xpath.product_name.length > 0) {
            $.each(xpath.product_name, function (index, value) {
                xPathResult = document.evaluate(value, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
                if (xPathResult.singleNodeValue != null) {
                    product_name = xPathResult.singleNodeValue.innerHTML;
                    return false;
                }
            });
        }
        //-- Shipping fee:
        var shipping_fee = 0;
        xPathResult = document.evaluate(xpath.shipping_fee, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
        if (xPathResult.singleNodeValue != null) {
            shipping_fee = parseFloat(xPathResult.singleNodeValue.innerHTML.replace("$", "").replace(",", ""));
        }
        var obj = {
            function_name: "get_fee_new",
            price: price,
            item_weight:parseFloat(weight[0].replace(/[^0-9.+-]/g, '')),
            unit: weight[1],
            product_name: product_name,
            shipping_fee: shipping_fee,
            url: window.location.href,
            label_id : label_id
        };
        chrome.runtime.sendMessage(obj, function (response) {
            if(response.response != "Success"){
                $('#product_total_amount_vnd').html('');
                //--Update Lightbox:
                $('.bao-gia').text(primarybutton_readytoclick);
                $(".hide-when-out-stock").addClass('hidden_div');
                $("#usexpress_order_confirm").html("<img src=" + ic_cart_url + ">  " + OrderButton_readytoclick);
                $("#more_infomation").html(note_outstock);
                return;
            }
            var rgx = /^[0-9]*\.?[0-9]*$/;
            var price_num = '' + response.PRICE;
            price_num = price_num.match(rgx);
            if (parseFloat(price_num)) {
                $('#product_amount').html(currency_symbol + usexpress_amz.numberWithCommas(Number(price_num).toFixed(2)));
            }
            else {
                $('#product_amount').html(out_stock);
            }
            if (parseFloat(shipping_fee)) {
                $('#product_fee_us_shipping_fee').html(currency_symbol + usexpress_jomashop.numberWithCommas(Number(shipping_fee).toFixed(0)));
            }
            else {
                $('#product_fee_us_shipping_fee').html('');
            }
            //-----
            if (response.TOTAL_SHIPPING_FEE > 0) {
                $('#product_total_shipping_fee').html(currency_symbol + usexpress_amz.numberWithCommas(response.TOTAL_SHIPPING_FEE));
            }
            else {
                $('#product_total_shipping_fee').html('');
            }
            //-----
            if (response.PRICE_LAST > 0) {
                $('#product_total_amount').html(currency_symbol + usexpress_amz.numberWithCommas(response.PRICE_LAST));
            }
            else {
                $('#product_total_amount').html('');
            }
            //-----
            if (response.FIRST_POUND_FEE > 0) {
                $('#product_fee_first_pound').html(currency_symbol + usexpress_amz.numberWithCommas(response.FIRST_POUND_FEE));
            }
            else {
                $('#product_fee_first_pound').html('');
            }
            //-----
            if (response.NEXT_POUND_FEE > 0) {
                $('#product_fee_after_pound').html(currency_symbol + usexpress_amz.numberWithCommas(response.NEXT_POUND_FEE));
            }
            else {
                $('#product_fee_after_pound').html('');
            }
            //-----
            if (response.LUXURY_FEE > 0) {
                $('#luxury_fee').html(currency_symbol + usexpress_amz.numberWithCommas(response.LUXURY_FEE));
            }
            else {
                $('#luxury_fee').html('');
            }
            //-----
            if (response.PRICE_LAST_VND > 0) {
                $('#product_total_amount_vnd').html(usexpress_amz.numberWithCommas(response.PRICE_LAST_VND) + " đ");
                $('.bao-gia').text(primarybutton_readytoclick);
                $("#usexpress_order_confirm").html("<img src=" + ic_cart_url + ">  " + OrderButton_readytoclick);
                $("#more_infomation").html(note_instock);
            }
            else {
                $('#product_total_amount_vnd').html('');
                //--Update Lightbox:
                $('.bao-gia').text(primarybutton_readytoclick);
                $(".hide-when-out-stock").addClass('hidden_div');
                $("#usexpress_order_confirm").html("<img src=" + ic_cart_url + ">  " + OrderButton_readytoclick);
                $("#more_infomation").html(note_outstock);
            }
            $("#usexpress_order_confirm").removeClass("bg-gray");
            url_usexpress = response.url_usexpress_detail;
            $("#btn_baogia").removeClass("disable-click");
            $("#btn_baogia").addClass("close-popup");
            $("#btn_baogia").removeClass("bao-gia");

        });
    });

});

$('body').on('click', '.close-popup', function (ev) {
    $('.modal-popup').removeClass("show");
    $('body').removeClass("open-popup");
    $("#btn_baogia").removeClass("close-popup");
    $("#btn_baogia").addClass("bao-gia");
});

//-- Order Button confirm:
$('body').on('click', '.usexpress_order_confirm', function () {
    var win = window.open(url_usexpress, 'US Express');
    if (win) {
        //Browser has allowed it to be opened
        win.focus();
    } else {
        //Browser has blocked it
        alert('Please allow popups for extension (Website)');
    }
});


var usexpress_amz = {
    // Get ASIN From URL
    GetASINFromCurrentPage: function (callback) {
        chrome.runtime.sendMessage({
            function_name: "get_product_code",
            label_id: label_id,
            url: window.location.href
        }, function (response) {
            callback(response.product_code);
        });
    },
    DetectPriceRange: function () {
        var check_range = setInterval(function () {
            var xPathResult = document.evaluate('//*[@class="a-price-range"]', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
            if (xPathResult.singleNodeValue != null) {
                $("#btn_baogia").removeClass("bao-gia");
                $("#btn_baogia").html("Vui lòng chọn kích cỡ / màu sắc");
                $("#btn_baogia").addClass("close-popup");
                $('#btn_baogia').addClass("disable-click");
            }
            else {
                usexpress_amz.GetASINFromCurrentPage(function (product_code) {
                    asin = product_code;
                    $("#btn_baogia").removeClass("close-popup");
                    $("#btn_baogia").addClass("bao-gia");
                    $("#btn_baogia").text(primarybutton_readytoclick);
                    $("#btn_baogia").removeClass("disable-click");
                    clearInterval(check_range);
                });
            }
        }, 2000);
    },
    numberWithCommas: function (x) {
        var text = Number(x).toFixed(2);
        return text.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    },

}
