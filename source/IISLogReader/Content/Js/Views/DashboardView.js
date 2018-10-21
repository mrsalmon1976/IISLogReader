$(document).ready(function () {
    var fvavue = new Vue({
        el: '#content-dashboard',
        data: {
            projectName: ''
        },
        // define methods under the `methods` object
        methods: {
            onNewProjectClick: function () {
                $('#dlg-project').modal('show');
            },
            onSaveProjectClick: function () {
                var that = this;
                $("#project-msg-error").addClass('hidden');
                var request = $.ajax({
                    url: "/project/save",
                    method: "POST",
                    data: {
                        name: this.projectName
                    },
                    dataType: 'json',
                    traditional: true
                });

                request.done(function (response) {
                    //debugger;
                    if (response.success) {
                        $('#dlg-project').modal('hide');
                        window.location.reload();
                    }
                    else {
                        that.showProjectValidationError(response.messages);
                    }
                });

                request.fail(function (xhr, textStatus) {
                    try {
                        that.showProjectValidationError(xhr.responseJSON.message);
                    }
                    catch (err) {
                        that.showProjectValidationError('A fatal error occurred');
                    }
                });
            },
            showProjectValidationError: function (error) {
                var err = error;
                if ($.isArray(err)) {
                    err = Collections.displayList(err);
                }
                $("#project-msg-error").html(err);
                $("#project-msg-error").removeClass('hidden');
            }
        }
    });
});