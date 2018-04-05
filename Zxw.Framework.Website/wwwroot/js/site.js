// Write your JavaScript code.
var viewmodel = {
    menus:ko.observableArray([]),
    loadMenus: function(url) {
        this.get(url, {}, function(data) {
            if (data.success == true) {
                viewmodel.menus(data.rows);
            } else {
                layer.alert(data.msg);
            }
        });
    },
    get: function(url, data, callback) {
        $.get(url, data, callback, "JSON");
    },
    post: function(url, data, callback) {
        $.post(url, data, callback, "JSON");
    },
    getMenuIconNoSpan: function(icon, name) {
        return '<i class="' + icon + '"></i> ' + name;
    },
    getMenuIcon: function(icon, name) {
        return '<i class="' + icon + '"></i> <span>' + name + '</span>';
    },
    getMenuIconExpand: function(icon, name) {
        return '<i class="' +
            icon +
            '"></i> <span>' +
            name +
            '</span><span class="pull-right-container"><i class="fa fa-angle-left pull-right"></i></span>';
    },
    menuClick: function(id, text, url) {
        //$("ul.sidebar-menu").find("li.active").removeClass("active");
        //$("#menu_" + id).addClass("active");
        $("#tabContainer").data("tabs")
            .addTab({ id: id, text: text, closeable: true, url: url });
    }
};

ko.applyBindings(viewmodel);