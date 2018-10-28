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

    // set up dropzone
    Dropzone.autoDiscover = false;
    var myDropzone = new Dropzone("#dz-project-files", {
        error: function (file, response)
        {
            $(file.previewElement).addClass("dz-error").find('.dz-error-message').text(response);
        }
    });
});
