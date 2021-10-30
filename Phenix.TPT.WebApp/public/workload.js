$(function () {
    fetchWorkday(vue.year, vue.month);
    fetchMyselfCompanyUsers(vue.year, vue.month, vue.state, vue.pageNo);
    fetchWorkSchedule(vue.year, vue.month, vue.state, vue.pageNo, true);
    fetchProjectInfos(vue.year, vue.month, vue.state, vue.pageNo, true);

    var tableHeadTop;
    var tableFootHeight;
    $(window).scroll(function () {
        if (tableHeadTop == null)
            tableHeadTop = $('#table-head').offset().top;
        if (tableFootHeight == null)
            tableFootHeight = $('#table-foot').height();
        let scrollTop = document.documentElement.scrollTop || document.body.scrollTop; //滚动条滚动时距离顶部的距离
        let windowHeight = document.documentElement.clientHeight || document.body.clientHeight; //可视区的高度
        let scrollHeight = document.documentElement.scrollHeight || document.body.scrollHeight; //滚动条的总高度
        //悬浮表头
        if (scrollTop >= tableHeadTop) { //滚动条的滑动距离大于等于表头距离浏览器顶部的距离就固定表头
            var tableHeadFirst = $("#table-head-first").width();
            $("#table-head-first").width(tableHeadFirst);
            $('#table-head').css({ 'position': 'fixed', 'top': 0 });
        } else //反之就不固定表头
            $('#table-head').css({ 'position': 'static' });
        //悬浮表尾
        if (scrollTop + windowHeight > scrollHeight - tableFootHeight) //滚动条滚动到了表尾就不固定表头
            $('#table-foot').css({ 'position': 'static' });
        else { //反之就固定表尾
            var tableFootFirst = $("#table-foot-first").width();
            $("#table-foot-first").width(tableFootFirst);
            $('#table-foot').css({ 'position': 'fixed', 'bottom': 0 });
        }
    });
});

