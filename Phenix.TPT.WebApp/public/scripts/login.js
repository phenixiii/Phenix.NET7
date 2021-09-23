function isValidCheckInInfo() {
    $("#userNameHint").html('');
    if (vue.userName == null || vue.userName.trim() === '') {
        $('#userNameHint').html('登录名不允许为空!');
        $('#userName').focus();
        return false;
    }

    $('#companyNameHint').html('');
    if (vue.companyName == null || vue.companyName.trim() === '') {
        $('#companyNameHint').html('公司名不允许为空!');
        $('#companyName').focus();
        return false;
    }

    return true;
}

function isValidLogonInfo() {
    if (!isValidCheckInInfo())
        return false;

    $('#passwordHint').html('');
    if (vue.password == null || vue.password.trim() === '') {
        $('#passwordHint').html('登录口令不允许为空!');
        $('#password').focus();
        return false;
    }

    return true;
}

function jumpPage() {
    var hint = $('#logonHint');
    hint.html('正在获取个人资料, 请稍等...');
    phAjax.getMyself({
        onSuccess: function(result) {
            hint.html('您好, ' + result.RegAlias);
            if (result.EMail == null || result.RegAlias == null)
                $('#patchMyselfDialog').modal('show');
            else
                window.location.href = 'index.html';
        },
        onError: function(XMLHttpRequest, textStatus) {
            hint.html(XMLHttpRequest.responseText);
            zdalert('获取个人资料失败', XMLHttpRequest.responseText);
        },
    });
}

var vue = new Vue({
    el: '#content',
    data: {
        userName: null,
        password: null,
        companyName: null,
        eMail: null,
        regAlias: null,
    },
    methods: {
        onCheckIn: function() {
            if (!isValidCheckInInfo())
                return;

            var hint = $('#logonHint');
            hint.html('正在登记, 请稍等...');
            phAjax.checkIn({
                companyName: this.companyName.trim(),
                userName: this.userName.trim(),
                onSuccess: function(result) {
                    hint.html(result);
                    zdalert('登记成功', result);
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdalert('登记失败', XMLHttpRequest.responseText);
                },
            });
        },

        onLogon: function() {
            if (!isValidLogonInfo())
                return;

            var hint = $('#logonHint');
            hint.html('正在登录, 请稍等...');
            phAjax.logon({
                companyName: this.companyName.trim(),
                userName: this.userName.trim(),
                password: this.password,
                onSuccess: function(result) {
                    hint.html(result);
                    jumpPage();
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdalert('登录失败', XMLHttpRequest.responseText);
                },
            });
        },

        onPatchMyself: function() {
            var hint = $('#logonHint');
            hint.html('正在更新, 请稍等...');
            phAjax.patchMyself({
                eMail: this.eMail.trim(),
                regAlias: this.regAlias.trim(),
                onSuccess: function(result) {
                    hint.html(result);
                    jumpPage();
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdalert('更新失败', XMLHttpRequest.responseText);
                },
            });
        },
    }
})