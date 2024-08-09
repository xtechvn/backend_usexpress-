$(document).ready(function () {
    automatic_purchase.Init();
});

var automatic_purchase = {
    Init: function () {
        let _searchModel = {
            Id: null,
            OrderNo: null,
            ProductCode: null,
            FromDate: null,
            PurchaseStatus: 0,
            ToDate: null,
        };

        let objSearch = {
            searchModel: _searchModel,
            currentPage: 1,
            pageSize: 20
        };

        this.SearchParam = objSearch;
        this.Search(objSearch);
    },
    Loading: function () {
        var html = '<div class="wrap_bg mb20">'
            + '<div class="placeholder mb10" style="height: 60px;"></div>'
            + '<div class="placeholder mb10" style="height: 60px; width: 300px;"></div>'
            + '<div class="box-create-api" style="padding:10px;">'
            + '<div class="placeholder mb10" style="height: 100px;"></div>'
            + '<div class="placeholder mb10" style="height: 100px;"></div>'
            + '<div class="placeholder" style="height: 100px;"></div>'
            + '</div>'
            + '</div>';
        return html;
    },

    Search: function (input) {
        $('#grid-data').html(this.Loading());
        $.ajax({
            url: "/AutomaticPurchase/search",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
            }
        });
    },

    OnPaging: function (value) {
        var objSearch = this.SearchParam;
        objSearch.currentPage = value;
        this.Search(objSearch);
    },

    ReLoad: function () {
        this.Search(this.SearchParam);
    },

    BasicSearch: function () {
        var objSearch = this.SearchParam;
        objSearch.searchModel.OrderNo = $('#ip-orderno').val().trim();
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    AdvanceSearch: function () {
        var objSearch = this.SearchParam;
        objSearch.searchModel.Id = $('#Id').val();
        objSearch.searchModel.OrderNo = $('#OrderNo').val();
        objSearch.searchModel.ProductCode = $('#ProductCode').val();
        objSearch.searchModel.PurchaseStatus = $('#PurchaseStatus').val();
        objSearch.searchModel.FromDate = $('#FromDate').val();
        objSearch.searchModel.ToDate = $('#ToDate').val();

        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnOpenAdvanceSearch: function () {
        $('.fillter-search').slideDown();
    },

    OnCloseAdvanceSearch: function () {
        $('#form-advance-search').trigger("reset");
        this.SearchParam.searchModel.Id = null;
        this.SearchParam.searchModel.OrderNo = null;
        this.SearchParam.searchModel.ProductCode = null;
        this.SearchParam.searchModel.PurchaseStatus = null;
        this.SearchParam.searchModel.FromDate = null;
        this.SearchParam.searchModel.ToDate = null;
        $('.fillter-search').slideUp();
    },

}