jQuery(document).ready(function () {
    product_general.get_product_history(null); // show product history   
    product_w.wating_product_result(product_code);
})

var product_w = {
    wating_product_result(product_code) {

        var get_product_detail = function () {
            $.ajax({
                url: "/Product/render-product-price.json",
                type: 'POST',
                data: { product_code: product_code, label_id: AMAZON },
                dataType: "json",
                success: function (data) {

                    if (data.status == SUCCESS) {
                        clearInterval(intervalId);
                        window.location.href = data.link_product;
                    }
                }
            })
        }
        var intervalId = setInterval(get_product_detail, 3000);
    }
}