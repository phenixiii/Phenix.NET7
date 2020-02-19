/*
    Phenix Framework 7 for .NET Core 3 & Orleans 3
    Copyright © 2007, 2020 Phenixヾ Studio All rights reserved.

    <script type="text/javascript" src="../lib/crypto-js/core-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/enc-base64-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/cipher-core-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/aes-min.js"></script>
    <script type="text/javascript" src="../lib/crypto-js/md5-min.js"></script>
    <script type="text/javascript" src="../lib/json2.js"></script>
    <script type="text/javascript" src="../lib/jquery.min.js"></script> --v3.2.1
    <script type="text/javascript" src="../lib/jquery.cookie.js"></script>
    <script type="text/javascript" src="../lib/jquery.base64.js"></script>
    <script type="text/javascript" src="../lib/signalr.min.js"></script>
    <script type="text/javascript" src="../lib/phenix7.js"></script>

    示例代码见 test 目录
 */
;
$.support.cors = true;

var phAjax = (function($) {
    var baseAddressCookieName = "P-BA";
    var userNameCookieName = "P-UN";
    var userKeyCookieName = "P-UK";

    var methodOverrideHeaderName = "X-HTTP-Method-Override";
    var authorizationHeaderName = "Phenix-Authorization";

    var maxChunkSize = 64 * 1024;

    var getBaseAddress = function() {
        var result;
        try {
            result = window.localStorage.getItem(baseAddressCookieName);
        } catch (e) {
            result = $.cookie(baseAddressCookieName);
        }
        return typeof result != "undefined" && result != null ? result : 'http://localhost:5000';
    };
    var setBaseAddress = function(value) {
        try {
            window.localStorage.removeItem(baseAddressCookieName);
            window.localStorage.setItem(baseAddressCookieName, value);
        } catch (e) {
            $.cookie(baseAddressCookieName, value, { path: '/' });
        }
    };

    var getUserName = function() {
        var result;
        try {
            result = window.localStorage.getItem(userNameCookieName);
        } catch (e) {
            result = $.cookie(userNameCookieName);
        }
        return typeof result != "undefined" && result != null ? result : "GUEST";
    };
    var setUserName = function(value) {
        try {
            window.localStorage.removeItem(userNameCookieName);
            window.localStorage.setItem(userNameCookieName, value);
        } catch (e) {
            $.cookie(userNameCookieName, value, { path: '/' });
        }
    };

    var getUserKey = function() {
        var result;
        try {
            result = window.localStorage.getItem(userKeyCookieName);
        } catch (e) {
            result = $.cookie(userKeyCookieName);
        }
        return typeof result != "undefined" && result != null ? result : CryptoJS.MD5("GUEST").toString().toUpperCase();;
    };
    var setUserKey = function(value) {
        try {
            window.localStorage.removeItem(userKeyCookieName);
            window.localStorage.setItem(userKeyCookieName, value);
        } catch (e) {
            $.cookie(userKeyCookieName, value, { path: '/' });
        }
    };

    // 身份验证token: [登录名],[时间戳(9位长随机数+ISO格式当前时间)],[签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)]
    var formatComplexAuthorization = function(userName, userKey) {
        var timestamp = phUtils.random(9) + new Date().toISOString();
        return encodeURIComponent(userName) + "," + timestamp + "," + phUtils.encrypt(timestamp, userKey);
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

        decrypt: function(cipherText) {
            return phUtils.decrypt(cipherText, getUserKey());
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
                contentType: "application/json;charset=utf-8",
                cache: false,
                crossDomain: true,
                timeout: 3000,
                beforeSend: function(XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader(authorizationHeaderName, formatComplexAuthorization(options.name, userKey));
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
                decryptResult: true,
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(JSON.parse(result)); //User
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
                data: newPassword,
                encryptData: true,
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

        // 接收消息（PULL）
        receiveMessage: function(options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(messages)为消息id(key)+content(array[key])数据字典集合
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: "/api/message/user-message",
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess(result);
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
        sendMessage: function(id, receiver, content, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: "PUT",
                path: "/api/message/user-message?id=" + id + "&receiver=" + encodeURIComponent(receiver),
                data: content,
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess();
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
                onSuccess: null, //调用成功的回调函数
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: "POST",
                path: "/api/message/user-message?receiver=" + encodeURIComponent(receiver),
                data: content,
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess();
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
                onSuccess: null, //调用成功的回调函数
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: "PATCH",
                path: "/api/message/user-message?id=" + id + "&burn=" + burn,
                onSuccess: function(result) {
                    if (typeof options.onSuccess == "function")
                        options.onSuccess();
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
                onThen: null, //连接成功的回调函数, 参数(connection)
                onFail: null, //连接失败的回调函数, 参数(connection, error)
                onReconnecting: null, //重新连接中的回调函数, 参数(connection, error)
                onReconnected: null, //重新连接好的回调函数, 参数(connection, connectionId)
                onClose: null, //连接关闭的回调函数, 参数(connection, error)
            };
            options = $.extend(defaults, options);
            var connection = new signalR.HubConnectionBuilder()
                .withUrl(phAjax.baseAddress + "/api/message/user-message-hub",
                    {
                        accessTokenFactory: function() {
                            return formatComplexAuthorization(getUserName(), getUserKey());
                        }
                    })
                .configureLogging("error")
                .withAutomaticReconnect()
                .build();
            connection.on('onReceived',
                function(messages) {
                    if (typeof options.onReceived == "function")
                        options.onReceived(JSON.parse(messages));
                });
            connection.onreconnecting(function(error) {
                if (typeof options.onReconnecting == "function")
                    options.onReconnecting(connection, error);
            });
            connection.onreconnected(function(connectionId) {
                if (typeof options.onReconnected == "function")
                    options.onReconnected(connection, connectionId);
            });
            connection.onclose(function(error) {
                if (typeof options.onClose == "function")
                    options.onClose(connection, error);
            });
            connection.start()
                .catch(function(error) {
                    if (typeof options.onFail == "function")
                        options.onFail(connection, error);
                }).then(function() {
                    if (typeof options.onConnected == "function")
                        options.onConnected(connection);
                });
        },

        uploadFiles: function(options) {
            var defaults = {
                message: null, //上传消息
                files: null, //上传文件(须是FileList/File对象(如果APP应用是本地图片，要么转成base64->File对象，要么转成网络图片->base64->File对象))
                onProgress: null, //执行进度的回调函数, 参数(fileName, chunkCount, chunkNumber, chunkSize)，回调函数返回值如为false则中止上传
                onSuccess: null, //调用成功的回调函数, 参数(message)为完成上传时返回消息
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            if (options.files != null)
                if (options.files instanceof FileList) {
                    for (var i = 0; i < options.files.length; i++) {
                        var file = options.files[i];
                        phAjax.uploadFileChunk(options.message, file, 1, options.onProgress, options.onSuccess, options.onError);
                    };
                } else {
                    phAjax.uploadFileChunk(options.message, options.files, 1, options.onProgress, options.onSuccess, options.onError);
                };
        },

        uploadFileChunk: function (message, file, chunkNumber, onProgress, onSuccess, onError) {
            var formData = new FormData();
            formData.append("message", message);
            var chunkCount = Math.ceil(file.size / maxChunkSize);
            var chunkSize = chunkNumber > 0 ? chunkNumber < chunkCount ? maxChunkSize : file.size - maxChunkSize * (chunkCount - 1) : 0;
            formData.append("chunkInfo",
                JSON.stringify({
                    FileName: file.name,
                    ChunkCount: chunkCount,
                    ChunkNumber: chunkNumber,
                    ChunkSize: chunkSize,
                    MaxChunkSize: maxChunkSize
                }));
            if (chunkNumber > 0) {
                var p = maxChunkSize * (chunkNumber - 1);
                formData.append("chunkBody", file.slice(p, p + chunkSize), file.name);
            };
            phAjax.call({
                type: "POST",
                path: "/api/inout/file",
                processData: false, //不要对data参数进行序列化处理
                contentType: false, //不要设置Content-Type请求头，因为文件数据是以multipart/form-data来编码
                data: formData,
                onSuccess: function(result) {
                    if (chunkNumber <= 0)
                        return;
                    if (typeof onProgress == "function")
                        if (!onProgress(file.name, chunkCount, chunkNumber, chunkSize)) {
                            phAjax.uploadFileChunk(message, file, 0, onProgress, onSuccess, onError);
                            return;
                        };
                    if (chunkNumber >= chunkCount) {
                        if (typeof onSuccess == "function")
                            onSuccess(result);
                        return;
                    };
                    phAjax.uploadFileChunk(message, file, chunkNumber + 1, onProgress, onSuccess, onError);
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof onError == "function")
                        onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        downloadFile: function(options) {
            var defaults = {
                message: null, //上传消息
                fileName: null, //下载文件名
                onProgress: null, //执行进度的回调函数, 参数(fileName, chunkCount, chunkNumber, chunkSize, chunkBody, chunkBuffer)，函数调用返回值如为false则中止下载
                onSuccess: null, //调用成功的回调函数, 参数(fileName, fileBlob)
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            phAjax.downloadFileChunk(options.message, options.fileName, 1, null, options.onProgress, options.onSuccess, options.onError);
        },

        downloadFileChunk: function (message, fileName, chunkNumber, chunkBuffer, onProgress, onSuccess, onError) {
            var formData = new FormData();
            formData.append("message", message);
            formData.append("fileName", fileName);
            formData.append("chunkNumber", chunkNumber);
            phAjax.call({
                type: "PUT",
                path: "/api/inout/file",
                processData: false, //不要对data参数进行序列化处理
                contentType: false, //不要设置Content-Type请求头，因为文件数据是以multipart/form-data来编码
                data: formData,
                onSuccess: function(result) {
                    if (result == null)
                        return;
                    result.chunkBody = $.base64.atob(result.chunkBody);
                    chunkBuffer = chunkBuffer == null ? result.chunkBody : chunkBuffer.concat(result.chunkBody);
                    if (typeof onProgress == "function") {
                        var goon = onProgress(result.fileName, result.chunkCount, result.chunkNumber, result.chunkSize, result.chunkBody, chunkBuffer);
                        if (typeof goon == "boolean" && !goon)
                            return;
                    };
                    if (result.chunkNumber >= result.chunkCount) {
                        if (typeof onSuccess == "function")
                            onSuccess(result.fileName, new Blob([phUtils.toUint8Array(chunkBuffer)]));
                        return;
                    };
                    phAjax.downloadFileChunk(message, fileName, chunkNumber + 1, chunkBuffer, onProgress, onSuccess, onError);
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    if (typeof onError == "function")
                        onError(XMLHttpRequest, textStatus, errorThrown);
                },
            });
        },

        // 呼叫
        call: function(options) {
            var defaults = {
                anonymity: false, //是否匿名访问
                type: "GET", //HttpMethod(GET/POST/PUT/PATCH/DELETE)
                path: null, //"/api/security/myself"
                data: null, //上传数据
                encryptData: false, //默认不加密上传数据（否则服务端的控制器代码请用Request.ReadBodyAsync(true)解密）
                decryptResult: false, //默认不解密返回数据（否则服务端的控制器代码请用this.EncryptAsync(result)加密）
                processData: true, //默认对data参数进行序列化处理
                contentType: "application/json;charset=utf-8", 
                cache: false, //默认不缓存
                timeout: 30000, //默认超时30秒
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onComplete: null, //调用完整的回调函数, 参数(XMLHttpRequest, textStatus)
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, errorThrown)
            };
            options = $.extend(defaults, options);
            if (typeof options.data != "string" && options.processData)
                options.data = JSON.stringify(options.data);
            $.ajax({
                type: (options.type == "PUT" || options.type == "PATCH" || options.type == "DELETE") ? "POST" : options.type,
                url: phAjax.baseAddress + options.path,
                dataType: options.decryptResult ? "text" : "json",
                processData: options.processData,
                contentType: options.contentType,
                cache: options.cache,
                crossDomain: true,
                timeout: options.timeout,
                beforeSend: function(XMLHttpRequest) {
                    if (!options.anonymity)
                        XMLHttpRequest.setRequestHeader(authorizationHeaderName, formatComplexAuthorization(getUserName(), getUserKey()));
                    XMLHttpRequest.setRequestHeader(methodOverrideHeaderName, options.type);
                },
                data: options.encryptData ? phAjax.encrypt(options.data) : options.data,
                success: function(result) {
                    if (typeof options.onSuccess == "function") {
                        options.onSuccess(options.decryptResult ? phAjax.decrypt(result) : result);
                        options.onSuccess = null;
                    }
                },
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status == 200) {
                        if (typeof options.onSuccess == "function") {
                            options.onSuccess(options.decryptResult ? phAjax.decrypt(XMLHttpRequest.responseText) : XMLHttpRequest.responseText);
                            options.onSuccess = null;
                        }
                    }
                    else if (XMLHttpRequest.status >= 400) {
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
var phUtils = (function() {
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
                return CryptoJS.enc.Base64.stringify(result.ciphertext);
            } else if (typeof (data) == "object") {
                data = JSON.stringify(data);
                result = CryptoJS.AES.encrypt(data, key, { iv: key, mode: CryptoJS.mode.CBC });
                return CryptoJS.enc.Base64.stringify(result.ciphertext);
            }
            return null;
        },
        
        decrypt: function(cipherText, key) {
            if (typeof (key) == "string")
                key = CryptoJS.MD5(CryptoJS.enc.Utf8.parse(key));
            var result = CryptoJS.AES.decrypt(cipherText, key, { iv: key, mode: CryptoJS.mode.CBC });
            return result.toString(CryptoJS.enc.Utf8);
        },
        
        toUint8Array: function(byteStr) {
            var i = byteStr.length;
            var result = new Uint8Array(i);
            while (i--) {
                result[i] = byteStr.charCodeAt(i);
            }
            return result;
        },
    }
})();