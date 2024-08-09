var listGroupProduct = [], listCampaign = [], listParent = [];

var utm_medium = '', utm_source = '', utm_campaign = '', utm_campaign_alias = '';

var listChildren = [], listGrandChildren = [], listLink = [], listLinkWrong = [], listLable = [];

var fromDate, toDate, campaignId = 0, cnt = 0, utm_sourceId = 0, utm_mediumId = 0, utm_campaignId = 0;

var _parrentId, _parrentName, _childrenId, _childrenName, _grandChilrenId, _grandChilrenName;

var listUtm_Source = [], listUtm_Medium = [], listUtm_Campaign = [];

function dateToString(date) {
    let month = date.getMonth() + 1;
    let day = String(date.getDate()).padStart(2, '0');
    let year = date.getFullYear();
    return day + '/' + month + '/' + year;
}

function dateStrToString(dateStr) {
    var spilt = dateStr.split('-');
    return spilt[2] + '/' + spilt[1] + '/' + spilt[0];
}

Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}

$(document).ready(function () {
    $('.datepicker-input').Zebra_DatePicker({
        format: 'd/m/Y'
    }).removeAttr('readonly');

    fromDate = dateToString(new Date());
    toDate = dateToString(new Date().addDays(7));
    $('#selectedMenu').hide();
    $('#erroradd').hide();
    $('#errorupdate').hide();
    $('#showLinkWrong').hide();
    _menu.InitGroupProduct();
    _menu.GetAllCampaign();
    setTimeout(function () {
        _menu.GetAllUtmMedium();
    }, 300);
    setTimeout(function () {
        _menu.GetAllUtmSource();
    }, 500);
    setTimeout(function () {
        _menu.GetAllUtmCampaign();
    }, 700);
    $("#fromDate").Zebra_DatePicker({
        onSelect: function () {
            $(this).change();
            fromDate = dateStrToString($(this).val());
            $('#fromDate').val(fromDate)
        }
    }).removeAttr('readonly');
    $("#toDate").Zebra_DatePicker({
        onSelect: function () {
            $(this).change();
            toDate = dateStrToString($(this).val());
            $('#toDate').val(toDate)
        }
    }).removeAttr('readonly');

    $("#selectCampaign").change(function () {
        campaignId = parseInt($(this).children(":selected").attr("value"));
        var campaignName = "";
        for (var i = 0; i < listCampaign.length; i++) {
            if (listCampaign[i].id == campaignId) {
                campaignName = listCampaign[i].campaignName
                utm_campaign = listCampaign[i].campaignName
            }
        }
        utm_campaign = $('#selectCampaign').val()
    });
    $("#selectUtmSource").change(function () {
        utm_sourceId = parseInt($(this).children(":selected").attr("value"));
        for (var i = 0; i < listUtm_Source.length; i++) {
            if (utm_sourceId == listUtm_Source[i].id) {
                $('#utm_sour').html(listUtm_Source[i].description);
                utm_source = listUtm_Source[i].description
            }
        }
    });
    $("#selectUtmCampaign").change(function () {
        utm_campaignId = parseInt($(this).children(":selected").attr("value"));
        for (var i = 0; i < listUtm_Campaign.length; i++) {
            if (utm_campaignId == listUtm_Campaign[i].id) {
                $('#utm_cp').html(listUtm_Campaign[i].description);
                utm_campaign = listUtm_Campaign[i].description
            }
        }
    });
    $("#selectUtmMedium").change(function () {
        utm_mediumId = parseInt($(this).children(":selected").attr("value"));
        for (var i = 0; i < listUtm_Medium.length; i++) {
            if (utm_mediumId == listUtm_Medium[i].id) {
                $('#utm_medi').html(listUtm_Medium[i].description);
                utm_medium = listUtm_Medium[i].description
            }
        }
    });
});

$(".tab-link").click(function () {
    $(".tab-link").removeClass("active");
    if (!$(this).hasClass("active")) {
        $(this).addClass("active");
    } else {
        $(this).removeClass("active");
    }
    var tabid = $(this).data("id");
    $(".item-tab-content").css("display", "none");
    $(".item-tab-content[data-id='" + tabid + "']").fadeIn();
});