function fetchWorkday(year, month) {
    var result = vue.workdays.find(item => item.Year === year && item.Month === month);
    if (result != null)
        return result;
    base.call({
        path: '/api/workday',
        pathParam: { year: year, month: month },
        onSuccess: function(result) {
            if (!vue.workdays.includes(result))
                vue.workdays.push(result);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取工作日失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
    return null;
}

function fetchMyselfCompanyUsers(year, month, state, pageNo) {
    phAjax.getMyselfCompanyUsers({
        includeDisabled: true,
        onSuccess: function(result) {
            vue.myselfCompanyUsers = result;
            fetchProjectWorkloads(filterMyselfCompanyUsers(result, vue.workSchedule, year, month, state), year, month, state, pageNo);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取公司员工资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function fetchWorkSchedule(year, month, state, pageNo, reset) {
    if (reset)
        vue.workSchedule = null;
    if (vue.workSchedule == null)
        phAjax.getMyself({
            onSuccess: function(result) {
                base.call({
                    path: '/api/work-schedule',
                    pathParam: { manager: result.Id, year: year, month: month },
                    onSuccess: function(result) {
                        vue.workSchedule = result;
                        fetchProjectWorkloads(filterMyselfCompanyUsers(vue.myselfCompanyUsers, result, year, month, state), year, month, state, pageNo);
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
        fetchProjectWorkloads(filterMyselfCompanyUsers(vue.myselfCompanyUsers, vue.workSchedule, year, month, state), year, month, state, pageNo);
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

function fetchProjectWorkloads(filteredMyselfCompanyUserPages, year, month, state, pageNo) {
    if (filteredMyselfCompanyUserPages == null)
        return;
    var workers = [];
    filteredMyselfCompanyUserPages[pageNo].forEach((item, index) => workers.push(item.Id));
    base.call({
        path: '/api/project-workload/all',
        pathParam: { workers: workers.toString(), year: year, month: month },
        onSuccess: function(result) {
            vue.projectWorkloads = result;
            filterProjectInfos(result, vue.projectInfos, year, month, state, pageNo);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取项目工作量失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function fetchProjectInfos(year, month, state, pageNo, reset) {
    if (reset)
        vue.projectInfos = null;
    if (vue.projectInfos == null)
        base.call({
            path: '/api/project-info/all',
            pathParam: { year: year, month: month },
            onSuccess: function(result) {
                vue.projectInfos = result;
                filterProjectInfos(vue.projectWorkloads, result, year, month, state, pageNo);
            },
            onError: function(XMLHttpRequest, textStatus, validityError) {
                vue.projectInfos = null;
                alert('获取项目失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
            },
        });
    else
        filterProjectInfos(vue.projectWorkloads, vue.projectInfos, year, month, state, pageNo);
}

function filterProjectInfos(projectWorkloads, projectInfos, year, month, state, pageNo) {
    if (projectWorkloads == null || projectInfos == null)
        return;
    var filteredProjectInfos = [];
    for (let worker in projectWorkloads)
        if (Object.prototype.hasOwnProperty.call(projectWorkloads, worker))
            projectWorkloads[worker].forEach((projectWorkload, index) => {
                var projectInfo = projectInfos.find(item => item.Id === projectWorkload.PiId);
                if (projectInfo != null) {
                    projectInfo[worker] = projectWorkload;
                    if (!filteredProjectInfos.includes(projectInfo)) {
                        filteredProjectInfos.push(projectInfo);
                        totalProjectWorkload(projectInfo);
                    }
                }
            });
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
    fetchWorkSchedule(year, month, state, pageNo, reset);
    fetchProjectInfos(year, month, state, pageNo, reset);
}

var sample;
$('body').click(function(event) {
    var target = $(event.target);
    if ($(".popover_open").length != 0) {
        if (!target.hasClass('mypopover') &&
            !target.hasClass('popover') &&
            !target.hasClass('popover-content') &&
            !target.hasClass('popover-title') &&
            !target.hasClass('arrow') &&
            !target.hasClass('vertical-center') &&
            !target.hasClass('lidiv') &&
            !target.hasClass('lip') &&
            !target.hasClass('liinput')) {
            $("#storepopover").append(sample);
            $(vm.nowtarget).popover('hide');
        }
    }
});

var vue = new Vue({
    el: '#content',
    data: {
        year: new Date().getFullYear(),
        month: new Date().getMonth() + 1,
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

        workdays: [],

        workSchedule: null,
        myselfCompanyUsers: null,
        filteredMyselfCompanyUserPages: {}, //用于界面绑定的myselfCompanyUsers分页子集
        projectWorkloads: null,
        projectInfos: null,
        filteredProjectInfos: [], //用于界面绑定的projectInfos子集

        date: (localStorage.getItem('date') != undefined && JSON.parse(localStorage.getItem('date'))) ? JSON.parse(localStorage.getItem('date')) : {
            Year: new Date().getDate() > 8 ? new Date().getFullYear() : getPastDate(new Date().getFullYear(), new Date().getMonth() + 1, 1).getFullYear(),
            Month: new Date().getDate() > 8 ? new Date().getMonth() + 1 : getPastDate(new Date().getFullYear(), new Date().getMonth() + 1, 1).getMonth() + 1,
        },

        projects: [],
        coworkers: [],
        projecttime: [],
        totalprojecttime: [],
        workercount: 200,
        nowchoose: [],
        nowchooseindex: "",
        nowchooseindexs: "",
        nowtarget: [],
        canedit: false,
        manageedit: false,
        investigateedit: false,
        developedit: false,
        testedit: false,
        implement: false,
        maintenance: false,
        remaindays: -1,
        myremain: -1,
        dataprojects: [], //存储project数据
        datatotalprojecttime: [],
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

        getTotalWorkload: function(myselfCompanyUser) {
            if (this.projectWorkloads != null)
                if (Object.prototype.hasOwnProperty.call(this.projectWorkloads, myselfCompanyUser.Id)) {
                    var result = 0;
                    this.projectWorkloads[myselfCompanyUser.Id].forEach((item, index) => {
                        item.totalWorkload =
                            item.ManageWorkload +
                            item.InvestigateWorkload +
                            item.DevelopWorkload +
                            item.TestWorkload +
                            item.ImplementWorkload +
                            item.MaintenanceWorkload;
                        result = result + item.totalWorkload;
                    });
                    return result;
                }
            return null;
        },

        showChangeWorkloadDialog: function(e, projectInfo, filteredMyselfCompanyUser) {
            //let ind = 0;
            //for (let i = 0; i < this.workSchedule.length; i++) {
            //    if (this.workSchedule[i].Customer == this.projects[index].Customer) {
            //        ind = i;
            //        break;
            //    }
            //}
            //if ((vm.user.Department.Name == items.Department && (((vm.user.Position.Name == "管理" || vm.user.UserName == items.ProjectManager || vm.user.UserName == items.DevelopManager) && (new Date() >= new Date(this.date.Year, this.date.Month - 1, "01")) && new Date() <= new Date(this.date.Year, this.date.Month, "09")) || ((vm.user.UserName == items.Worker) && (this.workSchedule[ind].Worker != null && this.workSchedule[ind].Worker.indexOf(items.Worker) > -1) && (new Date() >= new Date(this.date.Year, this.date.Month - 1, "01")) && new Date() <= new Date(this.date.Year, this.date.Month, "06"))))) {
            //    this.remaindays = this.projecttime[this.projecttime.length - 1][indexs].Workload;
            //    this.canedit = true;
            //} else {
            //    this.remaindays = -1;
            //    this.canedit = false;
            //}
            //this.manageedit = this.projects[index].ManageWork;
            //this.investigateedit = this.projects[index].InvestigateWork;
            //this.developedit = this.projects[index].DevelopWork;
            //this.testedit = this.projects[index].TestWork;
            //this.implementedit = this.projects[index].ImplementWork;
            //this.maintenanceedit = this.projects[index].MaintenanceWork;
            //if (items.Worker != items.ProjectManager && items.Worker != items.ProjectSupervisor && items.Worker != items.DevelopManager) this.manageedit = false;
            //this.nowtarget = e.target;
            //this.nowchoose = items;
            //this.nowchooseindex = index;
            //this.nowchooseindexs = indexs;
            //生成popover
            $(e.target).popover({
                container: "body",
                trigger: "manual",
                placement: function() {
                    //if (indexs > vm.pageSize / 2)
                    //    return 'left';
                    //else
                        return 'right';
                },
                html: true,
                content: function() {
                    sample = $("#changework");
                    return sample;
                },
            }).on('show.bs.popover', function() {
                $(e.target).addClass("popover_open");
            }).on('hide.bs.popover', function() {
                $(e.target).removeClass("popover_open");
            });
            //显示popover
            if ($(e.target).hasClass("popover_open")) {
                $("#storepopover").append(sample);
                $(e.target).popover("hide");
            } else if (document.getElementsByClassName("popover_open").length > 0) {
                $("#storepopover").append(sample);
                $(".popover_open").popover("hide");
                $(e.target).popover("show");
            } else {
                $(e.target).popover("show");
            }
            if (e && e.stopPropagation)
                //因此它支持W3C的stopPropagation()方法 
                e.stopPropagation();
            else //否则，我们需要使用IE的方式来取消事件冒泡 
                window.event.cancelBubble = true;
        },

        dowrite: function(items, index, indexs, changeprojecttime) {
            writeworkload(this.nowchoose.PW_PI_ID, this.date.Year, this.date.Month, this.nowchoose.Worker, this.nowchoose.ManageWorkloadRole, this.nowchoose.InvestigateWorkloadRole, this.nowchoose.DevelopWorkloadRole, this.nowchoose.TestWorkloadRole, this.nowchoose.ImplementWorkloadRole, this.nowchoose.MaintenanceWorkloadRole, this.nowchooseindex, this.nowchooseindexs, changeprojecttime), 500;
            var preworkload = parseInt(items.Workload);
            items.Workload = parseInt(items.ManageWorkloadRole) + parseInt(items.ImplementWorkloadRole) + parseInt(items.InvestigateWorkloadRole) + parseInt(items.DevelopWorkloadRole) + parseInt(items.TestWorkloadRole) + parseInt(items.MaintenanceWorkloadRole);
            var dataindex = 0;
            while (dataindex < this.dataprojects.length) {
                if (this.dataprojects[dataindex].PI_ID == items.PW_PI_ID)
                    break;
                dataindex++;
            }
            this.totalprojecttime[index] = this.totalprojecttime[index] - preworkload + items.Workload;
            this.datatotalprojecttime[dataindex] = this.totalprojecttime[index];
            this.projecttime[this.projects.length - 1][indexs].Workload = this.projecttime[this.projects.length - 1][indexs].Workload + preworkload - items.Workload;
            this.totalprojecttime[this.projects.length - 1] = this.totalprojecttime[this.projects.length - 1] + preworkload - items.Workload;
            this.datatotalprojecttime[this.datatotalprojecttime.length - 1] = this.totalprojecttime[this.projects.length - 1];
            this.remaindays = this.projecttime[this.projecttime.length - 1][indexs].Workload;
            if (items.Worker == this.user.UserName) {
                this.myremain = this.projecttime[this.projecttime.length - 1][indexs].Workload;
            }
        },

        changedata: function(e, itemname) {
            //var changeprojecttime = Object.assign({}, this.projecttime[this.nowchooseindex][this.nowchooseindexs]);
            var changeprojecttime = deepClone(this.projecttime[this.nowchooseindex][this.nowchooseindexs]);
            if (e.target.value == null || e.target.value == "") {
                e.target.value = 0;
            } else e.target.value = ~~e.target.value;
            var preqita = this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload;
            switch (itemname) {
                case '0':
                    var preitemitem = this.nowchoose.ManageWorkloadRole;
                    if (e.target.value - preitemitem <= preqita) {
                        this.nowchoose.ManageWorkloadRole = e.target.value;
                        this.projecttime[this.nowchooseindex][this.nowchooseindexs].ManageWorkloadRole = e.target.value;
                        this.dowrite(this.nowchoose, this.nowchooseindex, this.nowchooseindexs, changeprojecttime);
                    } else {
                        e.target.value = preitemitem;
                        zdalert('系统提示', '您只有' + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + '天待分配');
                    }
                    break;
                case '1':
                    var preitemitem = this.nowchoose.InvestigateWorkloadRole;
                    if (e.target.value - preitemitem <= preqita) {
                        this.nowchoose.InvestigateWorkloadRole = e.target.value;
                        this.projecttime[this.nowchooseindex][this.nowchooseindexs].InvestigateWorkloadRole = e.target.value;
                        this.dowrite(this.nowchoose, this.nowchooseindex, this.nowchooseindexs, changeprojecttime);
                    } else {
                        e.target.value = preitemitem;
                        zdalert('系统提示', '您只有' + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + '天待分配');
                    }
                    break;
                case '2':
                    var preitemitem = this.nowchoose.DevelopWorkloadRole;
                    if (e.target.value - preitemitem <= preqita) {
                        this.nowchoose.DevelopWorkloadRole = e.target.value;
                        this.projecttime[this.nowchooseindex][this.nowchooseindexs].DevelopWorkloadRole = e.target.value;
                        this.dowrite(this.nowchoose, this.nowchooseindex, this.nowchooseindexs, changeprojecttime);
                    } else {
                        e.target.value = preitemitem;
                        zdalert('系统提示', '您只有' + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + '天待分配');
                    }
                    break;
                case '3':
                    var preitemitem = this.nowchoose.TestWorkloadRole;
                    if (e.target.value - preitemitem <= preqita) {
                        this.nowchoose.TestWorkloadRole = e.target.value;
                        this.projecttime[this.nowchooseindex][this.nowchooseindexs].TestWorkloadRole = e.target.value;
                        this.dowrite(this.nowchoose, this.nowchooseindex, this.nowchooseindexs, changeprojecttime);
                    } else {
                        e.target.value = preitemitem;
                        zdalert('系统提示', '您只有' + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + '天待分配');
                    }
                    break;
                case '4':
                    var preitemitem = this.nowchoose.ImplementWorkloadRole;
                    if (e.target.value - preitemitem <= preqita) {
                        this.nowchoose.ImplementWorkloadRole = e.target.value;
                        this.projecttime[this.nowchooseindex][this.nowchooseindexs].ImplementWorkloadRole = e.target.value;
                        this.dowrite(this.nowchoose, this.nowchooseindex, this.nowchooseindexs, changeprojecttime);
                    } else {
                        e.target.value = preitemitem;
                        zdalert('系统提示', '您只有' + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + '天待分配');
                    }
                    break;
                case '5':
                    var preitemitem = this.nowchoose.MaintenanceWorkloadRole;
                    if (e.target.value - preitemitem <= preqita) {
                        this.nowchoose.MaintenanceWorkloadRole = e.target.value;
                        this.projecttime[this.nowchooseindex][this.nowchooseindexs].MaintenanceWorkloadRole = e.target.value;
                        this.dowrite(this.nowchoose, this.nowchooseindex, this.nowchooseindexs, changeprojecttime);
                    } else {
                        e.target.value = preitemitem;
                        zdalert('系统提示', '您只有' + this.projecttime[this.projects.length - 1][indexs].Workload + '天待分配');
                    }
                    break;
                default:
                    break;
            }
        },
    }
})