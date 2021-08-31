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
        return null;
    }
    $("#userNameHint").html('');

    result.password = $("#password").val().trim();
    if (result.password == null || result.password == "") {
        $("#passwordHint").html('口令不允许为空！');
        return null;
    }
    $("#passwordHint").html('');

    result.companyName = $("#companyName").val().trim();
    if (result.companyName == null || result.companyName == "") {
        $("#companyNameHint").html('公司名不允许为空！');
        return null;
    }
    $("#companyNameHint").html('');
    return result;
}

function jumpPage() {
    phAjax.getMyself({
        onSuccess: function(result) {
            if (result.Position.Roles.indexOf("经营管理") >= 0)
                window.location.href = "search.html";
            else
                window.location.href = "index.html";
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            zdalert('系统提示', '获取不到自己的资料!: ' + errorThrown.manage);
        },
    });
}

var v = new Vue({
    el: "#login",
    methods: {
        logon: function () {
            var inputs = fetchValidityInputs();
            phAjax.logon({
                companyName: inputs.companyName,
                userName: inputs.userName,
                password: inputs.password,
                onComplete: function(XMLHttpRequest, textStatus) {
                    jumpPage();
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    zdalert('登录系统', '登录失败!: ' + errorThrown.manage);
                },
            });
        },

        checkIn: function() {
            var inputs = fetchValidityInputs();
            phAjax.checkIn({
                companyName: inputs.companyName,
                userName: inputs.userName,
                onComplete: function(XMLHttpRequest, textStatus) {
                    zdalert('登记/注册', XMLHttpRequest.responseText);
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    zdalert('登记/注册', '操作失败!: ' + errorThrown.manage);
                },
            })
        }
    }
})