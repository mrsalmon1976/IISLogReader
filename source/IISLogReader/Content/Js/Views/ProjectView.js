$(document).ready(function () {
    var fvavue = new Vue({
        el: '#content-project-view',
        data: {
        },
        // define methods under the `methods` object
        methods: {
            onAddProjectFilesClick: function () {
                $('#dlg-project-files').modal('show');
            },
        }
    });

    $("#dz-project-files").dropzone({
        error: function (file, response)
        {
            debugger;
            $(file.previewElement).addClass("dz-error").find('.dz-error-message').text(response);
        }
    });
});