function getChildrenMenu(item) {
    listChildren = [];
    $('#selectedMenu').show();
    $("#childrenMenu").empty();
    $("#grandChildrenMenu").empty();
    _grandChilrenId = "";
    var parentId = parseInt(item.id);
    _parrentId = parentId
    //lay thong tin ban ghi cha
    var parentInfo = listGroupProduct.find(n => n.id == parentId);
    if (parentInfo != null) {
        $('#parrent').html(parentInfo.name + '<i class="fa fa-angle-right"></i>');
        $('#children').html('');
        $('#grandchildren').html('');
        _grandChilrenId = "";
    }

    item.style.color = "red"
    for (var i = 0; i < listParent.length; i++) {
        if (listParent[i].id != parentId) {
            $('#' + listParent[i].id).attr('style', 'color:black');
        }
    }
    for (var i = 0; i < listGroupProduct.length; i++) {
        if (listGroupProduct[i].parentId > -1 && listGroupProduct[i].parentId == parentId) {
            listChildren.push(listGroupProduct[i]);
        }
    }
    for (var i = 0; i < listChildren.length; i++) {
        $("#childrenMenu").append(
            "<li id='" + listChildren[i].id + "' onclick='getGrandChildrenMenu(this)'><a >" + listChildren[i].name + " <i class='fa fa-angle-right'></i></a></li>"
        );
    }
}

function getGrandChildrenMenu(item) {
    $("#grandChildrenMenu").empty();
    listGrandChildren = [];
    _grandChilrenId = "";
    item.style.color = "red"
    var parentId = parseInt(item.id);
    for (var i = 0; i < listChildren.length; i++) {
        if (listChildren[i].id != parentId) {
            $('#' + listChildren[i].id).attr('style', 'color:black');
        }
    }
    _childrenId = parentId;
    //lay thong tin ban ghi cha
    var parentInfo = listGroupProduct.find(n => n.id == parentId);
    if (parentInfo != null) {
        $('#children').html(parentInfo.name + '<i class="fa fa-angle-right"></i>');
        $('#grandchildren').html('');
    }

    for (var i = 0; i < listGroupProduct.length; i++) {
        if (listGroupProduct[i].parentId > -1 && listGroupProduct[i].parentId == parentId) {
            listGrandChildren.push(listGroupProduct[i]);
        }
    }
    for (var i = 0; i < listGrandChildren.length; i++) {
        $("#grandChildrenMenu").append(
            "<li id='" + listGrandChildren[i].id + "' onclick='getIdGrandChildren(this)'><a >" + listGrandChildren[i].name + "</li>"
        );
    }
}

function getIdGrandChildren(item) {
    var id = parseInt(item.id);
    item.style.color = "red"
    for (var i = 0; i < listGrandChildren.length; i++) {
        if (listGrandChildren[i].id != id) {
            $('#' + listGrandChildren[i].id).attr('style', 'color:black');
        }
    }
    _grandChilrenId = id
    var groupInfo = listGroupProduct.find(n => n.id == id);
    if (groupInfo != null)
        $('#grandchildren').html(groupInfo.name);
}

function searchMenu() {
    var value = $('#searchInput').val()
    var listParentTemp = []
    listChildren = []
    listGrandChildren = []
    if (value == "" || value == null) {
        listParent = []
        $("#parentMenu").empty();
        for (var i = 0; i < listGroupProduct.length; i++) {
            if (listGroupProduct[i].parentId == -1) {
                listParent.push(listGroupProduct[i]);
            }
        }
        for (var i = 0; i < listParent.length; i++) {
            $("#parentMenu").append(
                "<li id='" + listParent[i].id + "' onclick='getChildrenMenu(this)'><a>" + listParent[i].name + " <i class='fa fa-angle-right'></i></a></li>"
            );
        }
        return;
    }
    for (var i = 0; i < listGroupProduct.length; i++) {
        if (listGroupProduct[i].name.toLowerCase().indexOf(value) != -1) {
            //neu la menu cha
            if (listGroupProduct[i].parentId == -1) {
                var info = listParentTemp.find(n => n.id == listGroupProduct.id)
                if (info == null) {
                    listParentTemp.push(listGroupProduct[i]);
                }
            }
            else {
                //kiem tra menu con - de lay ra menu cha
                var parentInfo = listGroupProduct.find(n => n.id == listGroupProduct.parentId)
                if (parentInfo != null) {
                    //neu la menu cha cua menu con vua tim kiem
                    if (parentInfo.parentId == -1) {
                        var info = listParentTemp.find(n => n.id == listGroupProduct.id)
                        if (info == null) {
                            listParentTemp.push(listGroupProduct[i]);
                        }
                    }
                    else {
                        var parentInfo = listGroupProduct.find(n => n.id == listGroupProduct.parentId)
                        if (parentInfo != null && parentInfo.parentId == -1) {
                            var info = listParentTemp.find(n => n.id == listGroupProduct.id)
                            if (info == null) {
                                listParentTemp.push(listGroupProduct[i]);
                            }
                        }
                    }
                }
            }
        }
    }
    if (value != "" && value != null) {
        listParent = listParentTemp
        listChildren = []
        $("#parentMenu").empty();
        for (var i = 0; i < listParent.length; i++) {
            $("#parentMenu").append(
                "<li id='" + listParent[i].id + "' onclick='getChildrenMenu(this)'><a>" + listParent[i].name + " <i class='fa fa-angle-right'></i></a></li>"
            );
        }
    }
}

