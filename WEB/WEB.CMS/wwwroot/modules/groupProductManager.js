var listPosition = [];
var postionId = 0;

$(document).ready(function () {
    _groupProduct.Init();
});

var _changeInterval = null;
$('#ip-kup-search-group-product').keyup(function (e) {
    if (e.which === 13) {
        _groupProduct.OnSearchByName(e.target.value);
    } else {
        clearInterval(_changeInterval);
        _changeInterval = setInterval(function () {
            _groupProduct.OnSearchByName(e.target.value);
            clearInterval(_changeInterval);
        }, 500);
    }
});

$('#grid-data').on('click', '.btn-expand-child', function () {
    let seft = $(this);
    if (seft.siblings('ul').is(':hidden')) {
        seft.addClass('active');
        seft.siblings('ul').show();
    } else {
        seft.removeClass('active');
        seft.siblings('ul').hide();
    }
});

$('#grid-data').on('click', '.btn-add-group-product', function () {
    let seft = $(this);
    let id = seft.data('id');
    let title = "Thêm mới cấp con cho mục <b style='color: #1e5846'>" + seft.parent().siblings('a').text() + "</b>";
    _groupProduct.OpenFormCreate(id, title);
});

$('#btn-add-parent-category').on('click', function () {
    let title = "Thêm mới danh mục cấp cha";
    _groupProduct.OpenFormCreate(-1, title);
});

$('#grid-data').on('click', '.btn-edit-group-product', function () {
    let seft = $(this);
    let id = seft.data('id');
    let title = "Thông tin chuyên mục <b style='color: #1e5846'>" + seft.parent().siblings('a').text() + "</b>";
    _groupProduct.OpenFormEdit(id, title);
});

$('#grid-data').on('click', '.ckb-auto-crawl', function () {
    let seft = $(this);
    let id = seft.data('id');
    let type = seft.is(":checked") ? 1 : 0;
    if (type == 0) {
        seft.parent().siblings(".group__aff_checkbox").find('.ckb-aff-category').prop('checked', false);
    } else {
        seft.parent().siblings(".group__aff_checkbox").find('.ckb-aff-category').prop('disabled', false);
    }
    _groupProduct.OnSetupAutoCrawler(id, type);
});

$('#grid-data').on('click', '.ckb-aff-category', function () {
    let seft = $(this);
    let affid = seft.val();
    let type = seft.is(":checked") ? 1 : 0;
    var cateid = seft.closest('.group__aff_checkbox').siblings('.item-category-name').data('id');
    _groupProduct.OnSetupAffCategory(cateid, affid, type);
});


