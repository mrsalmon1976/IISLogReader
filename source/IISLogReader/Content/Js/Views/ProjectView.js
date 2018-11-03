$(document).ready(function () {
    var pvVue = new Vue({
        el: '#content-project-view',
        data: {
            projectId: $('#projectId').val(),
            isAvgLoadTimesLoaded: false,
            activeTab: null,
        },
        methods: {
            initialiseDropZone: function () {
                // set up dropzone
                Dropzone.autoDiscover = false;
                var myDropzone = new Dropzone("#dz-project-files", {
                    error: function (file, response) {
                        $(file.previewElement).addClass("dz-error").find('.dz-error-message').text(response);
                    }
                });
            },
            initaliseAvgLoadTimesGrid: function (projectId) {
                $("#grid-project-load-times").jsGrid({
                    width: "100%",
                    height: "440px",
                    sorting: true,
                    paging: true,
                    autoload: false,

                    controller: {
                        loadData: function () {
                            var d = $.Deferred();
                            $.ajax({
                                url: "/project/" + projectId + "/avgloadtimes",
                                method: 'POST',
                                dataType: "json"
                            }).done(function (response) {
                                d.resolve(response);
                            });

                            return d.promise();
                        }
                    },
                    loadIndicator: function (config) {
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
                    },
                    fields: [
                        { name: "uriStem", title: "URI Stem", type: "text" },
                        { name: "requestCount", title: "Request Count", type: "number", width: 50 },
                        { name: "avgTimeTakenMilliseconds", title: "Avg Time Taken (ms)", type: "number", width: 50 }
                    ]
                });
            },
            initaliseProjectFileGrid: function (projectId) {
                $("#grid-project-files").jsGrid({
                    width: "100%",
                    height: "440px",
                    sorting: true,
                    paging: true,
                    autoload: true,

                    controller: {
                        loadData: function () {
                            var d = $.Deferred();
                            $.ajax({
                                url: "/project/" + projectId + "/files",
                                method: 'POST',
                                dataType: "json"
                            }).done(function (response) {
                                d.resolve(response);
                            });

                            return d.promise();
                        }
                    },
                    loadIndicator: function (config) {
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
                    },
                    fields: [
                        { name: "fileName", title: "File Name", type: "text", width: 150, validate: "required" },
                        { name: "fileLength", title: "Size", type: "number", width: 50 },
                        { name: "recordCount", title: "Records", type: "number", width: 200 }
                    ]
                });
            },
            onAddProjectFilesClick: function () {
                $('#dlg-project-files').modal('show');
            },
            onLoadTimesTabShown: function () {
                if (!this.isAvgLoadTimesLoaded) {
                    $("#grid-project-load-times").jsGrid("loadData");
                    this.isAvgLoadTimesLoaded = true;
                }
                $("#grid-project-load-times").jsGrid("refresh");
            }
        },
        mounted: function () {
            var that = this;
            this.initaliseProjectFileGrid(this.projectId);
            this.initaliseAvgLoadTimesGrid(this.projectId);
            $(document).on('shown.bs.tab', 'a[data-toggle="tab"]', function (e) {
                that.activeTab = e.target;
                if (that.activeTab.hash == '#tab-loadtimes') {
                    that.onLoadTimesTabShown();
                }
            });
        },
    });



});


