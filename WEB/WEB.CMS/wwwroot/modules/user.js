$(document).ready(function () {
    var _searchData = {
        userName: '',
        strRoleId: '',
        status: -1,
        currentPage: 1,
        pageSize: 20
    };
    _user.Init(_searchData);

    $('#token-input-role').tokenInput('/Role/GetRoleSuggestionList', {
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
            _user.OnChangeRoleId($(this).val());
        },
        onDelete: function (item) {
            _user.OnChangeRoleId($(this).val());
        }
    });
});

var _changeInterval = null;
$("#ip-kup-search-user").keyup(function (e) {
    if (e.which === 13) {
        _user.OnChangeUser(e.target.value);
    } else {
        clearInterval(_changeInterval);
        _changeInterval = setInterval(function () {
            _user.OnChangeUser(e.target.value);
            clearInterval(_changeInterval);
        }, 500);
    }
});

$('.checkbox-tb-column').click(function () {
    let seft = $(this);
    let id = seft.data('id');
    if (seft.is(':checked')) {
        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
    } else {
        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
    }
});

$('#grid-data').on('click', 'tr.line-item', function () {
    let seft = $(this);
    let id = seft.data('id');
    let isloadajax = JSON.parse(seft.data('ajaxdetail'));
    let colSpan = $('.checkbox-tb-column:checked').length + 1;

    if (!isloadajax) {
        $.ajax({
            url: "/user/GetDetail",
            type: "post",
            data: { Id: id },
            success: function (result) {
                seft.after(result);
                seft.data('ajaxdetail', "true");
                $('.info-detail').children('td').attr('colspan', colSpan);
            }
        });
    }

    $('.info-detail').addClass('mfp-hide');
    $('.info-detail').children('td').attr('colspan', colSpan);

    if (seft.hasClass('active')) {
        seft.removeClass('active');
        seft.next().addClass('mfp-hide');
    } else {
        $('#grid-data tr.active').removeClass('active');
        seft.addClass('active');
        seft.next().removeClass('mfp-hide');
    }
});

$('#grid-data').on('click', '.btn-panel-detail', function () {
    let seft = $(this);
    let tab = seft.data('tab');
    seft.siblings().removeClass('active');
    seft.addClass('active');
    seft.closest('tr.info-detail').find('.tab-detail').hide();
    seft.closest('tr.info-detail').find('.tab-detail[data-id=' + tab + ']').show();
});

$('#grid-data').on('click', '.data-left li, .data-right li', function () {

    let seft = $(this);

    if (seft.closest('ul').hasClass('data-left')) {
        $('.data-right li').removeClass('active');
    } else {
        $('.data-left li').removeClass('active');
    }

    if (seft.hasClass('active')) {
        $(this).removeClass('active');
    } else {
        $(this).addClass('active');
    }

});