var _groupProduct = {
    Init: function () {
        let searchData = {
            Name: "",
            Status: -1
        };
        this.AFFList = [];
        this.SearchParam = searchData;
        this.Search(searchData);
        this.OnLoadTotalCrawl();
        this.OnLoadAffiliateType();
    },

    Search: function (input) {
        $.ajax({
            url: "/groupproduct/search",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
            }
        });
    },

    OnSearchByName: function (value) {
        var searchobj = this.SearchParam;
        searchobj.Name = value.trim();
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnSearchByStatus: function (value) {
        var searchobj = this.SearchParam;
        searchobj.Status = value;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    ReLoad: function () {
        this.Search(this.SearchParam);
    },

    Expand: function () {
        $('.expand').show();
        $('.btn-expand-child').addClass('active');
    },

    Collapse: function () {
        $('.expand').hide();
        $('.btn-expand-child').removeClass('active');
    },

    OpenFormCreate: function (id, title) {
        let url = '/GroupProduct/AddOrUpdate';
        let param = { id: id, type: 0 };
        _magnific.OpenSmallPopupWithHeader(title, url, param);
    },

    OpenFormEdit: function (id, title) {
        let url = '/GroupProduct/AddOrUpdate';
        let param = { id: id, type: 1 };
        _magnific.OpenSmallPopupWithHeader(title, url, param);
    },

    ConvertBase64toFile: function (dataurl) {
        var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        return new File([u8arr], 'cropedimage.png', { type: mime });
    },

    ReLoadElement: function (modelId, modelStatus, modelName, modelParentId, isHasLink) {
        var SeftDOM = $('#grid-data .item-category-' + modelId);
        var disabledText = '<span class="text-inactive">(Ngừng hoạt động)</span>';
        var checkboxText = '<label class="switch"><input type="checkbox" class="ckb-auto-crawl" data-id="' + modelId + '"><span class="slider round"></span></label>';

        var affListHtml = _groupProduct.AFFList.map(function (item) {
            return '<label class="check-list">'
                + '<input class="ckb-aff-category" type="checkbox" disabled value="' + item.id + '">'
                + '<span class="checkmark"></span>' + item.name
                + '</label>';
        }).join('');

        var affCheckBox = '<div class="group__aff_checkbox">' + affListHtml + '</div>';

        var elementDOM = '<li class="item-category-' + modelId + '">'
            + '<div class="dd-handle active">'
            + '<a class="item-category-name" data-id="' + modelId + '">' + modelName + '</a>'
            + '<div class="control">'
            + '<a class="btn-add-group-product" data-id="' + modelId + '"><img src="/images/icons/sql.png"></a>'
            + '<a class="btn-edit-group-product" data-id="' + modelId + '"><img src="/images/icons/edit.png"></a>'
            + '</div>'
            + (isHasLink ? checkboxText : "")
            + (isHasLink ? affCheckBox : "")
            + (modelStatus == 1 ? disabledText : "")
            + '</div>'
            + '</li>';

        var ParentDomHandle = SeftDOM.find('.item-category-name[data-id="' + modelId + '"]').parent();

        if (modelParentId > 0) {
            var ParentDOM = $('#grid-data .item-category-' + modelParentId);
            var IsParentHasChild = ParentDOM.find('.btn-expand-child').length > 0 ? true : false;

            if (!IsParentHasChild) {
                ParentDOM.prepend('<button class="colspan btn-expand-child active" data-action="collapse" type="button"><i class="fa fa-plus"></i></button>');
                ParentDOM.append('<ul class="expand lever2" style="display: block;"></ul>');
            }

            if (SeftDOM.length > 0) {
                var IsSeftHasChild = SeftDOM.find('.btn-expand-child').length > 0 ? true : false;

                SeftDOM.find('.item-category-name[data-id="' + modelId + '"]').html(modelName);
                if (modelStatus == 1) {
                    if (IsSeftHasChild)
                        SeftDOM.find('.dd-handle').append(disabledText);
                    else
                        ParentDomHandle.append(disabledText);
                } else {
                    ParentDomHandle.find('.text-inactive').remove();
                }

                if (isHasLink) {
                    if (ParentDomHandle.find('label.switch').length <= 0) ParentDomHandle.append(checkboxText);
                }
                else {
                    ParentDomHandle.find('label.switch').remove();
                }

            } else {
                ParentDOM.children('.expand.lever2').prepend(elementDOM);
            }
        }
        else {
            if (SeftDOM.length > 0) {
                SeftDOM.find('.item-category-name[data-id="' + modelId + '"]').html(modelName);
                if (modelStatus == 1) {
                    ParentDomHandle.append(disabledText);
                } else {
                    ParentDomHandle.find('.text-inactive').remove();
                }

                if (isHasLink) {
                    if (ParentDomHandle.find('label.switch').length <= 0) ParentDomHandle.append(checkboxText);
                }
                else {
                    ParentDomHandle.find('label.switch').remove();
                }

            } else {
                $('#grid-data').append(elementDOM);
            }
        }
    },

    OnSave: function () {
        let valid = true;
        let formvalid = $('#form-group-product');
        formvalid.validate({
            rules: {
                Name: "required",
            },
            messages: {
                Name: "Vui lòng nhập tên nhóm hàng"
            }
        });
        var arrGroupStore = [];
        $('#grid-mapping-label .itemt').each(function () {
            var seft = $(this);
            var _LabelId = parseInt(seft.find('.dropdown-store').val());
            var _Domain = seft.find('.dropdown-store option:selected').attr('domain');
            var _LinkStoreMenu = seft.find('.link-store').val().trim();

            if (_LabelId == -1 || _LinkStoreMenu == "") {
                _msgalert.error('Bạn phải chọn nhãn hàng và nhập Link Mapping. Vui lòng kiểm tra lại');
                valid = false;
                return false;
            }

            var hostName = _groupProduct.IsValidHttpUrl(_LinkStoreMenu);
            if (hostName == false) {
                _msgalert.error('Link Mapping sai định dạng. Vui lòng kiểm tra lại');
                valid = false;
                return false;
            } else if (hostName != _Domain) {
                _msgalert.error('Link Mapping không thuộc nhãn hàng. Vui lòng kiểm tra lại');
                valid = false;
                return false;
            }

            var duplicateCount = arrGroupStore.filter(x => x.LabelId == _LabelId && x.LinkStoreMenu == _LinkStoreMenu).length;
            if (duplicateCount > 0) {
                _msgalert.error('Mapping nhãn hàng đang bị trùng. Vui lòng kiểm tra lại');
                valid = false;
                return false;
            }

            arrGroupStore.push({
                LabelId: _LabelId,
                LinkStoreMenu: _LinkStoreMenu
            });
        });

        if (valid && formvalid.valid()) {
            let form = document.getElementById('form-group-product');
            var formData = new FormData(form);
            var imagedata = $('.image-preview').attr('src');

            //var mime = imagedata.match(/data:([a-zA-Z0-9]+\/[a-zA-Z0-9-.+]+).*,.*/);
            //if (mime && mime.length > 0) {
            //    formData.append('imageFile', this.ConvertBase64toFile(imagedata, 'cropedimage.png'));
            //}

            formData.append('ImageBase64', imagedata);
            formData.append('imageSize', $('.sl-image-size option:selected').attr('size'));
            formData.append('GroupProductStores', JSON.stringify(arrGroupStore));

            var _modelId = formData.get("Id");
            var _modelName = formData.get("Name");
            var _modelStatus = formData.get("Status");
            var _modelParentId = formData.get("ParentId");

            // Check status of group product 
            if (_modelStatus == 1 && _modelId > 0) {
                let title = "Xác nhận khóa nhóm hàng";
                let description = "Nhóm hàng và các nhóm con của nó sẽ bị khóa, Bạn có muốn lưu lại không?";
                _msgconfirm.openDialog(title, description, function () {
                    $.ajax({
                        url: '/groupproduct/upsert',
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false,
                        success: function (result) {
                            if (result.isSuccess) {
                                _msgalert.success(result.message);
                                _groupProduct.ReLoadElement(result.modelId, _modelStatus, _modelName, _modelParentId, result.isHasLink);
                                _groupProduct.OnLoadTotalCrawl();
                                $.magnificPopup.close();
                            } else {
                                _msgalert.error(result.message);
                            }
                        },
                        error: function (jqXHR) {
                        },
                        complete: function (jqXHR, status) {
                        }
                    });
                });
            } else {
                $.ajax({
                    url: '/groupproduct/upsert',
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (result) {
                        if (result.isSuccess) {
                            _msgalert.success(result.message);
                            _groupProduct.ReLoadElement(result.modelId, _modelStatus, _modelName, _modelParentId, result.isHasLink);
                            _groupProduct.OnLoadTotalCrawl();
                            $.magnificPopup.close();
                        } else {
                            _msgalert.error(result.message);
                        }
                    },
                    error: function (jqXHR) {
                    },
                    complete: function (jqXHR, status) {
                    }
                });
            }
        }
    },

    IsValidHttpUrl: function (strUrl) {
        let url;
        try {
            url = new URL(strUrl);
        } catch (_) {
            return false;
        }
        return url.hostname;
    },

    OnDelete: function (id) {
        var SeftDOM = $('#grid-data .item-category-' + id);
        var childLength = SeftDOM.find('.btn-expand-child').length;
        var siblingLength = SeftDOM.siblings().length;

        if (childLength > 0) {
            _msgalert.error('Nhóm hàng đang có cấp con.Bạn không thể xóa.');
            return;
        }

        let title = 'Thông báo xác nhận';
        let description = "Bạn có chắc chắn muốn xóa nhóm hàng này?";
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/groupproduct/delete",
                type: "post",
                data: { id: id },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        // _groupProduct.ReLoad();
                        if (siblingLength == 0) {
                            SeftDOM.parent().siblings('.btn-expand-child').remove();
                        }
                        SeftDOM.remove();
                        _groupProduct.OnLoadTotalCrawl();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        });
    },

    OnSetupAutoCrawler: function (id, type) {
        //let title = 'Thông báo xác nhận';
        //let description = "Bạn có chắc chắn muốn xóa nhóm hàng này?";
        //_msgconfirm.openDialog(title, description, function () {
        $.ajax({
            url: "/groupproduct/SetupAutoCrawler",
            type: "post",
            data: { id: id, type: type },
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    _groupProduct.OnLoadTotalCrawl();
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
        //});
    },

    GetAllPosition: function () {
        $.ajax({
            url: '/groupproduct/GetAllPosition',
            type: 'POST',
            data: '',
            success: function (result) {
                if (result.code == 1) {
                    listPosition = result.data;
                }
            },
            error: function (jqXHR) {
            },
            complete: function (jqXHR, status) {
            }
        });
        if (postionId > 0) {
            setTimeout(function () {
                $('#selectCampaign option:eq(' + postionId + ')').prop('selected', true);
            }, 500)
        }
    },

    AddOrUpdatePosition: function (id) {
        setTimeout(function () {
            _groupProduct.GetAllPosition();
        }, 1500);
        let title = 'Kích thước hiển thị';
        let url = '/GroupProduct/AddOrUpdatePosition';
        let param = { id: id };
        _magnific.OpenSmallPopupWithHeader(title, url, param);
    },

    OnCreatePosition: function () {
        var positionName = $('#positionName').val();
        var width = $('#width').val();
        var height = $('#height').val();
        if (positionName == null || positionName == '') {
            _msgalert.error('Bạn chưa nhập tên vị trí');
            return;
        }
        if (width == null || width == '') {
            _msgalert.error('Bạn chưa nhập chiều rộng');
            return;
        }
        if (height == null || height == '') {
            _msgalert.error('Bạn chưa nhập chiều cao');
            return;
        }
        var data = {
            PositionName: positionName,
            Height: height,
            Width: width,
            Id: postionId
        }
        $.ajax({
            url: '/groupproduct/AddPositionJson',
            type: 'POST',
            data: data,
            success: function (result) {
                if (result.code == 1) {
                    _msgalert.success(result.message)
                    _groupProduct.GetAllPosition();
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.message)
                }
            },
            error: function (jqXHR) {
            },
            complete: function (jqXHR, status) {
            }
        });
    },

    OnUpdatePosition: function () {
        var positionName = $('#positionName').val();
        var width = $('#width').val();
        var height = $('#height').val();
        if (positionName == null || positionName == '') {
            _msgalert.error('Bạn chưa nhập tên vị trí');
            return;
        }
        if (width == null || width == '') {
            _msgalert.error('Bạn chưa nhập chiều rộng');
            return;
        }
        if (height == null || height == '') {
            _msgalert.error('Bạn chưa nhập chiều cao');
            return;
        }
        var data = {
            PositionName: positionName,
            Height: height,
            Width: width,
            Id: postionId
        }
        $.ajax({
            url: '/groupproduct/UpdatePositionJson',
            type: 'POST',
            data: data,
            success: function (result) {
                if (result.code == 1) {
                    _msgalert.success(result.message)
                    _groupProduct.GetAllPosition();
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.message)
                }
            },
            error: function (jqXHR) {
            },
            complete: function (jqXHR, status) {
            }
        });
    },

    ChangePosition: function () {
        $('#positionName').val('')
        $('#width').val('')
        $('#height').val('')
        postionId = parseInt($('#selectPosition').children(":selected").attr("value"));
        if (listPosition != null) {
            var postionInfo = listPosition.result.find(n => n.id == postionId);
            if (postionInfo != null) {
                $('#positionName').val(postionInfo.positionName)
                $('#width').val(postionInfo.width)
                $('#height').val(postionInfo.height)
                $('#btnAddPostion').hide();
                $('#btnEditPostion').show();
            } else {
                $('#btnAddPostion').show();
                $('#btnEditPostion').hide();
            }
        } else {
            $('#btnAddPostion').show();
            $('#btnEditPostion').hide();
        }
    },

    OnCrawl: function (self) {
        let self_element = $(self);
        let parent = self_element.closest('#grid-mapping-label .itemt');
        var _group_id = self_element.data('groupid');
        var _LabelId = parseInt(parent.find('.dropdown-store').val());
        var _LinkStoreMenu = parent.find('.link-store').val().trim();
        if (_group_id == "" || _group_id == null || _LabelId == "" || _LabelId == null || _LinkStoreMenu == "" || _LinkStoreMenu == null) {
            _msgalert.error("Vui lòng chọn / điền đẩy đủ thông tin, sau đó thử lại");
            return;
        }
        var item = {
            groupProductid: _group_id,
            labelid: _LabelId,
            linkdetail: _LinkStoreMenu
        };
        $.ajax({
            url: '/groupproduct/DoCrawl',
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
    },

    OnLoadTotalCrawl: function () {
        $.ajax({
            url: "/groupproduct/GetGroupProductCrawledCount",
            type: "post",
            success: function (result) {
                if (result.isSuccess) {
                    $('.item-crawl-on-data').html(result.crawlOn);
                    $('.item-crawl-off-data').html(result.crawlOff);
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
    },

    OnSetupAffCategory: function (cateid, affid, type) {
        $.ajax({
            url: "/groupproduct/SetupAffiliateCategory",
            type: "post",
            data: { cateId: cateid, affId: affid, type: type },
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
    },

    OnLoadAffiliateType: function () {
        $.ajax({
            url: "/groupproduct/GetAffiliateList",
            type: "post",
            success: function (data) {
                _groupProduct.AFFList = JSON.parse(data);
            }
        });
    }

};