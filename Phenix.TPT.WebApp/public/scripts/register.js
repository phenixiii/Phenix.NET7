$(function() {
    var userAgent = window.navigator.userAgent;
    if (!(userAgent.indexOf('Chrome') > -1)) {　
        zdconfirm('系统提示', "推荐使用chrome浏览器，前去下载? ", function(ok) {
            if (ok) {
                window.location.href = "https://chrome.en.softonic.com/";
            }
        });
    }
});

function fetchValidityInputs() {
    var result = {};
    result.userName = $("#userName").val().trim();
    if (result.userName == null || result.userName == "") {
        $("#userNameHint").html('登录名不允许为空！');
        $("#userName").focus();
        return null;
    }
    $("#userNameHint").html('');

    result.password = $("#password").val().trim();
    if (result.password == null || result.password == "") {
        $("#passwordHint").html('口令不允许为空！');
        $("#password").focus();
        return null;
    }
    $("#passwordHint").html('');

    result.companyName = $("#companyName").val().trim();
    if (result.companyName == null || result.companyName == "") {
        $("#companyNameHint").html('公司名不允许为空！');
        $("#companyName").focus();
        return null;
    }
    $("#companyNameHint").html('');

    result.eMail = $("#eMail").val().trim();
    if (result.eMail == null || result.eMail == "") {
        $("#eMailHint").html('邮箱地址不允许为空！');
        $("#eMail").focus();
        return null;
    }
    $("#eMailHint").html('');

    result.regAlias = $("#regAlias").val().trim();
    if (result.regAlias == null || result.regAlias == "") {
        $("#regAliasHint").html('昵称不允许为空！');
        $("#regAlias").focus();
        return null;
    }
    $("#regAliasHint").html('');
    return result;
}

var v = new Vue({
    el: "#register",
    methods: {
        onRegister: function() {
            var inputs = fetchValidityInputs();
            if (inputs == null)
                return;

            var btn = $(".btn");
            var hint = $("#registerHint");
            hint.html('正在注册，请稍等...');
            phAjax.checkIn({
                companyName: inputs.companyName,
                userName: inputs.userName,
                hashName: false,
                phone: inputs.phone,
                eMail: inputs.eMail,
                regAlias: inputs.regAlias,
                onSuccess: function(result) {
                    hint.html('正在尝试登录为您设置登录口令...');
                    phAjax.changePassword({
                        companyName: inputs.companyName,
                        userName: inputs.userName,
                        hashName: false,
                        password: inputs.userName,
                        newPassword: inputs.password,
                        onSuccess: function(result) {
                            hint.html('完成注册');
                            zdconfirm(btn.val(),
                                '注册成功啦！是否帮您自动跳转到登录界面？',
                                function(result) {
                                    if (result)
                                        window.location.href = "login.html";
                                });
                        },
                        onError: function(XMLHttpRequest, textStatus) {
                            hint.html(XMLHttpRequest.responseText);
                            $("#passwordHint").html('口令不符合要求！');
                            zdalert(btn.val(),
                                XMLHttpRequest.responseText,
                                function(result) {
                                    $("#password").focus();
                                });
                        },
                    });
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdalert(btn.val(), XMLHttpRequest.responseText);
                },
            });
        },
    }
})