$('#grid-data').on('click', '.btn-change-user-status', function () {
    let seft = $(this);
    let userid = parseInt(seft.data('id'));
    let title = 'Xác nhận tài khoản';
    let description = 'Bạn có chắc chắn muốn đổi trạng thái hoạt động nhân viên này?';
    _msgconfirm.openDialog(title, description, function () {
        $.ajax({
            url: "/user/ChangeUserStatus",
            type: "post",
            data: { id: userid },
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    if (result.status === 0) {
                        seft.children('span').html('Khóa tài khoản');
                        seft.closest('.tab-detail').find('.text-status-user').html('Đang hoạt động');
                    } else {
                        seft.children('span').html('Mở tài khoản');
                        seft.closest('.tab-detail').find('.text-status-user').html('Tạm ngừng');
                    }
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
    });
});

var _user =  {

    Init: function (data) {
        this.SearchParam = data;
        this.Search(data);
    },

    Search: function (input) {
        $.ajax({
            url: "/user/search",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id');
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    } else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
            }
        });
    },

    ReLoad: function () {
        this.Search(this.SearchParam);
    },

    OnChangeUser: function (value) {
        var searchobj = this.SearchParam;
        searchobj.userName = value;
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

    OnChangeRoleId: function (value) {
        var searchobj = this.SearchParam;
        searchobj.strRoleId = value;
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

    OnOpenCreateForm: function () {
        let title = 'Thêm người dùng';
        let url = '/user/AddOrUpdate';
        let param = { Id: 0 };
        _magnific.OpenLargerPopup(title, url, param);
    },

    OnOpenEditForm: function (id) {
        let title = 'Cập nhật người dùng';
        let url = '/user/AddOrUpdate';
        let param = { Id: id };
        _magnific.OpenLargerPopup(title, url, param);
    },

    OnOpenCopyForm: function (id) {
        let title = 'Thêm người dùng';
        let url = '/user/AddOrUpdate';
        let param = { Id: id, IsClone: true };
        _magnific.OpenLargerPopup(title, url, param);
    },

    OnChangeImage: function () {
        const preview = document.querySelector('.img-preview');
        const file = document.querySelector('input[name=imagefile]').files[0];
        const reader = new FileReader();
        reader.addEventListener("load", function () {
            preview.src = reader.result;
        }, false);
        if (file) {
            reader.readAsDataURL(file);
        }
    },

    OnUpdateUser: function () {
        let FromCreate = $('#form-create-user');
        FromCreate.validate({
            rules: {
                UserName: "required",
                Email: {
                    required: true,
                    email: true
                },
                Password: {
                    required: true,
                    minlength: 6
                },
                RePassword: {
                    required: true,
                    equalTo: "#Password"
                }
            },
            messages: {
                UserName: "Vui lòng nhập tên đăng nhập",
                Email: {
                    required: 'Vui lòng nhập email',
                    email: 'Email không đúng định dạng'
                },
                Password: {
                    required: 'Vui lòng nhập mật khẩu',
                    minlength: 'Độ dài mật khẩu phải lớn hơn 6 kí tự'
                },
                RePassword: {
                    required: 'Vui lòng nhập lại mật khẩu',
                    equalTo: 'Mật khẩu không chính xác'
                }
            }
        });

        if (FromCreate.valid()) {
            let form = document.getElementById('form-create-user');
            var formData = new FormData(form);
            $.ajax({
                url: '/user/upsert',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        _user.ReLoad();
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
    },

    OnUpdateRole: function (userid, type) {
        let _arrRole = [];
        let _arrRoleData = [];

        if (type === 0) {
            $('.data-left[data-id=' + userid + '] li.active').each(function () {
                let _roleid = $(this).data('id');
                let _rolename = $(this).html();
                _arrRole.push(parseInt(_roleid));
                _arrRoleData.push({ roleid: _roleid, rolename: _rolename })
            });
        } else {
            $('.data-right[data-id=' + userid + '] li.active').each(function () {
                let _roleid = $(this).data('id');
                let _rolename = $(this).html();
                _arrRole.push(parseInt(_roleid));
                _arrRoleData.push({ roleid: _roleid, rolename: _rolename })
            });
        }

        if (_arrRole.length > 0) {
            let input = {
                userId: userid,
                arrRole: _arrRole,
                type: type
            }
            $.ajax({
                url: "/user/UpdateUserRole",
                type: "post",
                data: input,
                success: function (result) {
                    if (result.isSuccess) {
                        let strHtml = "";
                        _arrRoleData.map((obj) => strHtml += '<li data-id="' + obj.roleid + '">' + obj.rolename + '</li>');
                        if (type === 0) {
                            $('.data-left[data-id=' + userid + '] li.active').remove();
                            $('.data-right[data-id=' + userid + ']').append(strHtml);
                        } else {
                            $('.data-right[data-id=' + userid + '] li.active').remove();
                            $('.data-left[data-id=' + userid + ']').append(strHtml);
                        }
                    }
                }
            });
        }

    },

    OnResetPassword: function (id) {
        let title = 'Reset mật khẩu người dùng';
        let description = 'Bạn có chắc chắn muốn reset mật khẩu của nhân viên này?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/user/ResetPasswordByUserId",
                type: "post",
                data: { userId: id },
                success: function (data) {
                    if (data.isSuccess) {
                        _msgalert.success(data.message);
                        _magnific.OpenResetPasswordPopup(data.result);
                    } else {
                        _msgalert.error(data.message);
                    }
                }
            });
        });
    },

    OnResetMFA: function (id) {
        let title = 'Reset bảo mật 2 lớp';
        let description = 'Bạn có chắc chắn muốn reset MFA của nhân viên này?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/user/ResetMFA",
                type: "post",
                data: { id: id },
                success: function (data) {
                    if (data.isSuccess) {
                        _msgalert.success(data.message);
                    } else {
                        _msgalert.error(data.message);
                    }
                }
            });
        });
    }
};