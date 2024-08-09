
jQuery(document).ready(function () {
  
    news.get_product_history();
})
//-- Dynamic bind event:
$('body').on('click', '.usex_share_link', function (event) {
    var url =  window.location.href;
    //var url = 'https://usexpress.vn/';
    var platform = $(this).attr("data-platform");
    if (url == undefined || url.trim() == "" || platform == undefined || platform.trim() == "") {
        return;
    }
    switch (platform) {
        case "facebook": {
            window.open('https://www.facebook.com/sharer/sharer.php?u=' + url, 'targetWindow', 'toolbar=no,location=0,status=no,menubar=no,scrollbars=yes,resizable=yes,width=600,height=250');
        } break;
        case "copylink": {
            /* Copy the text inside the text field */
            navigator.clipboard.writeText(url);
            $('#copied_clipboard_notifi').css('display', '');
            setTimeout(function () {
                $('#copied_clipboard_notifi').css('display', 'none');
            }, 3000);
        } break;
    }

});
var news = {  
    get_product_history: function () {

        var j_list_hist = localStorage.getItem(PRODUCT_HISTORY);
        if (j_list_hist !== null) {

            var prod_history = JSON.parse(j_list_hist);
            var list_result = prod_history;
            if (list_result.length >= LIMIT_PRODUCT_HIST) {
                // append view

                var list_result_limit = list_result.slice(0, LIMIT_PRODUCT_HIST)
                $.ajax({
                    url: "/Product/render-product-history.json",
                    type: 'POST',
                    data: { j_data: JSON.stringify(list_result_limit) },
                    dataType: "json",
                    success: function (response) {
                        if (response.status === SUCCESS) {
                            $(".product-history").html(response.data);
                        } else {
                            console.log(response.msg);
                        }
                    }
                })
            } else {
                $(".product-history").remove();
            }
        } else {
            $(".product-history").remove();
        }
    }
};

