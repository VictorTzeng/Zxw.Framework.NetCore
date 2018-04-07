ko.bindingHandlers.iCheckBox = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $el = $(element);
        var observable = valueAccessor();
        $el.iCheck({
            checkboxClass: 'icheckbox_flat-green',
            inheritClass: true
        });

        var enable = allBindingsAccessor().enable;
        if (enable != undefined) {
            if (enable()) {
                $el.iCheck('enable');
            }
            else {
                $el.iCheck('disable');
            }
            var enabledSubs = enable.subscribeChanged(function (newValue, oldValue) {
                if (newValue != oldValue) {
                    $el.iCheck('update');
                }
            });
        }

        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            if (enabledSubs != null) {
                enabledSubs.dispose();
                enabledSubs = null;
            }
            $el.iCheck('destroy');
        });

        // ifChecked handles tabs and clicks
        $el.on('ifChecked', function (e) {
            observable(true);
        });
        $el.on('ifUnchecked', function (e) {
            observable(false);
        });

        ko.bindingHandlers.iCheckBox.update(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This update handles both the reverting of values from cancelling edits, and the initial value setting.
        var $el = $(element);
        var value = ko.unwrap(valueAccessor());
        if (value == true) {
            $el.iCheck('check');
        } else if (value == false || value == null || value == "") { // Handle clearing the value on reverts.
            $el.iCheck('uncheck');
        }
    }
};
ko.subscribable.fn.subscribeChanged = function (callback) {
    var that = this;

    if (!that.previousValueSubscription) {
        that.previousValueSubscription = this.subscribe(function (_oldValue) {
            that.oldValue = _oldValue;
        }, that, 'beforeChange');
    }
    var subscription = that.subscribe(function (latestValue) {
        callback(latestValue, that.oldValue);
    }, that);

    var protoDispose = subscription.dispose;
    subscription.dispose = function () {
        if (protoDispose) {
            protoDispose.call(this);
        }
        if (that.previousValueSubscription) {
            that.previousValueSubscription.dispose();
        }
        delete that.oldValue;
    }

    return subscription;
};