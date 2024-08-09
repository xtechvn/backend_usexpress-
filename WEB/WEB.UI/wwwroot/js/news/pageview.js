
jQuery(document).ready(function () {
    
    pv.log_pageview(article_id);
})

var pv = {
    log_pageview: function (article_id) {

        $.ajax({
            url: "/log-pageview.json",
            type: 'POST',
            data: { article_id: article_id },
            dataType: "json",
            success: function (response) {
                if (response.status !== SUCCESS) {
                    console.log(response.msg);
                }
            }
        })
    }
};

