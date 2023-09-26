Dropzone.autoDiscover = false;

$(document).ready(function () {
    var pvVue = new Vue({
        el: '#content-project-view',
        data: {
            projectId: $('#projectId').val(),
            isAvgLoadTimesLoaded: false,
            activeTab: null,
            reloadSeconds: 5,
            unprocessedCount: $('#unprocessedCount').val(),
            countdownTimer: null,
            aggregateRegEx: '',
            aggregateTarget: '',
            aggregateTest: '',
            aggregateIsIgnored: false,
            aggregateTestTextDefault: 'No regular expression or test URI captured',
            aggregateTestText: this.aggregateTestTextDefault,
            isProjectEditor: $('#isProjectEditor').val(),
            overviewTotalRequestCount: 0,
            overviewSuccessRequestCount: 0,
            overviewSuccessRequestPercentage: 0,
            overviewRedirectionRequestCount: 0,
            overviewRedirectionRequestPercentage: 0,
            overviewClientErrorRequestCount: 0,
            overviewClientErrorRequestPercentage: 0,
            overviewServerErrorRequestCount: 0,
            overviewServerErrorRequestPercentage: 0,
            logFileCount: 0,
            isOverviewLoading: true
        },
        methods: {
            deleteProject() {
                var pid = this.projectId;
                $.ajax({
                    url: "/project/delete/" + pid,
                    method: 'POST',
                    dataType: "json"
                }).done(function (response) {
                    window.location.href = '/';
                })
                    .fail(function (jqXHR, textStatus) {
                        alert("Request failed: " + textStatus);
                    });
            },
            initaliseAvgLoadTimesGrid: function (projectId) {
                var that = this;
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
                    loadIndicator: Utils.loadIndicator,
                    fields: [
                        {
                            name: "uriStemAggregate", title: "URI Stem",
                            itemTemplate: function (value) {
                                return '<a href="/project/' + that.projectId + '/requests?uri=' + encodeURIComponent(value) + '">' + value + '</a>';
                            }
                        },
                        { name: "requestCount", title: "Request Count", type: "number", width: 50 },
                        { name: "avgTimeTakenMilliseconds", title: "Avg Time Taken (ms)", type: "number", width: 50 }
                    ]
                });
            },
            initialiseDropzone: function () {
                var that = this;
                Dropzone.options.dropzoneFileUpload = {
                    error: function (file, response) {
                        $(file.previewElement).addClass("dz-error").find('.dz-error-message').text(response);
                    },
                    queuecomplete: function () {
                        that.reloadAll();
                    }
                };
                $('#dropzoneFileUpload').dropzone();

            },
            initaliseProjectFileGrid: function (projectId) {
                var that = this;
                $("#grid-project-files").jsGrid({
                    width: "100%",
                    height: "440px",
                    sorting: true,
                    paging: true,
                    autoload: false,
                    noDataContent: "No log files have been added to this project",
                    deleteConfirm: "Are you sure you want to delete this log file and requests from the project?",

                    controller: {
                        loadData: function () {
                            var d = $.Deferred();
                            $.ajax({
                                url: "/project/" + projectId + "/files",
                                method: 'POST',
                                dataType: "json"
                            }).done(function (response) {
                                var upc = 0;
                                for (var i = 0; i < response.length; i++) {
                                    if (!response[i].isProcessed) {
                                        upc++;
                                    }
                                }
                                that.unprocessedCount = upc;
                                that.initReloadCountdown();
                                d.resolve(response);
                            });

                            return d.promise();
                        }
                    },
                    loadIndicator: Utils.loadIndicator,
                    fields: [
                        { name: "id", title: "Id", visible: false, type: "number" },
                        { name: "fileName", title: "File Name", type: "text", width: 150, validate: "required" },
                        {
                            name: "fileLength", title: "Size", type: "number", width: 50, cellRenderer: function (value, item) {
                                return '<td>' + item.fileSize + '</td > ';
                            }
                        },
                        { name: "statusName", title: "Status", type: "text", width: 150 },
                        { name: "recordCount", title: "Records", type: "number", width: 50 },
                        { type: "control", editButton: false, clearFilterButton: false, modeSwitchButton: false, width: 25, visible: this.isProjectEditor }
                    ],
                    onItemDeleting: function (args) {
                        var id = args.item.id;
                        $.ajax({
                            url: "/logfile/delete/" + id,
                            method: "POST",
                            dataType: 'json',
                            traditional: true
                        }).done(function (response) {
                            that.reloadAll();
                        }).fail(function (jqXHR, textStatus) {
                            alert("Failed to delete file: " + textStatus);
                        });
                    }

                });
            },
            initaliseSettingsAggregatesGrid: function (projectId) {
                var that = this;
                $("#grid-settings-aggregates").jsGrid({
                    width: "100%",
                    sorting: true,
                    paging: false,
                    autoload: false,
                    noDataContent: "No aggregates have been added to this project",
                    deleteConfirm: "Are you sure you want to delete this aggregate?",
                    controller: {
                        loadData: function () {
                            var d = $.Deferred();
                            $.ajax({
                                url: "/project/" + projectId + "/aggregates",
                                method: 'POST',
                                dataType: "json"
                            }).done(function (response) {
                                d.resolve(response);
                            });
                            return d.promise();
                        }
                    },
                    loadIndicator: Utils.loadIndicator,
                    fields: [
                        { name: "id", title: "Id", visible: false, type: "number" },
                        { name: "regularExpression", title: "Regular Expression", type: "text", width: 150 },
                        {
                            name: "aggregateTarget", title: "Aggregate URI", type: "text", cellRenderer: function (value, item) {
                                if (item.isIgnored) {
                                    return '<td class="cell-ignored">IGNORED</td>';
                                }
                                return '<td>' + item.aggregateTarget + '</td > ';
                            }
                        },
                        { type: "control", editButton: false, clearFilterButton: false, modeSwitchButton: false, width: 25, visible: this.isProjectEditor }
                    ],
                    onItemDeleting: function (args) {
                        var id = args.item.id;
                        $.ajax({
                            url: "/project/requestaggregate/delete",
                            method: "POST",
                            data: {
                                id: id
                            },
                            dataType: 'json',
                            traditional: true
                        });
                    }
                });
            },
            initReloadCountdown: function () {
                if (this.unprocessedCount > 0) {
                    this.reloadSeconds = 15;
                    this.countdownTimer = setInterval(() => {
                        this.reloadSeconds--;
                        if (this.reloadSeconds <= 0) {
                            clearInterval(this.countdownTimer);
                            this.countdownTimer = null;
                            this.reloadAll();
                        }
                    }, 1000);
                }
            },
            initTabRefreshHandlers: function () {
                var that = this;
                $(document).on('shown.bs.tab', 'a[data-toggle="tab"]', function (e) {
                    that.activeTab = e.target;
                    if (that.activeTab.hash === '#tab-loadtimes') {
                        $("#grid-project-load-times").jsGrid("refresh");
                    }
                    else if (that.activeTab.hash === '#tab-settings') {
                        $("#grid-settings-aggregates").jsGrid("refresh");
                    }
                });
            },
            loadOverview: function () {
                var that = this;
                var pid = this.projectId;
                that.isOverviewLoading = true;
                $.ajax({
                    url: '/project/' + pid + '/overview',
                    method: 'GET',
                    dataType: "json"
                }).done(function (response) {
                    that.logFileCount = response.logFileCount;
                    that.overviewTotalRequestCount = response.totalRequestCount;
                    if (response.totalRequestCount > 0) {
                        that.overviewSuccessRequestCount = response.successRequestCount;
                        that.overviewSuccessRequestPercentage = parseInt(response.successRequestCount / response.totalRequestCount * 100);
                        that.overviewRedirectionRequestCount = response.redirectionRequestCount;
                        that.overviewRedirectionRequestPercentage = parseInt(response.redirectionRequestCount / response.totalRequestCount * 100);
                        that.overviewClientErrorRequestCount = response.clientErrorRequestCount;
                        that.overviewClientErrorRequestPercentage = parseInt(response.clientErrorRequestCount / response.totalRequestCount * 100);
                        that.overviewServerErrorRequestCount = response.serverErrorRequestCount;
                        that.overviewServerErrorRequestPercentage = parseInt(response.serverErrorRequestCount / response.totalRequestCount * 100);
                    }
                    that.isOverviewLoading = false;
                })
                    .fail(function (jqXHR, textStatus) {
                        alert("Request failed: " + textStatus);
                    });
            },
            onAddProjectFilesClick: function () {
                $('#dlg-project-files').modal('show');
            },
            onAggregateInputKeyUp: function () {
                var eleText = $('#project-aggregate-test');
                var eleIcon = $('#project-aggregate-icon')
                eleText.removeClass('bg-default').removeClass('bg-success').removeClass('bg-danger');
                eleIcon.removeClass('fa-thumbs-up').removeClass('fa-thumbs-down').removeClass('fa-warning');
                if (this.aggregateTest.length === 0) {
                    eleText.addClass('bg-default');
                    eleIcon.addClass('fa-warning');
                    this.aggregateTestText = this.aggregateTestTextDefault;
                    return;
                }

                var re = new RegExp(this.aggregateRegEx);
                if (re.test(this.aggregateTest)) {
                    if (this.aggregateIsIgnored) {
                        this.aggregateTestText = 'Success!  "' + this.aggregateTest + '" will be ignored';
                    }
                    else {
                        this.aggregateTestText = 'Success!  "' + this.aggregateTest + '" will be transformed into "' + this.aggregateTarget + '"';
                    }
                    eleText.addClass('bg-success');
                    eleIcon.addClass('fa-thumbs-up');
                }
                else {
                    this.aggregateTestText = 'Your test doesn\'t match your regular expression';
                    eleIcon.addClass('fa-thumbs-down');
                    eleText.addClass('bg-danger');
                }
            },
            onDeleteProjectClick: function () {
                var that = this;
                bootbox.confirm({
                    message: "Are you sure you want to delete this project?<br /><br />All files and related data will be deleted.",
                    buttons: {
                        cancel: {
                            label: 'Cancel',
                            className: 'btn-success'
                        },
                        confirm: {
                            label: 'Yes',
                            className: 'btn-danger'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            that.deleteProject();
                        }
                    }
                });
            },
            onNewProjectAggregateClick: function () {
                $('#dlg-project-aggregate').modal('show');
            },
            onSaveProjectAggregateClick: function () {
                var that = this;
                $("#project-aggregate-msg-error").addClass('hidden');
                var request = $.ajax({
                    url: "/project/requestaggregate/save",
                    method: "POST",
                    data: {
                        projectId: this.projectId,
                        regularExpression: that.aggregateRegEx,
                        aggregateTarget: (that.aggregateIsIgnored ? '' : that.aggregateTarget),
                        isIgnored: that.aggregateIsIgnored
                    },
                    dataType: 'json',
                    traditional: true
                });

                request.done(function (response) {
                    if (response.success) {
                        that.reloadAll();
                        $('#dlg-project-aggregate').modal('hide');
                        that.aggregateRegEx = '';
                        that.aggregateTarget = '';
                        that.aggregateTest = '';
                        that.aggregateIsIgnored = false;
                    }
                    else {
                        Utils.showError('#project-aggregate-msg-error', response.messages);
                    }
                });

                request.fail(function (xhr, textStatus) {
                    try {
                        Utils.showError('#project-aggregate-msg-error', xhr.responseJSON.message);
                    }
                    catch (err) {
                        Utils.showError('#project-aggregate-msg-error', 'A fatal error occurred: ' + (err === null ? 'Unknown' : err.message));
                    }
                });
            },
            // reloads all data on the screen
            reloadAll: function () {
                if (this.countdownTimer != null) {
                    clearInterval(this.countdownTimer);
                    this.countdownTimer = null;
                }
                this.loadOverview();
                $("#grid-project-files").jsGrid("loadData");
                $("#grid-project-load-times").jsGrid("loadData");
                $("#grid-settings-aggregates").jsGrid("loadData");
            }
        },
        computed: {
            clientErrorWidthCalculated: function() {
                return {
                    width: this.overviewClientErrorRequestPercentage + '%'
                }
            },
            redirectionWidthCalculated: function () {
                return {
                    width: this.overviewRedirectionRequestPercentage + '%'
                }
            },
            serverErrorWidthCalculated: function () {
                return {
                    width: this.overviewServerErrorRequestPercentage + '%'
                }
            },
            successWidthCalculated: function () {
                return {
                    width: this.overviewSuccessRequestPercentage + '%'
                }
            },
        },
        mounted: function () {
            this.initialiseDropzone();
            this.initaliseProjectFileGrid(this.projectId);
            this.initaliseAvgLoadTimesGrid(this.projectId);
            this.initaliseSettingsAggregatesGrid(this.projectId);
            this.initTabRefreshHandlers();
            this.reloadAll();
        },
    });
});


