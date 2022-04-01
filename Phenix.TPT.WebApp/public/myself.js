$(document).ready(function() {
    navbarControl();
});
new Vue({
    el: "#content",
    data: {
        user: [],
        oldpassword: "",
        newpassword: "",
        confirmpw: "",
        differ: false,
    },
    mounted: function() {
        this.user = globaluser;
        localStorage.removeItem('date');
    },
    methods: {
        checkpassword: function() {
            if (this.confirmpw != this.newpassword.substring(0, this.confirmpw.length)) {
                this.differ = true;
            } else this.differ = false;
        },
        check: function() {
            if (this.oldpassword.length > 0 && this.newpassword.length > 0 && this.confirmpw == this.newpassword) {
                $("#commit").removeAttr("disabled");
                return 'col-md-6 btn btn-primary';
            } else {
                $("#commit").attr('disabled', 'true');
                return 'col-md-6 btn btn-primary disabled';
            }
        },
        changePassword: function() {
            phAjax.changePassword(this.user.UserNumber, this.oldpassword, this.newpassword, function onCompleteLogOn(XMLHttpRequest) {
                if (XMLHttpRequest.status === 200) {
                    zdalert('系统提示', "修改成功，请重新登录！", function(r) {
                        if (r) {
                            localStorage.clear();
                            window.location.href = "login.html";
                        }
                    });
                } else {
                    zdalert('系统提示', "修改失败，请检查原登录密码!");
                }
            })
        },
    }
})