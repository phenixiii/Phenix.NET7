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

    result.companyName = $("#companyName").val().trim();
    if (result.companyName == null || result.companyName == "") {
        $("#companyNameHint").html('公司名不允许为空！');
        $("#companyName").focus();
        return null;
    }
    $("#companyNameHint").html('');
    return result;
}

function jumpPage() {
    phAjax.getMyself({
        onSuccess: function (result) {
            if (result.EMail == null || result.RegAlias == null)
                $("#patchMyselfDialog").modal('show');
            else if (result.Position.Roles.indexOf("经营管理") >= 0)
                window.location.href = "search.html";
            else
                window.location.href = "index.html";
        },
        onError: function(XMLHttpRequest, textStatus) {
            zdalert('系统提示', '获取不到自己的资料: ' + XMLHttpRequest.responseText);
        },
    });
}

var v = new Vue({
    el: "#login",
    methods: {
        onLogon: function() {
            var inputs = fetchValidityInputs();
            if (inputs == null)
                return;

            var btn = $(".btn");
            var hint = $("#logonHint");
            hint.html('正在登录，请稍等...');
            phAjax.logon({
                companyName: inputs.companyName,
                userName: inputs.userName,
                password: inputs.password,
                onSuccess: function(result) {
                    hint.html('登录成功');
                    jumpPage();
                },
                onError: function(XMLHttpRequest, textStatus) {
                    zdalert(btn.val(), XMLHttpRequest.responseText);
                },
            });
        },

        onCheckIn: function() {
            var inputs = fetchValidityInputs();
            if (inputs == null)
                return;

            var hint = $("#logonHint");
            hint.html('正在登记，请稍等...');
            phAjax.checkIn({
                companyName: inputs.companyName,
                userName: inputs.userName,
                onSuccess: function(result) {
                    hint.html('请查收动态口令并将之用于登录');
                    zdalert('登记', result);
                },
                onError: function(XMLHttpRequest, textStatus) {
                    hint.html(XMLHttpRequest.responseText);
                    zdconfirm('登记',
                        XMLHttpRequest.responseText,
                        function(result) {
                            if (result)
                                $("#patchMyselfDialog").modal('show');
                        });
                    zdalert('登记', XMLHttpRequest.responseText);
                },
            });
        },

        onPatchMyself: function() {
            phAjax.patchMyself({
                eMail: $("#eMail").val().trim(),
                onSuccess: function(result) {
                    zdconfirm($("#patchMyselfDialogLabel").val(), '提交成功');
                },
                onError: function(XMLHttpRequest, textStatus) {
                    zdalert($("#patchMyselfDialogLabel").val(), XMLHttpRequest.responseText);
                },
            });
        },
    }
})