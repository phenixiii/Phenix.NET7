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

function fetchCheckInInfo() {
    var result = {};

    result.userName = $('#userName').val().trim();
    if (result.userName == null || result.userName == "") {
        $('#userNameHint').html('登录名不允许为空！');
        $('#userName').focus();
        return null;
    }
    $("#userNameHint").html('');

    result.companyName = $('#companyName').val().trim();
    if (result.companyName == null || result.companyName == "") {
        $('#companyNameHint').html('公司名不允许为空！');
        $('#companyName').focus();
        return null;
    }
    $("#companyNameHint").html('');

    return result;
}

function fetchLogonInfo() {
    var result = fetchCheckInInfo();

    result.password = $('#password').val().trim();
    if (result.password == null || result.password == "") {
        $('#passwordHint').html('登录口令不允许为空！');
        $('#password').focus();
        return null;
    }
    $('#passwordHint').html('');


    return result;
}

function jumpPage() {
    var hint = $('#logonHint');
    phAjax.getMyself({
        onSuccess: function(result) {
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
    methods: {
        onCheckIn: function() {
            var inputs = fetchCheckInInfo();
            if (inputs == null)
                return;

            var hint = $('#logonHint');
            hint.html('正在登记，请稍等...');
            phAjax.checkIn({
                companyName: inputs.companyName,
                userName: inputs.userName,
                onSuccess: function(result) {
                    hint.html(result);
                    zdconfirm('登记成功', result);
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdalert('登记失败', XMLHttpRequest.responseText);
                },
            });
        },

        onLogon: function() {
            var inputs = fetchLogonInfo();
            if (inputs == null)
                return;

            var hint = $('#logonHint');
            hint.html('正在登录，请稍等...');
            phAjax.logon({
                companyName: inputs.companyName,
                userName: inputs.userName,
                password: inputs.password,
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
            phAjax.patchMyself({
                eMail: $('#eMail').val().trim(),
                onSuccess: function(result) {
                    hint.html(result);
                    jumpPage();
                },
                onError: function(XMLHttpRequest, textStatus) {
                    zdalert('更新失败', XMLHttpRequest.responseText);
                },
            });
        },
    }
})