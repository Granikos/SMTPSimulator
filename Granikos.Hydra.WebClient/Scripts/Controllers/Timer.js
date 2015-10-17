(function () {
    angular.module('Timer', ['ui.bootstrap.modal', 'ui-rangeSlider', 'ui.select'])

        .service("TimeTableService", ["$http", DataService("api/TimeTables")])

        .controller('TimerController', [
            '$scope', '$http', '$modal', 'TimeTableService', function ($scope, $http, $modal, TimeTableService) {
                $scope.timeTables = [];
                $scope.types = [];
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

                $http.get("api/TimeTables/Types")
                    .success(function (types) {
                        $scope.types = types;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $scope.update = function (timeTable) {
                    TimeTableService
                        .update(timeTable)
                        .then(function (data) {
                            var index = $scope.timeTables.indexOf(timeTable);
                            if (index > -1) {
                                $scope.timeTables[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + timeTable.Id).addClass('in');
                            }, 10);
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.delete = function (timeTable) {
                    TimeTableService
                        .delete(timeTable.Id)
                        .then(function () {
                            var index = $scope.timeTables.indexOf(timeTable);
                            if (index > -1) {
                                $scope.timeTables.splice(index, 1);
                            }
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.startAdd = function () {
                    $http.get("api/TimeTables/Empty")
                        .then(function (data) {
                            $scope.adding = true;
                            data.data.__adding__ = true;
                            data.data.Id = 'Add';
                            $scope.timeTables.push(data.data);
                            window.setTimeout(function () {
                                $('#collapseAdd').addClass('in');
                                $('#timeTableFormAdd :input').first().focus();
                                $('#timeTableFormAdd').scope().timeTableForms.formAdd.$setDirty();
                                $scope.$apply();
                            }, 10);
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.add = function (timeTable) {
                    delete timeTable.Id;
                    delete timeTable.__adding__;

                    TimeTableService
                        .add(timeTable)
                        .then(function (data) {
                            var index = $scope.timeTables.indexOf(timeTable);
                            if (index > -1) {
                                $scope.timeTables[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + data.data.Id).addClass('in');
                            }, 10);
                            $scope.adding = false;
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.cancelAdd = function (timeTable) {
                    var index = $scope.timeTables.indexOf(timeTable);
                    if (index > -1) {
                        $scope.timeTables.splice(index, 1);
                    }
                    $scope.adding = false;
                };
            }
        ]);
})();