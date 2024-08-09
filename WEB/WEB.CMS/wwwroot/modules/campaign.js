$(document).ready(function () {
    var _searchData = {
        fromTime: '',
        toTime: '',
        listLabelId: [],
        listCampaignId: [],
        status: -1,
        strLink: '',
        currentPage: 1,
        pageSize: 10
    };
    _menu.Init(_searchData);

    $('#token-input-campaign').tokenInput('/Campaign/GetCampaignSuggestionList', {
        queryParam: "name",
        hintText: "Nhập từ khóa tìm kiếm",
        searchingText: "Đang tìm kiếm...",
        placeholder: 'Nhập từ khóa tìm kiếm',
        searchDelay: 500,
        preventDuplicates: true,
        minChars: 4,
        noResultsText: "Không tìm thấy kết quả",
        tokenLimit: 10,
        onAdd: function (item) {
            _menu.OnChangeCampaignName($(this).val());
        },
        onDelete: function (item) {
            _menu.OnChangeCampaignName($(this).val());
        }
    });
});

$.validator.addMethod("blankSpace", function (value) {
    return value.trim().length > 0;
});

$('body').on('click', '.btn-toggle-cate', function () {
    var seft = $(this);
    if (seft.hasClass("plus")) {
        seft.parent().siblings('ul.lever2').show();
        seft.removeClass('plus').addClass('minus');
    } else {
        seft.parent().siblings('ul.lever2').hide();
        seft.removeClass('minus').addClass('plus');
    }
});

$('body').on('click', '.btn-extension-cate', function () {
    var seft = $(this);
    var cateId = seft.data('id');
    var campId = $('#elm-campaign-dropdown').val();
    var siblingLength = seft.parent().siblings('.content-campaign-groupproduct').length;
    if (siblingLength <= 0) {
        var objparam = {
            campaignId: campId,
            groupProductId: cateId
        };

        _menu.OnGetDetailCampaignGroupProduct(objparam, function (data) {
            var StrHtml = '<div class="flex flex_btn mb10 content-campaign-groupproduct">'
                + '<input class="cg-data-id" type="hidden" value="' + data.id + '">'
                + '<input type="number" class="form-control cg-data-order text-center border mb10" placeholder="Thứ tự" value="' + data.orderBox + '" style="width:60px">'
                + '<select class="form-control cg-data-status border ml10 mb10" style="width:140px">'
                + '<option value="0"' + (data.status == 0 ? " selected" : "") + '>Hoạt động</option>'
                + '<option value="1"' + (data.status == 1 ? " selected" : "") + '>Khóa/Tạm dừng</option>'
                + '</select>'
                + '<button class="btn green btn-save-cgduct"><i class="fa fa-save"></i></button>'
                + '<button class="btn red" onclick="$(this).parent().remove();"><i class="fa fa-minus-circle"></i></button>'
                + '</div>';
            $(StrHtml).insertAfter(seft.parent());
        });
    }
});

$('body').on('click', '.btn-save-cgduct', function () {
    var seft = $(this);
    var id = parseInt(seft.siblings('.cg-data-id').val());
    var order = parseInt(seft.siblings('.cg-data-order').val());
    var status = parseInt(seft.siblings('.cg-data-status').val());
    var objSave = {
        Id: id,
        Status: status,
        OrderBox: order
    };
    if (order < 0) {
        _msgalert.error('Số thứ tự phải lớn hơn hoặc bằng 0');
        return false;
    }

    _menu.OnSaveInfoCampaignGroupProduct(objSave, function (result) {
        if (result.isSuccess) {
            _msgalert.success(result.message);
            seft.parent().remove();
        } else {
            _msgalert.error(result.message);
        }
    });
});

