/*
    Phenix Framework for .NET Core 2.1
    Copyright © 2007, 2019 Phenixヾ Studio All rights reserved.

    <script type="text/javascript" src="../lib/crypto-js/core-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/enc-base64-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/cipher-core-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/aes-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/md5-min.js"></script>
    <script type="text/javascript" src="../lib/base64-binary.js"></script>
    <script type="text/javascript" src="../lib/json2.js"></script>
    <script type="text/javascript" src="../lib/jquery.min.js"></script> --v3.2.1
    <script type="text/javascript" src="../lib/signalr.min.js"></script> --v1.0.4
    <script type="text/javascript" src="../lib/jquery.cookie.js"></script>
    <script type="text/javascript" src="../lib/phenix7.js"></script>

    示例代码见 test 目录
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

    var setPatchOverrideHeader = function(XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader(METHOD_OVERRIDE_HEADER_NAME, "PATCH");
    };

    var setDeleteOverrideHeader = function(XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader(METHOD_OVERRIDE_HEADER_NAME, "DELETE");
    };

    // 身份验证token: [登录名],[时间戳(9位长随机数+ISO格式当前时间)],[签名(二次MD5登录口令/动态口令AES加密的时间戳)]
    var formatComplexAuthorization = function(userName, userKey) {
        var timestamp = phUtils.random(9) + new Date().toISOString();
        var result = encodeURIComponent(userName) + "," + timestamp + "," + phUtils.encrypt(timestamp, userKey);
        return result;
    };

    var formatComplexAuthorizationByStorage = function() {
        return formatComplexAuthorization(getUserName(), getUserKey());
    };

    var setComplexAuthorizationHeader = function(XMLHttpRequest, userName, userKey) {
        XMLHttpRequest.setRequestHeader(AUTHORIZATION_HEADER_NAME, formatComplexAuthorization(userName, userKey));
    };

    var setComplexAuthorizationHeaderByStorage = function(XMLHttpRequest) {
        setComplexAuthorizationHeader(XMLHttpRequest, getUserName(), getUserKey());
    };

    var heartbeatMessage = function(connection, onFail) {
        connection.invoke("Heartbeat")
            .catch(function(error) {
                if (typeof onFail == "function")
                    onFail(error);
                connection.start()
                    .catch(function(error) {
                        if (typeof onFail == "function") onFail(error);
                    });
            });
        setTimeout(function() {
            heartbeatMessage(connection, onFail);
        }, 30000);
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

        decrypt: function(hexStr) {
            return phUtils.decrypt(hexStr, getUserKey());
        },

        // 登记/注册(获取动态口令)
        checkIn: function(options) {
            var defaults = {
                baseAddress: phAjax.baseAddress, //"http://localhost:5000"
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
                baseAddress: phAjax.baseAddress, //"http://localhost:5000"
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
                    setComplexAuthorizationHeader(XMLHttpRequest, options.name, userKey);
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
        getMyself: function(options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: "/api/security/myself",
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(JSON.parse(phAjax.decrypt(result))); //User
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof options.onError == "function")
                        options.onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 修改登录口令
        // newPassword: 新登录口令
        changePassword: function(newPassword, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: "PATCH",
                path: "/api/security/myself",
                data: phAjax.encrypt(newPassword),
                onSuccess: function(result) {
                    setUserKey(CryptoJS.MD5(newPassword).toString().toUpperCase());
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result); //是否成功
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof options.onError == "function")
                        options.onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 获取64位序号
        getSequence: function(options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: "/api/data/sequence",
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result); //64位序号
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof options.onError == "function")
                        options.onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 获取64位增量值
        // key: 键
        // initialValue: 初值
        getIncrement: function(key, initialValue, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: "/api/data/increment?key=" + encodeURIComponent(key) + "&initialValue=" + initialValue,
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result); //64位增量值
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof options.onError == "function")
                        options.onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 发送消息
        // receiver: 接收用户
        // content: 消息内容
        sendMessage: function(receiver, content, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: "POST",
                path: "/api/message/user-message?receiver=" + encodeURIComponent(receiver),
                data: content,
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result); //undefined
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof options.onError == "function")
                        options.onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 接收消息（PULL）
        receiveMessage: function(options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: "/api/message/user-message",
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result); //结果集(消息ID-消息内容)
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof options.onError == "function")
                        options.onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 确认收到
        // id: 消息ID
        // burn: 是否销毁
        affirmReceivedMessage: function(id, burn, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: "PATCH",
                path: "/api/message/user-message?id=" + id + "&burn=" + burn,
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result); //undefined
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof options.onError == "function")
                        options.onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 订阅消息（PUSH）
        subscribeMessage: function(options) {
            var defaults = {
                onReceived: null, //处理收到消息, 参数(messages)为消息id(key)+content(array[key])数据字典集合
                onConnected: null, //连接成功的回调函数
                onFail: null, //连接失败的回调函数, 参数(error)
            };
            options = $.extend(defaults, options);
            var connection = new signalR.HubConnectionBuilder()
                .withUrl(phAjax.baseAddress + "/api/message/user-message-hub",
                    {
                        accessTokenFactory: function() {
                            return formatComplexAuthorizationByStorage();
                        }
                    })
                .configureLogging(signalR.LogLevel.Error)
                .build();
            connection.on('onReceived',
                function(messages) {
                    if (typeof options.onReceived == "function")
                        options.onReceived(messages);
                });
            connection.start()
                .catch(function(error) {
                    if (typeof options.onFail == "function")
                        options.onFail(error);
                }).then(function() {
                    setTimeout(function() {
                        heartbeatMessage(connection, options.onFail);
                    }, 30000);
                    if (typeof options.onConnected == "function")
                        options.onConnected();
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
                        setComplexAuthorizationHeaderByStorage(XMLHttpRequest);
                    if (options.type == "PUT")
                        setPutOverrideHeader(XMLHttpRequest);
                    else if (options.type == "PATCH")
                        setPatchOverrideHeader(XMLHttpRequest);
                    else if (options.type == "DELETE")
                        setDeleteOverrideHeader(XMLHttpRequest);
                },
                data: options.data,
                success: function(result) {
                    if (typeof options.onSuccess == "function") {
                        options.onSuccess(result);
                        options.onSuccess = null;
                    }
                },
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status == 200) {
                        if (typeof options.onSuccess == "function") {
                            options.onSuccess(XMLHttpRequest.responseText);
                            options.onSuccess = null;
                        }
                    }
                    else if (XMLHttpRequest.status >= 203) {
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