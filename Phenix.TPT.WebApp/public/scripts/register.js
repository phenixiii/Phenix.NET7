$(function() {
    var userAgent = window.navigator.userAgent;
    if (!(userAgent.indexOf('Chrome') > -1)) {　
        zdconfirm('系统提示', '推荐使用chrome浏览器，前去下载? ', function(ok) {
            if (ok) {
                window.location.href = "https://chrome.en.softonic.com/";
            }
        });
    }
});

function fetchLogonInfo() {
    var result = {};

    result.userName = $('#userName').val().trim();
    if (result.userName == null || result.userName == '') {
        $('#changePasswordDialog').modal('hide');
        $('#userNameHint').html('登录名不允许为空！');
        $('#userName').focus();
        return null;
    }
    $('#userNameHint').html('');

    result.companyName = $('#companyName').val().trim();
    if (result.companyName == null || result.companyName == '') {
        $('#changePasswordDialog').modal('hide');
        $('#companyNameHint').html('公司名不允许为空！');
        $('#companyName').focus();
        return null;
    }
    $('#companyNameHint').html('');

    return result;
}

function fetchValidityRegisterInfo() {
    var result = fetchLogonInfo();

    result.eMail = $('#eMail').val().trim();
    if (result.eMail == null || result.eMail == '') {
        $('#eMailHint').html('邮箱地址不允许为空！');
        $('#eMail').focus();
        return null;
    }
    $('#eMailHint').html('');

    result.regAlias = $('#regAlias').val().trim();
    if (result.regAlias == null || result.regAlias == '') {
        $('#regAliasHint').html('昵称不允许为空！');
        $('#regAlias').focus();
        return null;
    }
    $('#regAliasHint').html('');

    return result;
}

function fetchValidityPasswordInfo() {
    var result = fetchLogonInfo();

    result.password = $('#password').val().trim();
    if (result.password == null || result.password == "") {
        $('#passwordHint').html('登录口令不允许为空！');
        $('#password').focus();
        return null;
    }
    $('#passwordHint').html('');

    result.newPassword = $('#newPassword').val().trim();
    if (result.newPassword == null || result.newPassword == "") {
        $('#newPasswordHint').html('新口令不允许为空！');
        $('#newPassword').focus();
        return null;
    }
    $('#newPasswordHint').html('');

    result.newPassword2 = $('#newPassword2').val().trim();
    if (result.newPassword != result.newPassword2 ) {
        $('#newPassword2Hint').html('新口令重复确认有误！');
        $('#newPassword2').focus();
        return null;
    }
    $('#newPassword2Hint').html('');
    return result;
}

var v = new Vue({
    el: '#register',
    methods: {
        onRegister: function() {
            var inputs = fetchValidityRegisterInfo();
            if (inputs == null)
                return;
            
            var hint = $('#registerHint');
            hint.html('正在注册企业会员，请稍等...');
            phAjax.register({
                companyName: inputs.companyName,
                userName: inputs.userName,
                hashName: false,
                phone: inputs.phone,
                eMail: inputs.eMail,
                regAlias: inputs.regAlias,
                onSuccess: function(result) {
                    hint.html(result);
                    $('#password').val(inputs.userName);
                    $('#changePasswordDialog').modal('show');
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdalert('注册失败', XMLHttpRequest.responseText);
                },
            });
        },

        onShowChangePasswordDialog: function() {
            var inputs = fetchLogonInfo();
            if (inputs == null)
                return;

            $('#changePasswordDialog').modal('show');
        },

        onChangePassword: function() {
            var inputs = fetchValidityPasswordInfo();
            if (inputs == null)
                return;

            phAjax.changePassword({
                companyName: inputs.companyName,
                userName: inputs.userName,
                hashName: false,
                password: inputs.password,
                newPassword: inputs.newPassword,
                onSuccess: function(result) {
                    zdconfirm('修改口令成功',
                        '是否需要自动跳转到登录界面？',
                        function(result) {
                            if (result)
                                window.location.href = 'login.html';
                        });
                },
                onError: function(XMLHttpRequest, textStatus) {
                    zdalert('修改口令失败',
                        XMLHttpRequest.responseText,
                        function(result) {
                            $("#password").focus();
                        });
                },
            });
        },
    }
})