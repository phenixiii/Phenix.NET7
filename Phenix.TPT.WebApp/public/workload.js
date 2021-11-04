$(function() {
    fetchWorkday(vue.year, vue.month, vue.state, vue.pageNo);
    fetchMyselfCompanyUsers(vue.year, vue.month, vue.state, vue.pageNo);
    fetchWorkSchedule(vue.year, vue.month, vue.state, vue.pageNo, true);
    fetchProjectInfos(vue.year, vue.month, vue.state, vue.pageNo, true);
});

var tableHeadTop = null;
var tableFootHeight = null;

$(window).scroll(function() {
    if (tableHeadTop == null)
        tableHeadTop = $('#table-head').offset().top;
    if (tableFootHeight == null)
        tableFootHeight = $('#table-foot').height();
    let scrollTop = document.documentElement.scrollTop || document.body.scrollTop; //滚动条滚动时距离顶部的距离
    let windowHeight = document.documentElement.clientHeight || document.body.clientHeight; //可视区的高度
    let scrollHeight = document.documentElement.scrollHeight || document.body.scrollHeight; //滚动条的总高度
    //悬浮表头
    if (scrollTop >= tableHeadTop) { //滚动条的滑动距离大于等于表头距离浏览器顶部的距离就固定表头
        var tableHeadFirst = $('#table-head-first').width();
        $('#table-head-first').width(tableHeadFirst);
        $('#table-head').css({ 'position': 'fixed', 'top': 0 });
    } else //反之就不固定表头
        $('#table-head').css({ 'position': 'static' });
    //悬浮表尾
    if (scrollTop + windowHeight > scrollHeight - tableFootHeight) //滚动条滚动到了表尾就不固定表头
        $('#table-foot').css({ 'position': 'static' });
    else { //反之就固定表尾
        var tableFootFirst = $('#table-foot-first').width();
        $('#table-foot-first').width(tableFootFirst);
        $('#table-foot').css({ 'position': 'fixed', 'bottom': 0 });
    }
});

var changeWorkloadTarget = null;
var changeWorkloadHtml = null;

$('body').click(function(e) {
    //隐藏鼠标点出界的弹出框
    if (changeWorkloadTarget != null && !changeWorkloadHtml.includes($(e.target).html()))
        changeWorkloadTarget.popover('hide');
});

var workdays = [];
var projectInfos = null;

