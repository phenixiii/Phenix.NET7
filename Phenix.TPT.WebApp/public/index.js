$(function() {
    fetchProjectTypes();
    fetchMyselfCompanyUsers();
    fetchProjectInfos(true);
});

const queryNameCacheKey = 'index-query-name';
const queryPersonCacheKey = 'index-query-person';
const currentProjectInfoCacheKey = 'index-current-project-info';

function fetchProjectTypes() {
    base.call({
        path: '/api/project-type/all',
        onSuccess: function(result) {
            vue.projectTypes = result;
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取项目类型失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function fetchMyselfCompanyUsers() {
    phAjax.getMyselfCompanyUsers({
        includeDisabled: true,
        onSuccess: function(result) {
            vue.myselfCompanyUsers = result;
            filterProjectInfos(vue.projectInfos, result, true);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取公司员工资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function fetchProjectInfos(reset) {
    if (reset)
        vue.projectInfos = null;
    if (vue.projectInfos == null)
        base.call({
            path: '/api/project-info/all',
            pathParam: vue.filterTimeInterval,
            onSuccess: function(result) {
                vue.projectInfos = result;
                filterProjectInfos(result, vue.myselfCompanyUsers, reset);
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                vue.projectInfos = null;
                alert('获取项目失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
            },
        });
    else
        filterProjectInfos(vue.projectInfos, vue.myselfCompanyUsers, reset);
}

function locatingProjectInfo(projectInfo) {
    if (projectInfo != null) {
        var top = $('#' + projectInfo.Id).offset().top; // :id="projectInfo.Id"
        if (top < document.documentElement.scrollTop)
            setTimeout(function() { $('html,body').animate({ scrollTop: top }, 1000); }, 100);
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

function showPlanPanel(projectInfo) {
    Vue.set(projectInfo, 'showingPlan', true);
}

function hidePlanPanel(projectInfo) {
    Vue.delete(projectInfo, 'showingPlan');
}

function pushProjectStatuses(status) {
    if (!vue.projectStatuses.includes(status))
        vue.projectStatuses.push(status);
}

function pushSalesAreas(salesArea) {
    if (!vue.salesAreas.includes(salesArea))
        vue.salesAreas.push(salesArea);
}

function pushCustomers(customer) {
    if (!vue.customers.includes(customer))
        vue.customers.push(customer);
}

function filterProjectInfos(projectInfos, myselfCompanyUsers, reset) {
    if (projectInfos == null || myselfCompanyUsers == null)
        return;
    //清理界面
    vue.filteredProjectInfos.forEach((item, index) => {
        hidePlanPanel(item);
    });
    vue.filteredProjectInfos = [];
    //重置关联数据
    if (reset) {
        var lastDay = new Date(vue.filterTimeInterval.year, vue.filterTimeInterval.month, 1).setMonth(vue.filterTimeInterval.month, 0); //月度最后一天
        var projectManagers = [];
        var developManagers = [];
        var salesManagers = [];
        var maintenanceManagers = [];
        myselfCompanyUsers.forEach((item, index) => {
            if (new Date(item.RegTime) <= lastDay &&
                (item.DisabledTime == null || new Date(item.DisabledTime) > lastDay))
                phAjax.getPosition({
                    positionId: item.PositionId,
                    onSuccess: function(result) {
                        if (result.Name === '项目经理') {
                            projectManagers.push(item);
                            developManagers.push(item);
                        } else if (result.Name === '开发经理' ||
                            result.Name === '集成经理' ||
                            result.Name === '数据管理')
                            developManagers.push(item);
                        else if (result.Name === '销售经理')
                            salesManagers.push(item);
                        if (result.Roles.includes('质保维保'))
                            maintenanceManagers.push(item);
                    }
                });
        });
        vue.projectManagers = projectManagers;
        vue.developManagers = developManagers;
        vue.salesManagers = salesManagers;
        vue.maintenanceManagers = maintenanceManagers;
    }
    //记住处理中的项目
    var currentProjectId = vue.currentProjectInfo != null
        ? vue.currentProjectInfo.Id
        : window.localStorage.hasOwnProperty(currentProjectInfoCacheKey)
        ? window.localStorage.getItem(currentProjectInfoCacheKey)
        : null;
    vue.currentProjectInfo = null;
    //过滤出可处理的项目
    var filteredProjectInfos = [];
    projectInfos.forEach((item, index) => {
        pushProjectStatuses(item.CurrentStatus);
        pushProjectStatuses(item.AnnualMilestone);
        pushSalesAreas(item.SalesArea);
        pushCustomers(item.Customer);

        if (item.ContApproveDate != null)
            item.ContApproveDate = new Date(item.ContApproveDate).format('yyyy-MM-dd');

        switch (vue.state) {
        case 1: //当月自己负责项目
            if (!base.isMyProject(item))
                return;
            break;
        case 2: //当月关闭的项目
            if (item.ClosedDate === null)
                return;
            var closedDate = new Date(item.ClosedDate);
            if (closedDate.getFullYear() !== vue.filterTimeInterval.year ||
                closedDate.getMonth() + 1 !== vue.filterTimeInterval.month)
                return;
            break;
        }

        var queryUser = vue.queryPerson != null
            ? myselfCompanyUsers.find(item => item.Name === vue.queryPerson || item.RegAlias === vue.queryPerson)
            : null;

        if ((vue.queryName == null ||
                item.ProjectName.indexOf(vue.queryName) >= 0 ||
                item.Customer.indexOf(vue.queryName) >= 0 ||
                item.SalesArea.indexOf(vue.queryName) >= 0 ||
                item.ContNumber.indexOf(vue.queryMame) >= 0) &&
            (vue.queryPerson == null ||
                queryUser != null &&
                (item.ProjectManager === queryUser.Id ||
                    item.DevelopManager === queryUser.Id ||
                    item.MaintenanceManager === queryUser.Id ||
                    item.SalesManager === queryUser.Id) ||
                item.ProductVersion != null && item.ProductVersion.indexOf(vue.queryPerson) >= 0 ||
                item.CurrentStatus != null && item.CurrentStatus.indexOf(vue.queryPerson) >= 0)) {
            if (item.annualPlans == null)
                item.annualPlans = [];
            if (item.Id === currentProjectId)
                vue.currentProjectInfo = item;
            filteredProjectInfos.push(item);
        }
    });
    vue.filteredProjectInfos = filteredProjectInfos;
    //缓存查询条件
    if (vue.queryName != null)
        window.localStorage.setItem(queryNameCacheKey, vue.queryName);
    else
        window.localStorage.removeItem(queryNameCacheKey);
    if (vue.queryPerson != null)
        window.localStorage.setItem(queryPersonCacheKey, vue.queryPerson);
    else
        window.localStorage.removeItem(queryPersonCacheKey);
    //重新定位到处理中的项目
    locatingProjectInfo(vue.currentProjectInfo);
}

function addProjectInfo() {
    base.call({
        path: '/api/project-info',
        onSuccess: function(result) {
            result.annualPlans = [];
            vue.projectInfos.unshift(result);
            vue.filteredProjectInfos.unshift(result);
            vue.currentProjectInfo = result;
            showProjectInfoPanel(result);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            vue.projectInfos = null;
            alert('添加项目失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function showProjectInfo(projectInfo) {
    base.call({
        path: '/api/project-info',
        pathParam: { id: projectInfo.Id },
        onSuccess: function(result) {
            result.annualPlans = [];
            if (result.ContApproveDate != null)
                result.ContApproveDate = new Date(result.ContApproveDate).format('yyyy-MM-dd');
            if (result.OnlinePlanDate != null)
                result.OnlinePlanDate = new Date(result.OnlinePlanDate).format('yyyy-MM-dd');
            if (result.OnlineActualDate != null)
                result.OnlineActualDate = new Date(result.OnlineActualDate).format('yyyy-MM-dd');
            if (result.AcceptDate != null)
                result.AcceptDate = new Date(result.AcceptDate).format('yyyy-MM-dd');
            if (result.ClosedDate != null)
                result.ClosedDate = new Date(result.ClosedDate).format('yyyy-MM-dd');

            vue.projectInfos.splice(vue.projectInfos.indexOf(projectInfo), 1, result);
            vue.filteredProjectInfos.splice(vue.filteredProjectInfos.indexOf(projectInfo), 1, result);
            vue.currentProjectInfo = result;
            showProjectInfoPanel(result);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            vue.projectInfos = null;
            alert('获取项目失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function putProjectInfo(projectInfo) {
    base.call({
        type: "PUT",
        path: '/api/project-info',
        data: projectInfo,
        onSuccess: function(result) {
            pushSalesAreas(projectInfo.SalesArea);
            pushCustomers(projectInfo.Customer);

            if (confirm('成功提交资料, 是否需要合上资料面板?')) {
                hideProjectInfoPanel(projectInfo);
                locatingProjectInfo(projectInfo);
                if (projectInfo.annualPlans.length === 0)
                    if (confirm('是否需要展开计划面板以便跟踪项目状况?'))
                        nextAnnualPlan(projectInfo);
            }
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('提交资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function closeProject(projectInfo, closedDate) {
    base.call({
        type: 'DELETE',
        path: '/api/project-info',
        pathParam: { id: projectInfo.Id, closedDate: closedDate },
        onSuccess: function(result) {
            Vue.set(projectInfo, 'ClosedDate', closedDate);
            hideCloseProjectDialog();
            alert(projectInfo.ProjectName + ' 已成功归档!');
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            vue.projectInfos = null;
            alert('项目归档失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function nextAnnualPlan(projectInfo) {
    var year;
    if (projectInfo.annualPlans.length > 0)
        year = projectInfo.annualPlans[projectInfo.annualPlans.length - 1].Year - 1;
    else {
        var date = projectInfo.ClosedDate != null ? new Date(projectInfo.ClosedDate) : new Date();
        year = date.getFullYear();
    }

    base.call({
        path: '/api/project-annual-plan',
        pathParam: { projectId: projectInfo.Id, year: year },
        onSuccess: function(result) {
            pushProjectStatuses(result.AnnualMilestone);

            result.monthlyReports = [];
            projectInfo.annualPlans.push(result);
            vue.$forceUpdate();

            nextMonthlyReport(projectInfo, result);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取年度计划失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function putAnnualPlan(projectInfo, annualPlan) {
    base.call({
        type: "PUT",
        path: '/api/project-annual-plan',
        data: annualPlan,
        onSuccess: function(result) {
            pushProjectStatuses(annualPlan.AnnualMilestone);

            var now = new Date();
            if (annualPlan.Year === now.getFullYear())
                Vue.set(projectInfo, 'AnnualMilestone', annualPlan.AnnualMilestone);

            if (confirm('成功提交年度计划, 是否需要合上计划面板?')) {
                hidePlanPanel(projectInfo);
                locatingProjectInfo(projectInfo);
            }
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('提交年度计划失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function nextMonthlyReport(projectInfo, annualPlan) {
    var year = annualPlan.Year;
    var month;
    if (annualPlan.monthlyReports.length > 0) {
        var monthlyReport = annualPlan.monthlyReports[annualPlan.monthlyReports.length - 1];
        if (monthlyReport.Month === 1) {
            nextAnnualPlan(projectInfo);
            return;
        }
        month = monthlyReport.Month - 1;
    } else {
        var date = projectInfo.ClosedDate != null ? new Date(projectInfo.ClosedDate) : new Date();
        month = year === date.getFullYear() ? date.getMonth() + 1 : 12;
    }

    base.call({
        path: '/api/project-monthly-report',
        pathParam: { projectId: projectInfo.Id, year: year, month: month },
        onSuccess: function(result) {
            pushProjectStatuses(result.Status);

            annualPlan.monthlyReports.push(result);
            vue.$forceUpdate();

            showPlanPanel(projectInfo);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取月报失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function putMonthlyReport(projectInfo, monthlyReport) {
    base.call({
        type: "PUT",
        path: '/api/project-monthly-report',
        data: monthlyReport,
        onSuccess: function(result) {
            pushProjectStatuses(monthlyReport.Status);

            var now = new Date();
            if (monthlyReport.Year === now.getFullYear() &&
                monthlyReport.Month === now.getMonth() + 1)
                Vue.set(projectInfo, 'CurrentStatus', monthlyReport.Status);

            if (confirm('成功提交月报, 是否需要合上计划面板?')) {
                hidePlanPanel(projectInfo);
                locatingProjectInfo(projectInfo);
            }
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('提交月报失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

var vue = new Vue({
    el: '#content',
    data: {
        state: 0,
        stateTitle: [
            '当月项目',
            '当月自己负责项目',
            '当月关闭的项目',
        ],
        filterStateTitle: [
            '显示当月自己负责项目',
            '显示当月关闭的项目',
            '显示当月项目',
        ],
        filterTimeInterval: {
            year: new Date().getFullYear(),
            month: new Date().getMonth() + 1,
        },
        queryName: window.localStorage.hasOwnProperty(queryNameCacheKey) ? window.localStorage.getItem(queryNameCacheKey) : null,
        queryPerson: window.localStorage.hasOwnProperty(queryPersonCacheKey) ? window.localStorage.getItem(queryPersonCacheKey) : null,

        projectTypes: [],

        projectStatuses: [],
        salesAreas: [],
        customers: [],

        myselfCompanyUsers: null,
        projectManagers: [],
        developManagers: [],
        maintenanceManagers: [],
        salesManagers: [],

        projectInfos: null,
        filteredProjectInfos: [], //用于界面绑定的projectInfos子集
        currentProjectInfo: null,

        closedDate: new Date().format('yyyy-MM-dd'),
    },

    methods: {
        parseUserName: function(id) {
            if (this.myselfCompanyUsers != null) {
                var user = this.myselfCompanyUsers.find(item => item.Id === id);
                if (user != null)
                    return user.RegAlias;
            }
            return null;
        },

        onPlusMonth: function() {
            var now = new Date();
            if (this.filterTimeInterval.year === now.getFullYear() &&
                this.filterTimeInterval.month === now.getMonth() + 1) {
                alert('物来顺应，未来不迎~');
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
                alert('珍惜当下，安然即好~');
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

        onFiler: function() {
            this.state = this.state >= 2 ? 0 : this.state + 1;
            fetchProjectInfos(false);
        },

        onQueryProjectInfos: function() {
            fetchProjectInfos(false);
        },

        showSalesData: function(projectInfo) {
            if (!base.isMyProject(projectInfo))
                return;
            return (projectInfo.SalesArea != null ? '客户经理: ' : '') + this.parseUserName(projectInfo.SalesManager) +
                (projectInfo.SalesArea != null ? '\n销售区域: ' : '') + (projectInfo.SalesArea ?? '') +
                (projectInfo.Customer != null ? '\n服务客户: ' : '') + (projectInfo.Customer ?? '') +
                (projectInfo.SalesArea != null ? '\n维护经理: ' : '') + this.parseUserName(projectInfo.MaintenanceManager);
        },

        onAddProjectInfo: function() {
            if (!base.isMyProject())
                return;
            addProjectInfo();
        },

        onShowProjectInfo: function(projectInfo) {
            if (!base.isMyProject(projectInfo))
                return;
            showProjectInfo(projectInfo);
        },

        onPutProjectInfo: function(projectInfo) {
            if (!base.isMyProject(projectInfo))
                return;
            putProjectInfo(projectInfo);
        },

        onHideProjectInfo: function(projectInfo) {
            hideProjectInfoPanel(projectInfo);
            locatingProjectInfo(projectInfo);
        },

        showCloseProjectDialog: function(projectInfo) {
            if (!base.isMyProject(projectInfo))
                return;
            this.currentProjectInfo = projectInfo;
            if (projectInfo.ContAmount > projectInfo.TotalInvoiceAmount)
                if (!confirm('项目 ' + projectInfo.ProjectName + ' 还有 ' +
                    (projectInfo.ContAmount - projectInfo.TotalInvoiceAmount) +
                    ' 万元应收款未开票，归档后将无法整理开票信息! 是否还要继续归档?'))
                    return;
            showCloseProjectDialog();
        },

        onCloseProject: function() {
            if (!base.isMyProject(this.currentProjectInfo))
                return;
            closeProject(this.currentProjectInfo, this.closedDate);
        },

        onShowPlan: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            if (projectInfo.annualPlans.length === 0)
                nextAnnualPlan(projectInfo);
            else
                showPlanPanel(projectInfo);
        },

        onHidePlan: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            hidePlanPanel(projectInfo);
            locatingProjectInfo(projectInfo);
        },

        canPutAnnualPlan: function(projectInfo, annualPlan) {
            if (!base.isMyProject(projectInfo))
                return false;
            var now = new Date();
            if (projectInfo.ClosedDate != null && new Date(projectInfo.ClosedDate) < now.setDate(1)) //上月或更久已关闭项目
                return false;
            return new Date(annualPlan.Year + 1, 1, 1) >= now; //今年或之后的年度计划
        },

        onPutAnnualPlan: function(projectInfo, annualPlan) {
            if (!base.isMyProject(projectInfo))
                return;
            this.currentProjectInfo = projectInfo;
            putAnnualPlan(projectInfo, annualPlan);
        },

        onNextMonthlyReport: function(projectInfo, annualPlan) {
            this.currentProjectInfo = projectInfo;
            nextMonthlyReport(projectInfo, annualPlan);
        },

        canPutMonthlyReport: function(projectInfo, monthlyReport) {
            if (!base.isMyProject(projectInfo))
                return false;
            var now = new Date();
            if (projectInfo.ClosedDate != null && new Date(projectInfo.ClosedDate) < now.setDate(1)) //上月或更久已关闭项目
                return false;
            return new Date(monthlyReport.Year, monthlyReport.Month, 1) >= now.setMonth(now.getMonth() - 1, 1); //上月或之后的月报
        },

        onPutMonthlyReport: function(projectInfo, monthlyReport) {
            if (!base.isMyProject(projectInfo))
                return;
            this.currentProjectInfo = projectInfo;
            putMonthlyReport(projectInfo, monthlyReport);
        },
    }
})