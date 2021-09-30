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
        myself.Position.Roles.indexOf(projectRoles.经营管理) >= 0 ||
        myself.Position.Roles.indexOf(projectRoles.项目管理) >= 0 &&
        (projectInfo == null ||
            myself.Name === projectInfo.ProjectManager ||
            myself.Name === projectInfo.DevelopManager ||
            myself.Name === projectInfo.MaintenanceManager ||
            myself.Name === projectInfo.SalesManager);
}

function pushProjectStatuses(status) {
    if (!vue.projectStatuses.includes(status))
        vue.projectStatuses.push(status);
}

function locatingProjectInfo(projectInfo) {
    if (projectInfo != null) {
        var offsetTop = $('#' + projectInfo.Id).offset().top; // :id="projectInfo.Id"
        if (offsetTop < document.documentElement.scrollTop)
            setTimeout(function() { $('html,body').animate({ scrollTop: offsetTop }, 1000); }, 100);
    }
}

function showProjectInfoPanel(projectInfo) {
    Vue.set(projectInfo, 'showingProjectInfo', true);
}

function hideProjectInfoPanel(projectInfo) {
    Vue.delete(projectInfo, 'showingProjectInfo');
}

function showCloseProjectDialog() {
    $('#closeProjectDialog').modal('show'); // id="closeProjectDialog"
}

function hideCloseProjectDialog() {
    $('#closeProjectDialog').modal('hide'); //id="closeProjectDialog"
}

function showMonthlyReportPanel(projectInfo) {
    Vue.set(projectInfo, 'showingMonthlyReport', true);
}

function hideMonthlyReportPanel(projectInfo) {
    Vue.delete(projectInfo, 'showingMonthlyReport');
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
            pushProjectStatuses(item.CurrentStatus);
            pushProjectStatuses(item.AnnualMilestone);
            item.ContApproveDate = new Date(item.ContApproveDate).format('yyyy-MM-dd');

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

        locatingProjectInfo(vue.currentProjectInfo);
    }
}

