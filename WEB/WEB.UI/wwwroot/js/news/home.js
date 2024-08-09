jQuery(document).ready(function () {
    var category_id = parseInt($("#category_id").val());

    news.bind_article(category_id, 0, 3, 'top_news');
    news.bind_article(category_id, 3, 5, 'news_category_1');
    news.bind_article(category_id, 8, 5, 'news_category_2');    
})

$(document.body).on('click', '.btn_view_more_product', function (e) {
    var skip = $(".total_item_news").find('.us_item_news').length;
    news.render_news_more(401, skip, 10, "news_category_2");
});

var news = {
    bind_article(category_id, skip, take, location) {
        $.ajax({
            url: "/top-news.json",
            type: 'POST',
            data: { category_id: category_id, skip: skip, take: take, location: location },
            dataType: "json",
            success: function (response) {
                if (response.status === SUCCESS) {
                    $("." + location).html(response.data);

                    if (response.total_item === 0) {
                        $(".btn_view_more_product").remove();
                    }
                }
            }
        })
    },
    render_news_more(category_id, skip, take, location) {
        $.ajax({
            url: "/top-news.json",
            type: 'POST',
            data: { category_id: category_id, skip: skip, take: take, location: location },
            dataType: "json",
            success: function (response) {
                if (response.status === SUCCESS) {
                    if (skip > 0) {

                        $(".append-data").append(response.data); // view more
                        skip = $(".total_item_news").find('.us_item_news').length;

                        if (skip >= response.total_item) {
                            $(".btn_view_more_product").remove();
                        }
                    } else {
                        $("." + location).html(response.data); // onload
                    }
                }
            }
        })
    },
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