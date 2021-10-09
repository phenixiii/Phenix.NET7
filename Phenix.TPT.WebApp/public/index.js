$(function() {
    fetchProjectTypes();
    fetchProjectManagers();
    fetchProjectInfos(true);
});

const queryNameCacheKey = 'index-query-name';
const queryPersonCacheKey = 'index-query-person';
const currentProjectInfoCacheKey = 'index-current-project-info';

function isMyProject(projectInfo) {
    var myself = phAjax.getMyself();
    if (myself == null)
        return false;
    return phAjax.isInRole(base.projectRoles.经营管理) ||
        projectInfo == null && phAjax.isInRole(base.projectRoles.项目管理) ||
        projectInfo != null &&
        (myself.Id === projectInfo.ProjectManager ||
            myself.Id === projectInfo.DevelopManager ||
            myself.Id === projectInfo.MaintenanceManager ||
            myself.Id === projectInfo.SalesManager);
}

function fetchProjectTypes() {
    base.call({
        path: '/api/project-type/all',
        onSuccess: function(result) {
            vue.projectTypes = result;
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            zdalert('获取项目类型失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function fetchProjectManagers() {
    phAjax.getMyselfCompanyUsers({
        onSuccess: function(result) {
            vue.myselfCompanyUsers = result;
            result.forEach((item, index) => {
                phAjax.getPosition({
                    positionId: item.PositionId,
                    onSuccess: function(result) {
                        if (result.Name === base.position.项目经理) {
                            vue.projectManagers.push(item);
                            vue.developManagers.push(item);
                        } else if (result.Name === base.position.开发经理 ||
                            result.Name === base.position.集成经理 ||
                            result.Name === base.position.数据管理)
                            vue.developManagers.push(item);
                        else if (result.Name === base.position.销售经理)
                            vue.salesManagers.push(item);
                        if (result.Roles.includes(base.projectRoles.质保维保))
                            vue.maintenanceManagers.push(item);
                    }
                });
            });

            if (vue.queryPerson != null && vue.projectInfos != null)
                filterProjectInfos(vue.projectInfos); //如果有按负责人姓名查询的可能性则要等获取到公司员工资料后再执行本函数
            vue.$forceUpdate();
        },
        onError: function(XMLHttpRequest, textStatus) {
            vue.logonHint = XMLHttpRequest.responseText;
            zdalert('获取公司员工资料失败', XMLHttpRequest.responseText);
        },
    });
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

function showPlanPanel(projectInfo) {
    Vue.set(projectInfo, 'showingPlan', true);
}

function hidePlanPanel(projectInfo) {
    Vue.delete(projectInfo, 'showingPlan');
}

function filterProjectInfos(projectInfos) {
    if (vue.queryPerson != null && vue.myselfCompanyUsers == null) //如果有按负责人姓名查询的可能性则要等获取到公司员工资料后再执行本函数
        return;

    vue.filteredProjectInfos.forEach((item, index) => {
        hidePlanPanel(item);
    });
    vue.currentProjectInfo = null;
    vue.filteredProjectInfos = [];

    if (projectInfos != null) {
        var currentProjectId = vue.currentProjectInfo != null
            ? vue.currentProjectInfo.Id
            : window.localStorage.hasOwnProperty(currentProjectInfoCacheKey)
            ? window.localStorage.getItem(currentProjectInfoCacheKey)
            : null;
        projectInfos.forEach((item, index) => {
            pushProjectStatuses(item.CurrentStatus);
            pushProjectStatuses(item.AnnualMilestone);
            pushSalesAreas(item.SalesArea);
            pushCustomers(item.Customer);

            if (item.ContApproveDate != null)
                item.ContApproveDate = new Date(item.ContApproveDate).format('yyyy-MM-dd');
            if (item.OnlinePlanDate != null)
                item.OnlinePlanDate = new Date(item.OnlinePlanDate).format('yyyy-MM-dd');
            if (item.OnlineActualDate != null)
                item.OnlineActualDate = new Date(item.OnlineActualDate).format('yyyy-MM-dd');
            if (item.AcceptDate != null)
                item.AcceptDate = new Date(item.AcceptDate).format('yyyy-MM-dd');
            if (item.ClosedDate != null)
                item.ClosedDate = new Date(item.ClosedDate).format('yyyy-MM-dd');

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

            var queryUser = vue.queryPerson != null
                ? vue.myselfCompanyUsers.find(item => item.Name === vue.queryPerson || item.RegAlias === vue.queryPerson)
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
        base.call({
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
    base.call({
        path: '/api/project-info',
        onSuccess: function(result) {
            result.annualPlans = [];
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

function putProjectInfo(projectInfo) {
    base.call({
        type: "PUT",
        path: '/api/project-info',
        data: projectInfo,
        onSuccess: function (result) {
            pushSalesAreas(projectInfo.SalesArea);
            pushCustomers(projectInfo.Customer);

            zdconfirm('成功提交资料',
                '是否需要合上资料面板?',
                function(result) {
                    if (result) {
                        hideProjectInfoPanel(projectInfo);
                        locatingProjectInfo(projectInfo);
                    }
                });
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            zdalert('提交资料失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function closeProject(projectInfo, closedDate) {
    base.call({
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
            zdalert('获取年度计划失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function putAnnualPlan(projectInfo, annualPlan) {
    base.call({
        type: "PUT",
        path: '/api/project-annual-plan',
        pathParam: { projectId: projectInfo.Id },
        data: annualPlan,
        onSuccess: function(result) {
            pushProjectStatuses(annualPlan.AnnualMilestone);

            var now = new Date();
            if (annualPlan.Year === now.getFullYear())
                Vue.set(projectInfo, 'AnnualMilestone', annualPlan.AnnualMilestone);

            zdconfirm('成功提交年度计划',
                '是否需要合上计划面板?',
                function(result) {
                    if (result) {
                        hidePlanPanel(projectInfo);
                        locatingProjectInfo(projectInfo);
                    }
                });
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            zdalert('提交年度计划失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
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
            zdalert('获取月报失败', validityError != null ? validityError.Hint : XMLHttpRequest.responseText);
        },
    });
}

function putMonthlyReport(projectInfo, monthlyReport) {
    base.call({
        type: "PUT",
        path: '/api/project-monthly-report',
        pathParam: { projectId: projectInfo.Id },
        data: monthlyReport,
        onSuccess: function(result) {
            pushProjectStatuses(monthlyReport.Status);

            var now = new Date();
            if (monthlyReport.Year === now.getFullYear() &&
                monthlyReport.Month === now.getMonth() + 1)
                Vue.set(projectInfo, 'CurrentStatus', monthlyReport.Status);

            zdconfirm('成功提交月报',
                '是否需要合上计划面板?',
                function(result) {
                    if (result) {
                        hidePlanPanel(projectInfo);
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
        currentProjectInfo: null, //当前操作对象
        filteredProjectInfos: [], //用于界面绑定的projectInfos子集

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

        onShowProjectInfo: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            showProjectInfoPanel(projectInfo);
        },

        onPutProjectInfo: function(projectInfo) {
            this.currentProjectInfo = projectInfo;
            putProjectInfo(projectInfo);
        },

        onHideProjectInfo: function(projectInfo) {
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
            if (!isMyProject(projectInfo))
                return false;
            var now = new Date();
            if (projectInfo.ClosedDate != null && new Date(projectInfo.ClosedDate) < now.setDate(1)) //上月或更久已关闭项目
                return false;
            return new Date(annualPlan.Year + 1, 1, 1) >= now; //今年或之后的年度计划
        },

        onPutAnnualPlan: function(projectInfo, annualPlan) {
            this.currentProjectInfo = projectInfo;
            putAnnualPlan(projectInfo, annualPlan);
        },

        onNextMonthlyReport: function(projectInfo, annualPlan) {
            this.currentProjectInfo = projectInfo;
            nextMonthlyReport(projectInfo, annualPlan);
        },

        canPutMonthlyReport: function(projectInfo, monthlyReport) {
            if (!isMyProject(projectInfo))
                return false;
            var now = new Date();
            if (projectInfo.ClosedDate != null && new Date(projectInfo.ClosedDate) < now.setDate(1)) //上月或更久已关闭项目
                return false;
            return new Date(monthlyReport.Year, monthlyReport.Month, 1) >= now.setMonth(now.getMonth() - 1, 1); //上月或之后的月报
        },

        onPutMonthlyReport: function(projectInfo, monthlyReport) {
            this.currentProjectInfo = projectInfo;
            putMonthlyReport(projectInfo, monthlyReport);
        },
    }
})