$(function() {
    fetchMyselfCompanyUsers(vue.state);
    fetchWorkSchedules(vue.state);
});

const pastMonths = 3;
const newMonths = 6;

var workSchedules = null;

function fetchMyselfCompanyUsers(state) {
    phAjax.getMyselfCompanyUsers({
        includeDisabled: false,
        onSuccess: function(result) {
            vue.myselfCompanyUsers = result;
            filterMyselfCompanyUsers(result, workSchedules, state);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取公司员工资料失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function fetchWorkSchedules(state) {
    base.call({
        path: '/api/work-schedule/all',
        pathParam: { pastMonths: pastMonths, newMonths: newMonths },
        onSuccess: function(result) {
            workSchedules = result;
            filterMyselfCompanyUsers(vue.myselfCompanyUsers, result, state);
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('获取工作档期失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function filterMyselfCompanyUsers(myselfCompanyUsers, workSchedules, state) {
    if (myselfCompanyUsers == null || workSchedules == null)
        return;
    vue.filteredMyselfCompanyUsers = [];
    var filteredMyselfCompanyUsers = [];
    myselfCompanyUsers.forEach((user, userIndex) => {
        if (workSchedules[user.Id] != null) {
            if (user.Id === phAjax.getMyself().Id)
                filteredMyselfCompanyUsers.unshift(user); //自己的排第一
            else if (state !== 0 || phAjax.isInRole('经营管理') || !phAjax.isInRole('项目管理'))
                filteredMyselfCompanyUsers.push(user);
            else
                return;
            var priorWorkSchedule = null;
            user.workSchedules = workSchedules[user.Id];
            user.workSchedules.forEach((workSchedule, workScheduleIndex) => {
                workSchedule.manager = user; //绑定用
                if (priorWorkSchedule != null)
                    priorWorkSchedule.next = workSchedule; //导航用
                priorWorkSchedule = workSchedule;
            });
        }
    });
    vue.filteredMyselfCompanyUsers = filteredMyselfCompanyUsers;
    vue.state = state;
}

function putWorkSchedule(workSchedule, data) {
    base.call({
        type: "PUT",
        path: '/api/work-schedule',
        data: data,
        onSuccess: function(result) {
            workSchedule.Workers = data.Workers;
            vue.$forceUpdate();
        },
        onError: function(XMLHttpRequest, textStatus, validityError) {
            alert('提交工作档期失败:\n' + (validityError != null ? validityError.Hint : XMLHttpRequest.responseText));
        },
    });
}

function showChangeWorkScheduleDialog() {
    $('#changeWorkScheduleDialog').modal('show'); // id="changeWorkScheduleDialog"
}

function hideChangeWorkScheduleDialog() {
    $('#changeWorkScheduleDialog').modal('hide'); //id="changeWorkScheduleDialog"
}

var eventTime = null;

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

        currentWorkSchedule: { manager: {} },
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
    methods: {
        parseUserNames: function(ids) {
            if (this.myselfCompanyUsers != null) {
                var result = [];
                ids.forEach((id, index) => {
                    var user = this.myselfCompanyUsers.find(item => item.Id === id);
                    if (user != null)
                        result.push(user.RegAlias);
                });
                return result;
            }
            return ' ';
        },

        onFiler: function() {
            filterMyselfCompanyUsers(this.myselfCompanyUsers, workSchedules, this.state >= 1 ? 0 : this.state + 1);
        },

        allowChangeWorkSchedule: function(workSchedule, workScheduleIndex) {
            return workScheduleIndex >= pastMonths &&
                (workSchedule.Manager === phAjax.getMyself().Id || phAjax.isInRole('经营管理'));
        },

        extendWorkSchedule: function(workSchedule) {
            clearTimeout(eventTime); //清除计时器
            if (workSchedule.next != null) {
                var data = JSON.parse(JSON.stringify(phUtils.trimData(workSchedule.next, false, true)));
                data.Workers = JSON.parse(JSON.stringify(workSchedule.Workers));
                putWorkSchedule(workSchedule.next, data);
            }
        },

        onShowChangeWorkScheduleDialog: function(workSchedule) {
            clearTimeout(eventTime); //清除计时器
            eventTime = setTimeout(function() {
                    vue.currentWorkSchedule = workSchedule;
                    vue.myselfCompanyUsers.forEach((user, userIndex) => {
                        user.checked = workSchedule.Workers.find(item => item === user.Id) != null;
                    });
                    showChangeWorkScheduleDialog();
                }, 300);
        },

        onChangeWorkSchedule: function() {
            var data = JSON.parse(JSON.stringify(phUtils.trimData(this.currentWorkSchedule, false, true)));
            vue.myselfCompanyUsers.forEach((user, userIndex) => {
                if (user.checked)
                    data.Workers.push(user.Id);
            });
            putWorkSchedule(this.currentWorkSchedule, data);
            hideChangeWorkScheduleDialog();
        },
    }
})