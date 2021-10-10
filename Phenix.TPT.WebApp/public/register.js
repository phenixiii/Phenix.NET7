function isValidLogonInfo() {
    vue.userNameHint = '';
    if (vue.userName == null || vue.userName === '') {
        vue.userNameHint = '登录名不允许为空!'
        $('#userName').focus();
        return false;
    }

    vue.companyNameHint = '';
    if (vue.companyName == null || vue.companyName === '') {
        vue.companyNameHint = '公司名不允许为空!';
        $('#companyName').focus();
        return false;
    }

    return true;
}

function isValidRegisterInfo() {
    if (!isValidLogonInfo())
        return false;

    vue.eMailHint = '';
    if (vue.eMail == null || vue.eMail === '') {
        vue.eMailHint = '邮箱地址不允许为空!';
        $('#eMail').focus();
        return false;
    }

    vue.regAliasHint = '';
    if (vue.regAlias == null || vue.regAlias === '') {
        vue.regAliasHint = '昵称不允许为空!';
        $('#regAlias').focus();
        return false;
    }

    return true;
}

function isValidPasswordInfo() {
    if (!isValidLogonInfo())
        return false;

    vue.passwordHint = '';
    if (vue.password == null || vue.password === "") {
        vue.passwordHint = '登录口令不允许为空!';
        $('#password').focus();
        return false;
    }

    vue.newPasswordHint = '';
    if (vue.newPassword == null || vue.newPassword === "") {
        vue.newPasswordHint = '新口令不允许为空!';
        $('#newPassword').focus();
        return false;
    }

    vue.newPassword2Hint = '';
    if (vue.newPassword !== vue.newPassword2) {
        vue.newPassword2Hint = '新口令重复确认有误!';
        $('#newPassword2').focus();
        return false;
    }

    return true;
}

function register() {
    vue.registerHint = '正在注册企业会员, 请稍等...';
    phAjax.register({
        companyName: vue.companyName,
        userName: vue.userName,
        hashName: false,
        phone: vue.phone,
        eMail: vue.eMail,
        regAlias: vue.regAlias,
        onSuccess: function(result) {
            vue.registerHint = result;
            vue.password = vue.userName;
            $('#changePasswordDialog').modal('show');
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            vue.registerHint = validityError != null ? validityError.Hint : XMLHttpRequest.responseText;
            alert('注册失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function changePassword() {
    phAjax.changePassword({
        companyName: vue.companyName,
        userName: vue.userName,
        hashName: false,
        password: vue.password,
        newPassword: vue.newPassword,
        onSuccess: function(result) {
            if (confirm('成功修改口令:\n是否需要自动跳转到登录界面?'))
                window.location.href = 'login.html';
            else
                $('#changePasswordDialog').modal('hide');
        },
        onError: function(XMLHttpRequest, textStatus) {
            alert('修改口令失败:\n' + XMLHttpRequest.responseText);
            $("#password").focus();
        },
    });
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
        
        userNameHint: null,
        companyNameHint: null,
        eMailHint: null,
        regAliasHint: null,
        passwordHint: null,
        newPasswordHint: null,
        newPassword2Hint: null,
        registerHint: null,
    },
    methods: {
        onRegister: function() {
            if (isValidRegisterInfo())
                register();
        },

        showChangePasswordDialog: function() {
            if (isValidLogonInfo())
                $('#changePasswordDialog').modal('show');
            else
                $('#changePasswordDialog').modal('hide');
        },

        onChangePassword: function() {
            if (isValidPasswordInfo)
                changePassword();
        },
    }
})