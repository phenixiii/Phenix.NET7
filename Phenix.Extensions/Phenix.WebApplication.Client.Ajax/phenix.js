/*
    Phenix Framework for .NET Core
    Copyright © 2007, 2019 Phenixヾ Studio All rights reserved.

    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/CryptoJS/core-min.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/CryptoJS/enc-base64-min.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/CryptoJS/cipher-core-min.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/CryptoJS/aes-min.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/CryptoJS/md5-min.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/base64-binary.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/json2.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/jquery.min.js"></script> --v3.2.1
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/jquery.signalR.min.js"></script> --v2.3.0
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/jquery.cookie.js"></script>
    <script type="text/javascript" src="../Phenix.WebApplication.Client.Ajax/phenix.js"></script>

    使用方法参考 Phenix.WebApplication.Client.Test 目录下的示例代码
 */
;
$.support.cors = true;

var phAjax = (function($) {
    var BASE_ADDRESS_COOKIE_NAME = "P-BA";
    var USER_NAME_COOKIE_NAME = "P-UN";
    var USER_KEY_COOKIE_NAME = "P-UK";

    var METHOD_OVERRIDE_HEADER_NAME = "X-HTTP-Method-Override";
    var AUTHORIZATION_HEADER_NAME = "Phenix-Authorization";

    var getBaseAddress = function() {
        var result;
        try {
            result = window.localStorage.getItem(BASE_ADDRESS_COOKIE_NAME);
        } catch (e) {
            result = $.cookie(BASE_ADDRESS_COOKIE_NAME);
        }
        return typeof result != "undefined" && result != null ? result : 'http://localhost:5000';
    };
    var setBaseAddress = function(value) {
        try {
            window.localStorage.removeItem(BASE_ADDRESS_COOKIE_NAME);
            window.localStorage.setItem(BASE_ADDRESS_COOKIE_NAME, value);
        } catch (e) {
            $.cookie(BASE_ADDRESS_COOKIE_NAME, value, { path: '/' });
        }
    };

    var getUserName = function() {
        var result;
        try {
            result = window.localStorage.getItem(USER_NAME_COOKIE_NAME);
        } catch (e) {
            result = $.cookie(USER_NAME_COOKIE_NAME);
        }
        return typeof result != "undefined" && result != null ? result : "GUEST";
    };
    var setUserName = function(value) {
        try {
            window.localStorage.removeItem(USER_NAME_COOKIE_NAME);
            window.localStorage.setItem(USER_NAME_COOKIE_NAME, value);
        } catch (e) {
            $.cookie(USER_NAME_COOKIE_NAME, value, { path: '/' });
        }
    };

    var getUserKey = function() {
        var result;
        try {
            result = window.localStorage.getItem(USER_KEY_COOKIE_NAME);
        } catch (e) {
            result = $.cookie(USER_KEY_COOKIE_NAME);
        }
        return typeof result != "undefined" && result != null ? result : CryptoJS.MD5("GUEST").toString().toUpperCase();;
    };
    var setUserKey = function(value) {
        try {
            window.localStorage.removeItem(USER_KEY_COOKIE_NAME);
            window.localStorage.setItem(USER_KEY_COOKIE_NAME, value);
        } catch (e) {
            $.cookie(USER_KEY_COOKIE_NAME, value, { path: '/' });
        }
    };

    var setPutOverrideHeader = function(XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader(METHOD_OVERRIDE_HEADER_NAME, "PUT");
    };

    var setPatchOverrideHeader = function (XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader(METHOD_OVERRIDE_HEADER_NAME, "PATCH");
    };

    var setDeleteOverrideHeader = function(XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader(METHOD_OVERRIDE_HEADER_NAME, "DELETE");
    };

    //身份验证Header格式: Phenix-Authorization=[登录名],[时间戳(9位长随机数+ISO格式当前时间)],[签名(二次MD5登录口令/动态口令AES加密的时间戳)]
    var formatComplexAuthorizationHeader = function(XMLHttpRequest, userName, userKey) {
        var timestamp = phUtils.random(9) + new Date().toISOString();
        XMLHttpRequest.setRequestHeader(AUTHORIZATION_HEADER_NAME, encodeURIComponent(userName) + "," + timestamp + "," + phUtils.encrypt(timestamp, userKey));
    };

    var formatComplexAuthorizationHeaderByStorage = function(XMLHttpRequest) {
        formatComplexAuthorizationHeader(XMLHttpRequest, getUserName(), getUserKey());
    };

    return {
        get baseAddress() {
            return getBaseAddress();
        },

        get userName() {
            return getUserName();
        },

        encrypt: function(data) {
            return phUtils.encrypt(data, getUserKey());
        },

        decrypt: function (hexStr) {
            return phUtils.decrypt(hexStr, getUserKey());
        },

        // 登记/注册(获取动态口令)
        checkIn: function(options) {
            var defaults = {
                baseAddress: getBaseAddress(), //"http://localhost:5000"
                name: "ADMIN", //登录名(未注册则自动注册)
                phone: null, //手机(注册用可为空)
                eMail: null, //邮箱(注册用可为空)
                regAlias: null, //注册昵称(注册用可为空)
                onComplete: null, //调用完整的回调函数, 参数(XMLHttpRequest, textStatus)
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            $.ajax({
                type: "GET",
                url: options.baseAddress + "/api/security/gate" +
                    "?name=" + encodeURIComponent(options.name) +
                    "&phone=" + options.phone +
                    "&eMail=" + options.eMail +
                    "&regAlias=" + encodeURIComponent(options.regAlias),
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                cache: false,
                crossDomain: true,
                timeout: 3000,
                data: null,
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status == 200) {
                        setBaseAddress(options.baseAddress);
                        setUserName(options.name);
                    } else {
                        if (typeof options.onError == "function")
                            options.onError(XMLHttpRequest, textStatus, new Error(XMLHttpRequest.responseText));
                    }
                    if (typeof options.onComplete == "function")
                        options.onComplete(XMLHttpRequest, textStatus);
                },
            });
        },

        // 登录
        logon: function(options) {
            var defaults = {
                baseAddress: getBaseAddress(), //"http://localhost:5000"
                name: "ADMIN", //登录名
                password: "ADMIN", //登录口令/动态口令
                tag: new Date().toISOString(), //捎带数据(默认是客户端当前时间)
                onComplete: null, //调用完整的回调函数, 参数(XMLHttpRequest, textStatus)
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            var userKey = CryptoJS.MD5(options.password).toString().toUpperCase();
            $.ajax({
                type: "POST",
                url: options.baseAddress + "/api/security/gate",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                cache: false,
                crossDomain: true,
                timeout: 3000,
                beforeSend: function(XMLHttpRequest) {
                    formatComplexAuthorizationHeader(XMLHttpRequest, options.name, userKey);
                },
                data: phUtils.encrypt(options.tag, userKey),
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status == 200) {
                        setBaseAddress(options.baseAddress);
                        setUserName(options.name);
                        setUserKey(userKey);
                    } else {
                        if (typeof options.onError == "function")
                            options.onError(XMLHttpRequest, textStatus, new Error(XMLHttpRequest.responseText));
                    }
                    if (typeof options.onComplete == "function")
                        options.onComplete(XMLHttpRequest, textStatus);
                },
            });
        },

        // 获取自己资料
        getMyself: function(onSuccess, onError) {
            phAjax.call({
                path: "/api/security/myself",
                onSuccess: function(result) {
                    if (typeof onSuccess == "function")
                        onSuccess(result);
                },
                onError: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status != 200) {
                        if (typeof onError == "function")
                            onError(XMLHttpRequest, textStatus, new Error(XMLHttpRequest.responseText));
                    }
                },
            });
        },

        // 修改登录口令
        changePassword: function(newPassword, onSuccess, onError) {
            phAjax.call({
                type: "PATCH",
                path: "/api/security/myself",
                data: phAjax.encrypt(newPassword),
                onSuccess: function(result) {
                    setUserKey(CryptoJS.MD5(newPassword).toString().toUpperCase());
                    if (typeof onSuccess == "function")
                        onSuccess(result);
                },
                onError: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status != 200) {
                        if (typeof onError == "function")
                            onError(XMLHttpRequest, textStatus, new Error(XMLHttpRequest.responseText));
                    }
                },
            });
        },

        // 获取64位序号
        getSequence: function(onSuccess, onError) {
            phAjax.call({
                path: "/api/data/sequence",
                onSuccess: function(result) {
                    if (typeof onSuccess == "function")
                        onSuccess(result);
                },
                onError: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status != 200) {
                        if (typeof onError == "function")
                            onError(XMLHttpRequest, textStatus, new Error(XMLHttpRequest.responseText));
                    }
                },
            });
        },

        // 获取64位增量值
        getIncrement: function (key, initialValue, onSuccess, onError) {
            phAjax.call({
                path: "/api/data/increment?key=" + encodeURIComponent(key) + "&initialValue=" + initialValue,
                onSuccess: function(result) {
                    if (typeof onSuccess == "function")
                        onSuccess(result);
                },
                onError: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status != 200) {
                        if (typeof onError == "function")
                            onError(XMLHttpRequest, textStatus, new Error(XMLHttpRequest.responseText));
                    }
                },
            });
        },

        // 呼叫
        call: function(options) {
            var defaults = {
                anonymity: false, //是否匿名访问
                type: "GET", //请求方法(GET/POST/PUT/PATCH/DELETE)
                path: null, //"/api/security/myself"
                data: null,
                cache: false, //默认不缓存
                timeout: 30000, //默认超时30秒
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onComplete: null, //调用完整的回调函数, 参数(XMLHttpRequest, textStatus)
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            if (typeof options.data != "string")
                options.data = JSON.stringify(options.data);
            $.ajax({
                type: (options.type == "PUT" || options.type == "PATCH" || options.type == "DELETE") ? "POST" : options.type,
                url: phAjax.baseAddress + options.path,
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                cache: options.cache,
                crossDomain: true,
                timeout: options.timeout,
                beforeSend: function(XMLHttpRequest) {
                    if (!options.anonymity)
                        formatComplexAuthorizationHeaderByStorage(XMLHttpRequest);
                    if (options.type == "PUT")
                        setPutOverrideHeader(XMLHttpRequest);
                    else if (options.type == "PATCH")
                        setPatchOverrideHeader(XMLHttpRequest);
                    else if (options.type == "DELETE")
                        setDeleteOverrideHeader(XMLHttpRequest);
                },
                data: options.data,
                success: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result);
                },
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status >= 203) {
                        if (typeof options.onError == "function")
                            options.onError(XMLHttpRequest, textStatus, new Error(XMLHttpRequest.responseText));
                    }
                    if (typeof options.onComplete == "function")
                        options.onComplete(XMLHttpRequest, textStatus);
                },
            });
        },
    }
})(jQuery);