function fetchWorkday(year, month, state, pageNo) {
    var result = workdays.find(item => item.Year === year && item.Month === month);
    if (result != null)
        return result;
    base.call({
        path: '/api/workday',
        pathParam: { year: year, month: month },
        onSuccess: function(result) {
            if (!workdays.includes(result)) {
                workdays.push(result);
                filterProjectInfos(vue.workerProjectWorkloads, projectInfos, result, year, month, state, pageNo);
            }
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取工作日失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
    return null;
}

var myselfCompanyUsers = null;
var workSchedule = null;

function fetchMyselfCompanyUsers(year, month, state, pageNo) {
    phAjax.getMyselfCompanyUsers({
        includeDisabled: true,
        onSuccess: function(result) {
            myselfCompanyUsers = result;
            fetchWorkerProjectWorkloads(filterMyselfCompanyUsers(result, workSchedule, year, month, state), year, month, state, pageNo);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取公司员工资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function fetchWorkSchedule(year, month, state, pageNo, reset) {
    if (reset)
        workSchedule = null;
    if (workSchedule == null)
        phAjax.getMyself({
            onSuccess: function(result) {
                base.call({
                    path: '/api/work-schedule',
                    pathParam: { manager: result.Id, year: year, month: month },
                    onSuccess: function(result) {
                        workSchedule = result;
                        fetchWorkerProjectWorkloads(filterMyselfCompanyUsers(myselfCompanyUsers, result, year, month, state), year, month, state, pageNo);
                    },
                    onError: function(XMLHttpRequest, textStatus, validityError) {
                        alert('获取工作档期失败:\n' +
                            (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
                    },
                });
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                alert('获取个人资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
                base.gotoLogin();
            },
        });
    else
        fetchWorkerProjectWorkloads(filterMyselfCompanyUsers(myselfCompanyUsers, workSchedule, year, month, state), year, month, state, pageNo);
}

function filterMyselfCompanyUsers(myselfCompanyUsers, workSchedules, year, month, state) {
    if (myselfCompanyUsers == null || workSchedules == null)
        return null;
    var lastDay = new Date(year, month, 1).setMonth(month, 0); //月度最后一天
    var filteredMyselfCompanyUserPages = {};
    filteredMyselfCompanyUserPages.count = 0;
    var filteredMyselfCompanyUsers = [];
    filteredMyselfCompanyUsers.push(phAjax.getMyself()); //自己的排第一
    myselfCompanyUsers.forEach((item, index) => {
        if (filteredMyselfCompanyUsers.length >= vue.pageSize) //当前页满了就新开一页
        {
            filteredMyselfCompanyUserPages.count = filteredMyselfCompanyUserPages.count + 1;
            filteredMyselfCompanyUserPages[filteredMyselfCompanyUserPages.count] = filteredMyselfCompanyUsers;
            filteredMyselfCompanyUsers = [];
        } else if (new Date(item.RegTime) <= lastDay &&
            (!item.Disabled || item.DisabledTime == null || new Date(item.DisabledTime) > lastDay) &&
            (state !== 0 || workSchedules.Workers.includes(item.Id)))
            filteredMyselfCompanyUsers.push(item);
    });
    if (filteredMyselfCompanyUsers.length > 0) {
        filteredMyselfCompanyUserPages.count = filteredMyselfCompanyUserPages.count + 1;
        filteredMyselfCompanyUserPages[filteredMyselfCompanyUserPages.count] = filteredMyselfCompanyUsers;
    }
    vue.filteredMyselfCompanyUserPages = filteredMyselfCompanyUserPages;
    return filteredMyselfCompanyUserPages;
}

function fetchWorkerProjectWorkloads(filteredMyselfCompanyUserPages, year, month, state, pageNo) {
    if (filteredMyselfCompanyUserPages != null &&
        Object.prototype.hasOwnProperty.call(filteredMyselfCompanyUserPages, pageNo)) {
        var workers = [];
        filteredMyselfCompanyUserPages[pageNo].forEach((item, index) => workers.push(item.Id));
        base.call({
            path: '/api/project-workload/all',
            pathParam: { workers: workers.toString(), year: year, month: month },
            onSuccess: function(result) {
                vue.workerProjectWorkloads = result;
                filterProjectInfos(result, projectInfos, fetchWorkday(year, month, state, pageNo), year, month, state, pageNo);
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                alert('获取项目工作量失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
            },
        });
    }
}

function fetchProjectInfos(year, month, state, pageNo, reset) {
    if (reset)
        projectInfos = null;
    if (projectInfos == null)
        base.call({
            path: '/api/project-info/all',
            pathParam: { year: year, month: month },
            onSuccess: function(result) {
                projectInfos = result;
                filterProjectInfos(vue.workerProjectWorkloads, result, fetchWorkday(year, month, state, pageNo), year, month, state, pageNo);
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                projectInfos = null;
                alert('获取项目失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
            },
        });
    else
        filterProjectInfos(vue.workerProjectWorkloads, projectInfos, fetchWorkday(year, month, state, pageNo), year, month, state, pageNo);
}

function filterProjectInfos(workerProjectWorkloads, projectInfos, workdays, year, month, state, pageNo) {
    if (workerProjectWorkloads == null || projectInfos == null || workdays == null)
        return;
    var filteredProjectInfos = [];
    for (let worker in workerProjectWorkloads)
        if (Object.prototype.hasOwnProperty.call(workerProjectWorkloads, worker)) {
            let remainDays = workdays.Days;
            workerProjectWorkloads[worker].forEach((workerProjectWorkload, index) => {
                var projectInfo = projectInfos.find(item => item.Id === workerProjectWorkload.PiId);
                if (projectInfo != null) {
                    projectInfo[worker] = workerProjectWorkload;
                    if (!filteredProjectInfos.includes(projectInfo)) {
                        filteredProjectInfos.push(projectInfo);
                        totalProjectWorkload(projectInfo);
                    }
                    var totalWorkload =
                        workerProjectWorkload.ManageWorkload +
                        workerProjectWorkload.InvestigateWorkload +
                        workerProjectWorkload.DevelopWorkload +
                        workerProjectWorkload.TestWorkload +
                        workerProjectWorkload.ImplementWorkload +
                        workerProjectWorkload.MaintenanceWorkload;
                    workerProjectWorkload.totalWorkload = totalWorkload;
                    remainDays = remainDays - totalWorkload;
                }
            });
            workerProjectWorkloads[worker].remainDays = remainDays;
        }
    vue.filteredProjectInfos = filteredProjectInfos.sort(function(a, b) {
        return a.ProjectName.localeCompare(b.ProjectName);
    });
    vue.year = year;
    vue.month = month;
    vue.state = state;
    vue.pageNo = pageNo;
}

function totalProjectWorkload(projectInfo) {
    if (projectInfo.totalWorkload != null)
        return;
    base.call({
        path: '/api/project-workload/total',
        pathParam: { projectId: projectInfo.Id },
        onSuccess: function(result) {
            Vue.set(projectInfo, 'totalWorkload', result);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('汇总项目工作量失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function fetchAll(year, month, state, pageNo, reset) {
    if (pageNo < 1) {
        alert('空山不见人~');
        return;
    }
    if (pageNo > vue.filteredMyselfCompanyUserPages.count) {
        alert('云深不知处~');
        return;
    }
    if (reset)
        fetchWorkday(year, month, state, pageNo);
    fetchWorkSchedule(year, month, state, pageNo, reset);
    fetchProjectInfos(year, month, state, pageNo, reset);
}

var vue = new Vue({
    el: '#content',
    data: {
        year: new Date().getDate() > 5
            ? new Date().getFullYear()
            : new Date(new Date().setMonth(new Date().getMonth() - 1, 1)).getFullYear(),
        month: new Date().getDate() > 5
            ? new Date().getMonth() + 1
            : new Date(new Date().setMonth(new Date().getMonth() - 1, 1)).getMonth() + 1,
        state: 0,
        stateTitle: [
            '当月项目的工作量',
            '当月全员的工作量',
        ],
        filterStateTitle: [
            '显示当月全员的工作量',
            '显示当月项目的工作量',
        ],
        pageNo: 1,
        pageSize: 15,

        filteredMyselfCompanyUserPages: {},
        workerProjectWorkloads: null,
        filteredProjectInfos: [],
    },
    methods: {
        parseUserName: function(id) {
            if (myselfCompanyUsers != null) {
                var user = myselfCompanyUsers.find(item => item.Id === id);
                if (user != null)
                    return user.RegAlias;
            }
            return null;
        },

        onPlusMonth: function() {
            var now = new Date();
            var year = this.year;
            var month = this.month;
            if (year === now.getFullYear() &&
                month === now.getMonth() + 1) {
                alert('物来顺应，未来不迎~');
                return;
            }
            if (month === 12) {
                year = year + 1;
                month = 1;
            } else
                month = month + 1;
            fetchAll(year, month, this.state, 1, true);
        },

        onMinusMonth: function() {
            var year = this.year;
            var month = this.month;
            if (month === 1) {
                year = year - 1;
                month = 12;
            } else
                month = month - 1;
            fetchAll(year, month, this.state, 1, true);
        },

        onPlusYear: function() {
            var now = new Date();
            var year = this.year;
            var month = this.month;
            if (year === now.getFullYear()) {
                alert('珍惜当下，安然即好~');
                return;
            }
            if (year === now.getFullYear() - 1 &&
                month > now.getMonth() + 1)
                month = now.getMonth() + 1;
            year = year + 1;
            fetchAll(year, month, this.state, 1, true);
        },

        onMinusYear: function() {
            fetchAll(this.year - 1, this.month, this.state, 1, true);
        },

        onFiler: function() {
            fetchAll(this.year, this.month, this.state >= 1 ? 0 : this.state + 1, 1, false);
        },

        onFirstPage: function() {
            fetchAll(this.year, this.month, this.state, 1, false);
        },

        onPrePage: function() {
            fetchAll(this.year, this.month, this.state, this.pageNo - 1, false);
        },

        onNextPage: function() {
            fetchAll(this.year, this.month, this.state, this.pageNo + 1, false);
        },

        onLastPage: function() {
            fetchAll(this.year, this.month, this.state, this.filteredMyselfCompanyUserPages.count, false);
        },

        showChangeWorkloadPopover: function(e, projectInfo, myselfCompanyUser) {
            //构建弹出框
            var workerProjectWorkload = projectInfo[myselfCompanyUser.Id];
            var title = myselfCompanyUser.RegAlias + ': ' + projectInfo.ProjectName;
            var content =
                '<div>' +
                '   <ul class="list-group" style="padding: 0%; margin:0%; width: 240px;">' +
                '        <li class="list-group-item">' +
                '            <label class="manage-color" style="margin: 1% 5% 1% 0%; color: #fafafa; white-space: pre;" title="项目管理、开发管理、团队管理、内外协调等方面的工作（含维护阶段新增需求的项目管理）"> 项目管理 </label>' +
                (phAjax.isInRole('项目管理', myselfCompanyUser.PositionId)
                    ? '      <input style="width: 60%; text-align: center;" type="number" min="0" max="31" value="manageWorkload" oninput="vue.changeManageWorkload(value)" />'
                    : '      <input disabled="disabled" style="width: 60%; text-align: center; background: rgb(246, 246, 246);" title="如需填写请联系项目负责人开通权限" value="manageWorkload" /> ').
                    replace(/manageWorkload/g, workerProjectWorkload != null ? workerProjectWorkload.ManageWorkload : 0) +
                '        </li>' +
                '        <li class="list-group-item">' +
                '            <label class="investigate-color" style="margin: 1% 5% 1% 0%; color: #fafafa; white-space: pre;" title="需求调研、需求分析、需求确认、可行性分析、方案撰写等方面的工作（含维护阶段新增需求的调研分析）"> 调研分析 </label>' +
                (phAjax.isInRole('调研分析', myselfCompanyUser.PositionId)
                    ? '      <input style="width: 60%; text-align: center;" type="number" min="0" max="31" value="investigateWorkload" oninput="vue.changeInvestigateWorkload(value)">'
                    : '      <input disabled="disabled" style="width: 60%; text-align: center; background: rgb(246, 246, 246);" title="如需填写请联系项目负责人开通权限" value="investigateWorkload" />').
                    replace(/investigateWorkload/g, workerProjectWorkload != null ? workerProjectWorkload.InvestigateWorkload : 0) +
                '        </li>' +
                '        <li class="list-group-item">' +
                '            <label class="develop-color" style="margin: 1% 5% 1% 0%; color: #fafafa; white-space: pre;" title="系统设计、编码开发、单元测试、方案设计等方面的工作（含维护阶段新增需求的设计开发）"> 设计开发 </label>' +
                (phAjax.isInRole('设计开发', myselfCompanyUser.PositionId) || phAjax.isInRole('方案设计', myselfCompanyUser.PositionId)
                    ? '      <input style="width: 60%; text-align: center;" type="number" min="0" max="31" value="developWorkload" oninput="vue.changeDevelopWorkload(value)" />'
                    : '      <input disabled="disabled" style="width: 60%; text-align: center; background: rgb(246, 246, 246);" title="如需填写请联系项目负责人开通权限" value="developWorkload" />').
                    replace(/developWorkload/g, workerProjectWorkload != null ? workerProjectWorkload.DevelopWorkload : 0) +
                '        </li>' +
                '        <li class="list-group-item">' +
                '            <label class="test-color" style="margin: 1% 5% 1% 0%; color: #717171; white-space: pre;" title="功能验证、整体联调、缺陷修正、采购施工等方面的工作（含维护阶段新增需求的联调测试）"> 联调测试 </label>' +
                (phAjax.isInRole('测试联调', myselfCompanyUser.PositionId) || phAjax.isInRole('采购施工', myselfCompanyUser.PositionId)
                    ? '      <input style="width: 60%; text-align: center;" type="number" min="0" max="31" value="testWorkload" oninput="vue.changeTestWorkload(value)" />'
                    : '      <input disabled="disabled" style="width: 60%; text-align: center; background: rgb(246, 246, 246);" title="如需填写请联系项目负责人开通权限" value="testWorkload" />').
                    replace(/testWorkload/g, workerProjectWorkload != null ? workerProjectWorkload.TestWorkload : 0) +
                '        </li>' +
                '        <li class="list-group-item">' +
                '            <label class="implement-color" style="margin: 1% 5% 1% 0%; color: #fafafa; white-space: pre;" title="系统培训、部署、上线、交付验收等方面的工作（含维护阶段新增需求的部署升级）"> 培训实施 </label>' +
                (phAjax.isInRole('培训实施', myselfCompanyUser.PositionId) || phAjax.isInRole('交付验收', myselfCompanyUser.PositionId)
                    ? '      <input style="width: 60%; text-align: center;" type="number" min="0" max="31" value="implementWorkload" oninput="vue.changeImplementWorkload(value)" />'
                    : '      <input disabled="disabled" style="width: 60%; text-align: center; background: rgb(246, 246, 246);" title="如需填写请联系项目负责人开通权限" value="implementWorkload" />').
                    replace(/implementWorkload/g, workerProjectWorkload != null ? workerProjectWorkload.ImplementWorkload : 0) +
                '        </li>' +
                '        <li class="list-group-item">' +
                '            <label class="maintenance-color" style="margin: 1% 5% 1% 0%; color: #fafafa; white-space: pre;" title="系统上线后的质保和维保期间的接故、排故、咨询服务等方面的工作（不含新增需求的处理）"> 质保维保 </label>' +
                (phAjax.isInRole('质保维保', myselfCompanyUser.PositionId)
                    ? '      <input style="width: 60%; text-align: center;" type="number" min="0" max="31" value="maintenanceWorkload" oninput="vue.changeMaintenanceWorkload(value)" />'
                    : '      <input disabled="disabled" style="width: 60%; text-align: center; background: rgb(246, 246, 246);" title="如需填写请联系项目负责人开通权限" value="maintenanceWorkload" />').
                    replace(/maintenanceWorkload/g, workerProjectWorkload != null ? workerProjectWorkload.MaintenanceWorkload : 0) +
                '        </li>' +
                '        <li class="list-group-item">' +
                '            <div style="color: royalblue;">还剩 * 天可填报</div>'.
                    replace('*', this.workerProjectWorkloads != null ? Object.prototype.hasOwnProperty.call(this.workerProjectWorkloads, myselfCompanyUser.Id) ? this.workerProjectWorkloads[myselfCompanyUser.Id].remainDays : '?' : '?') +
                '            <div style="display: none;">*</div>'.replace('*', title) +
                '        </li>' +
                '    </ul>' +
                '</div>';
            if (changeWorkloadTarget != null && !changeWorkloadHtml.includes(title))
                changeWorkloadTarget.popover('hide');
            changeWorkloadTarget = $(e.target);
            changeWorkloadTarget.popover({
                title: title,
                container: 'body',
                trigger: 'manual',
                placement: 'auto right',
                html: 'true',
                content: content,
            });
            changeWorkloadTarget.popover('show');
            changeWorkloadHtml = content;
            //取消事件冒泡
            if (e && e.stopPropagation) 
                e.stopPropagation(); //支持W3C的方法 
            else
                window.event.cancelBubble = true; //使用IE的方式
        },

        changeManageWorkload: function (value) {
            alert(value);
        },

        changeInvestigateWorkload: function (value) {
            alert(value);
        },

        changeDevelopWorkload: function (value) {
            alert(value);
        },

        changeTestWorkload: function (value) {
            alert(value);
        },

        changeImplementWorkload: function (value) {
            alert(value);
        },

        changeMaintenanceWorkload: function (value) {
            alert(value);
        },
    }
})