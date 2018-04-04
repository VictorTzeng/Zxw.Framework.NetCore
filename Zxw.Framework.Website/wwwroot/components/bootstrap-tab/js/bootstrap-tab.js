/**
 * Bootstrap tab组件封装
 * @author billjiang  qq:475572229
 * @created 2017/7/24
 *
 */
(function ($, window, document, undefined) {
    'use strict';

    var pluginName = 'tabs';

    //入口方法
    $.fn[pluginName] = function (options) {
        var self = $(this);
        if (this == null)
            return null;
        var data = this.data(pluginName);
        if (!data) {
            data = new BaseTab(this, options);
            self.data(pluginName, data);
        }
        return data;
    };


    var BaseTab = function (element, options) {
        this.$element = $(element);
        this.options = $.extend(true, {}, this.default, options);
        this.init();
    }

    //默认配置
    BaseTab.prototype.default = {
        showIndex: 0, //默认显示页索引
        loadAll: true,//true=一次全部加在页面,false=只加在showIndex指定的页面，其他点击时加载，提高响应速度

    }

    //结构模板
    BaseTab.prototype.template = {
        ul_nav: '<ul id="myTab"  class="nav nav-tabs" style="margin-left:15px;"></ul>',
        ul_li: '<li><a href="#{0}" data-id="{0}" data-url="{2}" data-toggle="tab"><span>{1}</span></a></li>',
        ul_li_close: '<i class="fa fa-remove closeable" title="关闭"></i>'
    }

    //初始化
    BaseTab.prototype.init = function () {
        if (!this.options.data || this.options.data.length == 0) {
            console.error("请指定tab页数据");
            return;
        }
        //当前显示的显示的页面是否超出索引
        if (this.options.showIndex < 0 || this.options.showIndex > this.options.data.length - 1) {
            console.error("showIndex超出了范围");
            //指定为默认值
            this.options.showIndex = this.default.showIndex;
        }
        //清除原来的tab页
        this.$element.html("");
        this.builder(this.options.data);
        var self = this;
        this.$element.find("#myTab li a").click(function() {
            var src = $(this).attr("data-url");
            if (!$(this).parent().hasClass("active")) {
                $("#tab-content").attr("src", src);
            }
        });
        this.$element.find("#myTab li").find("i").click(function(e) {
            var prev = $(this).parent().parent().prev();
            if ($(this).parent().parent().hasClass("active")) {
                $(prev).find("a").click();
            }
            $(this).parent().parent().remove();
            //取消冒泡
            e.cancelBubble = true;
        });
    }

    //使用模板搭建页面结构
    BaseTab.prototype.builder = function (data) {
        var ul_nav = $(this.template.ul_nav);

        for (var i = 0; i < data.length; i++) {
            //nav-tab
            var ul_li = $(this.template.ul_li.format(data[i].id, data[i].text, data[i].url));
            //如果可关闭,插入关闭图标，并绑定关闭事件
            if (data[i].closeable) {
                var ul_li_close = $(this.template.ul_li_close);

                ul_li.find("a").append("&nbsp;");
                ul_li.find("a").append(ul_li_close);
            }

            ul_nav.append(ul_li);
        }

        this.$element.append(ul_nav);
        this.$element.find(".nav-tabs li").eq(0).find("a").tab("show");
        $("#tab-content").attr("src", data[0].url);
    }

    //新增一个tab页
    BaseTab.prototype.addTab=function (obj) {
        if (this.$element.find(".nav-tabs li a[data-id='" + obj.id + "']").length > 0) {
            this.$element.find(".nav-tabs li a[data-id='" + obj.id + "']").click();
            return;
        }
        var self=this;
        //nav-tab
        var ul_li = $(this.template.ul_li.format(obj.id, obj.text, obj.url));
        //如果可关闭,插入关闭图标，并绑定关闭事件
        if (obj.closeable) {
            var ul_li_close = $(this.template.ul_li_close);

            ul_li.find("a").append(ul_li_close);
            ul_li.find("a").append("&nbsp;");
        }
        
        this.$element.find(".nav-tabs:eq(0)").append(ul_li);

        this.$element.find(".nav-tabs li a[data-id='" + obj.id + "']").click(function() {
            var src = $(this).attr("data-url");
            if (!$(this).parent().hasClass("active")) {
                $("#tab-content").attr("src", src);
            }
        });


        if(obj.closeable) {
            this.$element.find(".nav-tabs li a[data-id='" + obj.id + "'] i.closeable").click(function(e) {
                var prev = $(this).parent().parent().prev();
                if ($(this).parent().parent().hasClass("active")) {
                    $(prev).find("a").click();
                }
                $(this).parent().parent().remove();
                //取消冒泡
                e.cancelBubble = true;
            });
        }

        this.$element.find(".nav-tabs li a[data-id='" + obj.id + "']").click();
    }

    //根据id获取活动也标签名
    BaseTab.prototype.find=function (tabId) {
        return this.$element.find(".nav-tabs li a[data-id='" + tabId + "']").text();
    }
    
    // 删除活动页
    BaseTab.prototype.remove=function (tabId) {
    	var self=this;
        $("#" + tabId).remove();
        this.$element.find(".nav-tabs li a[data-id='" + tabId + "']").parents("li").remove();
    }
    
    // 重新加载页面
    BaseTab.prototype.reload=function (obj) {
    	var self=this;
    	if(self.find(obj.id)!=null){
    		$("#" + obj.id).remove();
    		this.$element.find(".nav-tabs li a[data-id='" + tabId + "']").parents("li").remove();
    		self.addTab(obj);
    	}else{
    		self.addTab(obj);
    	}
    }

    //根据id设置活动tab页
    BaseTab.prototype.showTab=function (tabId) {
        var url = this.$element.find(".nav-tabs li a[data-id='" + tabId + "']").attr("data-url");
        $("#tab-content").attr("src", url);
    }

    //获取当前活动tab页的ID
    BaseTab.prototype.getCurrentTabId=function () {
        var href=this.$element.find(".nav-tabs li.active a").attr("data-id");
        return href;
    }

    //获取当前活动tab页的ID
    BaseTab.prototype.getCurrentTabUrl=function () {
        var href=this.$element.find(".nav-tabs li.active a").attr("data-url");
        return href;
    }


    String.prototype.format = function () {
        if (arguments.length == 0) return this;
        for (var s = this, i = 0; i < arguments.length; i++)
            s = s.replace(new RegExp("\\{" + i + "\\}", "g"), arguments[i]);
        return s;
    };
})(jQuery, window, document)
