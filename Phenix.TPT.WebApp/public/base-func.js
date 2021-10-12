$(function() {
    if (phAjax.userName === '')
        base.gotoLogin();
    else {
        var menu = document.getElementById('menu'); // <div id="menu"></div>
        var secMenu = document.createElement('section');
        secMenu.className = 'sec-menu';
        secMenu.innerHTML = [
            '<div class="container">',
            '    <div class="row">',
            '        <div class="col-md-12">',
            '            <span title="显示菜单">',
            '                <i class="fa fa-bars pull-left fa-2x menu-open-icon"></i>',
            '            </span>',
            '            <font face="Papyrus" size="6" color="black">Teamwork Project Tracker</font>',
            '            <i class="pull-right">',
            '                <a class="logout">退出</a>',
            '                <a id="userName">请稍等...</a>',
            '            </i>',
            '        </div>',
            '    </div>',
            '</div>',
        ].join('');
        menu.appendChild(secMenu);

        var sideMenu = document.createElement('div');
        sideMenu.id = 'side-menu';
        sideMenu.innerHTML = [
            '<ul>',
            '    <li title="关闭菜单">',
            '        <i class="fa fa-close fa-2x menu-close-icon"></i>',
            '    </li>',
            '    <li title="项目管理" onclick="base.gotoIndex()">',
            '        <i id="index" class="fa fa-cog fa-2x"></i>',
            '    </li>',
            '    <li title="工作档期" onclick="base.gotoSchedule()">',
            '        <i id="schedule" class="fa fa-tasks fa-2x"></i>',
            '    </li>',
            '    <li title="月工作量" onclick="base.gotoWorkload()">',
            '        <i id="workload" class="fa fa-calendar fa-2x"></i>',
            '    </li>',
            '    <li title="收支管理" onclick="base.gotoAccount()">',
            '        <i id="account" class="fa fa-jpy fa-2x"></i>',
            '    </li>',
            '    <li title="数据分析" onclick="base.gotoStatistic()">',
            '        <i id="statistic" class="fa fa-bar-chart fa-2x"></i>',
            '    </li>',
            '    <li title="修改密码" onclick="base.gotoMyself()">',
            '        <i id="myself" class="fa fa-key fa-2x"></i>',
            '    </li>',
            '</ul>',
        ].join('');
        menu.appendChild(sideMenu);

        $('.menu-open-icon').click(function (e) {
            e.preventDefault();
            var left = $('#side-menu').offset().left;
            if (left === -250) {
                $('#side-menu').animate({
                    left: '0px',
                    top: '0px'
                });
            } else {
                $('#side-menu').animate({
                    left: '-250px'
                });
            }
        });

        $('.menu-close-icon').click(function (e) {
            e.preventDefault();
            $('#side-menu').animate({
                left: '-250px'
            });
        });

        $('.logout').click(function (e) {
            e.preventDefault();
            phAjax.logout();
            base.gotoLogin();
        });

        phAjax.getMyself({
            onSuccess: function(result) {
                $('#userName').html('您好，' + (result.RegAlias ?? result.Name));
                if (!phAjax.isInRole('经营管理')) {
                    document.getElementById('account').style.color = 'gray';
                    if (window.location.href.indexOf('account.html') >= 0)
                        base.gotoLogin();
                }
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                alert('获取个人资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
                base.gotoLogin();
            },
        });
    }
})

var base = (function($) {
    var awaitCount = 0;

    function await() {
        awaitCount = awaitCount + 1;
        var waitHold = document.getElementById('wait-hold'); // <div id="wait-hold"></div>
        if (waitHold !== null)
            waitHold.style.display = 'block';
    }

    function waitOut() {
        awaitCount = awaitCount - 1;
        if (awaitCount === 0) {
            var waitHold = document.getElementById('wait-hold'); // <div id="wait-hold"></div>
            if (waitHold !== null)
                waitHold.style.display = 'none';
        }
    }

    return {
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
            await();
            phAjax.call({
                anonymity: options.anonymity,
                type: options.type,
                path: options.path,
                pathParam: options.pathParam,
                data: options.data,
                trimDataEmptyProperty: options.trimDataEmptyProperty,
                trimDataLocalProperty: options.trimDataLocalProperty,
                processData: options.processData,
                encryptData: options.encryptData,
                decryptResult: options.decryptResult,
                contentType: options.contentType,
                cache: options.cache,
                timeout: options.timeout,
                async: options.async,
                onSuccess: function(result) {
                    waitOut();
                    if (typeof options.onSuccess === 'function')
                        options.onSuccess(result);
                },
                onError: function(XMLHttpRequest, textStatus, validityError) {
                    waitOut();
                    if (typeof options.onError === 'function')
                        options.onError(XMLHttpRequest, textStatus, validityError);
                },
                onComplete: options.onComplete,
            });
        },

        gotoLogin: function() {
            if (window.location.href.indexOf('login.html') === -1)
                window.location.href = 'login.html';
        },

        gotoIndex: function() {
            if (window.location.href.indexOf('index.html') === -1)
                window.location.href = 'index.html';
        },

        gotoSchedule: function() {
            if (window.location.href.indexOf('schedule.html') === -1)
                window.location.href = 'schedule.html';
        },

        gotoWorkload: function() {
            if (window.location.href.indexOf('workload.html') === -1)
                window.location.href = 'workload.html';
        },

        gotoAccount: function() {
            if (window.location.href.indexOf('account.html') === -1) {
                if (phAjax.isInRole('经营管理'))
                    window.location.href = 'account.html';
            }
        },

        gotoStatistic: function() {
            if (window.location.href.indexOf('statistic.html') === -1)
                window.location.href = 'statistic.html';
        },

        gotoMyself: function() {
            if (window.location.href.indexOf('myself.html') === -1)
                window.location.href = 'myself.html';
        },
        
        isMyProject: function(projectInfo) {
            var myself = phAjax.getMyself();
            if (myself == null)
                return false;
            return phAjax.isInRole('经营管理') ||
                projectInfo == null && phAjax.isInRole('项目管理') ||
                projectInfo != null &&
                (myself.Id === projectInfo.ProjectManager ||
                    myself.Id === projectInfo.DevelopManager ||
                    myself.Id === projectInfo.MaintenanceManager ||
                    myself.Id === projectInfo.SalesManager);
        },
    }
})(jQuery);

Date.prototype.format = function(format) {
    var o = {
        'M+': this.getMonth() + 1, //month
        'd+': this.getDate(), //day
        'h+': this.getHours(), //hour
        'm+': this.getMinutes(), //minute
        's+': this.getSeconds(), //second
        'q+': Math.floor((this.getMonth() + 3) / 3), //quarter
        'S': this.getMilliseconds() //millisecond
    }
    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp('(' + k + ')').test(format))
            format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : ('00' + o[k]).substr(('' + o[k]).length));
    return format;
}