function fetchProjectInfos(reset) {
    if (reset || vue.projectInfos == null)
        window.callAjax({
            path: '/api/project-info/all',
            pathParam: vue.filterTimeInterval,
            onSuccess: function(result) {
                vue.projectInfos = result;
                filterProjectInfos(result);
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                vue.projectInfos = null;
                zdalert('获取项目失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
            },
        });
    else
        filterProjectInfos(vue.projectInfos);
}

function addProjectInfo() {
    window.callAjax({
        path: '/api/project-info',
        onSuccess: function(result) {
            result.monthlyReports = [];
            vue.currentProjectInfo = result;
            vue.filteredProjectInfos.unshift(result);
            vue.projectInfos.unshift(result);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            vue.projectInfos = null;
            zdalert('添加项目失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function closeProject(projectInfo, closedDate) {
    window.callAjax({
        type: 'DELETE',
        path: '/api/project-info',
        pathParam: { id: projectInfo.Id, closedDate: closedDate },
        onSuccess: function (result) {
            Vue.set(projectInfo, 'ClosedDate', closedDate);
            hideCloseProjectDialog();
            zdalert('成功归档', projectInfo.ProjectName);
        },
        onError: function (XMLHttpRequest, textStatus, validityError) {
            vue.projectInfos = null;
            zdalert('项目归档失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function nextMonthlyReport(projectInfo) {
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
            projectInfoId: projectInfo.Id,
            year: year,
            month: month
        },
        onSuccess: function(result) {
            pushProjectStatuses(result.Status);

            projectInfo.monthlyReports.push(result);
            vue.$forceUpdate();

            showMonthlyReportPanel(projectInfo);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            zdalert('获取月报失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function putMonthlyReport(projectInfo, monthlyReport) {
    window.callAjax({
        type: "PUT",
        path: '/api/project-monthly-report',
        pathParam: {
            projectInfoId: projectInfo.Id,
        },
        data: monthlyReport,
        onSuccess: function(result) {
            pushProjectStatuses(monthlyReport.Status);

            var now = new Date();
            if (monthlyReport.Year === now.getFullYear() &&
                monthlyReport.Month === now.getMonth() + 1)
                Vue.set(projectInfo, 'CurrentStatus', monthlyReport.Status);

            zdconfirm('成功提交月报',
                '是否需要合上月报面板?',
                function(result) {
                    if (result) {
                        hideMonthlyReportPanel(projectInfo);
                        locatingProjectInfo(projectInfo);
                    }
                });
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            zdalert('提交月报失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
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

        projectStatuses: [],

        projectInfos: null,
        currentProjectInfo: null, //当前操作对象
        filteredProjectInfos: [], //用于界面绑定的projectInfos子集

        closedDate: new Date().format('yyyy-MM-dd'),
    },

    methods: {
        onPlusMonth: function() {
            var now = new Date();
            if (this.filterTimeInterval.year === now.getFullYear() &&
                this.filterTimeInterval.month === now.getMonth() + 1) {
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
            var now = new Date();
            if (this.filterTimeInterval.year === now.getFullYear()) {
                zdalert('切换年份', '珍惜当下，安然即好~');
                return;
            }

            if (this.filterTimeInterval.year === now.getFullYear() - 1 &&
                this.filterTimeInterval.month > now.getMonth() + 1)
                this.filterTimeInterval.month = now.getMonth() + 1;
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

        canAddProject: function() {
            return isMyProject();
        },

        onAddProject: function() {
            addProjectInfo();
        },

        canEditProject: function(projectInfo) {
            return isMyProject(projectInfo);
        },

        onShowProjectInfo: function (projectInfo) {
            this.currentProjectInfo = projectInfo;
            showProjectInfoPanel(projectInfo);
        },

        onPutProjectInfo: function (projectInfo) {
            window.localStorage.setItem(currentProjectInfoCacheKey, projectInfo.Id);
            window.location.href = 'project-Info.html';
        },

        onHideProjectInfo: function (projectInfo) {
            hideProjectInfoPanel(projectInfo);
            locatingProjectInfo(projectInfo);
        },

        canCloseProject: function(projectInfo) {
            if (!isMyProject(projectInfo))
                return false;
            return projectInfo.ClosedDate == null;
        },

        showCloseProjectDialog: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            if (projectInfo.ContAmount > projectInfo.TotalInvoiceAmount) {
                zdconfirm('项目归档',
                    projectInfo.ProjectName + ' 项目还有 ' + (projectInfo.ContAmount - projectInfo.TotalInvoiceAmount) + ' 万元应收款未开票，归档后将无法更新! 是否继续?',
                    function (result) {
                        if (result)
                            showCloseProjectDialog();
                    });
            } else
                showCloseProjectDialog();
        },

        onCloseProject: function() {
            closeProject(this.currentProjectInfo, this.closedDate);
        },

        onShowMonthlyReport: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            if (projectInfo.monthlyReports.length === 0)
                nextMonthlyReport(projectInfo);
            else
                showMonthlyReportPanel(projectInfo);
        },

        onNextMonthlyReport: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            nextMonthlyReport(projectInfo);
        },

        onHideMonthlyReport: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            hideMonthlyReportPanel(projectInfo);
            locatingProjectInfo(projectInfo);
        },

        canPutMonthlyReport: function(projectInfo, monthlyReport) {
            if (!isMyProject(projectInfo))
                return false;
            var now = new Date();
            if (projectInfo.ClosedDate != null && new Date(projectInfo.ClosedDate) < now.setDate(1)) // 上月或更久已关闭项目
                return false;
            return new Date(monthlyReport.Year, monthlyReport.Month, 1) >= now.setMonth(now.getMonth() - 1, 1); //上月或之后的月报
        },

        onPutMonthlyReport: function(projectInfo, monthlyReport) {
            this.currentProjectInfo = projectInfo;
            putMonthlyReport(projectInfo, monthlyReport);
        },
    }
})