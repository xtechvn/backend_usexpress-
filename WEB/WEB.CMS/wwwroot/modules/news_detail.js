$(document).ready(function () {
    $('.datepicker-input').Zebra_DatePicker({
        format: 'd/m/Y H:i',
        onSelect: function () {
            $(this).change();
        }
    }).removeAttr('readonly');
});

_common.tinyMce('text-editor');

$('#detail-cate-panel .btn-toggle-cate').click(function () {
    var seft = $(this);
    if (seft.hasClass("plus")) {
        seft.siblings('ul.lever2').show();
        seft.removeClass('plus').addClass('minus');
    } else {
        seft.siblings('ul.lever2').hide();
        seft.removeClass('minus').addClass('plus');
    }
});

$('#news-tag').tagsinput({
    typeahead: {
        source: function (query) {
            var dataList = $.ajax({
                type: 'Post',
                url: "/News/GetSuggestionTag",
                async: false,
                dataType: 'json',
                data: {
                    name: query,
                },
                done: function (data) {
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                }
            }).responseJSON;
            return dataList;
        }, afterSelect: function () {
            this.$element[0].value = '';
        }
    }
});

var uploadCrop = $('#croppie-content').croppie({
    viewport: {
        width: 200,
        height: 150,
        type: 'square'
    },
    boundary: {
        width: 250,
        height: 250
    },
    url: '/images/icons/noimage.png'
});

$('.sl-image-size').change(function (e) {
    var value = e.target.value;
    var width = parseInt(value.split("x")[0]);
    var height = parseInt(value.split("x")[1]);
    var filedata = $('#image_file')[0].files[0];
    $('#croppie-content').croppie('destroy');
    if (filedata) {
        $('.wrap-croppie').show();
        $('.wrap-image-preview').hide();
        $('#btn-cropimage').show();
        var reader = new FileReader();
        reader.readAsDataURL(filedata);
        reader.onload = function () {
            $('#croppie-content').croppie({
                viewport: {
                    width: width,
                    height: height,
                    type: 'square'
                },
                boundary: {
                    width: 250,
                    height: 250
                },
                url: reader.result
            });
        };
    } else {
        $('#croppie-content').croppie({
            viewport: {
                width: width,
                height: height,
                type: 'square'
            },
            boundary: {
                width: 250,
                height: 250
            },
            url: '/images/icons/noimage.png'
        });
    }
});

$('#image_file').change(function (event) {
    var _validFileExtensions = ["jpg", "jpeg", "bmp", "gif", "png"];

    if (event.target.files && event.target.files[0]) {
        var fileType = event.target.files[0].name.split('.').pop();

        if (event.target.files[0].size > (1024 * 1024)) {
            _msgalert.error('File upload hiện tại có kích thước (' + Math.round(event.target.files[0].size / 1024 / 1024, 2) + ' Mb) vượt quá 1 Mb. Bạn hãy chọn lại ảnh khác');
            $(this).val('');
        }

        if (!_validFileExtensions.includes(fileType)) {
            _msgalert.error('File upload phải thuộc các định dạng : jpg, jpeg, bmp, gif, png');
            $(this).val('');
        }

        if (_validFileExtensions.includes(fileType) && event.target.files[0].size <= (1024 * 1024)) {
            $('.wrap-croppie').show();
            $('.wrap-image-preview').hide();
            $('#btn-cropimage').show();
            $('#btn-cancel-crop').show();
            var reader = new FileReader();
            reader.onload = function (e) {
                uploadCrop.croppie('bind', {
                    url: e.target.result
                });
            };
            reader.readAsDataURL(event.target.files[0]);
        }
    }
});

$('#btn-cropimage').click(function () {
    var size = $('.sl-image-size').val();
    if (size == "") {
        _msgalert.error('Bạn phải chọn kích thước để crop ảnh.');
    } else {
        uploadCrop.croppie('result', {
            type: 'canvas',
            size: 'original'
        }).then(function (base64img) {
            // $('.wrap-croppie').hide();
            // $('#btn-upload-image').show();
            // $('#btn-cropimage').hide();
            // $('#btn-cancel-crop').hide();

            switch (size) {
                case "250x250":
                    $('#img_1x1').attr('src', base64img);
                    $('#img_1x1').closest('.image-croped').removeClass('mfp-hide');
                    break;
                case "250x187":
                    $('#img_4x3').attr('src', base64img);
                    $('#img_4x3').closest('.image-croped').removeClass('mfp-hide');
                    break;
                case "250x140":
                    $('#img_16x9').attr('src', base64img);
                    $('#img_16x9').closest('.image-croped').removeClass('mfp-hide');
                    break;
            }
            // $('.wrap-image-preview').show();
            // $('#image_file').val('');
            // $('.btn-dynamic-enable').prop('disabled', false);
        });
    }
});

