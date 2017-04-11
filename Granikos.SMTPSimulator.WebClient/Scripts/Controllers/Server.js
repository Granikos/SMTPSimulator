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
            "$scope", "$uibModal", "TimeTableService", "$http", '$rootScope', function ($scope, $uibModal, TimeTableService, $http, $rootScope) {
                $rootScope.pageTitle = 'Server';
                $rootScope.pageSubtitle = '';
                $scope.running = null;
                $scope.status = "???";
                $scope.versions = [];
                $scope.localMailboxTotal = "???";
                $scope.externalMailboxTotal = "???";
                $scope.localGroups = [];
                $scope.externalGroups = [];
                $scope.timeTables = [];

                $http.get("api/Server/IsRunning")
                    .then(function(response) {
                        $scope.running = response.data;
                        $scope.status = response.data ? "Running" : "Stopped";
                    }, function(response) {
                        $scope.status = "Error";
                        showError(response.data.Message);
                    });

                $scope.start = function ($event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    $http.get("api/Server/Start")
                        .then(function () {
                            disabledButton();
                            $scope.running = true;
                            $scope.status = "Running";
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.stop = function ($event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    $http.get("api/Server/Stop")
                        .then(function () {
                            disabledButton();
                            $scope.running = false;
                            $scope.status = "Stopped";
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $http.get("api/Server/Version")
                    .then(function (response) {
                        $scope.versions = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $http.get("api/LocalGroups/WithCounts")
                    .then(function (response) {
                        $scope.localGroups = response.data.Items;
                        $scope.localMailboxTotal = response.data.MailboxTotal;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $http.get("api/ExternalGroups/WithCounts")
                    .then(function (response) {
                        $scope.externalGroups = response.data.Items;
                        $scope.externalMailboxTotal = response.data.MailboxTotal;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                TimeTableService.all()
                    .then(function (response) {
                        $scope.timeTables = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });
            }
        ]);
})();