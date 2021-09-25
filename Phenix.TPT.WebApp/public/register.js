function isValidLogonInfo() {
    $('#userNameHint').html('');
    if (vue.userName == null || vue.userName === '') {
        $('#userNameHint').html('登录名不允许为空!');
        $('#userName').focus();
        return false;
    }

    $('#companyNameHint').html('');
    if (vue.companyName == null || vue.companyName === '') {
        $('#companyNameHint').html('公司名不允许为空!');
        $('#companyName').focus();
        return false;
    }

    return true;
}

function isValidRegisterInfo() {
    if (!isValidLogonInfo())
        return false;

    $('#eMailHint').html('');
    if (vue.eMail == null || vue.eMail === '') {
        $('#eMailHint').html('邮箱地址不允许为空!');
        $('#eMail').focus();
        return false;
    }

    $('#regAliasHint').html('');
    if (vue.regAlias == null || vue.regAlias === '') {
        $('#regAliasHint').html('昵称不允许为空!');
        $('#regAlias').focus();
        return false;
    }

    return true;
}

function isValidPasswordInfo() {
    if (!isValidLogonInfo())
        return false;

    $('#passwordHint').html('');
    if (vue.password == null || vue.password === "") {
        $('#passwordHint').html('登录口令不允许为空!');
        $('#password').focus();
        return false;
    }

    $('#newPasswordHint').html('');
    if (vue.newPassword == null || vue.newPassword === "") {
        $('#newPasswordHint').html('新口令不允许为空!');
        $('#newPassword').focus();
        return false;
    }

    $('#newPassword2Hint').html('');
    if (vue.newPassword !== vue.newPassword2 ) {
        $('#newPassword2Hint').html('新口令重复确认有误!');
        $('#newPassword2').focus();
        return false;
    }

    return true;
}

var vue = new Vue({
    el: '#content',
    data: {
        userName: null,
        companyName: null,
        eMail: null,
        regAlias: null,
        password: null,
        newPassword: null,
        newPassword2: null,
    },
    methods: {
        onRegister: function() {
            if (!isValidRegisterInfo())
                return;
            
            var hint = $('#registerHint');
            hint.html('正在注册企业会员, 请稍等...');
            phAjax.register({
                companyName: this.companyName,
                userName: this.userName,
                hashName: false,
                phone: this.phone,
                eMail: this.eMail,
                regAlias: this.regAlias,
                onSuccess: function(result) {
                    hint.html(result);
                    this.password = this.userName;
                    $('#changePasswordDialog').modal('show');
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdalert('注册失败', XMLHttpRequest.responseText);
                },
            });
        },

        showChangePasswordDialog: function() {
            if (isValidLogonInfo())
                $('#changePasswordDialog').modal('show');
            else
                $('#changePasswordDialog').modal('hide');
        },

        onChangePassword: function() {
            if (!isValidPasswordInfo)
                return;

            phAjax.changePassword({
                companyName: this.companyName,
                userName: this.userName,
                hashName: false,
                password: this.password,
                newPassword: this.newPassword,
                onSuccess: function (result) {
                    zdconfirm('成功修改口令',
                        '是否需要自动跳转到登录界面?',
                        function(result) {
                            if (result)
                                window.location.href = 'login.html';
                            else
                                $('#changePasswordDialog').modal('hide');
                        });
                },
                onError: function(XMLHttpRequest, textStatus) {
                    zdalert('修改口令失败', XMLHttpRequest.responseText);
                    $("#password").focus();
                },
            });
        },
    }
})