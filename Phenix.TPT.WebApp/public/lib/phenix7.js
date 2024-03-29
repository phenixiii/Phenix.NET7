﻿/*
    Phenix Framework 7.6 for .NET 6
    Copyright © 2007, 2022 Phenixヾ Studio All rights reserved.

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
 */
;
$.support.cors = true;

var phAjax = (function($) {
    const baseAddressCookieName = 'PH-BA';
    const companyNameCookieName = 'PH-CN';
    const userNameCookieName = 'PH-UN';
    const sessionCookieName = 'PH-SS';
    const positionsCookieName = 'PH-PT';
    const myselfCookieName = 'PH-MS';
    const myselfCompanyCookieName = 'PH-MC';

    const methodOverrideHeaderName = 'X-HTTP-Method-Override';
    const authorizationHeaderName = 'PH-Authorization';

    const maxChunkSize = 64 * 1024;

    var setBaseAddress = function(value) {
        try {
            window.localStorage.removeItem(baseAddressCookieName);
            if (value != null)
                window.localStorage.setItem(baseAddressCookieName, value);
        } catch (e) {
            $.cookie(baseAddressCookieName, value, { path: '/' });
        }
    };
    var getBaseAddress = function() {
        var result;
        try {
            result = window.localStorage.getItem(baseAddressCookieName);
        } catch (e) {
            result = $.cookie(baseAddressCookieName);
        }
        return result != null ? result : window.location.origin.replace(window.location.port, 5000);
    };

    var setCompanyName = function(value) {
        try {
            window.localStorage.removeItem(companyNameCookieName);
            if (value != null)
                window.localStorage.setItem(companyNameCookieName, value);
        } catch (e) {
            $.cookie(companyNameCookieName, value, { path: '/' });
        }
    };
    var getCompanyName = function() {
        var result;
        try {
            result = window.localStorage.getItem(companyNameCookieName);
        } catch (e) {
            result = $.cookie(companyNameCookieName);
        }
        return result != null ? result : '';
    };

    var setUserName = function(value) {
        try {
            window.localStorage.removeItem(userNameCookieName);
            if (value != null)
                window.localStorage.setItem(userNameCookieName, value);
        } catch (e) {
            $.cookie(userNameCookieName, value, { path: '/' });
        }
    };
    var getUserName = function() {
        var result;
        try {
            result = window.localStorage.getItem(userNameCookieName);
        } catch (e) {
            result = $.cookie(userNameCookieName);
        }
        return result != null ? result : '';
    };

    var setSession = function(value) {
        try {
            window.localStorage.removeItem(sessionCookieName);
            if (value != null)
                window.localStorage.setItem(sessionCookieName, value);
        } catch (e) {
            $.cookie(sessionCookieName, value, { path: '/' });
        }
    };
    var getSession = function(password) {
        var result;
        if (password != null) {
            result = phUtils.encrypt(phUtils.random(9) + new Date().toISOString(), CryptoJS.MD5(password).toString().toUpperCase());
            setSession(result);
            return result;
        }
        try {
            result = window.localStorage.getItem(sessionCookieName);
        } catch (e) {
            result = $.cookie(sessionCookieName);
        }
        return result != null ? result : '';
    };
    
    // 身份验证token: [公司名],[登录名],[会话签名]
    var initializeComplexAuthorization = function(companyName, userName, password) {
        return encodeURIComponent(companyName) + ',' + encodeURIComponent(userName) + ',' +
            (password != null ? getSession(password) : phUtils.encrypt(phUtils.random(9) + new Date().toISOString(), getSession(null)));
    };

    // 身份验证token: [公司名],[登录名],[会话签名]
    var formatComplexAuthorization = function() {
        return initializeComplexAuthorization(getCompanyName(), getUserName(), null);
    };

    var setPositions = function(value) {
        try {
            window.localStorage.removeItem(positionsCookieName);
            if (value != null)
                window.localStorage.setItem(positionsCookieName, JSON.stringify(value));
        } catch (e) {
            $.cookie(positionsCookieName, value != null ? JSON.stringify(value) : null, { path: '/' });
        }
    };
    var getPositions = function() {
        var result;
        try {
            result = window.localStorage.getItem(positionsCookieName);
        } catch (e) {
            result = $.cookie(positionsCookieName);
        }
        return result != null ? JSON.parse(result) : null;
    };

    var setMyself = function(value) {
        try {
            window.localStorage.removeItem(myselfCookieName);
            if (value != null)
                window.localStorage.setItem(myselfCookieName, JSON.stringify(value));
        } catch (e) {
            $.cookie(myselfCookieName, value != null ? JSON.stringify(value) : null, { path: '/' });
        }
    };
    var getMyself = function() {
        var result;
        try {
            result = window.localStorage.getItem(myselfCookieName);
        } catch (e) {
            result = $.cookie(myselfCookieName);
        }
        return result != null ? JSON.parse(result) : null;
    };

    var setMyselfCompany = function(value) {
        try {
            window.localStorage.removeItem(myselfCompanyCookieName);
            if (value != null)
                window.localStorage.setItem(myselfCompanyCookieName, JSON.stringify(value));
        } catch (e) {
            $.cookie(myselfCompanyCookieName, value != null ? JSON.stringify(value) : null, { path: '/' });
        }
    };
    var getMyselfCompany = function() {
        var result;
        try {
            result = window.localStorage.getItem(myselfCompanyCookieName);
        } catch (e) {
            result = $.cookie(myselfCompanyCookieName);
        }
        return result != null ? JSON.parse(result) : null;
    };

    return {
        set baseAddress(value) {
            setBaseAddress(value);
        },

        get baseAddress() {
            return getBaseAddress();
        },

        get companyName() {
            return getCompanyName();
        },

        get userName() {
            return getUserName();
        },

        get userKey() {
            return getCompanyName() + '\u0004' + getUserName();
        },

        encrypt: function(data) {
            return phUtils.encrypt(data, getSession(null));
        },

        decrypt: function(cipherText) {
            return phUtils.decrypt(cipherText, getSession(null));
        },

        // 注册(初始口令即登录名)
        register: function(options) {
            var defaults = {
                baseAddress: phAjax.baseAddress, //"http://localhost:5000"
                companyName: '', //公司名
                userName: '', //登录名
                hashName: false, //Hash登录名
                phone: '', //手机
                eMail: '', //邮箱
                regAlias: '', //注册昵称
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的消息
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            if (options.hashName)
                options.userName = CryptoJS.MD5(options.userName).toString().toUpperCase();
            setSession(null);
            setMyself(null);
            setMyselfCompany(null);
            setPositions(null);
            setBaseAddress(options.baseAddress);
            phAjax.call({
                anonymity: true,
                type: 'PUT',
                path: '/api/security/myself',
                pathParam: { companyName: options.companyName, userName: options.userName, phone: options.phone, eMail: options.eMail, regAlias: options.regAlias },
                onSuccess: function(result) {
                    setCompanyName(options.companyName);
                    setUserName(options.userName);
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                },
                onError: options.onError,
            });
        },

        // 登记(获取动态口令)
        checkIn: function(options) {
            var defaults = {
                baseAddress: phAjax.baseAddress, //"http://localhost:5000"
                companyName: '', //公司名
                userName: '', //登录名
                hashName: false, //Hash登录名
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的消息
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus)
            };
            options = $.extend(defaults, options);
            if (options.hashName)
                options.userName = CryptoJS.MD5(options.userName).toString().toUpperCase();
            setSession(null);
            setMyself(null);
            setMyselfCompany(null);
            setPositions(null);
            $.ajax({
                type: 'GET',
                url: options.baseAddress + phUtils.addUrlParam('/api/security/gate', { companyName: options.companyName, userName: options.userName }),
                contentType: 'application/json;charset=utf-8',
                cache: false,
                crossDomain: true,
                timeout: 30000,
                data: null,
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status === 200) {
                        setBaseAddress(options.baseAddress);
                        setCompanyName(options.companyName);
                        setUserName(options.userName);
                        if (typeof options.onSuccess === 'function')
                            options.onSuccess(XMLHttpRequest.responseText);
                    } else {
                        if (typeof options.onError === 'function')
                            options.onError(XMLHttpRequest, textStatus);
                    }
                },
            });
        },

        // 登录
        logon: function(options) {
            var defaults = {
                baseAddress: null, //服务地址
                companyName: null, //公司名
                userName: null, //登录名
                hashName: false, //Hash登录名
                password: null, //登录口令/动态口令
                tag: new Date().toISOString(), //捎带数据(默认是客户端时间也可以是修改的新密码)
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的消息
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus)
            };
            options = $.extend(defaults, options);
            if (options.baseAddress == null)
                options.baseAddress = phAjax.baseAddress;
            if (options.companyName == null)
                options.companyName = phAjax.companyName;
            if (options.userName == null)
                options.userName = phAjax.userName;
            else if (options.hashName)
                options.userName = CryptoJS.MD5(options.userName).toString().toUpperCase();
            setSession(null);
            setMyself(null);
            setMyselfCompany(null);
            setPositions(null);
            $.ajax({
                type: 'PUT',
                url: options.baseAddress + '/api/security/gate',
                contentType: 'application/json;charset=utf-8',
                cache: false,
                crossDomain: true,
                timeout: 30000,
                beforeSend: function(XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader(authorizationHeaderName, initializeComplexAuthorization(options.companyName, options.userName, options.password));
                },
                data: phUtils.encrypt(options.tag, CryptoJS.MD5(options.password).toString().toUpperCase()),
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status === 200) {
                        setBaseAddress(options.baseAddress);
                        setCompanyName(options.companyName);
                        setUserName(options.userName);
                        phAjax.getMyself({ reset: true });
                        if (typeof options.onSuccess === 'function')
                            options.onSuccess(XMLHttpRequest.responseText);
                    } else {
                        if (typeof options.onError === 'function')
                            options.onError(XMLHttpRequest, textStatus);
                    }
                },
            });
        },
        
        // 修改登录口令
        changePassword: function(options) {
            var defaults = {
                baseAddress: null, //服务地址
                companyName: null, //公司名
                userName: null, //登录名
                hashName: false, //Hash登录名
                password: null, //登录口令/动态口令
                newPassword: null, //新登录口令
                onSuccess: null, //调用成功的回调函数
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus)
            };
            options = $.extend(defaults, options);
            phAjax.logon({
                baseAddress: options.baseAddress,
                companyName: options.companyName,
                userName: options.userName,
                hashName: options.hashName,
                password: options.password,
                tag: { newPassword: options.newPassword },
                onSuccess: options.onSuccess,
                onError: options.onError,
            });
        },

        // 登出
        logout: function(options) {
            var defaults = {
                cache: false, //默认不缓存
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: 'DELETE',
                path: '/api/security/gate',
                onComplete: function(XMLHttpRequest, textStatus) {
                    if (!options.cache) {
                        setCompanyName(null);
                        setUserName(null);
                    }
                    setSession(null);
                    setMyself(null);
                    setMyselfCompany(null);
                    setPositions(null);
                },
            });
        },

        // 获取岗位资料
        getPositions: function(options) {
            var defaults = {
                reset: false, //是否重新获取
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的Position对象的key-value数组
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            if (!options.reset) {
                var result = getPositions();
                if (result != null) {
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                    return result;
                }
            };
            phAjax.call({
                path: '/api/security/position/all',
                onSuccess: function(result) {
                    var positions = {};
                    result.forEach((item, index) => {
                        positions[item.Id] = item;
                    });
                    setPositions(positions);
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(positions);
                },
                onError: options.onError,
            });
        },

        // 获取岗位资料
        getPosition: function(options) {
            var defaults = {
                positionId: null, //默认为Myself岗位ID
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的Position对象
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            if (options.positionId == null) {
                var myself = phAjax.getMyself();
                if (myself != null) {
                    options.positionId = myself.PositionId;
                    if (options.positionId == null)
                        return null;
                } else
                    return null;
            }
            var positions = phAjax.getPositions({
                onSuccess: function(result) {
                    if (typeof options.onSuccess === 'function') {
                        options.onSuccess(result != null && Object.prototype.hasOwnProperty.call(result, options.positionId) ? result[options.positionId] : null);
                        options.onSuccess = null;
                    }
                },
                onError: options.onError,
            });
            if (positions != null) {
                var result = positions != null && Object.prototype.hasOwnProperty.call(positions, options.positionId) ? positions[options.positionId] : null;
                if (typeof options.onSuccess === 'function') {
                    options.onSuccess(result);
                    options.onSuccess = null;
                }
                return result;
            }
            return null;
        },

        isInRole: function(role, positionId) {
            if (role == null || role === '')
                return true;

            if (positionId == null) {
                var myself = phAjax.getMyself();
                if (myself != null) {
                    positionId = myself.PositionId;
                    if (positionId == null)
                        return true;
                } else
                    return false;
            }

            var positions = phAjax.getPositions();
            if (positions != null && Object.prototype.hasOwnProperty.call(positions, positionId)) {
                var position = positions[positionId];
                return position.Roles != null && position.Roles.includes(role);
            }
            return false;
        },

        // 获取自己资料
        getMyself: function(options) {
            var defaults = {
                reset: false, //是否重新获取
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的User对象
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            if (!options.reset) {
                var result = getMyself();
                if (result != null) {
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                    return result;
                }
            }
            phAjax.getPositions({ reset: options.reset });
            phAjax.getMyselfCompany({ reset: options.reset });
            phAjax.call({
                path: '/api/security/myself',
                decryptResult: true,
                onSuccess: function(result) {
                    setMyself(result);
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                },
                onError: options.onError,
            });
        },

        // 更新自己资料
        patchMyself: function(options) {
            var defaults = {
                phone: null, //手机
                eMail: null, //邮箱
                regAlias: null, //注册昵称
                onSuccess: null, //调用成功的回调函数
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: 'PATCH',
                path: '/api/security/myself',
                data: { Phone: options.phone, EMail: options.eMail, RegAlias: options.regAlias },
                trimDataEmptyProperty: true,
                encryptData: true,
                onSuccess: function(result) {
                    phAjax.getMyself({ reset: true });
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                },
                onError: options.onError,
            });
        },

        // 获取自己公司资料
        getMyselfCompany: function(options) {
            var defaults = {
                reset: false, //是否重新获取
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的Teams对象树
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            if (!options.reset) {
                var result = getMyselfCompany();
                if (result != null) {
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                    return result;
                }
            }
            phAjax.call({
                path: '/api/security/myself/company',
                onSuccess: function(result) {
                    setMyselfCompany(result);
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                },
                onError: options.onError,
            });
        },

        // 获取自己公司用户资料
        getMyselfCompanyUsers: function(options) {
            var defaults = {
                includeDisabled: false, //默认不包含已注销的
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的User对象集合
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.getPositions();
            phAjax.getMyselfCompany();
            phAjax.call({
                path: '/api/security/myself/company-user/all',
                pathParam: { includeDisabled: options.includeDisabled },
                decryptResult: true,
                onSuccess: options.onSuccess,
                onError: options.onError,
            });
        },

        // 获取64位序号
        getSequence: function(options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的64位序号
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: '/api/data/sequence',
                onSuccess: options.onSuccess,
                onError: options.onError,
            });
        },

        // 获取64位增量值
        // key: 键
        // initialValue: 初值
        getIncrement: function(key, initialValue, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的64位增量值
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: '/api/data/increment',
                pathParam: { key: key, initialValue: initialValue },
                onSuccess: options.onSuccess,
                onError: options.onError,
            });
        },

        // 发送消息
        // receiver: 接收用户
        // content: 消息内容
        sendMessage: function(receiver, content, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: 'POST',
                path: '/api/message/user-message',
                pathParam: { receiver: receiver },
                data: content,
                onSuccess: options.onSuccess,
                onError: options.onError,
            });
        },

        // 接收消息（PULL）
        receiveMessage: function(options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数, 参数(messages)为消息id(key)+content(array[key])数据字典集合
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.call({
                path: '/api/message/user-message',
                onSuccess: options.onSuccess,
                onError: options.onError,
            });
        },

        // 确认收到
        // id: 消息ID
        // burn: 是否销毁
        affirmReceivedMessage: function(id, burn, options) {
            var defaults = {
                onSuccess: null, //调用成功的回调函数
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.call({
                type: 'DELETE',
                path: '/api/message/user-message',
                pathParam: { id: id, burn: burn },
                onSuccess: options.onSuccess,
                onError: options.onError,
            });
        },

        // 订阅消息（PUSH）
        subscribeMessage: function(options) {
            var defaults = {
                groupName: null, //组名, 如为空是订阅UserMessage消息, 否则按组名订阅GroupMessage消息
                onReceived: null, //处理收到消息, 参数(messages)为消息id(key)+content(array[key])数据字典集合
                onThen: null, //连接成功的回调函数, 参数(connection)
                onFail: null, //连接失败的回调函数, 参数(connection, error)
                onReconnecting: null, //重新连接中的回调函数, 参数(connection, error)
                onReconnected: null, //重新连接好的回调函数, 参数(connection, connectionId)
                onClose: null, //连接关闭的回调函数, 参数(connection, error)
            };
            options = $.extend(defaults, options);
            var connection = new signalR.HubConnectionBuilder()
                .withUrl(phAjax.baseAddress + (options.groupName == null ? '/api/message/user-message-hub' : '/api/message/group-message-hub'),
                    {
                        accessTokenFactory: function() {
                            return formatComplexAuthorization();
                        }
                    })
                .configureLogging('error')
                .withAutomaticReconnect()
                .build();
            connection.on('onReceived',
                function(messages) {
                    if (typeof options.onReceived === 'function')
                        options.onReceived(options.groupName == null ? JSON.parse(messages) : messages);
                });
            connection.onreconnecting(function(error) {
                if (typeof options.onReconnecting === 'function')
                    options.onReconnecting(connection, error);
            });
            connection.onreconnected(function(connectionId) {
                if (options.groupName != null)
                    connection.invoke('SubscribeAsync', options.groupName);
                if (typeof options.onReconnected === 'function')
                    options.onReconnected(connection, connectionId);
            });
            connection.onclose(function(error) {
                if (typeof options.onClose === 'function')
                    options.onClose(connection, error);
            });
            connection.start()
                .catch(function(error) {
                    if (typeof options.onFail === 'function')
                        options.onFail(connection, error);
                }).then(function() {
                    if (options.groupName != null)
                        connection.invoke('SubscribeAsync', options.groupName);
                    if (typeof options.onThen === 'function')
                        options.onThen(connection);
                });
        },

        // 下载文件
        downloadFile: function(options) {
            var defaults = {
                path: '/api/inout/file', //路径
                message: null, //上传消息
                fileName: null, //下载文件名
                onProgress: null, //执行进度的回调函数, 参数(fileName, chunkCount, chunkNumber, chunkSize, chunkBody, chunkBuffer)，函数调用返回值如为false则中止下载
                onSuccess: null, //调用成功的回调函数, 参数(fileName, fileBlob)
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            phAjax.downloadFileChunk(options.path, options.message, options.fileName, 1, null, options.onProgress, options.onSuccess, options.onError);
        },

        downloadFileChunk: function(path, message, fileName, chunkNumber, chunkBuffer, onProgress, onSuccess, onError) {
            phAjax.call({
                path: path,
                pathParam: { message: message, fileName: fileName, chunkNumber: chunkNumber },
                onSuccess: function(result) {
                    if (result == null)
                        return;
                    result.ChunkBody = $.base64.atob(result.ChunkBody);
                    chunkBuffer = chunkBuffer == null ? result.ChunkBody : chunkBuffer.concat(result.ChunkBody);
                    if (typeof onProgress === 'function') {
                        var goon = onProgress(result.FileName, result.ChunkCount, result.ChunkNumber, result.ChunkSize, result.ChunkBody, chunkBuffer);
                        if (typeof goon === 'boolean' && !goon)
                            return;
                    }
                    if (result.ChunkNumber >= result.ChunkCount) {
                        if (typeof onSuccess === 'function')
                            onSuccess(result.FileName, new Blob([phUtils.toUint8Array(chunkBuffer)]));
                        return;
                    }
                    phAjax.downloadFileChunk(path, message, fileName, chunkNumber + 1, chunkBuffer, onProgress, onSuccess, onError);
                },
                onError: onError,
            });
        },

        // 上传文件
        uploadFiles: function(options) {
            var defaults = {
                path: '/api/inout/file', //路径
                message: null, //上传消息
                files: null, //上传文件(须是FileList/File对象(如果APP应用是本地图片，要么转成base64->File对象，要么转成网络图片->base64->File对象))
                onProgress: null, //执行进度的回调函数, 参数(fileName, chunkCount, chunkNumber, chunkSize)，回调函数返回值如为false则中止上传
                onSuccess: null, //调用成功的回调函数, 参数(message)为完成上传时返回消息
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
            };
            options = $.extend(defaults, options);
            if (options.files != null)
                if (options.files instanceof FileList) {
                    for (var i = 0; i < options.files.length; i++) {
                        var file = options.files[i];
                        phAjax.uploadFileChunk(options.path, options.message, file, 1, options.onProgress, options.onSuccess, options.onError);
                    }
                } else
                    phAjax.uploadFileChunk(options.path, options.message, options.files, 1, options.onProgress, options.onSuccess, options.onError);
        },

        uploadFileChunk: function(path, message, file, chunkNumber, onProgress, onSuccess, onError) {
            var chunkCount = Math.ceil(file.size / maxChunkSize);
            var chunkSize = chunkNumber > 0 ? chunkNumber < chunkCount ? maxChunkSize : file.size - maxChunkSize * (chunkCount - 1) : 0;
            var formData = new FormData();
            if (chunkNumber > 0) {
                var p = maxChunkSize * (chunkNumber - 1);
                formData.append('chunkBody', file.slice(p, p + chunkSize), file.name);
            }
            phAjax.call({
                type: 'PUT',
                path: path,
                pathParam: { message: message, fileName: file.name, chunkCount: chunkCount, chunkNumber: chunkNumber, chunkSize: chunkSize, maxChunkSize: maxChunkSize },
                processData: false, //不要对data参数进行序列化处理
                contentType: false, //不要设置Content-Type请求头，因为文件数据是以multipart/form-data来编码
                data: formData,
                onSuccess: function(result) {
                    if (chunkNumber <= 0)
                        return;
                    if (typeof onProgress === 'function')
                        if (!onProgress(file.name, chunkCount, chunkNumber, chunkSize)) {
                            phAjax.uploadFileChunk(path, message, file, 0, onProgress, onSuccess, onError);
                            return;
                        }
                    if (chunkNumber >= chunkCount) {
                        if (typeof onSuccess === 'function')
                            onSuccess(result);
                        return;
                    }
                    phAjax.uploadFileChunk(path, message, file, chunkNumber + 1, onProgress, onSuccess, onError);
                },
                onError: onError,
            });
        },

        // 呼叫
        call: function(options) {
            var defaults = {
                anonymity: false, //是否匿名访问
                type: 'GET', //HttpMethod(GET/POST/PUT/PATCH/DELETE)
                path: null, //路径
                pathParam: null, //URL参数
                data: null, //上传数据
                trimDataEmptyProperty: false, //默认不清理data的空属性值
                trimDataLocalProperty: true, //默认清理data的本地添加(小驼峰法命名)属性值
                processData: true, //默认对data参数进行序列化处理
                encryptData: false, //默认不加密data（否则服务端请用Request.ReadBodyAsync(true)解密）
                decryptResult: false, //默认不解密result（否则服务端请用this.EncryptAsync(result)加密, 下载经解密后可在onSuccess事件里用JSON.parse(result)还原为JavaScript对象）
                contentType: 'application/json;charset=utf-8', 
                cache: false, //默认不缓存
                timeout: 30000, //默认超时30秒
                async: true, //默认异步
                onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
                onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
                onComplete: null, //调用完成的回调函数, 参数(XMLHttpRequest, textStatus)
            };
            options = $.extend(defaults, options);
            if (options.pathParam !== null)
                options.path = phUtils.addUrlParam(options.path, options.pathParam);
            if (options.processData && options.data != null && typeof options.data === 'object') {
                if (options.trimDataEmptyProperty || options.trimDataLocalProperty)
                    options.data = phUtils.trimData(options.data, options.trimDataEmptyProperty, options.trimDataLocalProperty);
                options.data = JSON.stringify(options.data);
            }
            $.ajax({
                type: (options.type === 'PUT' || options.type === 'PATCH' || options.type === 'DELETE') ? 'POST' : options.type,
                url: phAjax.baseAddress + options.path,
                dataType: options.decryptResult ? 'text' : 'json',
                processData: options.processData,
                contentType: options.contentType,
                cache: options.cache,
                crossDomain: true,
                timeout: options.timeout,
                async: options.async,
                beforeSend: function(XMLHttpRequest) {
                    if (!options.anonymity)
                        XMLHttpRequest.setRequestHeader(authorizationHeaderName, formatComplexAuthorization());
                    XMLHttpRequest.setRequestHeader(methodOverrideHeaderName, options.type);
                },
                data: options.encryptData ? phAjax.encrypt(options.data) : options.data,
                success: function(result) {
                    if (typeof options.onSuccess === 'function') {
                        if (options.decryptResult)
                            try {
                                result = phAjax.decrypt(result);
                            } catch (e) {
                                options.onSuccess(result);
                                options.onSuccess = null;
                                return;
                            }
                        var obj;
                        try {
                            obj = JSON.parse(result);
                        } catch (e) {
                            options.onSuccess(result);
                            options.onSuccess = null;
                            return;
                        }
                        options.onSuccess(obj);
                        options.onSuccess = null;
                    }
                },
                complete: function(XMLHttpRequest, textStatus) {
                    if (XMLHttpRequest.status === 200) {
                        if (typeof options.onSuccess === 'function') {
                            var result = XMLHttpRequest.responseText;
                            if (options.decryptResult)
                                try {
                                    result = phAjax.decrypt(result);
                                } catch (e) {
                                    options.onSuccess(result);
                                    options.onSuccess = null;
                                    return;
                                }
                            var obj;
                            try {
                                obj = JSON.parse(result);
                            } catch (e) {
                                options.onSuccess(result);
                                options.onSuccess = null;
                                return;
                            }
                            options.onSuccess(obj);
                            options.onSuccess = null;
                        }
                    } else if (XMLHttpRequest.status >= 400) {
                        if (XMLHttpRequest.status === 409) {
                            if (typeof options.onError === 'function')
                                options.onError(XMLHttpRequest, textStatus, JSON.parse(XMLHttpRequest.responseText));
                        } else {
                            if (typeof options.onError === 'function')
                                options.onError(XMLHttpRequest, textStatus);
                        }
                    }
                    if (typeof options.onComplete === 'function')
                        options.onComplete(XMLHttpRequest, textStatus);
                }
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
            if (result.length >= length)
                return result.substr(0, length);
            result += phUtils.random(length - result.length);
            return result;
        },

        encrypt: function(data, key) {
            var result;
            if (typeof key === 'string')
                key = CryptoJS.MD5(CryptoJS.enc.Utf8.parse(key));
            if (typeof data === 'string') {
                result = CryptoJS.AES.encrypt(data, key, { iv: key, mode: CryptoJS.mode.CBC });
                return CryptoJS.enc.Base64.stringify(result.ciphertext);
            } else if (typeof data === 'object') {
                data = JSON.stringify(data);
                result = CryptoJS.AES.encrypt(data, key, { iv: key, mode: CryptoJS.mode.CBC });
                return CryptoJS.enc.Base64.stringify(result.ciphertext);
            }
            return null;
        },

        decrypt: function(cipherText, key) {
            if (typeof key === 'string')
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

        toUrlParam: function(data) {
            if (data != null && typeof data === 'object') {
                var result = '';
                for (let p in data)
                    if (Object.prototype.hasOwnProperty.call(data, p))
                        result += '&' + p[0].toLowerCase() + p.substring(1) + '=' + encodeURIComponent(data[p] ?? '');
                return result === '' ? result : result.substring(1);
            }
            return data;
        },

        addUrlParam: function(url, data) {
            var param = phUtils.toUrlParam(data);
            return param != null && param !== '' ? url + (url.indexOf('?') < 0 ? '?' : '') + param : url;
        },

        trimData: function(data, trimDataEmptyProperty, trimDataLocalProperty) {
            if (data != null && typeof data === 'object')
                if (data instanceof Array) {
                    return phUtils.trimArrayData(data, trimDataEmptyProperty, trimDataLocalProperty);
                } else {
                    var result = {};
                    for (let p in data)
                        if (Object.prototype.hasOwnProperty.call(data, p)) {
                            if (trimDataLocalProperty && !/^[A-Z]+$/.test(p[0]))
                                continue;
                            var value = data[p];
                            if (trimDataEmptyProperty && (value == null || value === ''))
                                continue;
                            result[p] = typeof value === 'object'
                                ? (value instanceof Array
                                    ? phUtils.trimArrayData(value, trimDataEmptyProperty, trimDataLocalProperty)
                                    : phUtils.trimData(value, trimDataEmptyProperty, trimDataLocalProperty))
                                : value;
                        }
                    return result;
                }
            return data;
        },

        trimArrayData: function(array, trimDataEmptyProperty, trimDataLocalProperty) {
            if (array != null && typeof array === 'object')
                if (array instanceof Array) {
                    var result = [];
                    array.forEach((item, index) => {
                        if (item != null && item !== '')
                            result.push(typeof item === 'object'
                                ? (item instanceof Array
                                    ? phUtils.trimArrayData(item, trimDataEmptyProperty, trimDataLocalProperty)
                                    : phUtils.trimData(item, trimDataEmptyProperty, trimDataLocalProperty))
                                : item
                            );
                    });
                    return result;
                } else
                    return phUtils.trimData(array, trimDataEmptyProperty, trimDataLocalProperty);
            return array;
        }
    }
})();