function deleteItem(item) {

    var id = parseInt(item.id);
    if (listLink != null && listLink.length > 0) {
        listLink.splice(id, 1);
    }
    $("#tbLink tbody").empty();
    $('#totallink').html(listLink.length);
    for (var i = 0; i < listLink.length; i++) {
        var labelInfo = listLable.find(n => listLink[i].toLowerCase().indexOf(n.domain.toLowerCase()) >= 0);
        $("#tbLink tbody").append(
            "<tr>" +
            "<td id='" + i + "'>" + listLink[i] + "</td>" +
            "<td>" + labelInfo.storeName.charAt(0).toUpperCase() + labelInfo.storeName.slice(1) + "</td>" +
            "<td> <button type='submit' id='" + i + "' onclick='deleteItem(this)' class='btn btn -default white'>Xóa</button></td>" +
            "</tr>"
        );
    }
}

function change_alias(alias) {
    var str = alias;
    str = str.toLowerCase();
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
    str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
    str = str.replace(/đ/g, "d");
    str = str.replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g, "");
    str = str.replace(/ + /g, "");
    str = str.trim();
    return str;
}

var _menu = {
    InitGroupProduct: function () {
        this.GetMenu();
    },

    GetMenu: function () {
        $.ajax({
            url: "/groupProduct/GetAllGroup",
            type: "post",
            data: "",
            success: function (result) {
                $('#group-product').html(result);
                listGroupProduct = result.data;
                listLable = result.dataLabel;
                $('#totallink').html(listGroupProduct.length);
                if (listGroupProduct != null && listGroupProduct.length > 0) {
                    for (var i = 0; i < listGroupProduct.length; i++) {
                        if (listGroupProduct[i].parentId == -1) {
                            listParent.push(listGroupProduct[i]);
                        }
                    }
                    for (var i = 0; i < listParent.length; i++) {
                        $("#parentMenu").append(
                            "<li id='" + listParent[i].id + "' onclick='getChildrenMenu(this)'><a>" + listParent[i].name + " <i class='fa fa-angle-right'></i></a></li>"
                        );
                    }
                }
            }
        });
    },

    UploadExcel: function () {
        var file = $('input[type=file]')[0].files[0];
        var formData = new FormData();
        formData.append('fileCrawl', file);
        $.ajax({
            url: '/groupProduct/UploadExcel',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (result) {
                listLink = [];
                listLinkWrong = [];
                $("#tbLink tbody").empty();
                $("#listLinkWrong").empty();
                $('#showLinkWrong').hide();
                $('#totallink').html(0);
                if (result.code == 2) {
                    _msgalert.error(result.message);
                } else if (result.code == -1) {
                    _msgalert.error(result.message);
                } else {
                    if (result != null && ((result.data != null && result.data.length > 0) || (result.dataLinkWrong != null && result.dataLinkWrong.length > 0))) {
                        $('.scrollbar_crawl thead').css("display", "contents");
                        listLink = result.data;
                        listLinkWrong = result.dataLinkWrong;
                        $('#totallink').html(listLink.length);
                        if (listLink.length > 0) {
                            for (var i = 0; i < listLink.length; i++) {
                                var labelInfo = listLable.find(n => listLink[i].toLowerCase().indexOf(n.domain.toLowerCase()) >= 0);
                                $("#tbLink tbody").append(
                                    "<tr>" +
                                    "<td id='" + i + "'>" + "<a href='" + listLink[i] + "' cklass='break-word' target='_blank'>" + listLink[i] + "</a>" + "</td>" +
                                    "<td>" + labelInfo.storeName.charAt(0).toUpperCase() + labelInfo.storeName.slice(1) + "</td>" +
                                    "<td> <button type='submit' id='" + i + "' onclick='deleteItem(this)' class='btn btn -default white'>Xóa</button></td>" +
                                    "</tr>"
                                );
                            }
                        }
                        if (listLinkWrong.length > 0) {
                            $('#showLinkWrong').show();
                            for (var i = 0; i < listLinkWrong.length; i++) {
                                $("#listLinkWrong").append("<a href='#' class='break-word'>" + listLinkWrong[i] + "</a><br/>");
                            }
                        }
                    }
                }
                document.getElementById('uploadExcel').value = "";
            },
            error: function (jqXHR) {
            },
            complete: function (jqXHR, status) {
            }
        });
    },

    PostData: function () {

        if (listLink == null || listLink == undefined || listLink.length <= 0) {
            _msgalert.error('Bạn phải tải danh sách link trước khi gửi yêu cầu.');
            return;
        }
        var lastId = 0;
        if (_grandChilrenId != null && _grandChilrenId != "") {
            lastId = parseInt(_grandChilrenId)
        } else if (_parrentId != null && _parrentId != "") {
            lastId = parseInt(_childrenId)
        } else {
            lastId = parseInt(_parrentId)
        }
        if (lastId == 0 || isNaN(lastId)) {
            _msgalert.error('Bạn phải chọn ngành hàng trước khi gửi yêu cầu.');
            return;
        }
        if (campaignId <= 0) {
            _msgalert.error('Bạn phải chọn chiến dịch trước khi gửi yêu cầu.');
            return;
        }
        if (fromDate == null || fromDate == '') {
            _msgalert.error('Bạn phải chọn thời gian bắt đầu hiệu lực.');
            return;
        }
        if (toDate == null || toDate == '') {
            _msgalert.error('Bạn phải chọn thời gian kết thúc hiệu lực.');
            return;
        }
        if (utm_sourceId <= 0) {
            _msgalert.error('Bạn phải chọn utm source trước khi gửi yêu cầu.');
            return;
        }
        var objData = {
            listLink: listLink,
            groupId: lastId,
            fromTime: fromDate,
            toTime: toDate,
            campaignId: campaignId,
            utm_campain: utm_campaign,
            utm_source: utm_source,
            utm_medium: utm_medium,
        }
        $.ajax({
            url: "/groupProduct/AddProductClassificationJson",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.data == 1) {
                    _msgalert.success(result.message);
                } else {
                    _msgalert.error(result.message);
                }
            },
            error: function (result) {
                _msgalert.error(result.message);
            }
        });
        var objData_2 = {
            listLink: listLink,
            groupId: lastId,
            campaignId: campaignId
        }
        $.ajax({
            url: "/groupProduct/PushCrawlDataToQueue",
            type: "POST",
            data: objData_2,
            success: function () {
              
            },
        });
    },

    OnOpenCreateCampaign: function () {
        if (fromDate == null || fromDate == '') {
            _msgalert.error('Bạn phải chọn thời gian bắt đầu chiến dịch.');
            return;
        }
        if (toDate == null || toDate == '') {
            _msgalert.error('Bạn phải chọn thời gian kết thúc chiến dịch.');
            return;
        }
        let title = 'Khởi tạo chiến dịch';
        let url = '/groupProduct/AddCampaign';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnOpenEditCampaign: function (id) {
        if (campaignId == null || campaignId == '' || campaignId < 1) {
            _msgalert.error('Bạn chưa chọn chiến dịch');
            return;
        }
        if (fromDate == null || fromDate == '') {
            _msgalert.error('Bạn phải chọn thời gian bắt đầu chiến dịch.');
            return;
        }
        if (toDate == null || toDate == '') {
            _msgalert.error('Bạn phải chọn thời gian kết thúc chiến dịch.');
            return;
        }
        let title = 'Cập nhật chiến dịch';
        let url = '/groupProduct/EditCampaign?id=' + campaignId;
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnOpenCreateUtmSource: function () {
        let title = 'Khởi tạo Utm Source';
        let url = '/groupProduct/AddAllCode?utm_source=true&utm_medium=false&utm_campaign=false';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnOpenEditUtmSource: function (id) {
        if (utm_sourceId == null || utm_sourceId == '' || utm_sourceId < 1) {
            _msgalert.error('Bạn chưa chọn utm source');
            return;
        }
        let title = 'Cập nhật Utm Source';
        let url = '/groupProduct/EditAllCode?id=' + utm_sourceId + '&utm_source=true&utm_medium=false&utm_campaign=false';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnOpenCreateUtmCampaign: function () {
        let title = 'Khởi tạo Utm Campaign';
        let url = '/groupProduct/AddAllCode?utm_source=false&utm_medium=false&utm_campaign=true';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnOpenEditUtmCampaign: function (id) {
        if (utm_campaignId == null || utm_campaignId == '' || utm_campaignId < 1) {
            _msgalert.error('Bạn chưa chọn utm campaign');
            return;
        }
        let title = 'Cập nhật Utm Campaign';
        let url = '/groupProduct/EditAllCode?id=' + utm_campaignId + '&utm_source=false&utm_medium=false&utm_campaign=true';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnOpenCreateUtmMedium: function () {
        let title = 'Khởi tạo Utm Medium';
        let url = '/groupProduct/AddAllCode?utm_source=false&utm_medium=true&utm_campaign=false';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    OnOpenEditUtmMedium: function (id) {
        if (utm_mediumId == null || utm_mediumId == '' || utm_mediumId < 1) {
            _msgalert.error('Bạn chưa chọn utm medium');
            return;
        }
        let title = 'Cập nhật Utm Medium';
        let url = '/groupProduct/EditAllCode?id=' + utm_mediumId + '&utm_source=false&utm_medium=true&utm_campaign=false';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },

    GetAllCampaign: function () {
        $("#selectCampaign").empty();
        $("#selectCampaign").append("<option value='0'>--- Lựa chọn chiến dịch ---</option>");
        $.ajax({
            url: "/groupProduct/GetAllCampaign",
            type: "post",
            data: "",
            success: function (result) {
                listCampaign = result.data;
                if (listCampaign != null && listCampaign.length > 0) {
                    for (var i = 0; i < listCampaign.length; i++) {
                        $("#selectCampaign").append("<option value=\"" + listCampaign[i].id + "\" id=\"option" + listCampaign[i].id + "\">" + listCampaign[i].campaignName + "</option>");
                    }
                }
            }
        });
        if (campaignId > 0) {
            setTimeout(function () {
                $('#selectCampaign option:eq(' + campaignId + ')').prop('selected', true);
            }, 500)
        }
    },

    AddCampaign: function () {
        var tenChienDich = $('#tenchiendich-add').val();
        var type = '';
        if (tenChienDich == undefined || tenChienDich == null || tenChienDich == '') {
            _msgalert.error('Bạn phải nhập tên chiến dịch.');
            return;
        }
        var objData = {
            CampaignName: tenChienDich,
            Type: type,
            fromTime: fromDate,
            toTime: toDate,
        }
        $.ajax({
            url: "/groupProduct/AddCampaignJson",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.code == 1) {
                    _msgalert.success(result.message);
                    _menu.GetAllCampaign();
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.message);
                }
            },
            error: function (result) {
                _msgalert.error(result.message);
            }
        });
    },

    EditCampaign: function (id) {
        var tenChienDich = $('#tenchiendich-update').val();
        var type = '';
        if (tenChienDich == undefined || tenChienDich == null || tenChienDich == '') {
            _msgalert.error('Bạn phải nhập tên chiến dịch.');
            return;
        }
        var objData = {
            CampaignName: tenChienDich,
            Id: campaignId,
            Type: type,
            fromTime: fromDate,
            toTime: toDate,
        }
        $.ajax({
            url: "/groupProduct/EditCampaignJson",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.code == 1) {
                    _msgalert.success(result.message);
                    _menu.GetAllCampaign();
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.message);
                }
            },
            error: function (result) {
                _msgalert.error(result.message);
            }
        });
    },

    CreateProductManual: function () {
        var asin = $("#ASIN").val()
        var json_product = $("#json_product").val()
        if (asin == null || asin == "") {
            _msgalert.error("Vui lòng nhập ASIN");
            return;
        }
        if (json_product == null || json_product == "") {
            _msgalert.error("Vui lòng nhập chuỗi json của sản phẩm");
            return;
        }
        var objData = {
            ASIN: asin,
            json_product: json_product,
        }
        $.ajax({
            url: "/groupProduct/CreateProductManual",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.code == 0) {
                    _msgalert.success(result.message);
                    $("#ASIN").val("")
                } else {
                    _msgalert.error(result.message);
                }
            },
            error: function (result) {
                _msgalert.error(result.message);
            }
        });
    },

    GetDetail: function (data_type) {
        var objData = {
            ASIN: $("#ASIN").val(),
            data_type: data_type
        }
        $.ajax({
            url: "/groupProduct/GetDetail",
            type: "POST",
            data: objData,
            success: function (result) {
                if (result.code == 0) {
                    $("#json_product").val(result.detail);
                } else {
                    $("#json_product").val(result.detail);
                }
            },
            error: function (result) {
                _msgalert.error(result.message);
            }
        });
    },//function dung cho tool setup manual v1

    GetAllUtmMedium: function () {
        $("#selectUtmMedium").empty();
        $("#selectUtmMedium").append("<option value='0' id='0'>--- Lựa chọn Utm Medium  ---</option>");
        $.ajax({
            url: "/groupProduct/GetAllUtmMedium",
            type: "post",
            data: "",
            success: function (result) {
                listUtm_Medium = result.data;
                if (listUtm_Medium != null && listUtm_Medium.length > 0) {
                    for (var i = 0; i < listUtm_Medium.length; i++) {
                        $("#selectUtmMedium").append("<option value=\"" + listUtm_Medium[i].id + "\" id=\"" + listUtm_Medium[i].codeValue + "\">" + listUtm_Medium[i].description + "</option>");
                    }
                }
            }
        });
        if (utm_mediumId > 0) {
            setTimeout(function () {
                // $('#selectUtmMedium option:eq(' + utm_mediumId + ')').prop('selected', true);
                $("#selectUtmMedium").val(utm_mediumId);
            }, 300)
        }
    },

    GetAllUtmSource: function () {
        $("#selectUtmSource").empty();
        $("#selectUtmSource").append("<option value='0' id='0'>--- Lựa chọn Utm Source  ---</option>");
        $.ajax({
            url: "/groupProduct/GetAllUtmSource",
            type: "post",
            data: "",
            success: function (result) {
                listUtm_Source = result.data;
                if (listUtm_Source != null && listUtm_Source.length > 0) {
                    for (var i = 0; i < listUtm_Source.length; i++) {
                        $("#selectUtmSource").append("<option value=\"" + listUtm_Source[i].id + "\" id=\"" + listUtm_Source[i].codeValue + "\">" + listUtm_Source[i].description + "</option>");
                    }
                }
            }
        });
        if (utm_sourceId > 0) {
            setTimeout(function () {
                //$('#selectUtmSource option:eq(' + utm_sourceId + ')').prop('selected', true);
                $("#selectUtmSource").val(utm_sourceId);
            }, 300)
        }
    },

    GetAllUtmCampaign: function () {
        $("#selectUtmCampaign").empty();
        $("#selectUtmCampaign").append("<option value='0' id='0'>--- Lựa chọn Utm Campaign  ---</option>");
        $.ajax({
            url: "/groupProduct/GetAllUtmCampaign",
            type: "post",
            data: "",
            success: function (result) {
                listUtm_Campaign = result.data;
                if (listUtm_Campaign != null && listUtm_Campaign.length > 0) {
                    for (var i = 0; i < listUtm_Campaign.length; i++) {
                        $("#selectUtmCampaign").append("<option value=\"" + listUtm_Campaign[i].id + "\" id=\""
                            + listUtm_Campaign[i].codeValue + "\">" + listUtm_Campaign[i].description + "</option>");
                    }
                }
            }
        });
        if (utm_campaignId > 0) {
            setTimeout(function () {
                $("#selectUtmCampaign").val(utm_campaignId);
            }, 300)
        }
    },

    AddUtmSource: function () {
        let FromCreate = $('#add-allcode');
        FromCreate.validate({
            rules: {
                Description: "required",
            },
            messages: {
                Description: "Bạn chưa nhập Utm Source",
            }
        });
        if (FromCreate.valid()) {
            let form = document.getElementById('add-allcode');
            var formData = new FormData(form);
            $.ajax({
                url: "/groupProduct/AddAllCodeJson",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.code > 0) {
                        _msgalert.success(result.message);
                        _menu.GetAllUtmSource();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.message);
                }
            });
        }
    },

    EditUtmSource: function (id) {
        if (utm_sourceId <= 0) {
            _msgalert.error('Bạn chưa chọn Utm Source');
            return;
        }
        let FromCreate = $('#edit-allcode');
        FromCreate.validate({
            rules: {
                Description: "required",
            },
            messages: {
                Description: "Bạn chưa nhập Utm Source",
            }
        });
        if (FromCreate.valid()) {
            let form = document.getElementById('edit-allcode');
            var formData = new FormData(form);
            formData.append('Id', utm_sourceId);
            $.ajax({
                url: "/groupProduct/EditAllCodeJson",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.code > 1) {
                        _msgalert.success(result.message);
                        _menu.GetAllUtmSource();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.message);
                }
            });
        }
    },

    AddUtmMedium: function () {
        let FromCreate = $('#add-allcode');
        FromCreate.validate({
            rules: {
                Description: "required",
            },
            messages: {
                Description: "Bạn chưa nhập Utm Medium",
            }
        });
        if (FromCreate.valid()) {
            let form = document.getElementById('add-allcode');
            var formData = new FormData(form);
            $.ajax({
                url: "/groupProduct/AddAllCodeJson",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.code > 1) {
                        _msgalert.success(result.message);
                        _menu.GetAllUtmMedium();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.message);
                }
            });
        }
    },

    EditUtmMedium: function (id) {
        if (utm_mediumId <= 0) {
            _msgalert.error('Bạn chưa chọn Utm Medium');
            return;
        }
        let FromCreate = $('#edit-allcode');
        FromCreate.validate({
            rules: {
                Description: "required",
            },
            messages: {
                Description: "Bạn chưa nhập Utm Medium",
            }
        });
        if (FromCreate.valid()) {
            let form = document.getElementById('edit-allcode');
            var formData = new FormData(form);
            $.ajax({
                url: "/groupProduct/EditAllCodeJson",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.code > 1) {
                        _msgalert.success(result.message);
                        _menu.GetAllUtmMedium();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.message);
                }
            });
        }
    },

    AddUtmCampaign: function () {
        let FromCreate = $('#add-allcode');
        FromCreate.validate({
            rules: {
                Description: "required",
            },
            messages: {
                Description: "Bạn chưa nhập Utm Campaign",
            }
        });
        if (FromCreate.valid()) {
            let form = document.getElementById('add-allcode');
            var formData = new FormData(form);
            $.ajax({
                url: "/groupProduct/AddAllCodeJson",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.code > 1) {
                        _msgalert.success(result.message);
                        _menu.GetAllUtmCampaign();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.message);
                }
            });
        }
    },

    EditUtmCampaign: function (id) {
        if (utm_campaignId <= 0) {
            _msgalert.error('Bạn chưa chọn Utm Campaign');
            return;
        }
        let FromCreate = $('#edit-allcode');
        FromCreate.validate({
            rules: {
                Description: "required",
            },
            messages: {
                Description: "Bạn chưa nhập Utm Campaign",
            }
        });
        if (FromCreate.valid()) {
            let form = document.getElementById('edit-allcode');
            var formData = new FormData(form);
            $.ajax({
                url: "/groupProduct/EditAllCodeJson",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.code > 1) {
                        _msgalert.success(result.message);
                        _menu.GetAllUtmCampaign();
                        $.magnificPopup.close();
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (result) {
                    _msgalert.error(result.message);
                }
            });
        }
    },
};