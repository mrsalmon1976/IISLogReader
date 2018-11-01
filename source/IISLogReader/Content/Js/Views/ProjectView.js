$(document).ready(function () {
    var fvavue = new Vue({
        el: '#content-project-view',
        data: {
            projectId: $('#projectId').val()
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

    $("#grid-project-files").jsGrid({
        width: "100%",
        height: "400px",
        sorting: true,
        paging: true,
        autoload: true,
 
        //data: clients,

        controller: {
            loadData: function() {
                var d = $.Deferred();
                $.ajax({
                    url: "/project/" + fvavue.projectId + "/files",
                    method: 'POST',
                    dataType: "json"
                }).done(function(response) {
                    d.resolve(response);
                });
 
                return d.promise();
            }
        },
        loadIndicator: function(config) {
            var container = config.container[0];
            var spinner = new Spinner();
 
            return {
                show: function() {
                    spinner.spin(container);
                },
                hide: function() {
                    spinner.stop();
                }
            };
        },
        fields: [
            { name: "fileName", title: "File Name", type: "text", width: 150, validate: "required" },
            { name: "fileLength", title: "Size", type: "number", width: 50 },
            { name: "recordCount", title: "Records", type: "number", width: 200 }
        ]
    });

});


