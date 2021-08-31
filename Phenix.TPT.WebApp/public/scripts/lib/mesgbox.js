(function($) {

    $.alerts = {
            alert: function(title, message, callback) {
                if (title == null) title = 'Alert';
                $.alerts._show(title, message, null, 'alert', function(result) {
                    if (callback) callback(result);
                });
            },

            confirm: function(title, message, callback) {
                if (title == null) title = 'Confirm';
                $.alerts._show(title, message, null, 'confirm', function(result) {
                    if (callback) callback(result);
                });
            },
            inversconfirm: function(title, message, callback) {
                if (title == null) title = 'Confirm';
                $.alerts._show(title, message, null, 'inversconfirm', function(result) {
                    if (callback) callback(result);
                });
            },


            _show: function(title, msg, value, type, callback) {

                var _html = "";

                _html += '<div id="mb_box"></div><div id="mb_con"><span id="mb_tit">' + title + '</span>';
                _html += '<div id="mb_msg">' + msg + '</div><div id="mb_btnbox">';
                if (type == "alert") {
                    _html += '<input id="mb_btn_ok" type="button" value="确定" />';
                }
                if (type == "confirm") {
                    _html += '<input id="mb_btn_ok" type="button" value="确定" />';
                    _html += '<input id="mb_btn_no" type="button" value="取消" />';
                }
                if (type == "inversconfirm") {
                    _html += '<input id="mb_btn_no" type="button" value="取消" />';
                    _html += '<input id="mb_btn_ok" type="button" value="确定" />';
                }
                _html += '</div></div>';

                //必须先将_html添加到body，再设置Css样式
                $("body").append(_html);
                GenerateCss();

                switch (type) {
                    case 'alert':

                        $("#mb_btn_ok").click(function() {
                            $.alerts._hide();
                            callback(true);
                        });
                        $("#mb_btn_ok").focus().keypress(function(e) {
                            if (e.keyCode == 13 || e.keyCode == 27) $("#mb_btn_ok").trigger('click');
                        });
                        break;
                    case 'confirm':

                        $("#mb_btn_ok").click(function() {
                            $.alerts._hide();
                            if (callback) callback(true);
                        });
                        $("#mb_btn_no").click(function() {
                            $.alerts._hide();
                            if (callback) callback(false);
                        });
                        $("#mb_btn_no").focus();
                        $("#mb_btn_ok, #mb_btn_no").keypress(function(e) {
                            if (e.keyCode == 13) $("#mb_btn_ok").trigger('click');
                            if (e.keyCode == 27) $("#mb_btn_no").trigger('click');
                        });
                        break;
                    case 'inversconfirm':

                        $("#mb_btn_ok").click(function() {
                            $.alerts._hide();
                            if (callback) callback(true);
                        });
                        $("#mb_btn_no").click(function() {
                            $.alerts._hide();
                            if (callback) callback(false);
                        });
                        $("#mb_btn_no").focus();
                        $("#mb_btn_ok, #mb_btn_no").keypress(function(e) {
                            if (e.keyCode == 13) $("#mb_btn_ok").trigger('click');
                            if (e.keyCode == 27) $("#mb_btn_no").trigger('click');
                        });
                        break;

                }
            },
            _hide: function() {
                $("#mb_box,#mb_con").remove();
            }
        }
        // Shortuct functions
    zdalert = function(title, message, callback) {
        $.alerts.alert(title, message, callback);
    }

    zdconfirm = function(title, message, callback) {
        $.alerts.confirm(title, message, callback);
    };
    zdinversconfirm = function(title, message, callback) {
        $.alerts.inversconfirm(title, message, callback);
    };




    //生成Css
    var GenerateCss = function() {

        $("#mb_box").css({
            width: '100%',
            height: '100%',
            zIndex: '99999',
            position: 'fixed',
            filter: 'Alpha(opacity=60)',
            backgroundColor: 'black',
            top: '0',
            left: '0',
            opacity: '0.6'
        });

        $("#mb_con").css({
            zIndex: '999999',
            width: '30%',
            position: 'fixed',
            backgroundColor: 'White',
            borderRadius: '15px'
        });

        $("#mb_tit").css({
            display: 'block',
            fontSize: '14px',
            color: '#444',
            padding: '10px 15px',
            backgroundColor: '#DDD',
            borderRadius: '15px 15px 0 0',
            borderBottom: '3px solid #009BFE',
            fontWeight: 'bold'
        });

        $("#mb_msg").css({
            padding: '20px',
            lineHeight: '20px',
            borderBottom: '1px dashed #DDD',
            fontSize: '13px'
        });

        $("#mb_ico").css({
            display: 'block',
            position: 'absolute',
            right: '10px',
            top: '9px',
            border: '1px solid Gray',
            width: '18px',
            height: '18px',
            textAlign: 'center',
            lineHeight: '16px',
            cursor: 'pointer',
            borderRadius: '12px',
            fontFamily: '微软雅黑'
        });

        $("#mb_btnbox").css({ margin: '15px 0 10px 0', textAlign: 'center' });
        $("#mb_btn_ok,#mb_btn_no").css({ width: '85px', height: '30px', color: 'white', border: 'none' });
        $("#mb_btn_ok").css({ backgroundColor: '#168bbb', marginRight: '10px', marginLeft: '10px' });
        $("#mb_btn_no").css({ backgroundColor: 'gray', marginRight: '10px', marginLeft: '10px' });


        //右上角关闭按钮hover样式
        $("#mb_ico").hover(function() {
            $(this).css({ backgroundColor: 'Red', color: 'White' });
        }, function() {
            $(this).css({ backgroundColor: '#DDD', color: 'black' });
        });

        var _widht = document.documentElement.clientWidth; //屏幕宽
        var _height = document.documentElement.clientHeight; //屏幕高

        var boxWidth = $("#mb_con").width();
        var boxHeight = $("#mb_con").height();

        //让提示框居中
        $("#mb_con").css({ top: (_height - boxHeight) / 2 + "px", left: (_widht - boxWidth) / 2 + "px" });
    }


})(jQuery);