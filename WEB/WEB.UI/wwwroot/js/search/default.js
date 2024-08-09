var saveSearch = false;
var delay_search = 500;
var min_length_search = 2;
jQuery(document).ready(function () {

    // Kiểm tra trình duyệt có hỗ trợ localStorage/sessionStorage.
    if (typeof (Storage) !== "undefined") {
        // load keyword research
        obj_search.bin_keyword_history();

        // EVENT
        $("#remove_history_search").click(function (event) {
            obj_search.remove_history_search();
        });

        $("#add_history_search").click(function (event) {
            var keyword_search = $(".keyword_search").val() == null ? "" : $(".keyword_search").val().trim();
            obj_search.add_history_search(keyword_search);
        });

        $(".search_type a").click(function (event) {
            var type_search = $(this).data("groupid");

            $("#txt_search_type").val(type_search);
        });

        //auto search change     
  
        setInterval('obj_search.autocomplete()', delay_search)
        $('.keyword_search').keyup(function () {
            if (!saveSearch) {
                saveSearch = true;
            }
        })
    }

    /// End lịch sử của tôi



})

var obj_search = {
    autocomplete: function () {        
        
        if (saveSearch) {
            
            var input_search = $(".keyword_search").val();
            var search_type =parseInt($("#txt_search_type").val());
            saveSearch = false;
            if (input_search.length > min_length_search) {
                obj_search.search_product(input_search, search_type);
            }
        }
    },
    search_product: function (input_search, search_type) {
        
        //var data_search = JSON.stringify({ 'input_search': input_search, 'search_type': search_type });        
        $(".suggest_search_data").html("");
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: 'POST',
            url: '/product/search-suggest.json?input_search=' + input_search + "&search_type=" + search_type,
            //data: data_search,
            //data: '{input_search:"' + input_search + '",search_type:' + search_type + '"}',
            success: function (data) {   
                
                if (data.result_search) {                 
                    $(".suggest_search_data").html(data.products);     
                    
                } else {                  
                    console.log("search not found !");
                }
            },
            failure: function (response) {                
                console.log(response);
            }
        });
    },
    add_history_search: function (keyword) { // add keyword
        if (keyword.length > 0) {
            var log_history_search = localStorage.getItem(HISTORY_SEARCH);

            if (log_history_search != null) {
                if (("," + log_history_search + ",").indexOf("," + keyword + ",") == -1) {
                    log_history_search = keyword + "," + log_history_search;

                    // sap xep lai
                    obj_search.order_top_history_search(TOTAL_KEYWORD_HISTORY_SEARCH, log_history_search);
                }
            } else {
                log_history_search = keyword;
                localStorage.setItem(HISTORY_SEARCH, log_history_search);
            }
        }
    },
    bin_keyword_history: function () { // parser keyword        
        var log_history_search = localStorage.getItem(HISTORY_SEARCH);
        var log_history_search_new;
        if (log_history_search != null) {

            var arr_keyword = log_history_search.split(',');
            $(".history-search").append("<h3>Lịch sử tìm kiếm</h3><a class='clear'id='remove_history_search'>Xóa tất cả</a>");
            var total_kw = arr_keyword.length;//> TOTAL_KEYWORD_HISTORY_SERCH ? TOTAL_KEYWORD_HISTORY_SERCH : arr_keyword.length;

            for (var i = 0; i <= total_kw - 1; i++) {
                $(".history-search").append("<li><a>" + arr_keyword[i] + "</a></li>");
            }

        }
    },
    remove_history_search: function () {
        localStorage.removeItem(HISTORY_SEARCH);
    },
    order_top_history_search: function (total_keep_kw, log_history_search) {

        var arr_keyword = log_history_search.split(',');
        if (log_history_search != null) {

            var total_remove = arr_keyword.length - TOTAL_KEYWORD_HISTORY_SEARCH;
            if (total_remove > 0) {
                // Xóa đi các keyword cũ
                for (var i = 0; i <= total_remove - 1; i++) {
                    //delete từ dưới lên                    
                    const index = arr_keyword.length - 1;
                    arr_keyword.splice(index, 1);
                }
                // Update local
                log_history_search = arr_keyword.join(",");
            }
        }
        localStorage.setItem(HISTORY_SEARCH, log_history_search);
    },
};


function beginSearch() {
 
    $(".search_product").html('<svg class="icon-us refresh"><use xlink:href="/images/icons/icon.svg#refresh"></use></svg>');
}
function completeSearch(response) {
    $(".search_product").html('<svg class="icon-us"><use xlink:href="/images/icons/icon.svg#search"></use></svg>');    
   
    var url_detail = response.responseJSON.url_redirect;
    window.location.href = url_detail;
}

