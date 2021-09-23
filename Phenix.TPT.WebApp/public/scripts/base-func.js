$(function() {
    if (phAjax.userName === '')
        gotoLogin();
    else {
        var menu = document.getElementById('menu');
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
            '        <i id="schedule" class="fa fa-sliders fa-2x"></i>',
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
                else if (result.Position.Roles.indexOf('经营管理') === 0)
                    gotoAccount();
                else {
                    document.getElementById('account').style.color = 'gray';
                    if (result.Position.Roles.indexOf('项目管理') === 0)
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
        if (myself.Position == null || myself.Position.Roles.indexOf('经营管理') === 0)
            window.location.href = 'account.html';
    }
}

function gotoStatistic() {
    if (window.location.href.indexOf('statistic.html') === -1)
        window.location.href = 'statistic.html';
}

function gotoMyself() {
    if (window.location.href.indexOf('myself.html') === -1)
        window.location.href = "myself.html"
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

function getcolor(work) {
    var color = 'white';
    switch (work) {
        case 'manage':
            color = 'rgb(128, 100, 162)';
            break;
        case 'investigate':
            color = 'rgb(0, 112, 192)';
            break;
        case 'develop':
            color = 'rgb(247, 150, 70)';
            break;
        case 'test':
            color = 'rgb(255, 255, 0)';
            break;
        case 'implement':
            color = 'rgb(0, 176, 240)';
            break;
        case 'maintenance':
            color = 'rgb(146, 208, 80)';
            break;
    }
    return color;
}