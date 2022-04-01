$(document).ready(function() {
    navbarControl();
    setTimeout(a, 1000);
});

window.onresize = function(e) {
    echarts.init(document.getElementById('chart')).resize();
}
var regyear = /\d{4}(?=-)/;

function a() {
    var projectname = JSON.parse(localStorage.getItem('projectname'));
    if (projectname) {
        location.href = "#" + projectname;
        localStorage.removeItem('projectname');
    }
}

function handleData(workloadG, amountG) {
    if (workloadG.length != amountG.length) {
        zdalert("系统提示", "数据不一致！")
    } else {
        for (var i = 0; i < amountG.length; i++) {
            var sortitem = {
                Customer: amountG[i][0][vm.rule],
                ContAmount: 0,
                ContInvoiceAmount: 0,
                ContUnbilled: 0,
                AnnualAmount: 0,
                AnnualInvoiceAmount: 0,
                AnnualUnbilledAmount: 0,
                ReservedProfitAmount: 0,
                AnnualProfitAmount: 0,
                EstimateWorkload: 0,
                Workload: 0,
            };
            workloadG[i].forEach(function(d) {
                sortitem.Workload += d.WorkloadSum;
            });
            amountG[i].forEach(function(d) {
                sortitem.ContAmount = addFix2(sortitem.ContAmount, d.ContAmount);
                sortitem.ContInvoiceAmount = addFix2(sortitem.ContInvoiceAmount, d.ContInvoiceAmount);
                sortitem.ContUnbilled = addFix2(sortitem.ContUnbilled, addFix2(d.ContAmount, -d.ContInvoiceAmount));
                sortitem.AnnualAmount = addFix2(sortitem.AnnualAmount, d.AnnualAmount);
                sortitem.AnnualInvoiceAmount = addFix2(sortitem.AnnualInvoiceAmount, d.AnnualInvoiceAmount);
                sortitem.AnnualUnbilledAmount = addFix2(sortitem.AnnualUnbilledAmount, addFix2(d.AnnualAmount, -d.AnnualInvoiceAmount));
                sortitem.ReservedProfitAmount = addFix2(sortitem.ReservedProfitAmount, d.ReservedProfitAmount);
                sortitem.AnnualProfitAmount = addFix2(sortitem.AnnualProfitAmount, d.AnnualProfitAmount);
                sortitem.EstimateWorkload = addFix2(sortitem.EstimateWorkload, d.EstimateWorkload == null ? 0 : d.EstimateWorkload);
            })
            if (sortitem.ContAmount > 0) {
                vm.sort.push(sortitem);
            }
        }
    }
}

