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
    }
};

ko.applyBindings(viewmodel);