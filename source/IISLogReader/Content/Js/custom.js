$('#calendar').datepicker({
		});

!function ($) {
    $(document).on("click","ul.nav li.parent > a ", function(){          
        $(this).find('em').toggleClass("fa-minus");      
    }); 
    $(".sidebar span.icon").find('em:first').addClass("fa-plus");
}

(window.jQuery);
	$(window).on('resize', function () {
  if ($(window).width() > 768) $('#sidebar-collapse').collapse('show')
})
$(window).on('resize', function () {
  if ($(window).width() <= 767) $('#sidebar-collapse').collapse('hide')
})

$(document).on('click', '.panel-heading span.clickable', function (e) {
    var $this = $(this);
    if (!$this.hasClass('panel-collapsed')) {
        $this.parents('.panel').find('.panel-body').slideUp();
        $this.addClass('panel-collapsed');
        $this.find('em').removeClass('fa-toggle-up').addClass('fa-toggle-down');
    } else {
        $this.parents('.panel').find('.panel-body').slideDown();
        $this.removeClass('panel-collapsed');
        $this.find('em').removeClass('fa-toggle-down').addClass('fa-toggle-up');
    }
});

$.fn.serializeForm = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

var Collections = function () { };

// Get a random integer between 'min' and 'max'.
Collections.displayList = function (arr) {
    var html = '<ul>';
    for (var i = 0; i < arr.length; i++) {
        html += '<li>' + arr[i] + '</li>';
    }
    html += '</ul>';
    return html;
};

var Numeric = function () { };

// Get a random integer between 'min' and 'max'.
Numeric.getRandomInt = function (min, max) {
    return Math.floor(Math.random() * (max - min + 1) + min);
};

var Utils = function () { };

// handles an ajax error by redirecting to the login screen if there was an authorisation error, 
// or displaying the error in an element specifed as a jquery object
Utils.handleAjaxError = function (xhr, jqMessagePanel) {
    if (xhr.status == 401) {
        bootbox.alert('You do not have authorisation to perform this action; you will now be redirected to the login page.', function (result) {
            window.location.href = '/login';
        });
        return;
    }
    var msg = xhr.statusText;
    try {
        var json = JSON.parse(msg);
        msg = json.message;
    }
    catch (error) {
    }
    if (msg == null || msg.length == 0) {
        msg = 'An unspecified error occurred.';
    }
    jqMessagePanel.html('<div class="alert alert-danger" role="alert">' + msg + '</div>');
};

Utils.showError = function (selector, error) {
    //debugger;
    var err = error;
    if ($.isArray(err)) {
        err = Collections.displayList(err);
    }
    $(selector).html(err);
    $(selector).removeClass('hidden');
};


$(document).ready(function () {
});

