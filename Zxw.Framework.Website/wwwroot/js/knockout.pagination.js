var viewmodel = {
    rows: ko.observableArray([]),
    total: ko.observable(0),
    pageSize: ko.observable(10),
    pageIndex: ko.observable(1),
    pageCount: ko.observable(0),
    pageNumbers: ko.observableArray(),
    targetPageIndex: ko.observable(1),
    remoteUrl: ko.observable(''),
    emptyText: ko.observable('暂无数据'),
    next: function () {
        if (this.pageIndex() < this.pageCount()) {
            this.pageIndex(this.pageIndex() + 1);
            loadData();            
        } else {
            layer.msg("已经是最后一页了");
        }
    },
    previous: function () {
        if (this.pageIndex() > 1) {
            this.pageIndex(this.pageIndex() - 1);
            loadData();            
        } else {
            layer.msg("已经是第一页了");
        }
    },
    gotopage: function (data, event) {
        if (data > this.pageCount() || data < 0) {
            layer.msg("您输入的页码超出范围了，无法为您完成跳转。");
        } else {
            this.pageIndex(data);
            loadData();            
        }
    },
    checkExistedData: function() {
        return this.total() > 0;
    },
    checkShowPagination: function() {
        return this.pageCount() > 1;
    },
    isFirst:function(){
        return this.pageIndex()==1;
    },
    isLast:function(){
        return this.pageIndex()==this.pageCount();
    },
    loadData:function() {
        this.get(
            this.remoteUrl(),
            {
                pageSize: this.pageSize(),
                pageIndex: this.pageIndex()
            },
            function (data) {
                if (data.success == true) {
                    viewmodel.pageNumbers.removeAll();
                    ko.mapping.fromJS(data, {}, viewmodel);
                    for (var i = 0; i < data.pageCount; i++) {
                        viewmodel.pageNumbers.push(i + 1);
                    }
                } else {
                    layer.alert(data.msg, { icon: 5 });
                    return;
                }
            }
        );
    },
    get: function(url, data, callback) {
        $.ajax({
            url: url,
            data: data,
            type: "GET",
            dataType: "JSON",
            success: callback,
            error: function(req) {
                layer.alert('网络错误，请稍后再试。', {icon:5});
            }
        });
    },
    post: function(url, data, callback) {
        $.ajax({
            url: url,
            data: data,
            type: "POST",
            dataType: "JSON",
            success: callback,
            error: function(req) {
                layer.alert('网络错误，请稍后再试。', {icon:5});
            }
        });
    },
    add: function(url,title) {
        this.open(url, title);
    },
    edit: function (url, title) {
        this.open(url, title);
    },
    open: function (url, title) {
        var self = this;
        layer.open({
            type: 2,
            title: title,
            shadeClose: true,
            shade: 0.6,
            area: ['600px', '80%'],
            content: url, //iframe的url
            end: function () {
                self.loadData();
            }
        });
    }, 
    active: function(url, data) {
        var self = this;
        self.get(url, data, function (rep) {
            var icon = rep.success == true ? 1 : 2;
            layer.alert(rep.msg,
                { icon: icon}, function(index){
                layer.close(index);
                self.loadData();
            });
        });
    },
    visualize: function(url, data) {
        var self = this;
        self.get(url, data, function(rep) {
            var icon = rep.success == true ? 1 : 2;
            layer.alert(rep.msg,
                { icon: icon }, function (index) {
                    layer.close(index);
                    self.loadData();
                });
        });
    },
    remove: function(url, data) {
        var self = this;
        layer.confirm("数据删除后将无法恢复，确定要删除该条数据吗？",
            {
                btn: ["知道了","再考虑"],
                icon: 3,
                title:'删除提示'
            },
            function () {
                self.get(url, data, function (rep) {
                    var icon = rep.success == true ? 1 : 2;
                    layer.alert(rep.msg,
                        { icon: icon }, function () {
                            layer.closeAll();
                            self.loadData();
                        });
                });
            });

    }
};
