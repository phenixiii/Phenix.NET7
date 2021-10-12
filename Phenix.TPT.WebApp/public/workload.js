$(function() {
    var tableHeadTop;
    $(window).scroll(function() {
        //获取要定位元素距离浏览器顶部的距离
        if (tableHeadTop == null)
            tableHeadTop = $("#tablehead").offset().top;
        //获取滚动条的滑动距离
        var scrollTop = $(this).scrollTop();
        //滚动条的滑动距离大于等于定位元素距离浏览器顶部的距离，就固定，反之就不固定
        if (scrollTop >= tableHeadTop) {
            var thfirst = $('#thfirst').width();
            $('#thfirst').width(thfirst);
            $('#tablehead').css({ 'position': 'fixed', 'top': 0 });
        } else if (scrollTop < tableHeadTop) {
            $('#tablehead').css({ 'position': 'static' });
        }
    });
});

function extractMyselfCompanyUsers() {
    phAjax.getMyselfCompanyUsers({
        includeDisabled: true,
        onSuccess: function (result) {
            vue.myselfCompanyUsers = result;
            if (vue.queryPerson != null && vue.projectInfos != null)
                filterProjectInfos(vue.projectInfos); //如果有按负责人姓名查询的可能性则要等获取到公司员工资料后再执行本函数
            vue.$forceUpdate();
        },
        onError: function (XMLHttpRequest, textStatus, validityError) {
            vue.logonHint = XMLHttpRequest.responseText;
            alert('获取公司员工资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
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
            onSuccess: function (result) {
                vue.projectInfos = result;
                filterProjectInfos(result);
            },
            onError: function (XMLHttpRequest, textStatus, validityError) {
                vue.projectInfos = null;
                alert('获取项目失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
            },
        });
    else
        filterProjectInfos(vue.projectInfos);
}


function fetchWorday(year, month) {
    //phAjax.ajax({
    //    uri: "Workday?year=" + year + "&month=" + month,
    //    onSuccess: function(result) {
    //        vm.workday = result;
    //    },
    //    onError: function(XMLHttpRequest, textStatus, errorThrown) {
    //        console.log("调用countService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
    //    },
    //});
};

function fetchWorkerCount(year, month) {
    //phAjax.ajax({
    //    uri: "Worker/WorkerCount?year=" + year + "&month=" + month,
    //    onSuccess: function(result) {
    //        vm.workercount = result;
    //    },
    //    onError: function(XMLHttpRequest, textStatus, errorThrown) {
    //        console.log("调用workdayService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
    //    },
    //});
};

var writeworkload = _.debounce(function(PW_PI_ID, year, month, worker, manageWorkloadRole, investigateWorkloadRole, developWorkloadRole, testWorkloadRole, implementWorkloadRole, maintenanceWorkloadRole, index, indexs, changeprojecttime) {
    //phAjax.ajax({
    //    type: "POST",
    //    uri: "MonthlyWorkload/?id=" + PW_PI_ID + "&year=" + year + "&month=" + month + "&worker=" + encodeURI(worker) + "&manageWorkloadRole=" + manageWorkloadRole + "&investigateWorkloadRole=" + investigateWorkloadRole + "&developWorkloadRole=" + developWorkloadRole + "&testWorkloadRole=" + testWorkloadRole + "&implementWorkloadRole=" + implementWorkloadRole + "&maintenanceWorkloadRole=" + maintenanceWorkloadRole,
    //    onComplete: function(XMLHttpRequest, textStatus) {
    //        if (XMLHttpRequest.status == 200) {} else {
    //            console.log("Warn!");
    //        }
    //    },
    //    onError: function(XMLHttpRequest, textStatus, errorThrown) {
    //        zdalert('系统提示', "工作量写入失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
    //        vm.recover(index, indexs, changeprojecttime);
    //    },
    //})
}, 500);

function fetchworkSchedule(year, month) {
    //phAjax.ajax({
    //    uri: "WorkSchedule/?year=" + year + "&month=" + month,
    //    onSuccess: function(result) {
    //        vm.workSchedule = result;
    //    },
    //    onError: function(XMLHttpRequest, textStatus, errorThrown) {
    //        console.log("调用scheduleService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
    //    },
    //});
}

function fetchProject(year, month) {
    //phAjax.ajax({
    //    uri: "ProjectInfo/?year=" + year + "&month=" + month,
    //    onSuccess: function(result) {
    //        vm.dataprojects = [];
    //        if (result.length <= 2 && vm.user.Position.Name == "管理") {
    //            window.location.href = "projectinfo.html"
    //        } else if (result.length > 2) {
    //            var xiujia;
    //            var qita;
    //            for (var i = 0; i < result.length; i++) {
    //                if (result[i].ProjectName == "休假") {
    //                    xiujia = result[i];
    //                    result.splice(i, 1);
    //                    i--;
    //                } else if (result[i].ProjectName == "其他") {
    //                    qita = result[i];
    //                    result.splice(i, 1);
    //                    i--;
    //                }
    //            }
    //            result.sort(function(a, b) {
    //                return a.ProjectName.localeCompare(b.ProjectName, 'zh-Hans-CN', {
    //                    sensitivity: 'accent'
    //                })
    //            });
    //            vm.dataprojects = result;
    //            vm.dataprojects.push(xiujia);
    //            vm.dataprojects.push(qita);
    //            fetchTotalWorkloads(vm.date.Year, vm.date.Month, vm.workercount);
    //        }
    //    },
    //    onError: function(XMLHttpRequest, textStatus, errorThrown) {
    //        console.log("调用projectService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
    //    },
    //});
}

/*function fetchWorker(year, month, pageSize, pageNo) {
    phAjax.beforeSend(function(XMLHttpRequest) {
        jQuery.support.cors = true;
        var nonce = Math.round(Math.random() * 999999999999999);
        var timestamp = new Date().toISOString();
        XMLHttpRequest.setRequestHeader("Phenix-Authorization",
            localStorage.value + "," + nonce + "," + timestamp + "," +
            CryptoJS.AES.encrypt(nonce + timestamp, localStorage.text, { iv: localStorage.text, mode: CryptoJS.mode.CBC }));
    });
    phAjax.ajax({
        uri: "Worker/WorkerNames?year=" + year + "&month=" + month + "&pageSize=" + pageSize + "&pageNo=" + pageNo,
        onSuccess: function(result) {
            vm.coworkers = [];
            if (result.length != 0) {
                var objs = eval(result);
                vm.coworkers = objs;
                vm.coworkers.sort(function(a, b) {
                    return a.localeCompare(b, 'zh-Hans-CN', {
                        sensitivity: 'accent'
                    })
                });
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用workerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}*/

function fetchMonthlyWorkloads(year, month, pageSize, pageNo, username, option) {
    //phAjax.ajax({
    //    uri: "MonthlyWorkload/?year=" + year + "&month=" + month + "&pageSize=" + pageSize + "&pageNo=" + pageNo + "&username=" + encodeURI(username) + "&option=" + option,
    //    onSuccess: function(result) {
    //        vm.projects = [];
    //        vm.totalprojecttime = [],
    //            vm.projecttime = [];
    //        var count = -1;
    //        if (result.length != 0) {
    //            result.sort(function(a, b) {
    //                return a.ProjectName.localeCompare(b.ProjectName, 'zh-Hans-CN', {
    //                    sensitivity: 'accent'
    //                }) * 2 + a.Worker.localeCompare(b.Worker, 'zh-Hans-CN', {
    //                    sensitivity: 'accent'
    //                })
    //            });
    //            var temptime = new Array();
    //            var j = 0;
    //            var xiujia = new Array();
    //            var qita = new Array();
    //            for (var i = 0; i < result.length; i++) {
    //                if (result[i].ProjectName != "其他" && result[i].ProjectName != "休假") {
    //                    if (i == 0 || result[i - 1].ProjectName != result[i].ProjectName) {
    //                        count++;
    //                        temptime[count] = new Array();
    //                        while (j < vm.dataprojects.length) {
    //                            if (vm.dataprojects[j].ProjectName == result[i].ProjectName) {
    //                                vm.projects.push(vm.dataprojects[j]);
    //                                vm.totalprojecttime.push(vm.datatotalprojecttime[j]);
    //                                break;
    //                            }
    //                            j++;
    //                        }
    //                    }
    //                    temptime[count].push(result[i]);
    //                } else if (result[i].ProjectName == "其他") {
    //                    if (result[i].Worker == vm.user.UserName) {
    //                        vm.myremain = result[i].Workload;
    //                    }
    //                    qita.push(result[i]);
    //                } else if (result[i].ProjectName == "休假") {
    //                    xiujia.push(result[i]);
    //                }
    //            }
    //            vm.projects.push(vm.dataprojects[vm.dataprojects.length - 2]);
    //            vm.projects.push(vm.dataprojects[vm.dataprojects.length - 1]);
    //            vm.totalprojecttime.push(vm.datatotalprojecttime[vm.datatotalprojecttime.length - 2]);
    //            vm.totalprojecttime.push(vm.datatotalprojecttime[vm.datatotalprojecttime.length - 1]);
    //            temptime.push(xiujia);
    //            temptime.push(qita);
    //            vm.projecttime = deepClone(temptime);
    //            /*for (var j = 0; j < temptime.length; j++) {
    //                if (temptime[j].length > 1) {
    //                    temptime[j].sort(function(a, b) {
    //                        return a.Worker.localeCompare(b.Worker, 'zh-Hans-CN', {
    //                            sensitivity: 'accent'
    //                        })
    //                    });
    //                }
    //                vm.projecttime.push(temptime[j]);
    //            }*/
    //        }
    //    },
    //    onError: function(XMLHttpRequest, textStatus, errorThrown) {
    //        console.log("调用workloadService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
    //    },
    //});
}

function fetchTotalWorkloads(year, month, workercount) {
    //phAjax.ajax({
    //    //uri: "MonthlyWorkload/?year=" + year + "&month=" + month + "&pageSize=" + workercount + "&pageNo=" + 1 + "&username=null&option=0",
    //    uri: "ProjectWorkloadSum/?year=" + year + "&month=" + month,
    //    onSuccess: function(result) {
    //        //vm.totalworkload = result;
    //        var total = new Array(vm.dataprojects.length);
    //        var offset = 0;
    //        if (result.length != 0) {
    //            result.sort(function(a, b) {
    //                return a.ProjectName.localeCompare(b.ProjectName, 'zh-Hans-CN', {
    //                    sensitivity: 'accent'
    //                })
    //            });
    //            console.log(vm.dataprojects);
    //            console.log(result);
    //            for (var i = 0; i < result.length; i++) {
    //                if (result[i].ProjectName == "其他") {
    //                    total[vm.dataprojects.length - 1] = result[i].Workload;
    //                    offset++;
    //                } else if (result[i].ProjectName == "休假") {
    //                    offset++;
    //                    total[vm.dataprojects.length - 2] = result[i].Workload;
    //                } else {
    //                    if (result[i].ProjectName != vm.dataprojects[i - offset].ProjectName)
    //                        console.log(i + '&' + (i - offset));
    //                    total[i - offset] = result[i].Workload;
    //                }
    //            }
    //            /*var total = new Array(vm.dataprojects.length);
    //            var index = 0;
    //            var offset = 0;
    //            for (var i = 0; i < result.length; i++) {
    //                if (result[i].ProjectName == "其他" && result[i].Worker == vm.user.UserName) {
    //                    vm.myremain = result[i].Workload;
    //                }
    //                if (i % workercount == 0) {
    //                    if (result[i].ProjectName == "其他") {
    //                        index = vm.dataprojects.length - 1;
    //                        offset++;
    //                    } else if (result[i].ProjectName == "休假") {
    //                        index = vm.dataprojects.length - 2;
    //                        offset++;
    //                    } else {
    //                        index = Math.floor(i / workercount) - offset;
    //                    }
    //                    total[index] = 0;
    //                }
    //                total[index] += result[i].Workload;*/
    //        }
    //        vm.datatotalprojecttime = total;
    //        fetchMonthlyWorkloads(vm.date.Year, vm.date.Month, vm.pageSize, vm.pageNo, vm.user.UserName, vm.filterstate);
    //    },
    //    onError: function(XMLHttpRequest, textStatus, errorThrown) {
    //        console.log("调用workloadService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
    //    },
    //});
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
        state: 0,
        stateTitle: [
            '当月自己的工作量', //含自己管理工作档期内的员工
            '当月全员的工作量',
        ],
        filterStateTitle: [
            '显示当月全员的工作量',
            '显示当月自己的工作量',
        ],
        filterTimeInterval: {
            year: new Date().getFullYear(),
            month: new Date().getMonth() + 1,
        },

        myselfCompanyUsers: null,
        filteredMyselfCompanyUsers: [], //用于界面绑定的myselfCompanyUsers子集
        Workloads: null,
        filteredWorkloads: [], //用于界面绑定的projectInfos子集

        date: (localStorage.getItem('date') != undefined && JSON.parse(localStorage.getItem('date'))) ? JSON.parse(localStorage.getItem('date')) : {
            Year: new Date().getDate() > 8 ? new Date().getFullYear() : getPastDate(new Date().getFullYear(), new Date().getMonth() + 1, 1).getFullYear(),
            Month: new Date().getDate() > 8 ? new Date().getMonth() + 1 : getPastDate(new Date().getFullYear(), new Date().getMonth() + 1, 1).getMonth() + 1,
        },
        projects: [],
        coworkers: [],
        workday: 0,
        projecttime: [],
        totalprojecttime: [],
        pageSize: 10,
        pageNo: 1,
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
        workSchedule: [],
    },
    mounted: function() {
        //this.user = globaluser;
        var totalwidth = 3 / 4 * $(".pad-botm").width();
        var width1 = 55;
        this.pageSize = Math.floor(totalwidth / width1) - 1;
        fetchWorkerCount(this.date.Year, this.date.Month);
        fetchProject(this.date.Year, this.date.Month);
        fetchWorday(this.date.Year, this.date.Month);
        fetchworkSchedule(this.date.Year, this.date.Month);
        localStorage.removeItem('date');
        localStorage.removeItem('filterstate');
    },
    watch: {
        pageNo: function() {
            if (this.pageNo == 1) {
                $('.fa-step-backward').addClass("myhide");
                $('.fa-step-forward').removeClass("myhide");
            } else if (this.pageNo == this.maxpage) {
                $('.fa-step-forward').addClass("myhide");
                $('.fa-step-backward').removeClass("myhide");
            } else {
                $('.fa-step-backward').removeClass("myhide");
                $('.fa-step-forward').removeClass("myhide");
            }
        }
    },
    computed: {
        maxpage: function() {
            return Math.ceil(this.workercount / this.pageSize);
        },
    },
    methods: {
        filter: function() {
            if (this.user.Department.Name == "公司管理" || this.user.Position.Name == "管理") {} else {
                if ((this.filterstate == 2 && !(this.user.Roles.项目管理)) || this.filterstate == 3) {
                    this.filterstate = 0
                } else this.filterstate++;
                fetchMonthlyWorkloads(this.date.Year, this.date.Month, this.pageSize, this.pageNo, this.user.UserName, this.filterstate);
            }
        },
        showproject: function (index) {
            var project = this.projects[index];
            if (this.user.Department.Name == "公司管理" || this.user.Position.Name == "管理" || this.user.Roles.质监质检 ||
                this.user.UserName == project.ProjectManager || this.user.UserName == project.DevelopManager) {//this.user.Roles.项目管理) {
                localStorage.setItem('project', JSON.stringify(project));
                window.location.href = "projectinfo.html";
                localStorage.setItem('filterstate', JSON.stringify(this.filterstate));
                localStorage.setItem('date', JSON.stringify(this.date));
            }
        },
        showpopover: function(e, items, index, indexs) {
            let ind = 0;
            for (let i = 0; i < this.workSchedule.length; i++) {
                if (this.workSchedule[i].Customer == this.projects[index].Customer) {
                    ind = i;
                    break;
                }
            }
            if ((vm.user.Department.Name == items.Department && (((vm.user.Position.Name == "管理" || vm.user.UserName == items.ProjectManager || vm.user.UserName == items.DevelopManager) && (new Date() >= new Date(this.date.Year, this.date.Month - 1, "01")) && new Date() <= new Date(this.date.Year, this.date.Month, "09")) || ((vm.user.UserName == items.Worker) && (this.workSchedule[ind].Worker != null && this.workSchedule[ind].Worker.indexOf(items.Worker) > -1) && (new Date() >= new Date(this.date.Year, this.date.Month - 1, "01")) && new Date() <= new Date(this.date.Year, this.date.Month, "06"))))) {
                this.remaindays = this.projecttime[this.projecttime.length - 1][indexs].Workload;
                this.canedit = true;
            } else {
                this.remaindays = -1;
                this.canedit = false;
            }
            this.manageedit = this.projects[index].ManageWork;
            this.investigateedit = this.projects[index].InvestigateWork;
            this.developedit = this.projects[index].DevelopWork;
            this.testedit = this.projects[index].TestWork;
            this.implementedit = this.projects[index].ImplementWork;
            this.maintenanceedit = this.projects[index].MaintenanceWork;
            if (items.Worker != items.ProjectManager && items.Worker != items.ProjectSupervisor && items.Worker != items.DevelopManager) this.manageedit = false;
            this.nowtarget = e.target;
            this.nowchoose = items;
            this.nowchooseindex = index;
            this.nowchooseindexs = indexs;
            //生成popover
            $(e.target).popover({
                container: "body",
                trigger: "manual",
                placement: function() {
                    if (indexs > vm.pageSize / 2) return 'left';
                    else return 'right';
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
            if (e && e.stopPropagation) {
                //因此它支持W3C的stopPropagation()方法 
                e.stopPropagation();
            }
            //否则，我们需要使用IE的方式来取消事件冒泡 
            else window.event.cancelBubble = true;
        },
        listenToMyBoy: function(somedata) {
            this.user = somedata;
        },
        addYear: function() { //有一点问题成不成功都在fetch
            var currentyear = this.date.Year;
            if (currentyear + 1 > new Date().getFullYear()) {
                zdalert('系统提示', "您不能查看未来~");
                return false;
            } else {
                if (currentyear + 1 == new Date().getFullYear() && this.date.Month > new Date().getMonth() + 1) {
                    this.date.Year = currentyear + 1;
                    this.date.Month = new Date().getMonth() + 1;
                } else this.date.Year = currentyear + 1;
                fetchWorkerCount(this.date.Year, this.date.Month);
                fetchProject(this.date.Year, this.date.Month);
                fetchWorday(this.date.Year, this.date.Month);
                fetchworkSchedule(this.date.Year, this.date.Month);
            }
        },
        addMonth: function() {
            var currentmonth = this.date.Month;
            if (this.date.Year >= new Date().getFullYear() && currentmonth + 1 > new Date().getMonth() + 1) {
                zdalert('系统提示', "您不能查看未来~");
                return false;
            } else {
                if (currentmonth == 12) {
                    this.date.Year += 1;
                    this.date.Month = 1;
                } else this.date.Month = currentmonth + 1;
                fetchWorkerCount(this.date.Year, this.date.Month);
                fetchProject(this.date.Year, this.date.Month);
                fetchWorday(this.date.Year, this.date.Month);
                fetchworkSchedule(this.date.Year, this.date.Month);
            }
        },
        minusYear: function() {
            var currentyear = this.date.Year;
            if (currentyear - 1 > new Date().getFullYear()) {
                zdalert('系统提示', "这个bug我不懂");
                return false;
            } else {
                if (currentyear - 1 < 2018) {
                    zdalert('系统提示', "那里什么都没有~");
                } else this.date.Year = currentyear - 1;
                if (this.date.Year == 2018 && this.date.Month < 9)
                    this.date.Month = 9;
                fetchWorkerCount(this.date.Year, this.date.Month);
                fetchProject(this.date.Year, this.date.Month);
                fetchWorday(this.date.Year, this.date.Month);
                fetchworkSchedule(this.date.Year, this.date.Month);
            }
        },
        minusMonth: function() {
            var currentmonth = this.date.Month;
            if (this.date.Year == 2018 && currentmonth - 1 < 9) {
                zdalert('系统提示', "那里什么都没有~");
            } else {
                if (currentmonth == 1) {
                    this.date.Year -= 1;
                    this.date.Month = 12;
                } else this.date.Month = currentmonth - 1;
                fetchWorkerCount(this.date.Year, this.date.Month);
                fetchProject(this.date.Year, this.date.Month);
                fetchWorday(this.date.Year, this.date.Month);
                fetchworkSchedule(this.date.Year, this.date.Month);
            }
        },
        prepage: function() {
            if (this.pageNo == 1) {
                zdalert('系统提示', "已经是第一页了");
            } else {
                this.pageNo -= 1;
                fetchMonthlyWorkloads(this.date.Year, this.date.Month, this.pageSize, this.pageNo, this.user.UserName, this.filterstate);
            }
        },
        nextpage: function() {
            this.pageNo += 1;
            if (this.pageNo > this.maxpage) {
                zdalert('系统提示', "没有啦");
                this.pageNo -= 1;
            } else {
                fetchMonthlyWorkloads(this.date.Year, this.date.Month, this.pageSize, this.pageNo, this.user.UserName, this.filterstate);
            }
        },
        firstpage: function() {
            this.pageNo = 1;
            fetchMonthlyWorkloads(this.date.Year, this.date.Month, this.pageSize, this.pageNo, this.user.UserName, this.filterstate);
        },
        lastpage: function() {
            this.pageNo = this.maxpage;
            fetchMonthlyWorkloads(this.date.Year, this.date.Month, this.pageSize, this.pageNo, this.user.UserName, this.filterstate);
        },
        recover: function(index, indexs, changeprojecttime) {
            this.projecttime[index][indexs].Workload = changeprojecttime.Workload;
            this.projecttime[index][indexs].ManageWorkloadRole = changeprojecttime.ManageWorkloadRole;
            this.projecttime[index][indexs].InvestigateWorkloadRole = changeprojecttime.InvestigateWorkloadRole;
            this.projecttime[index][indexs].DevelopWorkloadRole = changeprojecttime.DevelopWorkloadRole;
            this.projecttime[index][indexs].TestWorkloadRole = changeprojecttime.TestWorkloadRole;
            this.projecttime[index][indexs].ImplementWorkloadRole = changeprojecttime.ImplementWorkloadRole;
            this.projecttime[index][indexs].MaintenanceWorkloadRole = changeprojecttime.MaintenanceWorkloadRole;
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
                        zdalert('系统提示', "您只有" + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + "天待分配");
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
                        zdalert('系统提示', "您只有" + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + "天待分配");
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
                        zdalert('系统提示', "您只有" + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + "天待分配");
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
                        zdalert('系统提示', "您只有" + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + "天待分配");
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
                        zdalert('系统提示', "您只有" + this.projecttime[this.projects.length - 1][this.nowchooseindexs].Workload + "天待分配");
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
                        zdalert('系统提示', "您只有" + this.projecttime[this.projects.length - 1][indexs].Workload + "天待分配");
                    }
                    break;
                default:
                    break;
            }
        },
        changexiujia: function(items, index, indexs, e) {
            if (e.target.value == null || e.target.value == "") {
                e.target.value = 0;
            };
            var prexiujia = parseInt(items.Workload);
            var preqita = this.projecttime[this.projects.length - 1][indexs].Workload;
            if (e.target.value <= prexiujia + preqita) {
                items.Workload = parseInt(e.target.value);
                writeworkload(items.PW_PI_ID, this.date.Year, this.date.Month, items.Worker, items.Workload, 0, 0, 0, 0, 0);
                this.totalprojecttime[index] = this.totalprojecttime[index] - prexiujia + items.Workload;
                this.projecttime[this.projects.length - 1][indexs].Workload = this.projecttime[this.projects.length - 1][indexs].Workload + prexiujia - items.Workload;
                this.totalprojecttime[this.projects.length - 1] = this.totalprojecttime[this.projects.length - 1] + prexiujia - items.Workload;
                var dataindex = this.datatotalprojecttime.length - 1;
                this.datatotalprojecttime[dataindex - 1] = this.totalprojecttime[index];
                this.datatotalprojecttime[dataindex] = this.totalprojecttime[this.projects.length - 1]
            } else {
                if (prexiujia == 0)
                    e.target.value = "";
                else e.target.value = prexiujia;
                zdalert('系统提示', "您只有" + (prexiujia + preqita) + "天待分配");
            }
        },
    }
})