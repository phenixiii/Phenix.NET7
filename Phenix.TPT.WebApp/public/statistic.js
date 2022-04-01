$(document).ready(function() {
    navbarControl();
});
var regyear = /\d{4}(?=\.)/;
var regmonth = /\.(\d{1,2})/;

function fetchworker(start, end, startyear, startmonth, endyear, endmonth, department) {
    phAjax.ajax({
        uri: "Worker/IntervalWorker?start=" + start + "&end=" + end + "&department=" + encodeURI(department),
        onSuccess: function(result) {
            if (result.length > 0) {
                vm.worker = [];
                vm.worker = result;
                for (var j = 0; j < result.length; j++) {
                    vm.worker[j].symbolSize = 30 * Math.sqrt(vm.worker[j].maxvalue / (vm.worker[j].maxvalue + vm.worker[j].secondmaxvalue));
                    vm.worker[j].category = "worker"
                    vm.worker[j].itemStyle = {
                        color: getcolor(vm.worker[j].max),
                        borderColor: getcolor(vm.worker[j].secondmax),
                        borderType: 'solid',
                        borderWidth: 30 - 30 * Math.sqrt(vm.worker[j].maxvalue / (vm.worker[j].maxvalue + vm.worker[j].secondmaxvalue)),
                    }
                }
                switch (vm.charttype) {
                    case 'circlelink':
                        fetchlink(startyear, startmonth, endyear, endmonth, department);
                        break;
                    case 'forcelink':
                        fetchlink(startyear, startmonth, endyear, endmonth, department);
                        break;
                    case 'managelink':
                        fetchmanage(startyear, startmonth, endyear, endmonth, department);
                        break;
                    case 'projectlink':
                        fetchProject(startyear, startmonth, endyear, endmonth, department);
                        break;
                }
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用workerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchlink(startyear, startmonth, endyear, endmonth, department) {
    phAjax.ajax({
        uri: "WorkerWorker/WorkerRelationship?startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth + "&department=" + encodeURI(department),
        onSuccess: function(result) {
            if (result.length > 0) {
                vm.bargraphcount++;
                var chartbody = document.getElementById('Graph');
                var div = document.createElement("div");
                div.style.height = vm.chartheight + 'px';
                chartbody.appendChild(div);
                vm.workerrelation = result;
                if (vm.charttype == "circlelink")
                    vm.Relationdiagram(div, department);
                else if (vm.charttype == "forcelink")
                    vm.forcelink(div, department);
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用linkService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchProject(startyear, startmonth, endyear, endmonth, department) {
    phAjax.ajax({
        uri: "ProjectInfo/?startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth,
        onSuccess: function(result) {
            if (result.length != 0) {
                for (var i = 0; i < result.length; i++) {
                    vm.worker.push({
                        name: result[i].name,
                        symbolSize: 20 + 40 / result[result.length - 1].rank * result[i].rank,
                        itemStyle: {
                            color: 'rgb(188,188,188)',
                        },
                        category: "project"
                    })
                }
            }
            fetchprojectlink(startyear, startmonth, endyear, endmonth, department);
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用linkService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchprojectlink(startyear, startmonth, endyear, endmonth, department) {
    phAjax.ajax({
        uri: "ProjectActivity/ProjectWorker?startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth + "&department=" + encodeURI(department),
        onSuccess: function(result) {
            if (result.length > 0) {
                vm.bargraphcount++;
                var chartbody = document.getElementById('Graph');
                var div = document.createElement("div");
                div.style.height = vm.chartheight + 'px';
                chartbody.appendChild(div);
                vm.workerrelation = [];
                //删除没有被工作的项目？
                vm.workerrelation = result;
                vm.forcelink(div, department);
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用linkService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchmanage(startyear, startmonth, endyear, endmonth, department) {
    phAjax.ajax({
        uri: "ManageWorker/?startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth + "&department=" + encodeURI(department),
        onSuccess: function(result) {
            if (result.length > 0) {
                vm.bargraphcount++;
                var chartbody = document.getElementById('Graph');
                var div = document.createElement("div");
                div.style.height = vm.chartheight + 'px';
                chartbody.appendChild(div);
                vm.workerrelation = result;
                //vm.worker减少
                vm.forcelink(div, department);
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用linkService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchAllWorker() {
    phAjax.ajax({
        uri: "Worker/AllWorker?",
        onSuccess: function(result) {
            vm.allworker = result;
            vm.allitem = result;
            if (vm.allitem.indexOf(globaluser.UserName) > -1) {
                vm.option = vm.user.UserName;
            } else if (result.length != 0) {
                vm.option = result[0];
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allworkerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchDepartmentWorker() {
    phAjax.ajax({
        uri: "Worker/DepartmentWorker?option=" + encodeURI(vm.option),
        onSuccess: function(result) {
            if (result.length != 0) {
                vm.departmentworker = result;
                vm.Parallel();
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allworkerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchDepartmentProject() {
    phAjax.ajax({
        uri: "ProjectInfo/DepartmentProject?option=" + encodeURI(vm.option),
        onSuccess: function(result) {
            if (result.length != 0) {
                vm.departmentproject = [];
                for (var i = 0; i < result.length; i++) {
                    vm.departmentproject.push(result[i].ProjectName);
                }
                vm.Parallel();
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allprojectService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchAllProject() {
    phAjax.ajax({
        uri: "ProjectInfo/AllProject?",
        onSuccess: function(result) {
            vm.allproject = result;
            vm.allitem = result;
            if (result.length > 0) {
                vm.option = result[0];
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allprojectService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchAllDepartment() {
    phAjax.ajax({
        uri: "Department/WorkDepartment?",
        onSuccess: function(result) {
            vm.alldepartment = result;
            vm.allitem = result;
            if (vm.alldepartment.indexOf(globaluser.Department.Name) > -1) {
                vm.option = globaluser.Department.Name;
            } else if (result.length > 0) {
                vm.option = result[0];
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchBarWorkload(category, option, startyear, startmonth, endyear, endmonth, bargraph, index) { //部门
    phAjax.ajax({
        uri: "ProjectActivity/IntervalWorkload?category=" + category + "&option=" + encodeURI(option) + "&startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth,
        onSuccess: function(result) {
            vm.time = [];
            vm.manage = [];
            vm.investigate = [];
            vm.develop = [];
            vm.test = [];
            vm.implement = [];
            vm.maintenance = [];
            vm.vacation = [];
            vm.rest = [];
            if (result.length != 0) {
                var chartbody = document.getElementById('Graph');
                var div = document.createElement("div");
                div.className += 'itemchart';
                chartbody.appendChild(div);
                bargraph[vm.bargraphcount] = echarts.init(div);
                for (var i = 0; i < result.length; i++) {
                    vm.time.push(result[i].time);
                    vm.manage.push(result[i].Manage);
                    vm.investigate.push(result[i].Investigate);
                    vm.develop.push(result[i].Develop);
                    vm.test.push(result[i].Test);
                    vm.implement.push(result[i].Implement);
                    vm.maintenance.push(result[i].Maintenance);
                    vm.vacation.push(result[i].Vacation);
                    vm.rest.push(result[i].Rest);
                }
                vm.Bargraph(bargraph, vm.bargraphcount, option, false, true, null);
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchRedarWorkload(category, option, startyear, startmonth, endyear, endmonth) {
    phAjax.ajax({
        uri: "ProjectActivity/IntervalWorkload?category=" + category + "&option=" + encodeURI(option) + "&startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth,
        onSuccess: function(result) {
            var manage = 0;
            var investigate = 0;
            var develop = 0;
            var test = 0;
            var implement = 0;
            var maintenance = 0;
            for (var i = 0; i < result.length; i++) {
                manage += parseInt(result[i].Manage);
                investigate += parseInt(result[i].Investigate);
                develop += parseInt(result[i].Develop);
                test += parseInt(result[i].Test);
                implement += parseInt(result[i].Implement);
                maintenance += parseInt(result[i].Maintenance);
            }
            vm.radardata = [manage, investigate, develop, test, implement, maintenance]
            vm.Radar();
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchManagerProject(department, startyear, startmonth, endyear, endmonth) {
    phAjax.ajax({
        uri: "ProjectInfo/?syear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth + "&option=" + encodeURI(department),
        onSuccess: function(result) {
            var realparallel = new Array();
            var customer = [];
            var worker = [];
            result.forEach(function(c) {
                if (!(c.Customer in customer)) {
                    var temp = result.filter(function(m) {
                        return m.Customer == c.Customer;
                    })
                    var annual = 0;
                    var contamount = 0;
                    temp.forEach(function(n) {
                        annual = parseFloat((annual + parseFloat(n.AnnualAmount)).toFixed(2));
                        contamount = parseFloat((contamount + parseFloat(n.ContAmount)).toFixed(2));
                    })
                    customer[c.Customer] = {
                        content: c.Customer + ':' + annual + '/' + contamount,
                        count: 0,
                        amount: annual,
                    }
                }
                worker = handleworker(worker, c.ProjectManager, result);
                worker = handleworker(worker, c.DevelopManager, result);
                //worker = handleworker(worker, c.ProjectSupervisor, result);
                if (!realparallel.includes([worker[c.ProjectManager].content, customer[c.Customer].content]) && customer[c.Customer].amount > 0 && worker[c.ProjectManager].amount > 0) {
                    realparallel.push([worker[c.ProjectManager].content, customer[c.Customer].content])
                    worker[c.ProjectManager].count++;
                    customer[c.Customer].count++;
                }
                if (!realparallel.includes([worker[c.DevelopManager].content, customer[c.Customer].content]) && customer[c.Customer].amount > 0 && worker[c.ProjectManager].amount > 0) {
                    realparallel.push([worker[c.DevelopManager].content, customer[c.Customer].content])
                    worker[c.DevelopManager].count++;
                    customer[c.Customer].count++;
                }
                /*if (!realparallel.includes([worker[c.ProjectSupervisor].content, customer[c.Customer].content])) {
                    realparallel.push([worker[c.ProjectSupervisor].content, customer[c.Customer].content])
                    worker[c.ProjectSupervisor].count++;
                    customer[c.Customer].count++;
                }*/
            })
            vm.paralleldata = [];
            /*for (var i = parseInt(realparallel.length / 8); i < realparallel.length / 4; i++) {
                vm.paralleldata.push(realparallel[i]);
            }*/
            vm.paralleldata = realparallel;
            vm.departmentproject = [];
            customer = sortByValue(customer);
            for (var item in customer) {
                if (customer[item].amount > 0)
                    vm.departmentproject.push(customer[item].content);
            }
            vm.departmentworker = [];
            worker = sortByValue(worker);
            for (var item in worker) {
                if (worker[item].amount > 0)
                    vm.departmentworker.push(worker[item].content);
            }
            vm.Parallel();
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用fetchManagerProjectService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    })
}

function handleworker(worker, item, result) {
    if (!(item in worker)) {
        var temp = result.filter(function(m) {
            return m.ProjectManager == item || m.DevelopManager == item /*|| m.ProjectSupervisor == item*/ ;
        })
        var annual = 0;
        var contamount = 0;
        temp.forEach(function(n) {
            annual = parseFloat((annual + parseFloat(n.AnnualAmount)).toFixed(2));
            contamount = parseFloat((contamount + parseFloat(n.ContAmount)).toFixed(2));
        })
        worker[item] = {
            content: item + ':' + annual + '/' + contamount,
            count: 0,
            amount: annual,
        }
    }
    return worker;
}

function fetchParallel(option) {
    phAjax.ajax({
        uri: "WorkerProject/?option=" + encodeURI(option),
        onSuccess: function(result) {
            var realparallel = new Array(result.length)
            var departmentproject = new Array();
            var projectcount = 0;
            vm.specialparalleldata = []
            for (var i = 0; i < result.length; i++) {
                if (i == 0 || result[i].ProjectName != result[i - 1].ProjectName) {
                    projectcount++;
                    departmentproject[projectcount] = {
                        Name: result[i].ProjectName,
                        Count: 1
                    }
                } else {
                    departmentproject[projectcount].Count += 1;
                }
                if (result[i].Worker != result[i].ProjectManager && result[i].Worker != result[i].DevelopManager)
                    realparallel.push([result[i].Worker, result[i].ProjectName]);
                else
                    vm.specialparalleldata.push([result[i].Worker, result[i].ProjectName])
            }
            departmentproject.sort(function(a, b) {
                return a.Count - b.Count;
            })
            vm.departmentproject = [];
            for (let i = 0; i < departmentproject.length - 1; i++) {
                vm.departmentproject.push(departmentproject[i].Name)
            }
            result.sort(function(a, b) {
                return a.Worker.localeCompare(b.Worker, 'zh-Hans-CN', {
                    sensitivity: 'accent'
                })
            })
            var departmentworker = new Array();
            var workercount = 0;
            for (var i = 0; i < result.length; i++) {
                if (i == 0 || result[i].Worker != result[i - 1].Worker) {
                    workercount++;
                    departmentworker[workercount] = {
                        Name: result[i].Worker,
                        Count: 1,
                    }
                } else {
                    departmentworker[workercount].Count += 1;
                }
            }
            departmentworker.sort(function(a, b) {
                return a.Count - b.Count;
            })
            vm.departmentworker = [];
            for (let i = 0; i < departmentworker.length - 1; i++) {
                vm.departmentworker.push(departmentworker[i].Name)
            }
            vm.paralleldata = realparallel;
            vm.Parallel();
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    })
}

function dBar(chartbody, div, bargraph, projectcount, workercount, c, sum) {
    div[projectcount] = document.createElement("div");
    div[projectcount].className += 'itemchart';
    chartbody.appendChild(div[projectcount]);
    bargraph[projectcount] = echarts.init(div[projectcount]);
    var cLast = c.length - 1;
    vm.time = new Array(workercount);
    vm.manage = [];
    vm.investigate = [];
    vm.develop = [];
    vm.test = [];
    vm.implement = [];
    vm.maintenance = [];
    vm.total = [];
    vm.workerbartotal = 0;
    var timecount = 0;
    c.forEach(function(d) {
        vm.time[timecount] = d.Worker;
        if (sum) {
            vm.manage[timecount] = d.Managesum;
            vm.investigate[timecount] = d.Investigatesum;
            vm.develop[timecount] = d.Developsum;
            vm.test[timecount] = d.Testsum;
            vm.implement[timecount] = d.Implementsum;
            vm.maintenance[timecount] = d.Maintenancesum;
            vm.total[timecount] = (d.Managesum + d.Investigatesum + d.Developsum + d.Testsum + d.Implementsum + d.Maintenancesum);
            vm.workerbartotal += (d.Managesum + d.Investigatesum + d.Developsum + d.Testsum + d.Implementsum + d.Maintenancesum);
        } else {
            vm.manage[timecount] = d.ManageWorkloadRole;
            vm.investigate[timecount] = d.InvestigateWorkloadRole;
            vm.develop[timecount] = d.DevelopWorkloadRole;
            vm.test[timecount] = d.TestWorkloadRole;
            vm.implement[timecount] = d.ImplementWorkloadRole;
            vm.maintenance[timecount] = d.MaintenanceWorkloadRole;
            vm.total[timecount] = (d.ManageWorkloadRole + d.InvestigateWorkloadRole + d.DevelopWorkloadRole + d.TestWorkloadRole + d.ImplementWorkloadRole + d.MaintenanceWorkloadRole);
            vm.workerbartotal += (d.ManageWorkloadRole + d.InvestigateWorkloadRole + d.DevelopWorkloadRole + d.TestWorkloadRole + d.ImplementWorkloadRole + d.MaintenanceWorkloadRole);
        }
        timecount++;
    })
    for (var j = timecount; j < workercount; j++) {
        vm.time[j] = "";
        vm.manage[j] = "";
        vm.investigate[j] = "";
        vm.develop[j] = "";
        vm.test[j] = "";
        vm.implement[j] = "";
        vm.maintenance[j] = "";
        vm.total[j] = "";
    }
    vm.Bargraph(bargraph, projectcount, c[cLast].ProjectName, true, false, c[cLast]);
}

function fetchWorkerload(year, month, onlyme, PorC) {
    phAjax.ajax({
        uri: "Worker/DepartmentWorker?option=" + encodeURI(vm.option),
        onSuccess: function(result) {
            workercount = result.length;
            phAjax.ajax({
                uri: "WorkerProject/?year=" + year + "&month=" + month + "&onlyme=" + onlyme,
                onSuccess: function(result) {
                    if (result.length > 0) {
                        var chartbody = document.getElementById('Graph');
                        var div = new Array(),
                            bargraph = new Array(),
                            projectcount = 0;
                        vm.projectbarmax = 0;
                        if (PorC) {
                            var tempG = groupBy(result, "Customer");
                            tempG.forEach(function(c) {
                                var tAnnualAmount = 0;
                                var tReservedProfitAmount = 0;
                                var tContAmount = 0;
                                var tAnnualProfitAmount = 0;
                                groupBy(c, "ProjectName").forEach(function(d) {
                                    tAnnualAmount = addFix2(tAnnualAmount, d[0].AnnualInvoiceAmount);
                                    tReservedProfitAmount = addFix2(tReservedProfitAmount, d[0].ReservedProfitAmount);
                                    tContAmount = addFix2(tContAmount, d[0].ContAmount);
                                    tAnnualProfitAmount = addFix2(tAnnualProfitAmount, d[0].AnnualProfitAmount);
                                })
                                c.sort(function(a, b) {
                                    return a.Worker.localeCompare(b.Worker, 'zh-Hans-CN', {
                                        sensitivity: 'accent'
                                    });
                                })
                                let i = 1;
                                while (i < c.length) {
                                    if (c[i].Worker == c[i - 1].Worker) {
                                        c[i - 1].ManageWorkloadRole += c[i].ManageWorkloadRole;
                                        c[i - 1].InvestigateWorkloadRole += c[i].InvestigateWorkloadRole;
                                        c[i - 1].DevelopWorkloadRole += c[i].DevelopWorkloadRole;
                                        c[i - 1].TestWorkloadRole += c[i].TestWorkloadRole;
                                        c[i - 1].ImplementWorkloadRole += c[i].ImplementWorkloadRole;
                                        c[i - 1].MaintenanceWorkloadRole += c[i].MaintenanceWorkloadRole;
                                        c[i - 1].Workload += c[i].Workload;
                                        if (c[i - 1].Workload > vm.projectbarmax) vm.projectbarmax = c[i - 1].Workload;
                                        c.splice(i, 1);
                                    } else i++;
                                }
                                c.sort(function(a, b) {
                                    return -a.Workload + b.Workload;
                                })
                                var cLast = c.length - 1;
                                c[cLast].AnnualInvoiceAmount = tAnnualAmount;
                                c[cLast].ReservedProfitAmount = tReservedProfitAmount;
                                c[cLast].ContAmount = tContAmount;
                                c[cLast].AnnualProfitAmount = tAnnualProfitAmount;
                                c[cLast].ProjectName = c[cLast].Customer;
                            })
                            tempG.sort(function(a, b) {
                                return -a.length + b.length;
                            })
                            tempG.forEach(function(d) {
                                dBar(chartbody, div, bargraph, projectcount, workercount, d, false);
                                projectcount++;
                            })
                        } else {
                            var tempG = groupBy(result, "ProjectName");
                            tempG.sort(function(a, b) {
                                return -a.length + b.length
                            });
                            tempG.forEach(function(c) {
                                c.forEach(function(d) {
                                    if (d.Workload > vm.projectbarmax) vm.projectbarmax = d.Workload
                                })
                            })
                            tempG.forEach(function(c) {
                                dBar(chartbody, div, bargraph, projectcount, workercount, c, false);
                                projectcount++;
                            })
                        }
                    }
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
                },
            })
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allworkerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchworkerbar(option, onlyme, PorC) {
    var workercount = 0;
    phAjax.ajax({
        uri: "Worker/DepartmentWorker?option=" + encodeURI(vm.option),
        onSuccess: function(result) {
            workercount = result.length;
            phAjax.ajax({
                uri: "WorkerProject/?option=" + encodeURI(option) + "&onlyme=" + onlyme,
                onSuccess: function(result) {
                    if (result.length > 0) {
                        var chartbody = document.getElementById('Graph');
                        var div = new Array(),
                            bargraph = new Array(),
                            projectcount = 0;
                        vm.projectbarmax = 0;
                        if (PorC) {
                            var tempG = groupBy(result, "Customer");
                            debugger
                            tempG.forEach(function(c) {
                                var tAnnualAmount = 0;
                                var tReservedProfitAmount = 0;
                                var tContAmount = 0;
                                var tAnnualProfitAmount = 0;
                                groupBy(c, "ProjectName").forEach(function(d) {
                                    tAnnualAmount = addFix2(tAnnualAmount, d[0].AnnualAmount);
                                    tAnnualProfitAmount = addFix2(tAnnualProfitAmount, d[0].AnnualProfitAmount);
                                    tReservedProfitAmount = addFix2(tReservedProfitAmount, d[0].ReservedProfitAmount);
                                    tContAmount = addFix2(tContAmount, d[0].ContAmount);

                                })
                                c.sort(function(a, b) {
                                    return a.Worker.localeCompare(b.Worker, 'zh-Hans-CN', {
                                        sensitivity: 'accent'
                                    });
                                })
                                let i = 1;
                                while (i < c.length) {
                                    if (c[i].Worker == c[i - 1].Worker) {
                                        c[i - 1].Managesum += c[i].Managesum;
                                        c[i - 1].Investigatesum += c[i].Investigatesum;
                                        c[i - 1].Developsum += c[i].Developsum;
                                        c[i - 1].Testsum += c[i].Testsum;
                                        c[i - 1].Implementsum += c[i].Implementsum;
                                        c[i - 1].Maintenancesum += c[i].Maintenancesum;
                                        c[i - 1].WorkloadSum += c[i].WorkloadSum;
                                        c.splice(i, 1);
                                        if (c[i - 1].WorkloadSum > vm.projectbarmax) vm.projectbarmax = c[i - 1].WorkloadSum;
                                    } else i++;
                                }
                                c.sort(function(a, b) {
                                    return -a.WorkloadSum + b.WorkloadSum;
                                })
                                var cLast = c.length - 1;
                                c[cLast].AnnualAmount = tAnnualAmount;
                                c[cLast].ReservedProfitAmount = tReservedProfitAmount;
                                c[cLast].ContAmount = tContAmount;
                                c[cLast].AnnualProfitAmount = tAnnualProfitAmount;
                                c[cLast].ProjectName = c[cLast].Customer;
                            })
                            tempG.sort(function(a, b) {
                                return -a.length + b.length;
                            })
                            tempG.forEach(function(d) {
                                dBar(chartbody, div, bargraph, projectcount, workercount, d, true);
                                projectcount++;
                            })
                        } else {
                            var tempG = groupBy(result, "ProjectName");
                            tempG.forEach(function(c) {
                                c.forEach(function(d) {
                                    if (d.WorkloadSum > vm.projectbarmax) vm.projectbarmax = d.WorkloadSum;
                                })
                            })
                            tempG.forEach(function(c) {
                                dBar(chartbody, div, bargraph, projectcount, workercount, c, true);
                                projectcount++;
                            })
                        }
                    }
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
                },
            })
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allworkerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchlinedata(option, endyear, endmonth) {
    phAjax.ajax({
        uri: "ProjectActivity/?option=" + encodeURI(option) + "&startyear=2018&startmonth=9&endyear=" + endyear + "&endmonth=" + endmonth,
        onSuccess: function(result) {
            vm.linedata = result;
            vm.lines();
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    })
}

function dLine(time, chartbody, div, bargraph, projectcount, cLast, c, nowtimecount) {
    vm.manage = [];
    vm.investigate = [];
    vm.develop = [];
    vm.test = [];
    vm.implement = [];
    vm.maintenance = [];
    vm.total = [];
    vm.workerbartotal = 0;
    vm.time = deepClone(time);
    var plandate = "";
    if (c[cLast].OnlinePlanDate != null) {
        plandate = new Date(c[cLast].OnlinePlanDate);
        var index = vm.time.lastIndexOf(plandate.getFullYear() + '.' + (plandate.getMonth() + 1));
        vm.time[index] = 'bs' + plandate.getFullYear() + '.' + (plandate.getMonth() + 1);
    }
    if (c[cLast].OnlineActualDate != null) {
        var date = new Date(c[cLast].OnlineActualDate);
        var index = vm.time.lastIndexOf(date.getFullYear() + '.' + (date.getMonth() + 1));
        if (plandate == "" || plandate == date) {
            vm.time[index] = 'bs' + date.getFullYear() + '.' + (date.getMonth() + 1);
        } else if (plandate > date) {
            vm.time[index] = 'ls' + date.getFullYear() + '.' + (date.getMonth() + 1);
        } else if (plandate < date) {
            vm.time[index] = 'hs' + date.getFullYear() + '.' + (date.getMonth() + 1);
        }
    }
    var timecount = 0;
    div[projectcount] = document.createElement("div");
    div[projectcount].className += 'itemchart';
    chartbody.appendChild(div[projectcount]);
    bargraph[projectcount] = echarts.init(div[projectcount]);
    for (var i = 0; i < c.length; i++) {
        if (c[i].Year != regyear.exec(vm.time[timecount])[0] || c[i].Month != regmonth.exec(vm.time[timecount])[1]) {
            vm.manage.push(0);
            vm.investigate.push(0);
            vm.develop.push(0);
            vm.test.push(0);
            vm.implement.push(0);
            vm.maintenance.push(0);
            vm.total.push(0);
            i--;
        } else {
            vm.manage.push(c[i].ManageWorkloadRole);
            vm.investigate.push(c[i].InvestigateWorkloadRole);
            vm.develop.push(c[i].DevelopWorkloadRole);
            vm.test.push(c[i].TestWorkloadRole);
            vm.implement.push(c[i].ImplementWorkloadRole);
            vm.maintenance.push(c[i].MaintenanceWorkloadRole);
            vm.workerbartotal += (c[i].ManageWorkloadRole + c[i].InvestigateWorkloadRole + c[i].DevelopWorkloadRole + c[i].TestWorkloadRole + c[i].ImplementWorkloadRole + c[i].MaintenanceWorkloadRole);
            vm.total.push(c[i].ManageWorkloadRole + c[i].InvestigateWorkloadRole + c[i].DevelopWorkloadRole + c[i].TestWorkloadRole + c[i].ImplementWorkloadRole + c[i].MaintenanceWorkloadRole);
        }
        timecount++;
    }
    if (timecount < nowtimecount) {
        vm.manage.push(0);
        vm.investigate.push(0);
        vm.develop.push(0);
        vm.test.push(0);
        vm.implement.push(0);
        vm.maintenance.push(0);
        vm.total.push(0);
    }
    vm.Line(bargraph, projectcount, c[cLast].ProjectName, true, c[cLast]);
}

function fetchIntervalProject(option, endyear, endmonth, onlyme, PorC) {
    var changedate = getPastDate(endyear, endmonth, -1)
    endyear = changedate.getFullYear();
    endmonth = changedate.getMonth() + 1;
    phAjax.ajax({
        uri: "ProjectActivity/GetIntervalProject?option=" + encodeURI(option) + "&endyear=" + endyear + "&endmonth=" + endmonth + "&onlyme=" + onlyme,
        onSuccess: function(result) {
            if (result.length != 0) {
                //遍历获取时间区间和最大值
                var startyear, startmonth;
                vm.projectbarmax = 0;
                var tempsum = 0;
                for (var i = 0; i < result.length; i++) {
                    if (startyear == null || new Date(startyear, startmonth) > new Date(result[i].Year, result[i].Month)) {
                        startyear = result[i].Year;
                        startmonth = result[i].Month;
                    }
                    if (result[i].OnlinePlanDate != null && (endyear == null || new Date(endyear, endmonth) < new Date(result[i].OnlinePlanDate.substring(0, 4), result[i].OnlinePlanDate.substring(5, 7)))) {
                        endyear = result[i].OnlinePlanDate.substring(0, 4);
                        endmonth = result[i].OnlinePlanDate.substring(5, 7);
                    }
                    if (result[i].OnlineActualDate != null && (endyear == null || new Date(endyear, endmonth) < new Date(result[i].OnlineActualDate.substring(0, 4), result[i].OnlineActualDate.substring(5, 7)))) {
                        endyear = result[i].OnlineActualDate.substring(0, 4);
                        endmonth = result[i].OnlineActualDate.substring(5, 7);
                    }
                    tempsum = result[i].ManageWorkloadRole + result[i].InvestigateWorkloadRole + result[i].DevelopWorkloadRole + result[i].TestWorkloadRole + result[i].ImplementWorkloadRole + result[i].MaintenanceWorkloadRole;
                    if (tempsum > vm.projectbarmax) vm.projectbarmax = tempsum;
                }
                vm.time = [];
                var nowtimecount = 0;
                while (endyear > startyear || (endyear == startyear && endmonth >= startmonth)) {
                    vm.time.push(startyear + '.' + startmonth);
                    var temp = new Date(new Date(startyear, startmonth - 1).setMonth((new Date(startyear, startmonth - 1).getMonth() + 1)));
                    if (temp < new Date())
                        nowtimecount++;
                    startyear = temp.getFullYear();
                    startmonth = temp.getMonth() + 1;
                }
                var time = deepClone(vm.time);
                var chartbody = document.getElementById('Graph');
                var div = new Array(),
                    bargraph = new Array();
                var projectcount = 0;
                if (PorC) { //客户
                    var tempG = groupBy(result, "Customer");
                    debugger
                    tempG.forEach(function(c) {
                        var tWorkload_ = 0;
                        var tAnnualAmount = 0;
                        var tReservedProfitAmount = 0;
                        var tContAmount = 0;
                        var tAnnualProfitAmount = 0;
                        groupBy(c, "ProjectName").forEach(function(d) {
                            tWorkload_ += d[0].Workload_;
                            if (d[0].LastDate != null && new Date(d[0].LastDate).getFullYear() == new Date().getFullYear()) {
                                //tAnnualAmount = addFix2(tAnnualAmount, d[0].AnnualAmount);
                                tAnnualAmount = addFix2(tAnnualAmount, d[0].AnnualInvoiceAmount);
                            }
                            tReservedProfitAmount = addFix2(tReservedProfitAmount, d[0].ReservedProfitAmount);
                            tContAmount = addFix2(tContAmount, d[0].ContAmount);
                            tAnnualProfitAmount = addFix2(tAnnualProfitAmount, d[0].AnnualProfitAmount);
                        })
                        c.sort(function(a, b) {
                            if (a.Year != b.Year) return a.Year - b.Year;
                            else return a.Month - b.Month;
                        })
                        let i = 1;
                        while (i < c.length) {
                            if (c[i].Customer == c[i - 1].Customer && c[i].Year == c[i - 1].Year && c[i].Month == c[i - 1].Month) {
                                c[i - 1].ManageWorkloadRole += c[i].ManageWorkloadRole;
                                c[i - 1].InvestigateWorkloadRole += c[i].InvestigateWorkloadRole;
                                c[i - 1].DevelopWorkloadRole += c[i].DevelopWorkloadRole;
                                c[i - 1].TestWorkloadRole += c[i].TestWorkloadRole;
                                c[i - 1].ImplementWorkloadRole += c[i].ImplementWorkloadRole;
                                c[i - 1].MaintenanceWorkloadRole += c[i].MaintenanceWorkloadRole;
                                c.splice(i, 1);
                            } else i++
                        }
                        var cLast = c.length - 1;
                        c[cLast].Workload_ = tWorkload_;
                        c[cLast].AnnualInvoiceAmount = tAnnualAmount;
                        c[cLast].ReservedProfitAmount = tReservedProfitAmount;
                        c[cLast].ContAmount = tContAmount;
                        c[cLast].AnnualProfitAmount = tAnnualProfitAmount;
                        c[cLast].ProjectName = c[cLast].Customer;
                        if (tWorkload_ > vm.projectbarmax) vm.projectbarmax = tWorkload_;
                    })
                    tempG.sort(function(a, b) {
                        return -a[a.length - 1].Workload_ + b[b.length - 1].Workload_
                    })
                    tempG.forEach(function(e) {
                        var eLast = e.length - 1;
                        dLine(time, chartbody, div, bargraph, projectcount, eLast, e, nowtimecount);
                        projectcount++;
                    })
                } else { //项目
                    groupBy(result, "ProjectName").forEach(function(c) {
                        var cLast = c.length - 1;
                        var plandate = "";
                        if (c[cLast].OnlinePlanDate != null) {
                            plandate = new Date(c[cLast].OnlinePlanDate);
                            var index = vm.time.lastIndexOf(plandate.getFullYear() + '.' + (plandate.getMonth() + 1));
                            vm.time[index] = 'bs' + plandate.getFullYear() + '.' + (plandate.getMonth() + 1);
                        }
                        if (c[cLast].OnlineActualDate != null) {
                            var date = new Date(c[cLast].OnlineActualDate);
                            var index = vm.time.lastIndexOf(date.getFullYear() + '.' + (date.getMonth() + 1));
                            if (plandate == "" || plandate == date) {
                                vm.time[index] = 'bs' + date.getFullYear() + '.' + (date.getMonth() + 1);
                            } else if (plandate > date) {
                                vm.time[index] = 'ls' + date.getFullYear() + '.' + (date.getMonth() + 1);
                            } else if (plandate < date) {
                                vm.time[index] = 'hs' + date.getFullYear() + '.' + (date.getMonth() + 1);
                            }
                        }
                        dLine(time, chartbody, div, bargraph, projectcount, cLast, c, nowtimecount);
                        projectcount++;
                    })
                }
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用Service失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchCustomerWorkload(department, startyear, startmonth, endyear, endmonth) {
    phAjax.ajax({
        uri: "CustomerWorkload/?department=" + encodeURI(department) + "&startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth,
        onSuccess: function(result) {
            if (result.length > 0) {
                vm.projectworkload = [];
                var projectcolor = new Array();
                var sprojectcolor = new Array();
                var rate = new Array();
                var count = 0;
                for (var i = 0; i < result.length; i++) {
                    if (i == 0 || result[i].Customer != result[i - 1].Customer) {
                        if (i != 0) {
                            getCustomercolor = function() {
                                var max = "";
                                var maxnumber = 0;
                                var smax = "";
                                var smaxnumber = 0;
                                var a = vm.projectworkload[vm.projectworkload.length - 1]
                                if (a.manage > a.investigate) {
                                    max = "manage";
                                    maxnumber = a.manage
                                    smax = "investigate";
                                    smaxnumber = a.investigate;
                                } else {
                                    max = "investigate";
                                    maxnumber = a.investigate;
                                    smax = "manage";
                                    smaxnumber = a.manage
                                }
                                if (a.develop > maxnumber) {
                                    smax = max;
                                    smaxnumber = maxnumber;
                                    max = "develop";
                                    maxnumber = a.develop
                                } else if (a.develop > smaxnumber) {
                                    smax = "develop";
                                    smaxnumber = a.develop;
                                }
                                if (a.test > maxnumber) {
                                    smax = max;
                                    smaxnumber = maxnumber;
                                    max = "test";
                                    maxnumber = a.test;
                                } else if (a.test > maxnumber) {
                                    smax = "test";
                                    Smaxnumber = a.test;
                                }
                                if (a.implement > maxnumber) {
                                    smax = max;
                                    smaxnumber = maxnumber;
                                    max = "implement";
                                    maxnumber = a.implement;
                                } else if (a.implement > smaxnumber) {
                                    smax = "implement";
                                    smaxnumber = a.implement;
                                }
                                if (a.maintenance > maxnumber) {
                                    smax = max;
                                    smaxnumber = maxnumber;
                                    max = "maintenance";
                                    maxnumber = a.maintenance;
                                } else if (a.maintenance > smaxnumber) {
                                    smax = "maintenance";
                                    smaxnumber = a.maintenance;
                                }
                                projectcolor[count] = getcolor(max);
                                sprojectcolor[count] = getcolor(smax);
                                rate[count] = Math.sqrt(maxnumber / (maxnumber + smaxnumber));
                                count++;
                            }
                            getCustomercolor();
                        }
                        vm.projectworkload.push({
                            "key": result[i].Customer,
                            "value": result[i].Workload,
                            "manage": result[i].ManageWorkloadRole,
                            "investigate": result[i].InvestigateWorkloadRole,
                            "develop": result[i].DevelopWorkloadRole,
                            "test": result[i].TestWorkloadRole,
                            "implement": result[i].ImplementWorkloadRole,
                            "maintenance": result[i].MaintenanceWorkloadRole,
                        })
                    } else {
                        vm.projectworkload[vm.projectworkload.length - 1].value += result[i].Workload;
                        vm.projectworkload[vm.projectworkload.length - 1].manage += result[i].ManageWorkloadRole;
                        vm.projectworkload[vm.projectworkload.length - 1].investigate += result[i].InvestigateWorkloadRole;
                        vm.projectworkload[vm.projectworkload.length - 1].develop += result[i].DevelopWorkloadRole;
                        vm.projectworkload[vm.projectworkload.length - 1].test += result[i].TestWorkloadRole;
                        vm.projectworkload[vm.projectworkload.length - 1].implement += result[i].ImplementWorkloadRole;
                        vm.projectworkload[vm.projectworkload.length - 1].maintenance += result[i].MaintenanceWorkloadRole;
                    }
                }
                getCustomercolor();
                vm.Bubble(projectcolor, sprojectcolor, rate);
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allworkerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchAnnualProject(department) {
    phAjax.ajax({
        uri: "ProjectInfo/DepartmentProject?option=" + encodeURI(department),
        onSuccess: function(result) {
            vm.annualamount = new Array(vm.time.length);
            for (let i = 0; i < vm.time.length; i++) {
                let j = 0;
                vm.annualamount[i] = 0;
                while (j < result.length) {
                    if (result[j].ProjectName == vm.time[i]) {
                        vm.annualamount[i] = result[j].AnnualAmount;
                        break;
                    } else j++;
                }
            }
            var chartbody = document.getElementById('Graph');
            var div = new Array(),
                bargraph = new Array();
            div[0] = document.createElement("div");
            div[0].style.height = vm.time.length * 25 + "px";
            div[0].style.marginTop = 10 + "px";
            chartbody.appendChild(div[0]);
            bargraph[0] = echarts.init(div[0]);
            vm.InverseBargraph(bargraph, 0, "项目人力资源分摊");
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {}
    })
}

function fetchProjectWorkload(option, startyear, startmonth, endyear, endmonth) {
    phAjax.ajax({
        uri: "ProjectActivity/GetProjectWorkload?option=" + encodeURI(option) + "&startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth,
        onSuccess: function(result) {
            var datacount = -1;
            var dataWorkload = new Array();
            vm.time = [];
            vm.manage = [];
            vm.investigate = [];
            vm.develop = [];
            vm.test = [];
            vm.implement = [];
            vm.maintenance = [];
            for (var i = 0; i < result.length; i++) {
                if (i == 0 || result[i].ProjectName != dataWorkload[datacount].key) {
                    dataWorkload.push({
                        key: result[i].ProjectName,
                        value: result[i].Workload,
                        manage: result[i].ManageWorkloadRole,
                        investigate: result[i].InvestigateWorkloadRole,
                        develop: result[i].DevelopWorkloadRole,
                        test: result[i].TestWorkloadRole,
                        implement: result[i].ImplementWorkloadRole,
                        maintenance: result[i].MaintenanceWorkloadRole,
                    })
                    datacount++;
                } else {
                    dataWorkload[datacount].value += result[i].Workload;
                    dataWorkload[datacount].manage += result[i].ManageWorkloadRole;
                    dataWorkload[datacount].investigate += result[i].InvestigateWorkloadRole;
                    dataWorkload[datacount].develop += result[i].DevelopWorkloadRole;
                    dataWorkload[datacount].test += result[i].TestWorkloadRole;
                    dataWorkload[datacount].implement += result[i].ImplementWorkloadRole;
                    dataWorkload[datacount].maintenance += result[i].MaintenanceWorkloadRole;
                }
            }
            dataWorkload.sort(function(a, b) {
                return a.value - b.value;
            })
            dataWorkload.forEach(function(a) {
                vm.time.push(a.key);
                vm.manage.push(a.manage);
                vm.investigate.push(a.investigate);
                vm.develop.push(a.develop);
                vm.test.push(a.test);
                vm.implement.push(a.implement);
                vm.maintenance.push(a.maintenance);
            })
            fetchAnnualProject(option);
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ",response: " + XMLHttpRequest.responseText);
        },
    })
}

function fetchBoxWorkload(startyear, startmonth, endyear, endmonth) {
    phAjax.ajax({
        uri: "ProjectActivity/IntervalWorkload?category=4" + "&option=0" + "&startyear=" + startyear + "&startmonth=" + startmonth + "&endyear=" + endyear + "&endmonth=" + endmonth,
        onSuccess: function(result) {
            if (result.length > 0) {
                var manage = new Array();
                var investigate = new Array();
                var develop = new Array();
                var test = new Array();
                var implement = new Array();
                var maintenance = new Array();
                for (var i = 0; i < result.length; i++) {
                    if (result[i].Manage != 0)
                        manage.push(result[i].Manage);
                    if (result[i].Investigate != 0)
                        investigate.push(result[i].Investigate);
                    if (result[i].Develop != 0)
                        develop.push(result[i].Develop);
                    if (result[i].Test != 0)
                        test.push(result[i].Test);
                    if (result[i].Implement != 0)
                        implement.push(result[i].Implement);
                    if (result[i].Maintenance != 0)
                        maintenance.push(result[i].Maintenance);
                }
                vm.boxmanage = manage;
                vm.boxinvestigate = investigate;
                vm.boxdevelop = develop;
                vm.boxtest = test;
                vm.boximplement = implement;
                vm.boxmaintenance = maintenance;
                vm.Boxplot();
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}
var vm = new Vue({
    el: "#content",
    data: {
        radardata: [],
        worker: [],
        workerrelation: [{
            source: "",
            target: "",
            name: '',
            lineStyle: {
                width: 4
            }
        }],
        projectbardata: [],
        projectworkload: [],
        linedata: [],
        departmentworker: [],
        departmentproject: [],
        paralleldata: [],
        interval: "3",
        allworker: [],
        allproject: [],
        alldepartment: [],
        allitem: [],
        annualamount: [],
        time: [],
        boxmanage: [],
        boxinvestigate: [],
        boxdevelop: [],
        boxtest: [],
        boximplement: [],
        boxmaintenance: [],
        manage: [],
        investigate: [],
        develop: [],
        test: [],
        implement: [],
        maintenance: [],
        vacation: [],
        rest: [],
        checkedValue: "3",
        bargraphcount: 0,
        projectbarmax: 0,
        chartheight: 434,
        charttype: "workerbar",
        workerbartime: getPastDate(new Date().getFullYear(), new Date().getMonth() + 1, 1),
        value3: false,
        onlyme: false,
        workerbartotal: 0,
        value4: false,
        specialparalleldata: [],
        total: [],
        value5: false,
        option: "",
    },
    mounted: function() {
        this.user = globaluser;
        this.alldepartment.push(this.user.Department.Name);
        this.allitem.push(this.user.Department.Name);
        this.option = this.user.Department.Name;
        fetchAllDepartment();
        this.chartheight = $("#Graph").height();
        //window.setTimeout("vm.draw()", 1000);
    },
    watch: {
        checkedValue: function() {
            switch (this.checkedValue) {
                case '1':
                    this.option = "";
                    fetchAllWorker();
                    break;
                case '2':
                    this.option = "";
                    fetchAllProject();
                    break;
                case '3':
                    this.option = "";
                    fetchAllDepartment();
                    break;
            }
        },
        onlyme: function() {
            this.draw();
        },
        workerbartime: function() {
            this.draw();
        },
        value3: function() {
            this.draw();
        },
        value4: function() {
            this.draw();
        },
        value5: function() {
            this.draw();
        },
        charttype: function() {
            if (!(this.charttype == 'bar' && this.interval != 4))
                this.draw();
        },
        interval: function() {
            this.draw();
        }, //把他们组合成一个属性吧
        option: function() {
            this.draw();
        }
    },
    methods: {
        draw: function() {
            var startyear = new Date().getFullYear();
            var startmonth = new Date().getMonth() + 1;
            var endyear = getPastDate(startyear, startmonth, 1).getFullYear();
            var endmonth = getPastDate(startyear, startmonth, 1).getMonth() + 1;
            switch (this.interval) {
                case '1':
                    startyear = endyear;
                    startmonth = endmonth;
                    break;
                case '2':
                    startyear = getPastDate(startyear, startmonth, 3).getFullYear();
                    startmonth = getPastDate(startyear, startmonth, 3).getMonth() + 1;
                    break;
                case '3':
                    startyear = getPastDate(startyear, startmonth, 6).getFullYear();
                    startmonth = getPastDate(startyear, startmonth, 6).getMonth() + 1;
                    break;
                case '4':
                    startyear = getPastDate(startyear, startmonth, 12).getFullYear();
                    startmonth = getPastDate(startyear, startmonth, 12).getMonth() + 1;
                    break;
                case '5':
                    startyear = getPastDate(startyear, startmonth, 1).getFullYear();
                    startmonth = 1;
                    break;
                default:
                    console.log("nomatch");
                    break;
            }
            $("#Graph").empty();
            this.bargraphcount = 0;
            switch (this.charttype) {
                case 'circlelink':
                    this.bargraphcount = 0;
                    if (this.alldepartment.length > 1) fetchAllDepartment();
                    for (var i = 0; i < this.alldepartment.length; i++) {
                        fetchworker(new Date(startyear, startmonth - 1).format("yyyy-MM-dd"), new Date(endyear, endmonth - 1).format("yyyy-MM-dd"), startyear, startmonth, endyear, endmonth, this.alldepartment[i]);
                    }
                    break;
                case 'forcelink':
                    if (this.alldepartment.length > 1) fetchAllDepartment();
                    for (var i = 0; i < this.alldepartment.length; i++) {
                        fetchworker(new Date(startyear, startmonth - 1).format("yyyy-MM-dd"), new Date(endyear, endmonth - 1).format("yyyy-MM-dd"), startyear, startmonth, endyear, endmonth, this.alldepartment[i]);
                    }
                    break;
                case 'managelink':
                    if (this.alldepartment.length > 1) fetchAllDepartment();
                    for (var i = 0; i < this.alldepartment.length; i++) {
                        fetchworker(new Date(startyear, startmonth - 1).format("yyyy-MM-dd"), new Date(endyear, endmonth - 1).format("yyyy-MM-dd"), startyear, startmonth, endyear, endmonth, this.alldepartment[i]);
                    }
                    break;
                case 'projectlink':
                    if (this.alldepartment.length > 1) fetchAllDepartment();
                    for (var i = 0; i < this.alldepartment.length; i++) {
                        fetchworker(new Date(startyear, startmonth - 1).format("yyyy-MM-dd"), new Date(endyear, endmonth - 1).format("yyyy-MM-dd"), startyear, startmonth, endyear, endmonth, this.alldepartment[i]);
                    }
                    break;
                case 'box':
                    fetchBoxWorkload(startyear, startmonth, endyear, endmonth);
                    break;
                case 'radar':
                    fetchRedarWorkload(this.checkedValue, this.option, startyear, startmonth, endyear, endmonth);
                    break;
                case 'bar':
                    if (this.alldepartment.length > 1) fetchAllDepartment();
                    var bargraph = new Array();
                    for (var i = 0; i < this.alldepartment.length; i++) {
                        fetchBarWorkload(this.checkedValue, this.alldepartment[i], startyear, startmonth, endyear, endmonth, bargraph, i);
                    }
                    break;
                case 'parallel':
                    if (this.value4) {
                        fetchManagerProject(this.option, startyear, startmonth, endyear, endmonth)
                    } else fetchParallel(this.option);
                    break;
                case 'bubble':
                    fetchCustomerWorkload(this.option, startyear, startmonth, endyear, endmonth);
                    fetchProjectWorkload(this.option, startyear, startmonth, endyear, endmonth);
                    break;
                case 'lines':
                    fetchlinedata(this.option, endyear, endmonth);
                    break;
                case 'projectbar':
                    fetchIntervalProject(this.option, endyear, endmonth, this.onlyme, this.value5);
                    break;
                case 'workerbar':
                    this.value3 ?
                        fetchWorkerload(this.workerbartime.getFullYear(), this.workerbartime.getMonth() + 1, this.onlyme, this.value5) :
                        fetchworkerbar(this.option, this.onlyme, this.value5);
                    break;
                default:
                    console.log("nomatch");
                    break;
            }
        },
        lines: function() {
            var max = 0;
            var project = new Array();
            var count = 0,
                timecount = 0;
            var time = new Array();
            var cursoryear = 2018,
                cursormonth = 9;
            var endyear = getPastDate(new Date().getFullYear(), new Date().getMonth() + 1, 1).getFullYear();
            var endmonth = getPastDate(new Date().getFullYear(), new Date().getMonth() + 1, 1).getMonth() + 1;
            while (endyear > cursoryear || (endyear == cursoryear && endmonth >= cursormonth)) {
                time.push(cursoryear + '.' + cursormonth);
                var temp = new Date(new Date(cursoryear, cursormonth - 1).setMonth((new Date(cursoryear, cursormonth - 1).getMonth() + 1)));
                cursoryear = temp.getFullYear();
                cursormonth = temp.getMonth() + 1;
            }
            if (vm.linedata.length > 0) {
                project[count] = {
                    "Name": "",
                    "Data": [],
                }
                project[count].Name = vm.linedata[0].ProjectName;
                project[count].Data = new Array();
                if (vm.linedata[0].Year != time[timecount].substring(0, 4) || vm.linedata[0].Month != time[timecount].substring(5, time[timecount].length)) project[count].Data.push("")
                else
                    project[count].Data.push(vm.linedata[0].Workload);
                max = vm.linedata[0].Workload;
                timecount++;
                for (var i = 1; i < vm.linedata.length; i++) {
                    if (vm.linedata[i].Workload > max) max = vm.linedata[i].Workload;
                    if (vm.linedata[i].ProjectName != project[count].Name) {
                        count++;
                        project[count] = {
                            "Name": "",
                            "Data": [],
                        }
                        project[count].Name = vm.linedata[i].ProjectName;
                        project[count].Data = new Array();
                        timecount = 0;
                    }
                    if (vm.linedata[i].Year != time[timecount].substring(0, 4) || vm.linedata[i].Month != time[timecount].substring(5, time[timecount].length)) project[count].Data.push("")
                    else
                        project[count].Data.push(vm.linedata[i].Workload);
                    timecount++;

                }
            }
            var series = new Array(project.length);
            for (var i = 0; i < project.length; i++) {
                for (var j = 0; j < project[i].Data.length; j++) {
                    project[i].Data[j] = project[i].Data[j] + i * max * 2;
                }
                project[i].Data[0] = {
                    value: project[i].Data[0],
                    label: {
                        show: true,
                        position: "left",
                        formatter: project[i].Name,
                    }
                }
                series[i] = {
                    name: project[i].Name,
                    type: 'line',
                    symbol: 'circle',
                    symbolSize: 4,
                    smooth: true,
                    data: project[i].Data,
                }
            }
            document.getElementById('Graph').style.height = (100 * project.length) + "px";
            var lineschart = echarts.init(document.getElementById('Graph'));
            linesoption = {
                backgroundColor: '#333',
                title: {
                    text: '项目人力资源涨落图',
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                },
                grid: {
                    left: '30%', //图表距边框的距离
                    right: '0%',
                    bottom: '3%',
                    containLabel: true
                },
                tooltip: {
                    trigger: 'axis',
                    triggerOn: 'click',
                    formatter: function(params) {
                        var content = "<p>";
                        //两个for将params中需要的参数嵌入HTML代码块字符串content中
                        for (var i = 0; i < params.length; i++) {
                            content += params[i].seriesName + ":" + (params[i].value - i * max * 2).toString() + "<br/>"
                        }
                        //return出去后echarts会调用html()函数将content字符串代码化
                        content += "</p>"
                        return content;
                    },
                },
                xAxis: {
                    data: ["2018-9"],
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: "gray",
                    },
                    boundaryGap: false,
                },
                yAxis: {
                    type: "value",
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: "gray",
                        show: false
                    },
                    scale: true,
                },
                dataZoom: [{
                    type: 'inside', //鼠标滚轮
                    realtime: true,
                }, ],
                series: series,
            }
            lineschart.setOption(linesoption, {
                notMerge: true,
            });
        },
        Parallel: function() {
            div = document.createElement("div");
            var chartbody = document.getElementById('Graph');
            div.style.height = parseInt(vm.departmentproject.length * 25 > vm.departmentworker.length * 25 ? vm.departmentproject.length * 25 : vm.departmentworker.length * 25) + "px";
            chartbody.appendChild(div);
            var parallelchart = echarts.init(div);
            paralleloption = {
                backgroundColor: '#333',
                title: {
                    text: vm.value4 ? '员工负责客户关系图' : '员工参与项目关系图',
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                },
                parallelAxis: [{
                        dim: 0,
                        name: vm.value4 ? '负责人' : '员工',
                        type: 'category',
                        //数据会倒序显示，奇妙
                        data: this.departmentworker,
                        boundaryGap: false,
                        nameTextStyle: {
                            color: 'rgb(238, 197, 102)'
                        },
                        axisLine: {
                            lineStyle: {
                                color: 'rgb(238, 197, 102)'
                            }
                        },
                        axisLabel: {
                            color: 'rgb(238, 197, 102)',
                            interval: false,
                            inside: true,
                        }
                    },
                    {
                        dim: 1,
                        name: vm.value4 ? '客户' : '项目',
                        type: 'category',
                        data: this.departmentproject,
                        boundaryGap: false,
                        nameTextStyle: {
                            color: '#E59C87'
                        },
                        axisLine: {
                            lineStyle: {
                                color: '#E59C87'
                            }
                        },
                        axisLabel: {
                            color: '#E59C87',
                            show: true,
                            interval: false,
                        },
                    },
                ],
                parallel: {
                    top: 100,
                    right: vm.value4 ? "20%" : "40%",
                    layout: 'horizontal',
                },
                series: [{
                    type: 'parallel',
                    data: this.paralleldata,
                    lineStyle: {
                        normal: {
                            width: 1,
                            color: '#7288AA',
                        }
                    }
                }, {
                    type: 'parallel',
                    data: this.specialparalleldata,
                    lineStyle: {
                        normal: {
                            width: 1,
                            color: 'rgb(0,122,205)', //'#7288AA',
                        }
                    }
                }]
            }
            parallelchart.setOption(paralleloption, {
                notMerge: true,
            });
        },
        Bubble: function(projectcolor, sprojectcolor, rate) {
            var width = $("#Graph").width();
            var title = "项目人力资源分摊图";
            //var color = d3.scale.category20c(); //设置不同颜色
            /*布局设置*/
            var bubble = d3.layout.pack() //初始化包图                       
                .sort(null) //后面的数减去前面的数排序，正负都变，null顺序不变
                .size([width, this.chartheight]) //设置范围
                .padding(1.5); //设置间距
            /*获取并添加svg元素，并设置宽高*/
            $("#Graph").empty();
            var svg = d3.select("#Graph").append("svg")
                .attr("width", width)
                .attr("height", this.chartheight)
                //添加背景
            svg.append("g")
                .append("rect")
                .attr("x", 0)
                .attr("y", 0)
                .attr("width", width)
                .attr("height", this.chartheight)
                .style("fill", "#333")
                //.style("stroke-width", 2)
                //.style("stroke", "#E7E7E7");
                //添加标题
            if (title != "") {
                svg.append("g")
                    .append("text")
                    .text(title)
                    .attr("class", "title")
                    .attr("x", 20)
                    .attr("y", 36)
                    .style("fill", "gray")
                    .style("font-size", 18)
                    .style("font-weight", "bolder");
            }
            //画legend
            svg.append("g")
                .append("image")
                .attr("x", width - 200)
                .attr("y", 60)
                .attr("height", 200)
                .attr("width", 200)
                .attr("xlink:href", "images/legend.jpg")
                //var data = { 贾嫒: 45494.848, 巩嫒: 16720.788, 余嫒: 26449.724, 梁安: 21023.016, 彭安: 3729.6 };
                /*entries可以将如上类型的格式转换成{key:家园,value:343434}的数组*/
            var result = vm.projectworkload; //d3.entries();
            /*以下是字符串拼接*/
            var startString = "{\"name\": \"flare\",\"children\": ["; //开头字符串
            result.forEach(function(dude) { //遍历result并且拼接
                    startString += "{\"name\":\"" + dude.key + "\",\"size\":" + dude.value /*+ ",\"develop\":" + dude.develop*/ + "},";
                })
                /*去除最后一个末尾的逗号，这个逗号会影响后面JSON.parse的使用*/
            startString = startString.substring(0, startString.length - 1);
            /*拼接尾部字符串*/
            startString += "]}";
            /*将拼接好的字符串转换成json对象*/
            var json2 = JSON.parse(startString);
            /*绘图部分*/
            //console.log(classes(json2));
            var node = svg.selectAll(".node")
                .data(bubble.nodes(classes(json2)) //绑定数据（配置结点）
                    .filter(function(d) {
                        return !d.children;
                    })) //数据过滤，满足条件返回自身（没孩子返回自身，有孩子不返回，这里目的是去除父节点）
                .enter().append("g")
                .attr("class", "node")
                .attr("transform", function(d) {
                    return "translate(" + d.x + "," + d.y + ")";
                }); //设定g移动
            node.append("title")
                .text(function(d) {
                    return d.className + ": " + (d.value);
                }); //设置移入时候显示数据   数据名和值
            node.append("circle")
                .attr("r", function(d) {
                    return d.r;
                }) //设置圆的半径
                .style("fill", function(d, i) {
                    return sprojectcolor[i] //color(d.value) need d no why;
                });
            node.append("circle")
                .attr("r", function(d, i) {
                    return d.r * rate[i];
                }) //设置圆的半径
                .style("fill", function(d, i) {
                    return projectcolor[i] //color(d.value) need d no why;
                }); //为圆形涂色
            node.append("clipPath")
                .attr("id", function(d) {
                    return "clip-" + d.id;
                })
                //@onlywan 设置use元素及其内容物为id为d.id的元素，即上面定义的 circle元素 ，作为裁减路径
                .append("use")
                .attr("xlink:href", function(d) {
                    return "#" + d.id;
                });
            node.append("text")
                //.attr("clip-path", d => d.clipUid)
                //.attr("dy", ".3em")
                //.selectAll("tspan")
                //.style("text-anchor", "middle")//设置文本对齐
                /*.data(function(d) {
                    var temp = new Array();
                    for (var i = 1; i <= Math.ceil(d.className.length / d.r * 5); i++)
                        temp[i - 1] = d.className.substring((i - 1) * d.r / 5, i * d.r / 5)
                    return temp;
                })
                .enter().append("tspan")
                .attr("x", 0)
                .attr("y", function(d, i, nodes) {
                    if (i < d.length / 2)
                        return i * 14;
                })*/
                .style("text-anchor", "middle")
                .text(function(d) {
                    return d.className.substring(0, d.r / 6);
                });
            // Returns a flattened hierarchy containing all leaf nodes under the root.
            function classes(root) {
                var classes = []; //存储结果的数组
                /*自定义递归函数
                 *
                 * 第二个参数指传入的json对象
                 * */
                function recurse(name, node) {
                    if (node.children) //如果有孩子结点 （这里的children不是自带的，是json里面有的）
                    {
                        node.children.forEach(function(child) { //将孩子结点中的每条数据
                            recurse(node.name, child);
                        })
                    } else {
                        classes.push({
                            className: node.name,
                            value: node.size
                        })
                    }; //如果自身是孩子结点的，将内容压入数组
                }
                recurse(null, root);
                return {
                    children: classes
                }; //返回所有的子节点  （包含在children中)             
            }
        },
        Radar: function() {
            echarts.dispose(document.getElementById('Graph'));
            var maxwork = 0;
            for (var i = 0; i < 6; i++) {
                if (maxwork < this.radardata[i]) {
                    maxwork = this.radardata[i]
                }
            }
            var radargraph = echarts.init(document.getElementById('Graph'));
            radaroption = {
                backgroundColor: '#333',
                title: {
                    text: "员工参与角色雷达图",
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                },
                tooltip: {},
                radar: {
                    shape: 'circle',
                    name: {
                        fontWeight: 'bolder',
                        fontSize: 14,
                    },
                    splitLine: {
                        color: "rbg(230,230,230)"
                    },
                    splitArea: {
                        show: false,
                        areaStyle: {
                            color: {
                                type: 'radial',
                                x: 0.5,
                                y: 0.5,
                                r: 0.5,
                                colorStops: [{
                                    offset: 0,
                                    color: '#78C1CF' // 0% 处的颜色
                                }, {
                                    offset: 1,
                                    color: '#fff' // 100% 处的颜色
                                }],
                                globalCoord: false // 缺省为 false
                            }
                        }
                    },
                    indicator: [{
                            name: '管理',
                            color: 'rgb(128, 100, 162)',
                            max: maxwork
                        },
                        {
                            name: '需求',
                            color: 'rgb(0, 112, 192)',
                            max: maxwork
                        },
                        {
                            name: '研发',
                            color: 'rgb(247, 150, 70)',
                            max: maxwork
                        },
                        {
                            name: '测试',
                            color: 'rgb(255, 255, 0)',
                            max: maxwork
                        },
                        {
                            name: '实施',
                            color: 'rgb(0, 176, 240)',
                            max: maxwork
                        },
                        {
                            name: '维保',
                            color: 'rgb(146, 208, 80)',
                            max: maxwork
                        }
                    ]
                },
                series: [{
                    type: 'radar',
                    lineStyle: {
                        color: 'rgb(120,193,255)',
                    },
                    areaStyle: {
                        normal: {
                            color: 'rgb(120,193,255)',
                            opacity: 0.7
                        }
                    },
                    data: [{
                        value: this.radardata,
                        name: '工作侧重'
                    }, ]
                }]
            }
            radargraph.setOption(radaroption, {
                notMerge: true,
            });
        },
        Boxplot: function() {
            echarts.dispose(document.getElementById('Graph'));
            var data = echarts.dataTool.prepareBoxplotData([
                this.boxmanage,
                this.boxinvestigate,
                this.boxdevelop,
                this.boxtest,
                this.boximplement,
                this.boxmaintenance
            ]);
            var boxplot = echarts.init(document.getElementById('Graph'));
            boxplotoption = {
                backgroundColor: '#333',
                title: [{
                    text: '箱线图',
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                    //left: 'center',
                }, ],
                tooltip: {
                    trigger: 'item',
                    axisPointer: {
                        type: 'shadow'
                    }
                },
                itemStyle: {
                    color: 'rbg(150,150,150)',
                    borderColor: '#2977E1',
                },
                grid: {
                    left: '10%',
                    right: '10%',
                    bottom: '15%'
                },
                xAxis: {
                    type: 'category',
                    data: ["管理", "需求", "研发", "测试", "实施", "维保"],
                    boundaryGap: true,
                    nameGap: 30,
                    splitArea: {
                        show: false
                    },
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: "gray",
                    }
                },
                yAxis: {
                    type: 'value',
                    name: '天',
                    splitArea: {
                        show: true
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: "gray",
                    }
                },
                series: [{
                        name: 'boxplot',
                        type: 'boxplot',
                        data: data.boxData,
                        tooltip: {
                            formatter: function(param) {
                                return [
                                    param.name + ': ',
                                    '外限:' + param.data[5],
                                    '上四分数: ' + param.data[4],
                                    '中位数: ' + param.data[3],
                                    '下四分数: ' + param.data[2],
                                    '内限: ' + param.data[1]
                                ].join('<br/>');
                            }
                        }
                    },
                    {
                        name: 'outlier',
                        type: 'scatter',
                        data: data.outliers
                    }
                ]
            };
            boxplot.setOption(boxplotoption, {
                notMerge: true,
            });
        },
        InverseBargraph: function(bargraph, index, option) {
            bargraphption = {
                backgroundColor: '#333',
                title: {
                    text: option,
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                },
                tooltip: {
                    trigger: "axis"
                },
                legend: {
                    data: ["管理", "需求", "研发", "测试", "实施", "维保", "年度预算"],
                    textStyle: {
                        color: "gray"
                    },
                    show: !index,
                },
                grid: {
                    bottom: '10%',
                    left: '40%',
                },
                xAxis: [{
                        type: 'value',
                        position: 'top',
                        splitLine: {
                            show: false
                        },
                        nameTextStyle: {
                            color: "gray"
                        },
                        axisLine: {
                            lineStyle: {
                                color: "gray"
                            }
                        },
                        axisLabel: {
                            color: "gray",
                        }
                    },
                    {
                        type: 'value',
                        splitLine: {
                            show: false
                        },
                        nameTextStyle: {
                            color: "gray"
                        },
                        axisLine: {
                            lineStyle: {
                                color: "gray"
                            }
                        },
                        axisLabel: {
                            color: "gray",
                        }
                    }
                ],
                yAxis: [{
                    data: this.time,
                    type: 'category',
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: "gray",
                        interval: false,
                        rotate: 0,
                    }
                }],
                series: [{
                        name: "管理",
                        type: "bar",
                        stack: "业务",
                        data: this.manage,
                        xAxisIndex: 0,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(128, 100, 162)"
                            }
                        }
                    },
                    {
                        name: "需求",
                        type: "bar",
                        stack: "业务",
                        xAxisIndex: 0,
                        data: this.investigate,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(0, 112, 192)"
                            }
                        }
                    },
                    {
                        name: "研发",
                        type: "bar",
                        stack: "业务",
                        xAxisIndex: 0,
                        data: this.develop,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(247, 150, 70)"
                            }
                        }
                    },
                    {
                        name: "测试",
                        type: "bar",
                        stack: "业务",
                        xAxisIndex: 0,
                        data: this.test,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(255, 255, 0)"
                            }
                        }
                    },
                    {
                        name: "实施",
                        type: "bar",
                        stack: "业务",
                        xAxisIndex: 0,
                        data: this.implement,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(0, 176, 240)"
                            }
                        }
                    },
                    {
                        name: "维保",
                        type: "bar",
                        stack: "业务",
                        xAxisIndex: 0,
                        data: this.maintenance,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(146, 208, 80)"
                            }
                        }
                    },
                    {
                        name: "年度预算",
                        type: 'line',
                        symbol: 'diamond',
                        symbolSize: 8,
                        stack: "年度预算",
                        xAxisIndex: 1,
                        data: this.annualamount,
                        label: {
                            show: true,
                            position: 'right',
                        },
                        itemStyle: {
                            color: 'rgb(200,200,200)',
                        },
                        lineStyle: {
                            color: 'rgb(200,200,200)',
                            width: 1,
                        }
                    }
                ]
            };
            bargraph[index].setOption(bargraphption, {
                notMerge: true,
            });
        },
        Bargraph: function(bargraph, index, option, samey, dept, item) {
            this.bargraphcount++;
            if (vm.charttype == 'workerbar')
                var a = item;
            bargraphption = {
                backgroundColor: '#333',
                title: [{
                    text: option,
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                }, {
                    show: vm.charttype == 'workerbar',
                    text: vm.charttype == 'workerbar' ? ("总计：" + this.workerbartotal + "天\n" +
                        (a.ContAmount > 0 || a.AnnualAmount > 0 ? ("营收：" + a.AnnualInvoiceAmount + "/" + a.ContAmount + "万\n") : "") +
                        (a.ReservedProfitAmount > 0 || a.AnnualProfitAmount > 0 ? ("预留：" + a.ReservedProfitAmount + "万") : "")) : "",
                    x: 'right',
                    textStyle: {
                        color: vm.charttype == 'workerbar' ? (this.workerbartotal * 0.2 < a.ContAmount - a.ReservedProfitAmount ? "rgb(34,177,76)" : (this.workerbartotal * 0.15 <= a.ContAmount - a.ReservedProfitAmount ? "rgb(255,200,100)" : "rgb(255,0,0)")) : 'gray',
                        fontSize: 16,
                    },
                    padding: 20,
                }],
                tooltip: {
                    trigger: "axis",
                },
                legend: {
                    data: dept ? ["管理", "需求", "研发", "测试", "实施", "维保", "休假", "其他"] : ["管理", "需求", "研发", "测试", "实施", "维保"],
                    textStyle: {
                        color: "gray"
                    },
                    show: !index,
                },
                grid: {
                    bottom: '10%',
                },
                dataZoom: [{
                    type: 'inside',
                    show: true,
                    xAxisIndex: [0],
                    start: 0,
                    end: this.charttype == 'workerbar' ? 60 : 100,
                    maxSpan: 100,
                    zoomLock: true,
                }],
                xAxis: {
                    data: this.time,
                    type: 'category',
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: function(value, index) {
                            if (value.lastIndexOf('bs') > -1) return 'white'
                            else if (value.lastIndexOf('hs') > -1) return 'red'
                            else if (value.lastIndexOf('ls') > -1) return '#409EFF'
                            else return 'gray'
                        },
                        formatter: function(value, index) {
                            var result;
                            if (value.lastIndexOf('bs') > -1) result = value.substring(2, value.length)
                            else if (value.lastIndexOf('hs') > -1) result = value.substring(2, value.length)
                            else if (value.lastIndexOf('ls') > -1) result = value.substring(2, value.length)
                            else result = value;
                            if (result.substring(0, 4) == new Date().getFullYear()) {
                                result = result.substring(5, 7);
                            }
                            return result;
                        },
                        interval: false,
                        rotate: option == "项目人力资源分摊" ? -20 : 0,
                    }
                },
                yAxis: {
                    max: function(value) {
                        if (samey) {
                            return vm.projectbarmax;
                        } else return value.max;
                    },
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: "gray",
                    }
                },
                series: [{
                        name: "管理",
                        type: "bar",
                        stack: "业务",
                        data: this.manage,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(128, 100, 162)"
                            }
                        }
                    },
                    {
                        name: "需求",
                        type: "bar",
                        stack: "业务",
                        data: this.investigate,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(0, 112, 192)"
                            }
                        },
                    },
                    {
                        name: "研发",
                        type: "bar",
                        stack: "业务",
                        data: this.develop,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(247, 150, 70)"
                            }
                        },
                    },
                    {
                        name: "测试",
                        type: "bar",
                        stack: "业务",
                        data: this.test,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(255, 255, 0)"
                            }
                        }
                    },
                    {
                        name: "实施",
                        type: "bar",
                        stack: "业务",
                        data: this.implement,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(0, 176, 240)"
                            }
                        }
                    },
                    {
                        name: "维保",
                        type: "bar",
                        stack: "业务",
                        data: this.maintenance,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(146, 208, 80)",
                            }
                        },
                    },
                    {
                        name: "休假",
                        type: "bar",
                        stack: "业务",
                        data: dept ? this.vacation : null,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(128, 128, 128)"
                            }
                        }
                    },
                    {
                        name: "其他",
                        type: "bar",
                        stack: "业务",
                        data: dept ? this.rest : null,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(255, 0, 0)"
                            }
                        }
                    },
                    {
                        name: "总和",
                        type: "line",
                        stack: "总和",
                        symbol: 'nonecircle',
                        hoverAnimation: false,
                        symbolSize: 1,
                        data: this.total,
                        label: {
                            show: true,
                            position: 'top',
                            color: "rgb(0, 112, 192)"
                        },
                        itemStyle: {
                            normal: {
                                color: "#333"
                            }
                        },
                        lineStyle: {
                            width: 0,
                        }
                    }
                ]
            };
            bargraph[index].setOption(bargraphption, {
                notMerge: true,
            });
            bargraph[index].group = "group1";
            if (this.bargraphcount == this.alldepartment.length) {
                echarts.connect('group1');
            }
        },
        Line: function(bargraph, index, option, samey, result) {
            this.bargraphcount++;
            var a = result;
            bargraphption = {
                backgroundColor: '#333',
                title: [{
                    text: option,
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                }, {
                    show: true,
                    text: "总计：" + this.workerbartotal + "天\n" +
                        (a.ContAmount > 0 || a.AnnualAmount > 0 ? ("营收：" + a.AnnualInvoiceAmount + "/" + a.ContAmount + "万\n") : "") +
                        (a.ReservedProfitAmount > 0 || a.AnnualProfitAmount > 0 ? ("预留：" + a.ReservedProfitAmount + "万") : ""),
                    x: 'right',
                    textStyle: {
                        color: this.workerbartotal * 0.2 < a.ContAmount - a.ReservedProfitAmount ? "rgb(34,177,76)" : (this.workerbartotal * 0.15 <= a.ContAmount - a.ReservedProfitAmount ? "rgb(255,200,100)" : "rgb(255,0,0)"),
                        fontSize: 16,
                    },
                    padding: 20,
                }],
                tooltip: {
                    trigger: "axis"
                },
                legend: {
                    data: ["管理", "需求", "研发", "测试", "实施", "维保"],
                    textStyle: {
                        color: "gray"
                    },
                    show: !index,
                },
                grid: {
                    bottom: '12%',
                },
                xAxis: {
                    data: this.time,
                    boundaryGap: false,
                    type: 'category',
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: function(value, index) {
                            if (value.lastIndexOf('bs') > -1) return 'white'
                            else if (value.lastIndexOf('hs') > -1) return 'red'
                            else if (value.lastIndexOf('ls') > -1) return '#409EFF'
                            else return 'gray'
                        },
                        formatter: function(value, index) {
                            var result;
                            if (value.lastIndexOf('bs') > -1) result = value.substring(2, value.length)
                            else if (value.lastIndexOf('hs') > -1) result = value.substring(2, value.length)
                            else if (value.lastIndexOf('ls') > -1) result = value.substring(2, value.length)
                            else result = value;
                            if (result.substring(0, 4) == new Date().getFullYear()) {
                                result = result.substring(5, 7);
                            }
                            return result;
                        },
                        interval: false,
                        rotate: -20,
                    }
                },
                yAxis: {
                    max: function(value) {
                        if (samey) {
                            return vm.projectbarmax;
                        } else return value.max;
                    },
                    splitLine: {
                        show: false
                    },
                    nameTextStyle: {
                        color: "gray"
                    },
                    axisLine: {
                        lineStyle: {
                            color: "gray"
                        }
                    },
                    axisLabel: {
                        color: "gray",
                    }
                },
                series: [{
                        name: "管理",
                        type: "line",
                        symbol: 'circle',
                        symbolSize: 2,
                        areaStyle: {
                            opacity: 1,
                        },
                        stack: "业务",
                        data: this.manage,
                        itemStyle: {
                            normal: {
                                color: "rgb(128, 100, 162)"
                            }
                        }
                    },
                    {
                        name: "需求",
                        type: "line",
                        symbol: 'circle',
                        symbolSize: 2,
                        areaStyle: { opacity: 1, },
                        stack: "业务",
                        data: this.investigate,
                        itemStyle: {
                            normal: {
                                color: "rgb(0, 112, 192)"
                            }
                        }
                    },
                    {
                        name: "研发",
                        type: "line",
                        symbol: 'circle',
                        symbolSize: 2,
                        areaStyle: { opacity: 1, },
                        stack: "业务",
                        data: this.develop,
                        itemStyle: {
                            normal: {
                                color: "rgb(247, 150, 70)"
                            }
                        }
                    },
                    {
                        name: "测试",
                        type: "line",
                        symbol: 'circle',
                        symbolSize: 2,
                        areaStyle: { opacity: 1, },
                        stack: "业务",
                        data: this.test,
                        itemStyle: {
                            normal: {
                                color: "rgb(255, 255, 0)"
                            }
                        }
                    },
                    {
                        name: "实施",
                        type: "line",
                        symbol: 'circle',
                        symbolSize: 2,
                        areaStyle: { opacity: 1, },
                        stack: "业务",
                        data: this.implement,
                        itemStyle: {
                            normal: {
                                color: "rgb(0, 176, 240)"
                            }
                        }
                    },
                    {
                        name: "维保",
                        type: "line",
                        symbol: 'circle',
                        symbolSize: 2,
                        areaStyle: { opacity: 1, },
                        stack: "业务",
                        data: this.maintenance,
                        itemStyle: {
                            normal: {
                                color: "rgb(146, 208, 80)"
                            }
                        }
                    }, {
                        name: "总和",
                        type: "line",
                        stack: "总和",
                        symbol: 'nonecircle',
                        hoverAnimation: false,
                        symbolSize: 1,
                        data: this.total,
                        label: {
                            show: true,
                            position: 'top',
                            color: "rgb(0, 112, 192)"
                        },
                        itemStyle: {
                            normal: {
                                color: "#333"
                            }
                        },
                        lineStyle: {
                            width: 0,
                        }
                    }
                ]
            };
            bargraph[index].setOption(bargraphption, {
                notMerge: true,
            });
            bargraph[index].group = "group1";
            if (this.bargraphcount == this.alldepartment.length) {
                echarts.connect('group1');
            }
        },
        Relationdiagram: function(div, option) {
            var relationdiagram = echarts.init(div);
            relationdiagramoption = {
                backgroundColor: '#333',
                title: {
                    text: option,
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                },
                tooltip: {
                    formatter: function(x) {
                        return x.data.des;
                    }
                },
                toolbox: {
                    show: true,
                    feature: {
                        myTool1: {
                            show: true,
                            title: " ",
                            icon: 'image://images/legend.jpg',
                            onclick: function() {}
                        },
                    },
                    itemSize: 200,
                    top: 60,
                },
                series: [{
                    type: 'graph',
                    layout: 'circular',
                    symbolSize: 20,
                    roam: true,
                    edgeLabel: {
                        normal: {
                            textStyle: {
                                fontSize: 20
                            }
                        }
                    },
                    circular: {
                        rotateLabel: true
                    },
                    draggable: true,
                    focusNodeAdjacency: true,
                    lineStyle: {
                        normal: {
                            color: '#7288AA',
                            curveness: 0.2,
                        }
                    },
                    //hoverAnimation: true,
                    emphasis: {
                        edgeLabel: {
                            show: true,
                            formatter: function(x) {
                                return x.data.source + "和" + x.data.target + "：" + x.data.lineStyle.width;
                            },
                            fontSize: 13,
                        }
                    },
                    label: {
                        normal: {
                            show: true,
                            position: 'right',
                            formatter: '{b}'
                        }
                    },
                    data: this.worker,
                    //[{
                    //name: '',
                    //des: '鼠标悬浮显示的内容',
                    //symbolSize: 10,
                    /*itemStyle: {
                        normal: {
                            color: 'red'
                        }
                    }*/
                    //}]
                    links: this.workerrelation,
                }]
            };
            relationdiagram.setOption(relationdiagramoption, {
                notMerge: true,
            });
        },
        forcelink: function(div, option) {
            var relationdiagram = echarts.init(div);
            relationdiagramoption = {
                backgroundColor: '#333',
                title: {
                    text: option, //this.charttype == "forcelink" ? '员工工作关系导向图' : this.charttype == "projectlink" ? "员工参与项目导向图" : "部门管理关系导向图",
                    textStyle: {
                        color: "gray"
                    },
                    padding: 20,
                },
                toolbox: {
                    show: true,
                    feature: {
                        myTool1: {
                            show: true,
                            title: " ",
                            icon: 'image://images/legend.jpg',
                            onclick: function() {}
                        },
                    },
                    itemSize: 200,
                    top: 60,
                },
                tooltip: {
                    formatter: function(x) {
                        return x.data.des;
                    }
                },
                series: [{
                    type: 'graph',
                    layout: 'force', //force为力引导布局circular
                    symbolSize: 20,
                    roam: true,
                    edgeSymbol: this.charttype == "forcelink" ? null : ['none', 'arrow'],
                    focusNodeAdjacency: true,
                    edgeSymbolSize: [0, 7],
                    edgeLabel: {
                        normal: {
                            textStyle: {
                                fontSize: 20
                            }
                        }
                    },
                    force: {
                        repulsion: 2500,
                        edgeLength: [10, 50]
                    },
                    draggable: true,
                    categories: ["worker", "project"],
                    itemStyle: {
                        normal: {
                            color: '#FA7292'
                        }
                    },
                    lineStyle: {
                        normal: {
                            color: '#7288AA',
                            curveness: 0.2,
                        }
                    },
                    emphasis: {
                        edgeLabel: {
                            show: true,
                            formatter: function(x) {
                                if (vm.charttype == "managelink")
                                    return x.data.target + "和" + x.data.source + "：" + x.data.lineStyle.width;
                                else return x.data.source + "和" + x.data.target + "：" + x.data.lineStyle.width;
                            },
                            fontSize: 13,
                            //color: 'rgb(200,200,200)',
                        }
                    },
                    label: {
                        normal: {
                            show: true,
                            position: 'right',
                            formatter: '{b}'
                        }
                    },
                    data: this.worker,
                    links: this.workerrelation,
                }]
            };
            relationdiagram.setOption(relationdiagramoption, {
                notMerge: true,
            });
        },
    }
})