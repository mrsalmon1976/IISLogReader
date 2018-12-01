$(document).ready(function () {
    var pvVue = new Vue({
        el: '#content-uristemaggregate-view',
        data: {
            projectId: $('#projectId').val(),
            uri: $('#uri').val(),
        },
        methods: {
            initaliseRequestGrid: function () {
                var that = this;
                $("#grid-uristem-aggregate").jsGrid({
                    width: "100%",
                    height: "600px",
                    sorting: true,
                    paging: true,
                    autoload: false,
                    noDataContent: "No requests found matching URI",
                    loadIndication: true,

                    controller: {
                        loadData: function () {
                            var d = $.Deferred();
                            $.ajax({
                                url: "/project/" + that.projectId + "/requests/detail",
                                method: 'POST',
                                dataType: "json",
                                data: { uri: that.uri }
                            }).done(function (response) {
                                d.resolve(response);
                            });

                            return d.promise();
                        }
                    },
                    loadIndicator: Utils.loadIndicator,
                    fields: [
                        { name: "uriStem", title: "URI Stem", type: "text" },
                        { name: "protocolStatus", title: "Status", type: "text", width: 50 },
                        { name: "timeTaken", title: "Time Taken (ms)", type: "number", width: 50 },
                        { name: "method", title: "Method", type: "text", width: 30 },
                        { name: "requestDateTime", title: "Date", type: "text", width: 50, cellRenderer: function (value, item) { return '<td>' + moment(value).format('YYYY-MM-DD HH:mm:ss') + '</td>'; } }
                    ]
                });
            },
            // reloads all data on the screen
            reloadAll: function () {
                $("#grid-uristem-aggregate").jsGrid("loadData");
            }
        },
        mounted: function () {
            this.initaliseRequestGrid();
            this.reloadAll();
        },
    });
});


