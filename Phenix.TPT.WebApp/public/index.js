$(function () {
    fetchProjectInfos(true);
});

const queryNameCacheKey = 'index-query-name';
const queryPersonCacheKey = 'index-query-person';
const currentProjectInfoCacheKey = 'index-current-project-info';

function isMyProject(projectInfo) {
    var myself = phAjax.getMyself();
    if (myself == null)
        return false;
    return myself.Position == null ||
        myself.Position.Roles.indexOf('经营管理') === 0 ||
        myself.Position.Roles.indexOf('项目管理') === 0 &&
        (projectInfo == null ||
            myself.Name === projectInfo.ProjectManager ||
            myself.Name === projectInfo.DevelopManager ||
            myself.Name === projectInfo.MaintenanceManager ||
            myself.Name === projectInfo.SalesManager);
}

function hideMonthlyReportPanel(projectInfo) {
    var result = false;
    if (projectInfo.switchMonthlyReportButton != null && projectInfo.switchMonthlyReportButton.hasClass('fa-chevron-up')) {
        projectInfo.switchMonthlyReportButton.removeClass('fa-chevron-up');
        projectInfo.switchMonthlyReportButton.addClass('fa-chevron-down');
        projectInfo.switchMonthlyReportButton.attr('title', '填写月报');
        delete projectInfo.switchMonthlyReportButton;
        result = true;
    }
    if (projectInfo.monthlyReportPanel != null && !projectInfo.monthlyReportPanel.hasClass('hide')) {
        projectInfo.monthlyReportPanel.addClass('hide');
        delete projectInfo.monthlyReportPanel;
        result = true;
    }
    return result;
}

function showMonthlyReportPanel(projectInfo) {
    if (projectInfo.switchMonthlyReportButton != null && projectInfo.switchMonthlyReportButton.hasClass('fa-chevron-down')) {
        projectInfo.switchMonthlyReportButton.removeClass('fa-chevron-down');
        projectInfo.switchMonthlyReportButton.addClass('fa-chevron-up');
        projectInfo.switchMonthlyReportButton.attr("title", '收起月报');
    };
    if (projectInfo.monthlyReportPanel != null && projectInfo.monthlyReportPanel.hasClass('hide')) {
        projectInfo.monthlyReportPanel.removeClass('hide');
    }
}

function filterProjectInfos(projectInfos) {
    vue.filteredProjectInfos.forEach((item, index) => {
        hideMonthlyReportPanel(item);
    });
    vue.filteredProjectInfos = [];

    var currentProjectId = vue.currentProjectInfo != null
        ? vue.currentProjectInfo.Id
        : window.localStorage.hasOwnProperty(currentProjectInfoCacheKey)
        ? window.localStorage.getItem(currentProjectInfoCacheKey)
        : null;
    vue.currentProjectInfo = null;

    if (projectInfos != null) {
        projectInfos.forEach((item, index) => {
            switch (vue.filterState) {
            case 0: //显示当月自己负责项目
                if (!isMyProject(item))
                    return;
                break;
            case 1: //显示当月关闭的项目
                if (item.ClosedDate === null)
                    return;
                var closedDate = new Date(item.ClosedDate);
                if (closedDate.getFullYear() !== vue.filterTimeInterval.year ||
                    closedDate.getMonth() + 1 !== vue.filterTimeInterval.month)
                    return;
                break;
            }

            if ((vue.queryName == null ||
                    item.ProjectName.indexOf(vue.queryName) >= 0 ||
                    item.Customer.indexOf(vue.queryName) >= 0 ||
                    item.SalesArea.indexOf(vue.queryName) >= 0 ||
                    item.ContNumber.indexOf(vue.queryMame) >= 0) &&
                (vue.queryPerson == null ||
                    item.ProjectManager === vue.queryPerson ||
                    item.DevelopManager === vue.queryPerson ||
                    item.MaintenanceManager === vue.queryPerson ||
                    item.SalesManager === vue.queryPerson ||
                    item.ProductVersion != null && item.ProductVersion.indexOf(vue.queryPerson) >= 0 ||
                    item.CurrentStatus != null && item.CurrentStatus.indexOf(vue.queryPerson) >= 0)) {
                if (item.monthlyReports == null)
                    item.monthlyReports = [];
                if (currentProjectId === item.Id)
                    vue.currentProjectInfo = item;
                vue.filteredProjectInfos.push(item);
            }
        });

        if (vue.queryName != null)
            window.localStorage.setItem(queryNameCacheKey, vue.queryName);
        else
            window.localStorage.removeItem(queryNameCacheKey);
        if (vue.queryPerson != null)
            window.localStorage.setItem(queryPersonCacheKey, vue.queryPerson);
        else
            window.localStorage.removeItem(queryPersonCacheKey);

        if (vue.currentProjectInfo != null)
            setTimeout(function () { $('html,body').animate({ scrollTop: $('#' + vue.currentProjectInfo.Id).offset().top }, 1000); }, 2000);
    }

    vue.projectInfos = projectInfos;
}

