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
                        window.location.href = '/project/' + response.projectId;
                    }
                    else {
                        Utils.showError('#project-msg-error', response.messages);
                    }
                });

                request.fail(function (xhr, textStatus) {
                    try {
                        Utils.showError('#project-msg-error', xhr.responseJSON.message);
                    }
                    catch (err) {
                        Utils.showError('#project-msg-error', 'A fatal error occurred: ' + (err === null ? 'Unknown' : err.message));
                    }
                });
            }
        },
        mounted: function () {
            $('#dlg-project').on('shown.bs.modal', function (e) {
                // do something...
                //alert('shown');
                $('#projectName').focus();
            });
        }
    });
});