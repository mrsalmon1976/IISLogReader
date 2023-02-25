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
        $this.closest('.panel').find('.panel-body').slideUp();
        $this.addClass('panel-collapsed');
        $this.find('em').removeClass('fa-toggle-up').addClass('fa-toggle-down');
    } else {
        $this.closest('.panel').find('.panel-body').slideDown();
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

Utils.loadIndicator = function (config) {
    var container = config.container[0];
    var spinner = new Spinner();

    return {
        show: function () {
            spinner.spin(container);
        },
        hide: function () {
            spinner.stop();
        }
    };
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

var MainView = function () {

    var that = this;
    var txtPassword = null;
    var txtConfirmPassword = null;
    var msg = null;

    this.init = function () {
        //this.loadUsers();
        $('#btn-profile').on('click', function () { that.showUserProfileForm(''); });
        $('#btn-submit-user-profile').on('click', function () { that.submitChangePassword(''); });
        this.txtPassword = $('#user-profile-password');
        this.txtConfirmPassword = $('#user-profile-confirm-password');
        this.msg = $('#msg-user-profile');
    //    $('#btn-submit-user').on('click', that.submitForm);
    //    $('#dlg-user').on('shown.bs.modal', function () {
    //        $('#txt-user').focus();
    //    });
    };

    this.setProfileMessage = function (errorMsg, success) {
        if (success) {
            this.msg.removeClass('label-danger').addClass('label-success');
        }
        else {
            this.msg.removeClass('label-success').addClass('label-danger');
        }
        this.msg.text(errorMsg);
        this.msg.show();
    };

    this.showUserProfileForm = function() {
        //$("#msg-error").addClass('hidden');
        $('#dlg-user-profile').modal('show');
        this.txtPassword.val('');
        this.txtConfirmPassword.val('');
        this.msg.hide();
    };

    this.submitChangePassword = function () {

        if (this.txtPassword.val().length < 5) {
            this.setProfileMessage('Password must be at least 5 characters', false);
            return;
        }
        if (this.txtPassword.val() != this.txtConfirmPassword.val()) {
            this.setProfileMessage('Password and confirmation must match', false);
            return;
        }

        var formData = $('#form-user-profile').serializeForm();
        var request = $.ajax({
            url: "/user/changepassword",
            method: "POST",
            data: formData,
            dataType: 'json',
            traditional: true
        });

        request.done(function (response) {
            if (response.success === false) {
                that.setProfileMessage(response.messages[0], false);
            }
            else {
                that.setProfileMessage('Password changed!', true);
                that.txtPassword.val('');
                that.txtConfirmPassword.val('');
            }
        });

    };

}

$(document).ready(function () {
    var mv = new MainView();
    mv.init();
});