$('#btn-cancel-crop').click(function () {
    $('.wrap-croppie').hide();
    // $('#btn-upload-image').show();
    $('#btn-cropimage').hide();
    $('#btn-cancel-crop').hide();
    $('.wrap-image-preview').show();
    $('#image_file').val('');
    $('.sl-image-size').val('');
    // $('.btn-dynamic-enable').prop('disabled', false);
});

var _newsDetail = {

    OnOpenRelationForm: function (id) {
        let title = 'Chèn tin liên quan';
        let url = '/News/RelationArticle';
        let param = { Id: id };
        _magnific.OpenLargerPopup(title, url, param);
    },

    OnSave: function (articleStatus) {
        let formvalid = $('#form-news');

        formvalid.validate({
            rules: {
                Title: {
                    required: true,
                    maxlength: 300
                },
                Lead: {
                    required: true,
                    maxlength: 500
                },
                Position: {
                    min:0,
                    max:3
                }
            },
            messages: {
                Title:
                {
                    required: "Vui lòng nhập tiêu đề cho bài viết",
                    maxlength: "Tiêu đề cho bài viết không được vượt quá 300 ký tự"
                },
                Lead: {
                    required: "Vui lòng nhập mô tả ngắn cho bài viết",
                    maxlength: "Tiêu đề cho bài viết không được vượt quá 500 ký tự"
                },
                Position: {
                    min: "Vị trí bài viết phải trong khoảng 0 đến 3",
                    max: "Vị trí bài viết phải trong khoảng 0 đến 3"
                }
            }
        });

        if (formvalid.valid()) {
            var _body = tinymce.activeEditor.getContent();
            var _tags = $('#news-tag').tagsinput('items');
            var _categories = [];
            var _articleIdList = [];

            if ($('.ckb-news-cate:checked').length > 0) {
                $('.ckb-news-cate:checked').each(function () {
                    _categories.push($(this).val());
                });
            }

            if ($('.item-related-article').length > 0) {
                $('.item-related-article').each(function () {
                    _articleIdList.push(parseFloat($(this).data('id')));
                });
            }

            if (_categories.length <= 0) {
                _msgalert.error('Bạn phải chọn chuyên mục cho bài viết');
                return false;
            }

            var _model = {
                Id: $('#Id').val(),
                Title: $('#Title').val(),
                Lead: $('#Lead').val(),
                Body: _body,
                Status: articleStatus,
                ArticleType: $('#ArticleType:checked').val(),
                Image169: $('#img_16x9').attr('src') == undefined ? "" : $('#img_16x9').attr('src'),
                Image43: $('#img_4x3').attr('src') == undefined ? "" : $('#img_4x3').attr('src'),
                Image11: $('#img_1x1').attr('src') == undefined ? "" : $('#img_1x1').attr('src'),
                Tags: _tags,
                Categories: _categories,
                RelatedArticleIds: _articleIdList,
                PublishDate: ConvertToJSONDateTime($('#PublishDate').val()),
                DownTime: ConvertToJSONDateTime($('#DowntimeDate').val()),
                Position: $('#Position').val()
            }

            if (_model.Image169 == "" && _model.Image43 == "" && _model.Image11 == "") {
                _msgalert.error('Bạn phải upload ít nhất một ảnh đại diện cho tin bài');
                return false;
            }

            $.ajax({
                url: '/news/upsert',
                type: 'POST',
                data: JSON.stringify(_model),
                dataType: 'JSON',
                contentType: "application/json",
                traditional: true,
                // data: { model: _model },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        setTimeout(function () {
                            window.location.href = "/news/detail/" + result.dataId;
                        }, 300);
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (jqXHR) {

                }
            });
        } else {
            _msgalert.error('Bạn phải nhập thông tin đầy đủ và chính xác cho bài viết');
        }
    },

    OnChangeArticleStatus: function (id, status) {
        let actionName = '';
        let title = 'Cập nhật trạng thái bài viết';

        switch (parseInt(status)) {
            case 0:
                actionName = "đăng bài viết";
                break;
            case 2:
                actionName = "hạ bài viết";
                break;
        }

        let description = 'Bạn có chắc chắn muốn ' + actionName + '?';

        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: '/news/ChangeArticleStatus',
                type: 'POST',
                data: { Id: id, articleStatus: status },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        setTimeout(function () {
                            window.location.href = "/news/detail/" + result.dataId;
                        }, 200);
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (jqXHR) {
                }
            });
        });
    },

    OnDelete: function (id) {
        let title = 'Xác nhận xóa bài viết';
        let description = 'Bạn có chắc chắn muốn xóa bài viết này?';

        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: '/news/DeleteArticle',
                type: 'POST',
                data: { Id: id },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        setTimeout(function () {
                            window.location.href = "/news";
                        }, 200);
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (jqXHR) {
                }
            });
        });
    }
}