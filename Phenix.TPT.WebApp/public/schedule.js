$(function() {
    fetchWorkSchedules(vue.state);
});

const pastMonths = 3;
const newMonths = 6;

function fetchWorkSchedules(state) {
    vue.myselfCompanyUsers = [];
    vue.filteredMyselfCompanyUsers = [];
    phAjax.getMyselfCompanyUsers({
        includeDisabled: false,
        onSuccess: function(result) {
            vue.myselfCompanyUsers = result;
            result.forEach((item, index) => {
                if (phAjax.isInRole('项目管理', item.PositionId)) {
                    if (state !== 0 || phAjax.isInRole('经营管理'))
                        vue.filteredMyselfCompanyUsers.push(item);
                    else if (item.Id === phAjax.getMyself().Id)
                        vue.filteredMyselfCompanyUsers.unshift(item); //自己的排第一
                    else
                        return;
                    base.call({
                        path: '/api/work-schedule/all',
                        pathParam: { manager: item.Id, pastMonths: pastMonths, newMonths: newMonths },
                        onSuccess: function(result) {
                            item.workSchedules = result;
                        },
                        onError: function(XMLHttpRequest, textStatus, validityError) {
                            alert('获取工作档期失败:\n' +
                                (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
                        },
                    });
                }
            });
            vue.state = state;
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取公司员工资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function saveWorkSchedule(item) {
    phAjax.ajax({
        type: "POST",
        uri: "WorkSchedule",
        data: item,
        onComplete: function(XMLHttpRequest, textStatus) {
            if (XMLHttpRequest.status == 200) {}
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            zdalert('系统提示', "工作档期写入失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

var vue = new Vue({
    el: '#content',
    data: {
        timeline: [],
        state: 0,
        stateTitle: [
            '当月项目的档期',
            '当月全员的档期',
        ],
        filterStateTitle: [
            '显示当月全员的档期',
            '显示当月项目的档期',
        ],

        myselfCompanyUsers: [],
        filteredMyselfCompanyUsers: [],
    },
    created: function() {
        var year = base.getDeadline().getFullYear();
        var month = base.getDeadline().getMonth() + 1;
        for (let i = -pastMonths; i < newMonths; i++) {
            if (month + i < 1) {
                this.timeline.push({
                    year: year - 1,
                    month: month + i + 12,
                });
            } else if (month + i > 12) {
                this.timeline.push({
                    year: year + 1,
                    month: month + i - 12,
                });
            } else {
                this.timeline.push({
                    year: year,
                    month: month + i,
                });
            }
        }
    },
    //watch: {
    //    workSchedule: {
    //        handler: function(newval, oldval) {
    //            if (newval[0][1].Year != undefined) {
    //                this.unchoose = [];
    //                for (let i = 0; i < newval[0].length; i++) {
    //                    this.unchoose.push(deepClone(this.allworker));
    //                    for (let j = 0; j < newval.length; j++) {
    //                        console.log(newval[j][i].Worker);
    //                        if (newval[j][i].Worker) {
    //                            let result = newval[j][i].Worker.split(" ");
    //                            console.log(j + '/' + i);
    //                            console.log(result);
    //                            for (let m = 0; m < result.length - 1; m++) {
    //                                let index = this.unchoose[i].indexOf(result[m]);
    //                                if (index > -1) {
    //                                    this.unchoose[i].splice(index, 1);
    //                                }
    //                            }
    //                            console.log(this.unchoose[i]);
    //                        }
    //                    }
    //                }
    //            }
    //        },
    //        deep: true,
    //        immediate: true,
    //    }
    //},
    methods: {
        onFiler: function () {
            fetchWorkSchedules(this.state >= 1 ? 0 : this.state + 1);
        },

        fullworkeritem: function(year, month, customer) {
            var result = new Array();
            this.fullworker.filter(function(c) { return c.Year == year && c.Month == month && c.Customer == customer }).forEach(function(a) {
                result.push(a.Worker);
            });
            return result;
        },
        surplus: function(total, minus) {
            if (total != null) {
                var result = total.split(" ");
                minus.forEach(function(a) {
                    var index = result.indexOf(a);
                    if (index > -1) {
                        result.splice(index, 1);
                    }
                })
                return result.join(" ");
            }
        },
        select: function(index, indexs, e) {
            clearTimeout(time); //首先清除计时器
            time = setTimeout(function() {
                if (vm.workSchedule[index][indexs].Managers == null) {
                    zdalert("系统提示", "该客户没有项目");
                } else if (indexs > 2 && globaluser.Department.Name != "公司管理") {
                    vm.openindex = index;
                    vm.openindexs = indexs;
                    for (var i = 0; i < vm.chooseworker.length; i++) {
                        if (vm.workSchedule[vm.openindex][vm.openindexs].Worker != null && vm.workSchedule[vm.openindex][vm.openindexs].Worker.indexOf(vm.allworker[i]) > -1) {
                            Vue.set(vm.chooseworker, i, true);
                        } else Vue.set(vm.chooseworker, i, false);
                    }
                    if (vm.workSchedule[vm.openindex][vm.openindexs].Managers.indexOf(globaluser.UserName) > -1 || (globaluser.Position != null && globaluser.Position.Name == "管理"))
                        vm.canchange = false;
                    else vm.canchange = true;
                    $("#myModal").modal('show');
                }
            }, 300);
        },
        closemodal: function() {
            $("#myModal").modal('hide');
        },
        saveschedule: function() {
            this.workSchedule[this.openindex][this.openindexs].Worker = "";
            for (var i = 0; i < this.chooseworker.length; i++) {
                if (this.chooseworker[i]) {
                    this.workSchedule[this.openindex][this.openindexs].Worker += this.allworker[i] + " ";
                }
            }
            saveWorkSchedule(this.workSchedule[this.openindex][this.openindexs]);
            $("#myModal").modal('hide');
        },
        copy: function(index, indexs) {
            clearTimeout(time);
            if (this.workSchedule[index][indexs].Managers.indexOf(globaluser.UserName) > -1 || (globaluser.Position != null && globaluser.Position.Name == "管理")) {
                if (indexs + 1 > 1 && this.workSchedule[index][indexs + 1].Managers != null) {
                    this.workSchedule[index][indexs + 1].Worker = this.workSchedule[index][indexs].Worker;
                    saveWorkSchedule(this.workSchedule[index][indexs + 1]);
                }
            }
        }
    }
})