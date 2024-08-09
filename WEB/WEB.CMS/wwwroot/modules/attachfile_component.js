var _wrapperImage = $("#image-choice-list");
$(document).ready(function () {
    _wrapperImage.lightGallery();
});

var _attachfile = {
    UploadFile: function (elem) {
        this.SaveAttachFile(elem[0].files);
    },

    RefreshlightGallery: function () {
        _wrapperImage.data('lightGallery').destroy(true);
        _wrapperImage.lightGallery();
    },

    SaveAttachFile: function (files) {
        var formData = new FormData();
        var _dataId = parseFloat($('#ip-data-attach-id').val());
        var _type = parseFloat($('#ip-data-attach-type').val());
        for (var i = 0; i != files.length; i++) {
            formData.append("attachFiles", files[i]);
        }
        formData.append("DataId", _dataId);
        formData.append("Type", _type);
        $.ajax({
            url: "/AttachFile/UploadFile",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                if (data.isSuccess) {
                    _msgalert.success(data.message);
                    var _dataFile = JSON.parse(data.dataFile);
                    _dataFile.forEach(function (item) {
                        _wrapperImage.append('<div class="col-sm-3 col-4 mb10" data-src="' + item.FilePath + '" data-id="' + item.Id + '">'
                            + '<div class="choose-ava">'
                            + '<img src="' + item.FilePath + '" />'
                            + '<button type="button" class="delete" onclick="_attachfile.DeleteAttachFile(' + item.Id + ')">×</button>'
                            + '</div >'
                            + '</div >');
                    });
                    _attachfile.RefreshlightGallery();
                } else {
                    _msgalert.error(data.message);
                }
            }
        });
    },

    DeleteAttachFile: function (id) {
        _msgconfirm.openDialog("Xác nhận xóa file", "Bạn có chắc muốn xóa file này?", function () {
            $.ajax({
                url: "/AttachFile/DeleteFile",
                data: { AttachId: id },
                type: "POST",
                success: function (data) {
                    if (data.isSuccess) {
                        _msgalert.success(data.message);
                        $('#image-choice-list div[data-id="' + id + '"]').remove();
                        _attachfile.RefreshlightGallery();
                    } else {
                        _msgalert.error(data.message);
                    }
                }
            });
        });
    }
};