/*
    工具集
 */
var phUtils = (function () {
    return {
        random: function(length) {
            var result = Math.random().toString(36).substr(2);
            if (result.length >= length) {
                return result.substr(0, length);
            }
            result += phAjax.random(length - result.length);
            return result;
        },

        encrypt: function(data, key) {
            var result;
            if (typeof (key) == "string")
                key = CryptoJS.MD5(CryptoJS.enc.Utf8.parse(key));
            if (typeof (data) == "string") {
                result = CryptoJS.AES.encrypt(data, key, { iv: key, mode: CryptoJS.mode.CBC });
                return result.ciphertext.toString();
            } else if (typeof (data) == "object") {
                data = JSON.stringify(data);
                result = CryptoJS.AES.encrypt(data, key, { iv: key, mode: CryptoJS.mode.CBC });
                return result.ciphertext.toString();
            }
            return null;
        },
        
        decrypt: function (hexStr, key) {
            if (typeof (key) == "string")
                key = CryptoJS.MD5(CryptoJS.enc.Utf8.parse(key));
            var result = CryptoJS.AES.decrypt(CryptoJS.enc.Base64.stringify(CryptoJS.enc.Hex.parse(hexStr)), key, { iv: key, mode: CryptoJS.mode.CBC });
            return result.toString(CryptoJS.enc.Utf8);
        },
    }
})();