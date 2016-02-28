(function () {
    angular.module("Server", ["ui.grid", "ui.grid.edit", "ui.grid.rowEdit", "ui.grid.selection", "ui.bootstrap.modal", "ui.bootstrap.tooltip"])
        .service("BindingService", ["$http", DataService("api/ReceiveConnectors")])
        .service("SubnetService", ["$http", DataService("api/ServerSubnets")])

        .service("TimeTableService", ["$http", DataService("api/TimeTables")])

        .service("ServerService", [
            "$http", function($http) {
                this.get = function() {
                    return $http.get("api/Server");
                };

                this.set = function(config) {
                    return $http.post("api/Server", config);
                };
            }
        ])
        .controller("ServerController", [
            "$scope", "$uibModal", "TimeTableService", "$http", function ($scope, $uibModal, TimeTableService, $http) {
                $scope.running = null;
                $scope.status = "???";
                $scope.versions = [];
                $scope.localMailboxTotal = "???";
                $scope.externalMailboxTotal = "???";
                $scope.localGroups = [];
                $scope.externalGroups = [];
                $scope.timeTables = [];

                $http.get("api/Server/IsRunning")
                    .success(function(running) {
                        $scope.running = running;
                        $scope.status = running ? "Running" : "Stopped";
                    })
                    .error(function(data) {
                        $scope.status = "Error";
                        showError(data.Message);
                    });

                $scope.start = function ($event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    $http.get("api/Server/Start")
                        .success(function () {
                            disabledButton();
                            $scope.running = true;
                            $scope.status = "Running";
                        })
                        .error(function (data) {
                            disabledButton();
                            showError(data.Message);
                        });
                };

                $scope.stop = function ($event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    $http.get("api/Server/Stop")
                        .success(function () {
                            disabledButton();
                            $scope.running = false;
                            $scope.status = "Stopped";
                        })
                        .error(function (data) {
                            disabledButton();
                            showError(data.Message);
                        });
                };

                $http.get("api/Server/Version")
                    .success(function (versions) {
                        $scope.versions = versions;
                    })
                    .error(function (data) {
                        showError(data.Message);
                    });

                $http.get("api/LocalGroups/WithCounts")
                    .success(function (groups) {
                        $scope.localGroups = groups.Items;
                        $scope.localMailboxTotal = groups.MailboxTotal;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $http.get("api/ExternalGroups/WithCounts")
                    .success(function (groups) {
                        $scope.externalGroups = groups.Items;
                        $scope.externalMailboxTotal = groups.MailboxTotal;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                TimeTableService.all()
                    .success(function (timeTables) {
                        $scope.timeTables = timeTables;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });
            }
        ]);
})();