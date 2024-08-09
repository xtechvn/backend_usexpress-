
jQuery(document).ready(function () {
    $('body').addClass('page_article');  
    menu.bind();
})
var menu = {
    bind:function() {
        $.ajax({
            url: "/menu-news.json",
            type: 'POST',
            dataType: "json",
            success: function (response) {
                if (response.status === SUCCESS) {
                    $(".menu_news").html(response.data);

                    var category_id = parseInt($("#category_id").val());
                    $('.category_' + category_id).addClass('active');
                }
            }
        })    
    }
};