var _menu = {
    Init: function (data) {
        this.SearchParam = data;
        this.Search(data);
    },

    Search: function (input) {
        $('#grid-data').html('<img src="/images/icons/loading.gif" style="width:50px; height:50px;" />');
        $.ajax({
            url: "/Campaign/SearchData",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
            }
        });
    },

    ReLoad: function () {
        this.Search(this.SearchParam);
    },

    OnChangeCampaignName: function (value) {
        var searchobj = this.SearchParam;
        searchobj.listCampaignId = value
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnSearchLink: function (item) {
        var searchobj = this.SearchParam;
        if (item == null || item == undefined) {
            searchobj.strLink = $('#linkproduct').val()
        } else {
            searchobj.strLink = item.value
        }
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeLabel: function (value) {
        var searchobj = this.SearchParam;
        if (searchobj.listLabelId.length == 0) {
            searchobj.listLabelId.push(value);
        } else {
            var flag = false
            for (var i = 0; i < searchobj.listLabelId.length; i++) {
                if (searchobj.listLabelId[i] == value) {
                    flag = true
                    break
                }
                if (flag) {
                    break
                }
            }
            if (!flag) {
                searchobj.listLabelId.push(value);
            } else {
                var idx = searchobj.listLabelId.indexOf(value)
                searchobj.listLabelId.splice(idx, 1);
            }
        }
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeStatus: function (value) {
        var searchobj = this.SearchParam;
        searchobj.status = value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeFromTime: function (date) {
        var searchobj = this.SearchParam;
        searchobj.fromTime = date.value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeToTime: function (date) {
        var searchobj = this.SearchParam;
        searchobj.toTime = date.value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnPaging: function (value) {
        var searchobj = this.SearchParam;
        searchobj.currentPage = value;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnLoadUserData: function (id, callback) {
        $.ajax({
            url: "/role/RoleListUser",
            type: "post",
            data: { Id: id },
            success: function (result) {
                callback(result);
            }
        });
    },

    OnLoadMenuPermission: function (id, callback) {
        $.ajax({
            url: "/role/RolePermission",
            type: "post",
            data: { Id: id },
            success: function (result) {
                callback(result);
            }
        });
    },

    OnUpdateUserRole: function (roleid, userid, type) {
        let obj = {
            roleId: parseInt(roleid),
            userId: userid,
            type: type
        };
        console.log(obj);
        $.ajax({
            url: "/role/UpdateUserRole",
            type: "post",
            data: obj,
            success: function (result) {
            }
        });
    },

    OnShowError: function (id) {
        let title = 'Thông tin lỗi';
        let url = '/campaign/Error';
        let param = { id: id };
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnDelete: function (id) {
        let title = "Xác nhận xóa sản phẩm Ads";
        let description = "Sản phẩm sẽ bị xóa và không được crawl về hệ thống nữa. Bạn có chắc chắn xóa không?";
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/Campaign/delete",
                type: "post",
                data: { Id: id },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        _menu.ReLoad();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        });
    },

    RedirectManageCampaign: function () {
        window.location.href = "/GroupProduct/ProductCategory?isFromCampaign=true";
    },

    // -----------------CampaignManagement---------------------

    LoadCampaignManagement: function () {
        let title = 'Quản lý chiến dịch';
        let url = '/Campaign/CampaignManagement';
        _magnific.OpenSmallPopupWithHeader(title, url, null);
    },

    ReloadCampaignDropDown: function (camId) {
        $.ajax({
            url: "/Campaign/GetJsonCampaignList",
            type: "post",
            success: function (result) {
                if (result != "" && result != null) {
                    var data = JSON.parse(result);
                    var StrOption = '<option value="-1">Lựa chọn chiến dịch</option>';
                    data.map(function (obj) {
                        if (camId != null && camId > 0 && camId == obj.id) {
                            StrOption += '<option value="' + obj.id + '" selected data-start="' + obj.start_date + '" data-end="' + obj.end_date + '" data-link="' + obj.social_link + '" >' + obj.name + '</option>'
                        } else {
                            StrOption += '<option value="' + obj.id + '" data-start="' + obj.start_date + '" data-end="' + obj.end_date + '" data-link="' + obj.social_link + '" >' + obj.name + '</option>'
                        }
                    });
                    $('#elm-campaign-dropdown').empty().html(StrOption);
                }
            }
        });
    },

    OnCreateCampaign: function () {
        $('#panel-create-campaign').removeClass('mfp-hide');
    },

    OnEditCampaign: function () {
        var elmDOM = $('#elm-campaign-dropdown option:selected');
        var id = parseInt(elmDOM.attr('value'));
        var name = elmDOM.text().trim();
        if (id > 0) {
            $('#campaign-id').val(id);
            $('#campaign-name').val(name);
            $('#panel-create-campaign').removeClass('mfp-hide');
        } else {
            _msgalert.error('Bạn phải chọn chiến dịch để chỉnh sửa');
        }
    },

    OnCloseForm: function () {
        $('#campaign-id').val(0);
        $('#campaign-name').val('');
        $('#panel-create-campaign').addClass('mfp-hide');
        $('#campaign-name-error').remove();
    },

    OnSaveCampaign: function () {
        let FromValid = $('#form-create-campaign');
        FromValid.validate({
            rules: {
                CampaignName: {
                    required: true,
                    blankSpace: true,
                },
            },
            messages: {
                CampaignName: {
                    required: "Vui lòng nhập tên chiến dịch",
                    blankSpace: "Phải chứa ít nhất 1 ký tự [A-Z, a-z, 0-9]",
                },
            }
        });

        if (FromValid.valid()) {
            var dataObj = {
                Id: $('#campaign-id').val(),
                Name: $('#campaign-name').val()
            }

            $.ajax({
                url: "/Campaign/SaveCampaign",
                type: "post",
                data: dataObj,
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        _menu.ReloadCampaignDropDown(result.camp_id);
                        _menu.OnCloseForm();
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        }
    },

    OnChangeCampaign: function () {
        var camId = $('#elm-campaign-dropdown').val();
        var elmDOM = $('#elm-campaign-dropdown option:selected');

        if (camId > 0) {
          
            var start_date = elmDOM.data('start');
            var end_date = elmDOM.data('end');
            var social_link = elmDOM.data('link');

            $('#social_link').val(social_link);
            $('#camp_start_date').val(start_date);
            $('#camp_end_date').val(end_date);

            $('.campaign_prop').removeClass('mfp-hide');
        } else {
            $('.campaign_prop').addClass('mfp-hide');
        }

        $.ajax({
            url: "/Campaign/GetCategoryTreeViewOfCampaign",
            type: "post",
            data: { campaignId: camId },
            success: function (result) {
                $('#grid-category-data').html(result);
            }
        });
    },

    OnSaveCampaignGroupProduct: function () {
        var camId = $('#elm-campaign-dropdown').val();
        var _categories = [];
        if ($('.ckb-news-cate:checked').length > 0) {
            $('.ckb-news-cate:checked').each(function () {
                _categories.push($(this).val());
            });
        }

        if (_categories.length <= 0) {
            _msgalert.error('Bạn phải chọn ít nhất 1 chuyên mục để cập nhật cho chiến dịch');
            return false;
        }

        var social_link = $('#social_link').val();
        var start_date = ConvertToJSONDate($('#camp_start_date').val());
        var end_date = ConvertToJSONDate($('#camp_end_date').val());

        $.ajax({
            url: "/Campaign/SaveCampaignGroupProduct",
            type: "post",
            data: { campaignId: camId, startDate: start_date, endDate: end_date, socialLink: social_link, arrGroupProduct: _categories, },
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    _menu.ReloadCampaignDropDown(camId);
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
    },

    OnSaveInfoCampaignGroupProduct: function (input, callback) {
        $.ajax({
            url: "/Campaign/SaveInfoCampaignGroupProduct",
            type: "post",
            data: input,
            success: function (result) {
                callback(result);
            }
        });
    },

    OnGetDetailCampaignGroupProduct: function (input, callback) {
        $.ajax({
            url: "/Campaign/GetDetailCampaignGroupProduct",
            type: "post",
            data: input,
            success: function (result) {
                if (result.isSuccess) {
                    callback(result.data);
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
    }

    // -----------------End CampaignManagement-----------------
};

var _campaign = {
    OnDoCrawl: function (self) {
        let self_element = $(self);
        var _group_id = self_element.data('groupid');
        var _LabelId = parseInt(self_element.data('labelid'));
        var _LinkStoreMenu = self_element.data('link');
        if (_group_id == "" || _group_id == null || _LabelId == "" || _LabelId == null || _LinkStoreMenu == "" || _LinkStoreMenu == null) {
            _msgalert.error("Vui lòng điền đẩy đủ thông tin, sau đó thử lại");
            return;
        }
        var item = {
            groupProductid: _group_id,
            labelid: _LabelId,
            linkdetail: _LinkStoreMenu
        };
        $.ajax({
            url: '/Campaign/DoCrawl',
            type: 'POST',
            data: {
                item: item
            },
            success: function (result) {
                if (result.code == 1) {
                    _msgalert.success(result.message);
                }
            },
            error: function (jqXHR) {
            },
            complete: function (jqXHR, status) {
            }
        });
    }
}