function fetchDepartmentProject(option) {
    phAjax.ajax({
        uri: "ProjectWorkloadNotime/",
        onSuccess: function(result) {
            result = result.filter(function(c) { return c.Closed == false })
            result.sort(function(a, b) {
                return a[vm.rule].localeCompare(b[vm.rule], 'zh-Hans-CN', {
                    sensitivity: 'accent'
                })
            });
            var workloadG = groupBy(result, vm.rule);
            phAjax.ajax({
                uri: "ProjectInfo/DepartmentProject?option=" + encodeURI(option),
                onSuccess: function(subresult) {
                    vm.sort = [];
                    subresult = subresult.filter(function(c) { return c.Closed == false && c.ContAmount > 0 })
                    subresult.sort(function(a, b) {
                        return a[vm.rule].localeCompare(b[vm.rule], 'zh-Hans-CN', {
                            sensitivity: 'accent'
                        })
                    });
                    vm.amountG = groupBy(subresult, vm.rule);
                    handleData(workloadG, vm.amountG);
                    drawAorP();
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    console.log("调用allprojectService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
                },
            })
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allworkerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    })
}

function fetchAnnualProject(department) {
    phAjax.ajax({
        uri: "ProjectWorkloadAnnual/",
        onSuccess: function(result) {
            result.sort(function(a, b) {
                return a[vm.rule].localeCompare(b[vm.rule], 'zh-Hans-CN', {
                    sensitivity: 'accent'
                })
            });
            var workloadG = groupBy(result, vm.rule);
            phAjax.ajax({
                uri: "ProjectInfo/DepartmentProject?option=" + encodeURI(department),
                onSuccess: function(subresult) {
                    vm.sort = [];
                    vm.annualAmountTotal = 0;
                    vm.annualInvoicedAmountTotal = 0;
                    vm.ReservedProfitAmount = 0;
                    subresult = subresult.filter(function(c) { return c.ContAmount > 0 })
                    subresult.sort(function(a, b) {
                        return a[vm.rule].localeCompare(b[vm.rule], 'zh-Hans-CN', {
                            sensitivity: 'accent'
                        })
                    });
                    vm.amountG = groupBy(subresult, vm.rule);
                    handleData(workloadG, vm.amountG)
                    drawAorP()
                },
                onError: function(XMLHttpRequest, textStatus, errorThrown) {
                    console.log("调用allprojectService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
                },
            });
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用allworkerService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function drawAorP() {
    vm.departmentproject = [];
    vm.annualAmountTotal = 0;
    vm.annualInvoicedAmountTotal = 0;
    vm.ReservedProfitAmount = 0;
    if (vm.category == 'annual') {
        var sortType = vm.sortvalue ? "Workload" : "AnnualAmount";
        var amountType = "AnnualAmount",
            invoiceType = "AnnualInvoiceAmount",
            unbillType = "AnnualUnbilledAmount",
            profitType = "AnnualProfitAmount";
    } else if (vm.category == 'project') {
        var sortType = vm.sortvalue ? "Workload" : "ContAmount";
        var amountType = "ContAmount",
            invoiceType = "ContInvoiceAmount",
            unbillType = "ContUnbilled",
            profitType = "ReservedProfitAmount";
    }
    vm.sort.sort(function(a, b) {
        return -a[sortType] + b[sortType]
    });
    vm.sort.forEach(function(c) {
        let i = 0;
        while (i < vm.amountG.length) {
            if (vm.amountG[i][0][vm.rule] == c.Customer) {
                vm.amountG[i].sort(function(a, b) {
                    return -a[sortType] + b[sortType]
                })
                vm.amountG[i].forEach(function(d) {
                    if (d[amountType] > 0) vm.departmentproject.push(d)
                })
                break;
            } else {
                i++;
            }
        }
    })
    vm.showinvoice = new Array(vm.departmentproject.length);
    vm.invoicedata = new Array(vm.departmentproject.length);
    vm.dropup = new Array(vm.departmentproject.length);
    vm.customer = [];
    vm.invoiced = [];
    vm.unbilled = [];
    vm.reservedprofitamount = [];
    vm.linedata = [];
    for (var i = 0; i < vm.sort.length; i++) {
        vm.customer.push(vm.sort[i].Customer);
        vm.invoiced.push(vm.sort[i][invoiceType]);
        vm.unbilled.push(vm.sort[i][unbillType]);
        vm.reservedprofitamount.push(vm.sort[i][profitType]);
        vm.linedata.push(vm.sort[i].Workload);
        vm.annualAmountTotal = addFix2(vm.annualAmountTotal, addFix2(vm.sort[i][invoiceType], vm.sort[i][unbillType]));
        vm.annualInvoicedAmountTotal = addFix2(vm.annualInvoicedAmountTotal, vm.sort[i][invoiceType]);
        vm.ReservedProfitAmount = addFix2(vm.ReservedProfitAmount, vm.sort[i][profitType]);
    }
    vm.Bar();
}

function fetchAllDepartment() {
    phAjax.ajax({
        uri: "Department/WorkDepartment?",
        onSuccess: function(result) {
            vm.optionlist = result;
            if (result.length > 0 && vm.option.length == 0) {
                vm.option = vm.optionlist[0];
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用alldepartmentService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function fetchinvoice(PI_ID, index) {
    phAjax.ajax({
        uri: "ProjectInvoice/?id=" + PI_ID,
        onSuccess: function(result) {
            Vue.set(vm.invoicedata, index, result);
            var btn = document.getElementsByClassName("button1")[index];
            if (vm.dropup[index] == "" || vm.dropup[index] == undefined) {
                Vue.set(vm.dropup, index, "dropup");
                Vue.set(vm.showinvoice, index, true);
                btn.innerHTML = '收起明细<span class="caret" style="color: rgb(255, 255, 255);"></span>';
            } else {
                Vue.set(vm.dropup, index, "");
                Vue.set(vm.showinvoice, index, false);
                btn.innerHTML = '开票明细<span class="caret" style="color: rgb(255, 255, 255);"></span>';
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("调用invoiceService失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    });
}

function deletein(id, index, indexs) {
    phAjax.ajax({
        type: "DELETE",
        uri: "ProjectInvoice?id=" + id,
        onComplete: function(XMLHttpRequest, textStatus) {
            if (XMLHttpRequest.status == 200) {
                vm.departmentproject[index].ContInvoiceAmount = parseFloat((vm.departmentproject[index].ContInvoiceAmount - parseFloat(vm.invoicedata[index][indexs].Amount.toFixed(2))).toFixed(2));
                if (regyear.exec(vm.invoicedata[index][indexs].Date)[0] == new Date().getFullYear())
                    vm.departmentproject[index].AnnualInvoiceAmount = parseFloat((vm.departmentproject[index].AnnualInvoiceAmount - parseFloat(vm.invoicedata[index][indexs].Amount.toFixed(2))).toFixed(2));
                vm.invoicedata[index].splice(indexs, 1);
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            zdalert('系统提示', "删除失败！");
            console.log("调用Service失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
        },
    })
}

function saveInvoice(invoice, index, option) {
    phAjax.ajax({
        type: "POST",
        uri: "ProjectInvoice",
        data: invoice,
        onComplete: function(XMLHttpRequest, textStatus) {
            if (XMLHttpRequest.status == 200) {
                if (option == -1) {
                    vm.departmentproject[index].ContInvoiceAmount = parseFloat((vm.departmentproject[index].ContInvoiceAmount + parseFloat(invoice.Amount)).toFixed(2));
                    if (regyear.exec(invoice.Date)[0] == new Date().getFullYear())
                        vm.departmentproject[index].AnnualInvoiceAmount = parseFloat((vm.departmentproject[index].AnnualInvoiceAmount + parseFloat(invoice.Amount)).toFixed(2));
                    vm.newinvoice.Amount = "";
                    vm.newinvoice.Date = "";
                    vm.newinvoice.Remark = "";
                } else {
                    vm.departmentproject[index].ContInvoiceAmount = parseFloat((vm.departmentproject[index].ContInvoiceAmount - parseFloat(option.toFixed(2)) + parseFloat(invoice.Amount)).toFixed(2));
                    if (regyear.exec(invoice.Date)[0] == new Date().getFullYear())
                        vm.departmentproject[index].AnnualInvoiceAmount = parseFloat((vm.departmentproject[index].AnnualInvoiceAmount - parseFloat(option.toFixed(2)) + parseFloat(invoice.Amount)).toFixed(2));
                }
                Vue.set(vm.dropup, index, "");
                fetchinvoice(vm.departmentproject[index].PI_ID, index);
            }
        },
        onError: function(XMLHttpRequest, textStatus, errorThrown) {
            zdalert('系统提示', "保存失败！\n\r请检查所输入的内容！");
            console.log("调用Service失败! status: " + XMLHttpRequest.statusText + ", response: " + XMLHttpRequest.responseText);
            vm.invoicedata = deepClone(vm.tempinvoicedata);
            vm.change = false;
            $("#myModal").modal('hide');
        },
    });
}

var vm = new Vue({
    el: "#content",
    data: {
        user: [],
        category: "project",
        rule: "Customer",
        option: "",
        optionlist: [],
        departmentproject: [],
        customer: [],
        invoiced: [],
        unbilled: [],
        reservedprofitamount: [],
        sort: [],
        hide: "visibility:hidden",
        invoicedata: [],
        showinvoice: [],
        dropup: [],
        annualAmountTotal: 0,
        annualInvoicedAmountTotal: 0,
        ReservedProfitAmount: 0,
        linedata: [],
        newinvoice: {
            PI_ID: "",
            PI_PI_ID: "",
            Amount: "",
            Date: "",
            Remark: "",
        },
        datedisable: true,
        sortvalue: false,
        amountG: [],
    },
    mounted: function() {
        this.user = globaluser;
        if (this.user.Department.Name != "公司管理" && this.user.Position.Name != "管理" && !(this.user.Roles.项目管理)) {
            window.location.href = "index.html"
        } else {
            var option = JSON.parse(localStorage.getItem('departmentoption'))
            let category = JSON.parse(localStorage.getItem('category'));
            let rule = JSON.parse(localStorage.getItem('rule'));
            if (category) {
                this.category = category;
                localStorage.removeItem('category');
            } else {
                if (this.user.Department.Name != "公司管理" && this.user.Position.Name == "管理") {
                    this.category = "annual";
                }
            }
            if (rule) {
                this.rule = rule;
                localStorage.removeItem('rule');
            }
            if (!option) {
                if (this.user.Department.Name != "公司管理") {
                    this.optionlist.push(this.user.Department.Name);
                    this.option = this.user.Department.Name;
                }
            } else {
                this.optionlist.push(option);
                this.option = option;
            }
            fetchAllDepartment();
            localStorage.removeItem("departmentoption")
        }
    },
    watch: {
        category: function() {
            this.search();
        },
        option: function() {
            this.search();
        },
        rule: function() {
            this.search();
        },
        sortvalue: function() {
            drawAorP();
        }
    },
    methods: {
        write: function(e) {
            if ($(e.target).parent().find(".fa-check").hasClass("hide")) {
                $(e.target).parent().find(".fa-check").removeClass("hide");
                $(e.target).addClass("hide");
                $(e.target).parent().find(".invoice").removeAttr("disabled");
                vm.datedisable = false;
            }
        },
        saveinvoice: function(index, indexs, e) {
            if (indexs != -1) {
                var option = this.invoicedata[index][indexs].Amount;
                this.invoicedata[index][indexs].Amount = $(e.target).parent().find(".invoiceamount").val();
                saveInvoice(this.invoicedata[index][indexs], index, option);
                $(e.target).parent().find(".fa-pencil").removeClass("hide");
                $(e.target).addClass("hide");
                $(e.target).parent().find(".invoice").attr("disabled", 'true');
            } else {
                this.newinvoice.PI_PI_ID = this.departmentproject[index].PI_ID;
                saveInvoice(this.newinvoice, index, -1);
                if ($(e.target).length > 0) {
                    $(e.target).parent().parent().addClass("hide");
                    $(e.target).parent().parent().parent().find(".addli").removeClass("hide");
                } else {
                    e.parent().parent().addClass("hide");
                    e.parent().parent().parent().find(".addli").removeClass("hide");
                }
            }
            vm.datedisable = true;
        },
        end: function(index, e) {
            var a = $(e.target).parent().parent().parent().find(".newrecord");
            zdconfirm('系统确认框', '你确认提交该条数据吗？', function(r) {
                if (r) {
                    vm.newinvoice.Amount = vm.departmentproject[index].ContAmount - vm.departmentproject[index].ContInvoiceAmount;
                    vm.newinvoice.Date = new Date().getFullYear() + '-' + (new Date().getMonth() + 1) + '-' + new Date().getDate();
                    vm.newinvoice.Remark = '完结（' + vm.user.UserName + '）';
                    if (a.hasClass("hide")) {
                        a.removeClass("hide")
                        $(e.target).parent().parent().addClass("hide");
                    }
                    vm.saveinvoice(index, -1, a.find(".fa-check"));
                }
            });
        },
        add: function(e) {
            var a = $(e.target).parent().parent().parent().find(".newrecord");
            if (a.hasClass("hide")) {
                a.removeClass("hide")
                $(e.target).parent().parent().addClass("hide");
            }
        },
        deleteinvoice: function(id, index, indexs, e) {
            zdconfirm('系统确认框', '确定要删除此项记录么？', function(r) {
                if (r) {
                    if (indexs == -1) {
                        vm.newinvoice.Amount = "";
                        vm.newinvoice.Date = "";
                        vm.newinvoice.Remark = "";
                        $(e.target).parent().parent().parent().find(".newrecord").addClass("hide");
                        $(e.target).parent().parent().parent().find(".addli").removeClass("hide");
                    } else deletein(id, index, indexs);
                }
            });

        },
        showproject: function(index) {
            localStorage.setItem('project', JSON.stringify(this.departmentproject[index]));
            localStorage.setItem('departmentoption', JSON.stringify(this.option));
            localStorage.setItem('category', JSON.stringify(this.category));
            localStorage.setItem('rule', JSON.stringify(this.rule));
            window.location.href = "projectinfo.html"
        },
        search: function() {
            if (this.category == "project") {
                fetchDepartmentProject(this.option);
            } else {
                fetchAnnualProject(this.option);
            }
        },
        searchinvoice: function(index) {
            fetchinvoice(this.departmentproject[index].PI_ID, index);
        },
        Bar: function() {
            chart = echarts.init(document.getElementById('chart'))
            baroption = {
                title: [{
                        text: (this.category == "annual") ? "预计开票:\n实际开票:\n年度预留:" : null,
                        top: 10,
                        right: 130,
                        textAlign: "right",
                        textStyle: {
                            color: "rgb(0,47,147)",
                            fontStyle: "normal",
                            fontWeight: "bold",
                            fontFamily: "Arial",
                            align: "right",
                            fontSize: 16,
                        }
                    },
                    {
                        text: (this.category == "annual") ? this.annualAmountTotal.toFixed(2) + "万\n" + this.annualInvoicedAmountTotal.toFixed(2) + "万\n" + this.ReservedProfitAmount.toFixed(2) + "万" : null,
                        top: 10,
                        right: 40,
                        textAlign: "right",
                        textStyle: {
                            color: "rgb(0,47,147)",
                            fontStyle: "normal",
                            fontWeight: "bold",
                            fontFamily: "Arial",
                            align: "right",
                            fontSize: 16,
                        }
                    }
                ],
                tooltip: {
                    trigger: "axis"
                },
                grid: {
                    bottom: 120,
                },
                legend: {
                    data: ["未开票", "已开票", "预留", "工作量"],
                },
                xAxis: {
                    data: this.customer,
                    type: 'category',
                    splitLine: {
                        show: false
                    },
                    axisLabel: {
                        interval: 0,
                        formatter: function(value) {
                            //x轴的文字改为竖版显示
                            var str = value.split("");
                            return str.join("\n");
                        },
                        color: function(params, index) {
                            if (vm.sort[index].EstimateWorkload > 0 && vm.linedata[index] > vm.sort[index].EstimateWorkload) {
                                return 'rgb(255,0,0)';
                            } else if (vm.linedata[index] * 0.2 < vm.invoiced[index] + vm.unbilled[index] - vm.reservedprofitamount[index]) //-利益
                            {
                                return 'rgb(34,177,76)';
                            } else if (vm.linedata[index] * 0.2 >= vm.invoiced[index] + vm.unbilled[index] - vm.reservedprofitamount[index] && vm.linedata[index] * 0.15 <= vm.invoiced[index] + vm.unbilled[index] - vm.reservedprofitamount[index]) //-利益
                            {
                                return 'rgb(255,200,100)';
                            } else {
                                return 'rgb(255,0,0)';
                            }
                        }
                    },
                },
                yAxis: [{
                        type: 'value',
                        position: 'left',
                        splitLine: {
                            show: false
                        },
                    },
                    {
                        type: 'value',
                        position: 'right',
                        splitLine: {
                            show: false
                        },
                    }
                ],
                series: [{
                        name: "已开票",
                        type: "bar",
                        stack: "金额", //折叠显示
                        data: this.invoiced,
                        yAxisIndex: 0,
                        barMaxWidth: 38,
                        //显示颜色
                        itemStyle: {
                            normal: {
                                color: "rgb(0,47,147)",
                                borderColor: "rgb(0,47,147)"
                            }
                        }
                    },
                    {
                        name: "未开票",
                        type: "bar",
                        stack: "金额",
                        data: this.unbilled,
                        yAxisIndex: 0,
                        barMaxWidth: 38,
                        itemStyle: {
                            normal: {
                                color: "rgb(250,250,255)",
                                borderColor: "rgb(0,47,147)"
                            }
                        }
                    },
                    {
                        name: '预留',
                        stack: "利益", //用于堆叠累加
                        yAxisIndex: 0,
                        type: 'line',
                        symbol: 'diamond',
                        symbolSize: 10,
                        data: vm.reservedprofitamount,
                        label: {
                            show: true,
                        },
                        itemStyle: {
                            color: 'rgb(252,144,149)',
                        },
                        lineStyle: {
                            color: 'rgb(252,144,149)',
                        }
                    },
                    {
                        name: '工作量',
                        stack: "工作量",
                        yAxisIndex: 1,
                        type: 'line',
                        symbol: 'diamond',
                        symbolSize: 10,
                        data: this.linedata,
                        label: {
                            show: true,
                            formatter: function(params) {
                                if (vm.sort[params.dataIndex].EstimateWorkload > 0 && params.data > vm.sort[params.dataIndex].EstimateWorkload) {
                                    return '{a|' + params.data + '}';
                                } else if (params.data * 0.2 < vm.invoiced[params.dataIndex] + vm.unbilled[params.dataIndex] - vm.reservedprofitamount[params.dataIndex]) //-利益
                                {
                                    return '{c|' + params.data + '}';
                                } else if (params.data * 0.2 >= vm.invoiced[params.dataIndex] + vm.unbilled[params.dataIndex] - vm.reservedprofitamount[params.dataIndex] && params.data * 0.15 <= vm.invoiced[params.dataIndex] + vm.unbilled[params.dataIndex] - vm.reservedprofitamount[params.dataIndex]) //-利益
                                {
                                    return '{b|' + params.data + '}';
                                } else {
                                    return '{a|' + params.data + '}';
                                }
                            },
                            rich: {
                                a: {
                                    color: 'rgb(255,0,0)' //红色
                                },
                                b: {
                                    color: 'rgb(255,200,100)' //橘色
                                },
                                c: {
                                    color: 'rgb(34,177,76)', //绿色
                                }
                            }
                        },
                        itemStyle: {
                            color: 'rgb(34,177,76)',
                        },
                        lineStyle: {
                            color: 'rgb(34,177,76)',
                        }
                    }
                ]
            }
            chart.setOption(baroption);
        },
    }
})