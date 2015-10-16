(function () {
    angular.module('Timer', ['ui.bootstrap.modal', 'ui-rangeSlider', 'ui.select'])

        .service("TimeTableService", ["$http", DataService("api/TimeTables")])

        .controller('TimerController', [
            '$scope', '$http', '$modal', 'TimeTableService', function ($scope, $http, $modal, TimeTableService) {
                $scope.timeTables = [];
                $scope.localGroups = [];
                $scope.externalGroups = [];
                $scope.localMailboxTotal = "???";
                $scope.externalMailboxTotal = "???";

                $scope.searchLocalUsers = function (search) {
                    return $http.get("api/LocalUsers/Search/" + search)
                        .then(function (data) {
                            return data.data;
                        });
                };

                $scope.searchExternalUsers = function (search) {
                    return $http.get("api/ExternalUsers/Search/" + search)
                        .then(function (data) {
                            return data.data;
                        });
                };

                $http.get("api/LocalGroups/WithCounts")
                    .success(function (groups) {
                        $scope.localGroups = groups.Items;
                        // TODO: Make this cleaner
                        $scope.localGroups.unshift({
                            Id: null,
                            Name: 'All',
                            Count: groups.MailboxTotal
                        });
                        $scope.localMailboxTotal = groups.MailboxTotal;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $http.get("api/ExternalGroups/WithCounts")
                    .success(function (groups) {
                        $scope.externalGroups = groups.Items;
                        // TODO: Make this cleaner
                        $scope.externalGroups.unshift({
                            Id: null,
                            Name: 'All',
                            Count: groups.MailboxTotal
                        });
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