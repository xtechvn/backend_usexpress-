jQuery(document).ready(function () {  
    general.bind_top_pageview('top_page_view_news');
})

var general = {
    bind_top_pageview(location) {
        $.ajax({
            url: "/top-news-pageview.json",
            type: 'POST',            
            dataType: "json",
            success: function (response) {
                if (response.status === SUCCESS) {
                    $("." + location).html(response.data);                    
                }
            }
        })
    }
};