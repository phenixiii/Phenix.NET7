function isValidCheckInInfo() {
    vue.userNameHint = '';
    if (vue.userName == null || vue.userName.trim() === '') {
        vue.userNameHint = '登录名不允许为空!';
        $('#userName').focus();
        return false;
    }

    vue.companyNameHint = '';
    if (vue.companyName == null || vue.companyName.trim() === '') {
        vue.companyNameHint = '公司名不允许为空!';
        $('#companyName').focus();
        return false;
    }

    return true;
}

function isValidLogonInfo() {
    if (!isValidCheckInInfo())
        return false;

    vue.passwordHint = '';
    if (vue.password == null || vue.password.trim() === '') {
        vue.passwordHint = '登录口令不允许为空!';
        $('#password').focus();
        return false;
    }

    return true;
}

function jumpPage() {
    vue.logonHint = '正在获取个人资料, 请稍等...';
    phAjax.getMyself({
        onSuccess: function(result) {
            vue.logonHint = '您好, ' + result.RegAlias;
            if (result.EMail == null || result.RegAlias == null)
                $('#patchMyselfDialog').modal('show');
            else if (phAjax.isInRole('经营管理'))
                window.location.href = 'account.html';
            else if (phAjax.isInRole('项目管理'))
                window.location.href = 'index.html';
            else
                window.location.href = 'workload.html';
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            vue.logonHint = validityError != null ? validityError.Hint : XMLHttpRequest.responseText;
            alert('获取个人资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function resetBaseAddress() {
    var baseAddress = prompt('请输入正确的数据服务IP地址: ', phAjax.baseAddress);
    if (baseAddress !== phAjax.baseAddress && baseAddress != null && baseAddress !== '') {
        phAjax.baseAddress = baseAddress;
        return true;
    }
    return false;
}

function checkIn() {
    vue.logonHint = '正在登记, 请稍等...';
    phAjax.checkIn({
        companyName: vue.companyName.trim(),
        userName: vue.userName.trim(),
        onSuccess: function(result) {
            vue.logonHint = result;
            alert('成功登记:\n' + result);
        },
        onError: function(XMLHttpRequest, textStatus) {
            vue.logonHint = XMLHttpRequest.responseText;
            alert('登记失败:\n' + XMLHttpRequest.responseText);
            if (resetBaseAddress())
                checkIn();
        },
    });
}

function logon() {
    vue.logonHint = '正在登录, 请稍等...';
    phAjax.logon({
        companyName: vue.companyName.trim(),
        userName: vue.userName.trim(),
        password: vue.password,
        onSuccess: function(result) {
            vue.logonHint = result;
            jumpPage();
        },
        onError: function(XMLHttpRequest, textStatus) {
            vue.logonHint = XMLHttpRequest.responseText;
            alert('登录失败:\n' + XMLHttpRequest.responseText);
            if (resetBaseAddress())
                logon();
        },
    });
}

function patchMyself() {
    vue.logonHint = '正在更新, 请稍等...';
    phAjax.patchMyself({
        eMail: vue.eMail.trim(),
        regAlias: vue.regAlias.trim(),
        onSuccess: function(result) {
            vue.logonHint = result;
            jumpPage();
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            vue.logonHint = validityError != null ? validityError.Hint : XMLHttpRequest.responseText;
            alert('更新失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

var vue = new Vue({
    el: '#content',
    data: {
        userName: phAjax.userName,
        password: null,
        companyName: phAjax.companyName,
        eMail: null,
        regAlias: null,

        userNameHint: null,
        companyNameHint: null,
        passwordHint: null,
        logonHint: null,
    },
    methods: {
        onCheckIn: function() {
            if (isValidCheckInInfo())
                checkIn();
        },

        onLogon: function() {
            if (isValidLogonInfo())
                logon();
        },

        onPatchMyself: function() {
            patchMyself();
        },
    }
})