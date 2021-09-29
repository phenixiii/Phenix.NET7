$(function() {
    if (phAjax.userName === '')
        gotoLogin();
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
            '    <li title="项目管理" onclick="gotoIndex()">',
            '        <i id="index" class="fa fa-cog fa-2x"></i>',
            '    </li>',
            '    <li title="工作档期" onclick="gotoSchedule()">',
            '        <i id="schedule" class="fa fa-tasks fa-2x"></i>',
            '    </li>',
            '    <li title="月工作量" onclick="gotoWorkload()">',
            '        <i id="workload" class="fa fa-calendar fa-2x"></i>',
            '    </li>',
            '    <li title="收支管理" onclick="gotoAccount()">',
            '        <i id="account" class="fa fa-jpy fa-2x"></i>',
            '    </li>',
            '    <li title="数据分析" onclick="gotoStatistic()">',
            '        <i id="statistic" class="fa fa-bar-chart fa-2x"></i>',
            '    </li>',
            '    <li title="修改密码" onclick="gotoMyself()">',
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
            window.phAjax.logout();
            gotoLogin();
        });

        phAjax.getMyself({
            onSuccess: function(result) {
                $('#userName').html('您好，' + (result.RegAlias ?? result.Name));
                if (result.Position == null)
                    gotoIndex();
                else if (result.Position.Roles.indexOf(projectRoles.经营管理) >= 0)
                    gotoAccount();
                else {
                    document.getElementById('account').style.color = 'gray';
                    if (result.Position.Roles.indexOf(projectRoles.项目管理) >= 0)
                        gotoIndex();
                    else
                        gotoWorkload();
                }
            },
            onError: function(XMLHttpRequest, textStatus) {
                zdalert('获取个人资料失败',
                    XMLHttpRequest.responseText,
                    function(result) {
                        gotoLogin();
                    });
            },
        });
    }
})

var projectRoles = {
    经营管理: "经营管理",
    项目管理: "项目管理",
    调研分析: "调研分析",
    设计开发: "设计开发",
    测试联调: "测试联调",
    培训实施: "培训实施",
    质保维保: "质保维保",
    方案设计: "方案设计",
    采购施工: "采购施工",
    交付验收: "交付验收",
}

function gotoLogin() {
    if (window.location.href.indexOf('login.html') === -1)
        window.location.href = 'login.html';
}

function gotoIndex() {
    if (window.location.href.indexOf('index.html') === -1)
        window.location.href = 'index.html';
}

function gotoSchedule() {
    if (window.location.href.indexOf('schedule.html') === -1)
        window.location.href = 'schedule.html';
}

function gotoWorkload() {
    if (window.location.href.indexOf('workload.html') === -1)
        window.location.href = 'workload.html';
}

function gotoAccount() {
    if (window.location.href.indexOf('account.html') === -1) {
        var myself = phAjax.getMyself();
        if (myself.Position == null || myself.Position.Roles.indexOf(projectRoles.经营管理) >= 0)
            window.location.href = 'account.html';
    }
}

function gotoStatistic() {
    if (window.location.href.indexOf('statistic.html') === -1)
        window.location.href = 'statistic.html';
}

function gotoMyself() {
    if (window.location.href.indexOf('myself.html') === -1)
        window.location.href = 'myself.html';
}

function await() {
    var waitHold = document.getElementById('wait-hold'); // <div id="wait-hold"></div>
    if (waitHold !== null)
        if (waitHold.style.display === 'block')
            return false;
        else
            waitHold.style.display = 'block';
    return true;
}

function waitOut() {
    var waitHold = document.getElementById('wait-hold'); // <div id="wait-hold"></div>
    if (waitHold !== null)
        if (waitHold.style.display === 'none')
            return false;
        else
            waitHold.style.display = 'none';
    return true;
}

function callAjax(options) {
    var defaults = {
        anonymity: false, //是否匿名访问
        type: 'GET', //HttpMethod(GET/POST/PUT/PATCH/DELETE)
        path: null, //路径
        pathParam: null, //URL参数
        data: null, //上传数据
        trimData: false, //默认不清理data的空属性值
        processData: true, //默认对data参数进行序列化处理
        encryptData: false, //默认不加密data（否则服务端请用Request.ReadBodyAsync(true)解密）
        decryptResult: false, //默认不解密result（否则服务端请用this.EncryptAsync(result)加密, 下载经解密后可在onSuccess事件里用JSON.parse(result)还原为JavaScript对象）
        contentType: 'application/json;charset=utf-8',
        cache: false, //默认不缓存
        timeout: 30000, //默认超时30秒
        onSuccess: null, //调用成功的回调函数, 参数(result)为返回的数据
        onError: null, //调用失败的回调函数, 参数(XMLHttpRequest, textStatus, validityError), validityError为有效性错误对象{ Key, StatusCode, Hint, MessageType }
        onComplete: null, //调用完成的回调函数, 参数(XMLHttpRequest, textStatus)
    };
    options = $.extend(defaults, options);
    if (window.await())
        phAjax.call({
            anonymity: options.anonymity,
            type: options.type,
            path: options.path,
            pathParam: options.pathParam,
            data: options.data,
            trimData: options.trimData,
            processData: options.processData,
            encryptData: options.encryptData,
            decryptResult: options.decryptResult,
            contentType: options.contentType,
            cache: options.cache,
            timeout: options.timeout,
            onSuccess: function(result) {
                window.waitOut();
                if (typeof options.onSuccess === 'function')
                    options.onSuccess(result);
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                window.waitOut();
                if (typeof options.onError === 'function')
                    options.onError(XMLHttpRequest, textStatus, validityError);
            },
            onComplete: options.onComplete,
        });
}

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