function fetchProjectInfos(reset) {
    if (reset || vue.projectInfos == null)
        window.callAjax({
            path: '/api/project-info',
            pathParam: vue.filterTimeInterval,
            onSuccess: function(result) {
                filterProjectInfos(result);
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                filterProjectInfos(null);
                zdalert('获取项目资料失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
            },
        });
    else
        filterProjectInfos(vue.projectInfos);
}

function fetchMonthlyReport(projectInfo) {
    var year;
    var month;
    if (projectInfo.monthlyReports.length > 0) {
        var monthlyReport = projectInfo.monthlyReports[projectInfo.monthlyReports.length - 1];
        if (monthlyReport.Month === 1) {
            year = monthlyReport.Year - 1;
            month = 12;
        } else {
            year = monthlyReport.Year;
            month = monthlyReport.Month - 1;
        }
    } else {
        var date = projectInfo.ClosedDate != null ? new Date(projectInfo.ClosedDate) : new Date();
        year = date.getFullYear();
        month = date.getMonth() + 1;
    }

    window.callAjax({
        path: '/api/project-monthly-report',
        pathParam: {
            id: projectInfo.Id,
            year: year,
            month: month
        },
        onSuccess: function(result) {
            projectInfo.monthlyReports.push(result);
            vue.currentProjectInfo = projectInfo;
            showMonthlyReportPanel(projectInfo);

            if (projectInfo.nextMonthlyReportButton != null) {
                projectInfo.nextMonthlyReportButton.addClass('hide');
                delete projectInfo.nextMonthlyReportButton;
            }
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            hideMonthlyReportPanel(projectInfo);
            zdalert('获取项目月报失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function putMonthlyReport(projectInfo, monthlyReport) {
    window.callAjax({
        type: "PUT",
        path: '/api/project-monthly-report',
        pathParam: {
            id: projectInfo.Id,
        },
        data: monthlyReport,
        onSuccess: function (result) {
            zdconfirm('成功提交项目月报',
                '是否需要合上月报填写面板?',
                function (result) {
                    if (result)
                        hideMonthlyReportPanel(projectInfo);
                });
        },
        onError: function (XMLHttpRequest, textStatus, validityError) {
            zdalert('提交项目月报失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function closeProject(projectInfo, closedDate) {
    window.callAjax({
        type: 'DELETE',
        path: '/api/project-info',
        pathParam: { id: projectInfo.Id, closedDate: closedDate },
        onSuccess: function (result) {
            vue.projectInfos = null;
            Vue.set(projectInfo, 'ClosedDate', closedDate);
            zdalert('成功关闭项目', projectInfo.ProjectName);
            $('#closeProjectDialog').modal('hide');
        },
        onError: function (XMLHttpRequest, textStatus, validityError) {
            filterProjectInfos(null);
            zdalert('关闭项目失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

var vue = new Vue({
    el: '#content',
    data: {
        state: 0,
        stateTitle: [
            '当月全部项目',
            '当月自己负责项目',
            '当月关闭的项目',
        ],
        filterState: 0,
        filterStateTitle: [
            '显示当月自己负责项目',
            '显示当月关闭的项目',
            '显示当月全部项目',
        ],
        filterTimeInterval: {
            year: new Date().getFullYear(),
            month: new Date().getMonth() + 1,
        },
        queryName: window.localStorage.hasOwnProperty(queryNameCacheKey) ? window.localStorage.getItem(queryNameCacheKey) : null,
        queryPerson: window.localStorage.hasOwnProperty(queryPersonCacheKey) ? window.localStorage.getItem(queryPersonCacheKey) : null,

        projectInfos: null,
        currentProjectInfo: null, //当前操作对象
        filteredProjectInfos: [], //用于界面绑定的projectInfos子集

        closedDate: new Date().format('yyyy-MM-dd'),
    },

    methods: {
        onPlusMonth: function() {
            var date = new Date();
            if (this.filterTimeInterval.year === date.getFullYear() &&
                this.filterTimeInterval.month === date.getMonth() + 1) {
                zdalert('切换月份', '物来顺应，未来不迎~');
                return;
            }

            if (this.filterTimeInterval.month === 12) {
                this.filterTimeInterval.year = this.filterTimeInterval.year + 1;
                this.filterTimeInterval.month = 1;
            } else
                this.filterTimeInterval.month = this.filterTimeInterval.month + 1;
            fetchProjectInfos(true);
        },

        onMinusMonth: function() {
            if (this.filterTimeInterval.month === 1) {
                this.filterTimeInterval.year = this.filterTimeInterval.year - 1;
                this.filterTimeInterval.month = 12;
            } else
                this.filterTimeInterval.month = this.filterTimeInterval.month - 1;
            fetchProjectInfos(true);
        },

        onPlusYear: function() {
            var date = new Date();
            if (this.filterTimeInterval.year === date.getFullYear()) {
                zdalert('切换年份', '珍惜当下，安然即好~');
                return;
            }

            if (this.filterTimeInterval.year === date.getFullYear() - 1 &&
                this.filterTimeInterval.month > date.getMonth() + 1)
                this.filterTimeInterval.month = date.getMonth() + 1;
            this.filterTimeInterval.year = this.filterTimeInterval.year + 1;
            fetchProjectInfos(true);
        },

        onMinusYear: function() {
            this.filterTimeInterval.year = this.filterTimeInterval.year - 1;
            fetchProjectInfos(true);
        },

        onFilerProject: function() {
            if (this.filterState === 2) {
                this.state = 0;
                this.filterState = 0;
            } else {
                this.state = this.state + 1;
                this.filterState = this.filterState + 1;
            }
            fetchProjectInfos(false);
        },

        onQueryProject: function() {
            fetchProjectInfos(false);
        },

        canAddProject: function () {
            return isMyProject();
        },

        onAddProject: function () {
            window.localStorage.removeItem(currentProjectInfoCacheKey);
            window.location.href = 'project-Info.html';
        },

        canEditProject: function (projectInfo) {
            return isMyProject(projectInfo);
        },

        onEditProject: function (projectInfo) {
            window.localStorage.setItem(currentProjectInfoCacheKey, projectInfo.Id);
            window.location.href = 'project-Info.html';
        },

        onSwitchMonthlyReport: function (projectInfo, e) {
            if (!hideMonthlyReportPanel(projectInfo)) {
                projectInfo.switchMonthlyReportButton = $(e.target);
                projectInfo.monthlyReportPanel = $('#' + projectInfo.Id).find('#monthlyReportPanel');
                if (projectInfo.monthlyReports.length > 0)
                    showMonthlyReportPanel(projectInfo);
                else
                    fetchMonthlyReport(projectInfo);
            }
        },

        onHideMonthlyReport: function (projectInfo) {
            hideMonthlyReportPanel(projectInfo);
        },

        onNextMonthlyReport: function (projectInfo, e) {
            projectInfo.nextMonthlyReportButton = $(e.target);
            fetchMonthlyReport(projectInfo);
        },

        canPutMonthlyReport: function(projectInfo, monthlyReport) {
            if (!isMyProject(projectInfo))
                return false;
            if (projectInfo.ClosedDate != null && new Date(projectInfo.ClosedDate) < new Date().setDate(1))
                return false;
            return new Date(monthlyReport.Year, monthlyReport.Month, 1) > new Date().setDate(-1);
        },

        onPutMonthlyReport(projectInfo, monthlyReport, e) {
            putMonthlyReport(projectInfo, monthlyReport);
        },

        canCloseProject: function (projectInfo) {
            if (!isMyProject(projectInfo))
                return false;
            return projectInfo.ClosedDate == null;
        },

        showCloseProjectDialog: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            if (projectInfo.ContAmount > projectInfo.TotalInvoiceAmount) {
                zdconfirm('关闭项目',
                    '该项目还有' + (projectInfo.ContAmount - projectInfo.TotalInvoiceAmount) + '余款未开票! 是否仍然关闭?',
                    function(result) {
                        if (result)
                            $('#closeProjectDialog').modal('show');
                    });
            } else
                $('#closeProjectDialog').modal('show');
        },

        onCloseProject: function () {
            closeProject(this.currentProjectInfo, this.closedDate);
        },

    }
})