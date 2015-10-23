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

                $scope.showSettings = function (scope) {
                    var type = scope.tt.Type.charAt(0).toUpperCase() + scope.tt.Type.slice(1);
                    $modal
                        .open({
                            templateUrl: 'Views/Timer/' + type + 'TypeDialog.html',
                            controller: type + 'TimerTypeController',
                            scope: scope,
                            backdrop: 'static',
                            keyboard: false
                        });
                };
            }
        ])
        .directive('staticTimetableEditor', function () {
            return {
                link: function ($scope, element, attrs) {
                    var startDay, startInterval, down = false;
                    var endDay, endInterval;

                    function offset(day, time) {
                        return day * $scope.numIntervals + time;
                    };

                    function doToggle(startDay, endDay, startInterval, endInterval) {
                        var minDay = startDay < endDay ? startDay : endDay;
                        var maxDay = startDay > endDay ? startDay : endDay;
                        var minInterval = startInterval < endInterval ? startInterval : endInterval;
                        var maxInterval = startInterval > endInterval ? startInterval : endInterval;

                        for (var i = minDay; i <= maxDay; i++) {
                            for (var j = minInterval; j <= maxInterval; j++) {
                                var o = offset(i, j);
                                var value = $scope.data[o];
                                $scope.data[o] = value === '1' ? '0' : '1';
                            }
                        }

                        $scope.timeTableForms['form' + $scope.$parent.tt.Id].$setDirty();
                        $scope.$apply();
                    }

                    element.mousedown(function (event) {
                        var target = $(event.target);
                        if (target.filter('td').length > 0 && target.parent('.dataRow').length > 0) {
                            startDay = target.attr('day') * 1;
                            startInterval = target.attr('interval') * 1;
                            down = true;
                            target.addClass('toggeling');
                        }
                    });
                    element.parents('body, .modal').mouseup(function (event) {
                        if (down) {
                            var target = $(event.target);
                            if (target.filter('td').length > 0 && target.parent('.dataRow').length > 0) {
                                endDay = target.attr('day') * 1;
                                endInterval = target.attr('interval') * 1;
                            }
                            element.find('td.toggeling').removeClass('toggeling');
                            doToggle(startDay, endDay, startInterval, endInterval);

                            down = false;
                        }
                    });
                    element.mousemove(function (event) {
                        if (down) {
                            var target = $(event.target);
                            if (target.filter('td').length > 0 && target.parent('.dataRow').length > 0) {
                                element.find('td.toggeling').removeClass('toggeling');
                                endDay = target.attr('day') * 1;
                                endInterval = target.attr('interval') * 1;
                                var minDay = startDay < endDay ? startDay : endDay;
                                var maxDay = startDay > endDay ? startDay : endDay;
                                var minInterval = startInterval < endInterval ? startInterval : endInterval;
                                var maxInterval = startInterval > endInterval ? startInterval : endInterval;
                                for (var i = minDay; i <= maxDay; i++) {
                                    for (var j = minInterval; j <= maxInterval; j++) {
                                        element.find('td[day="' + i + '"][interval="' + j + '"]').addClass('toggeling');
                                    }
                                }
                            }
                        }
                    });
                }
            };
        })
        .controller('DynamicTimerTypeController', [
            '$scope', '$http', '$modal', function ($scope, $http, $modal) {
            }
        ])
        .controller('StaticTimerTypeController', ['$scope', function ($scope) {
            var rawData = $scope.$parent.tt.Parameters.staticData;
            $scope.days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
            $scope.numIntervals = 24;
            $scope.intervals = new Array($scope.numIntervals);

            $scope.offset = function (day, time) {
                return day * $scope.numIntervals + time;
            };

            $scope.save = function () {
                for (var j = 0; j < $scope.data.length; j++) {
                    $scope.data[j] = $scope.data[j] === '1' ? '1' : '0';
                }
                $scope.$parent.tt.Parameters.staticData = $scope.data.join(',');
                $scope.$close();
            };

            $scope.reset = function () {
                $scope.data = rawData ? rawData.split(',') : new Array(24 * 7);
            };

            $scope.empty = function () {
                for (var i = 0; i < $scope.data.length; i++) {
                    $scope.data[i] = '0';
                }

                $scope.timeTableForms['form' + $scope.$parent.tt.Id].$setDirty();
            };

            $scope.reset();
        }
        ]);
})();