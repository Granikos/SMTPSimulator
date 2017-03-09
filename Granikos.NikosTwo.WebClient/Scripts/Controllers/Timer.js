(function () {
    angular.module('Timer', ['ui.bootstrap.modal', 'ui-rangeSlider', 'ui.select', "chart.js", "angularMultiSlider"])
        .service("TimeTableService", ["$http", DataService("api/TimeTables")])
        .controller('TimerController', [
            '$scope', '$http', '$uibModal', 'TimeTableService', '$rootScope', function ($scope, $http, $uibModal, TimeTableService, $rootScope) {
                $rootScope.pageTitle = 'Settings';
                $rootScope.pageSubtitle = 'Timer';
                $scope.timeTables = [];
                $scope.types = [];
                $scope.templates = [];
                $scope.attachments = [];
                $scope.localGroups = [];
                $scope.externalGroups = [];
                $scope.localMailboxTotal = "???";
                $scope.externalMailboxTotal = "???";

                $scope.days = [
                    { name: 'Monday', value: 1 },
                    { name: 'Tuesday', value: 2 },
                    { name: 'Wednesday', value: 3 },
                    { name: 'Thursday', value: 4 },
                    { name: 'Friday', value: 5 },
                    { name: 'Saturday', value: 6 },
                    { name: 'Sunday', value: 0 }
                ];

                $scope.searchLocalUsers = function (search) {
                    return $http.get("api/LocalUsers/Search/" + search)
                        .then(function (response) {
                            return response.data;
                        });
                };

                $scope.searchExternalUsers = function (search) {
                    return $http.get("api/ExternalUsers/Search/" + search)
                        .then(function (response) {
                            return response.data;
                        });
                };

                $http.get("api/LocalGroups/WithCounts")
                    .then(function (response) {
                        var groups = response.data;
                        $scope.localGroups = groups.Items;
                        // TODO: Make this cleaner
                        $scope.localGroups.unshift({
                            Id: 0,
                            Name: 'All',
                            Count: groups.MailboxTotal
                        });
                        $scope.localMailboxTotal = groups.MailboxTotal;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $http.get("api/ExternalGroups/WithCounts")
                    .then(function (response) {
                        var groups = response.data;
                        $scope.externalGroups = groups.Items;
                        // TODO: Make this cleaner
                        $scope.externalGroups.unshift({
                            Id: 0,
                            Name: 'All',
                            Count: groups.MailboxTotal
                        });
                        $scope.externalMailboxTotal = groups.MailboxTotal;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                TimeTableService.all()
                    .then(function (response) {
                        $scope.timeTables = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $http.get("api/TimeTables/Types")
                    .then(function (response) {
                        $scope.types = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $http.get("api/TimeTables/MailTemplates")
                    .then(function (response) {
                        $scope.templates = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $http.get("api/TimeTables/Attachments")
                    .then(function (response) {
                        $scope.attachments = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $scope.update = function (timeTable, $event) {
                    var button = $($event.currentTarget).find('button.update-button');
                    var disabledButton = disableClickedButton(button);
                    TimeTableService
                        .update(timeTable)
                        .then(function (response) {
                            disabledButton();
                            var index = $scope.timeTables.indexOf(timeTable);
                            if (index > -1) {
                                $scope.timeTables[index] = response.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + timeTable.Id).addClass('in');
                            }, 10);
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.delete = function (timeTable, $event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    TimeTableService
                        .delete(timeTable.Id)
                        .then(function () {
                            disabledButton();
                            var index = $scope.timeTables.indexOf(timeTable);
                            if (index > -1) {
                                $scope.timeTables.splice(index, 1);
                            }
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.startAdd = function () {
                    $http.get("api/TimeTables/Empty")
                        .then(function (response) {
                            $scope.adding = true;
                            response.data.__adding__ = true;
                            response.data.Id = 'Add';
                            $scope.timeTables.push(response.data);
                            window.setTimeout(function () {
                                $('#collapseAdd').addClass('in');
                                $('#timeTableFormAdd :input').first().focus();
                                $('#timeTableFormAdd').scope().timeTableForms.formAdd.$setDirty();
                                $scope.$apply();
                            }, 10);
                        }, function (response) {
                            showError(response.data.Message);
                        });
                };

                $scope.add = function (timeTable, $event) {
                    var button = $($event.currentTarget).find('button.add-button');
                    var disabledButton = disableClickedButton(button);

                    delete timeTable.Id;

                    TimeTableService
                        .add(timeTable)
                        .then(function (response) {
                            disabledButton();
                            var index = $scope.timeTables.indexOf(timeTable);
                            if (index > -1) {
                                $scope.timeTables[index] = response.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + response.data.Id).addClass('in');
                            }, 10);
                            $scope.adding = false;
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
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
                    $uibModal
                        .open({
                            templateUrl: 'Views/Timer/' + type + 'TypeDialog.html',
                            controller: type + 'TimerTypeController',
                            scope: scope,
                            backdrop: 'static',
                            keyboard: false,
                            size: 'lg'
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
            '$scope', '$http', '$uibModal', '$timeout', function ($scope, $http, $uibModal, $timeout) {
                var rawData = $scope.$parent.tt.Parameters.dynamicData;

                if (!$scope.$parent.tt.Parameters.dynamicTotalMails) $scope.$parent.tt.Parameters.dynamicTotalMails = 1000;
                var actChart;
                $scope.$on('create', function (event, chart) {
                    actChart = chart;
                    $timeout(function() { chart.resize(chart.render, true); }, 100);
                });
                $scope.labels = new Array(24).fill(1).map(function (x, i) { return i + ''; });
                $scope.data = [new Array(24).fill(1)];
                $scope.options = {
                    responsive: true,
                    maintainAspectRatio: false
                };
                /*
                $scope.normalizeDocument = [];
                $scope.start = 8;
                $scope.values = [];
                $scope.sliders = [
                  { value: 8,  title: 'Start' },
                  { value: 10, title: 'Peak 1 Start' },
                  { value: 11, title: 'Peak 1 End' },
                  { value: 14, title: 'Peak 2 Start' },
                  { value: 16, title: 'Peak 2 End' },
                  { value: 18, title: 'End' }
                ];
                $scope.rangeArray = [
                ]
                $scope.peak1s = 10;
                $scope.peak1e = 11;
                $scope.peak2s = 14;
                $scope.peak2e = 16;
                $scope.end = 18;
                var totalMails = 10000;
                var data = [];
                for (i = 0; i < $scope.start; i++) {
                    data.push(0);
                }
                for (i = $scope.start; i < $scope.peak1s; i++) {
                    data.push(1);
                }
                for (i = $scope.peak1s; i <= $scope.peak1e; i++) {
                    data.push(1.5);
                }
                for (i = $scope.peak1e + 1; i < $scope.peak2s; i++) {
                    data.push(1);
                }
                for (i = $scope.peak2s; i <= $scope.peak2e; i++) {
                    data.push(1.5);
                }
                for (i = $scope.peak2e + 1; i <= $scope.end; i++) {
                    data.push(1);
                }
                for (i = $scope.end + 1; i < 24; i++) {
                    data.push(0);
                }
                var l = [];
                var sum = 0;
                for (i = 0; i < 24; i++) {
                    var m2 = (i - 2) >= 0 ? data[i - 2] : 0;
                    var m1 = (i - 1) >= 0 ? data[i - 1] : 0;
                    var p1 = (i + 2) < 24 ? data[i + 1] : 0;
                    var p2 = (i + 2) < 24 ? data[i + 2] : 0;

                    var value = m2 + 2 * m1 + 5 * data[i] + 2 * p1 + p2;
                    l.push(value);
                    sum += value;
                }
                for (i = 0; i < 24; i++) {
                    l[i] = l[i] / sum;
                }
                for (i = 0; i < 24; i++) {
                    l[i] = Math.round(l[i] * totalMails);
                }
                $scope.data.push(l);
                */

                var normalizedData = [];

                $scope.normalizeData = function() {
                    var totalMails = $scope.$parent.tt.Parameters.dynamicTotalMails;
                    var sum = 0;
                    var i;
                    for (i = 0; i < 24; i++) {
                        sum += $scope.data[0][i];
                    }
                    for (i = 0; i < 24; i++) {
                        normalizedData[i] = $scope.data[0][i] / sum;
                        $scope.data[0][i] = normalizedData[i] * totalMails;
                    }
                }

                function getColFromEvent(evt) {
                    var pos = Chart.helpers.getRelativePosition(evt);
                    var col = -1;
                    for (var j = 0; j < 24; j++) {
                        var bar = actChart.datasets[0].bars[j];
                        var minX = bar.x - bar.width / 2;
                        var maxX = bar.x + bar.width / 2;

                        if (minX <= pos.x && maxX >= pos.x) {
                            col = j;
                            break;
                        }
                    }

                    return col;
                }

                function getYFromEvent(evt) {
                    var pos = Chart.helpers.getRelativePosition(evt);

                    var scalingFactor = actChart.scale.drawingArea() / (actChart.scale.min - actChart.scale.max);
                    var y = (actChart.scale.endPoint - pos.y) / scalingFactor + actChart.scale.min;

                    return y;
                }

                $scope.save = function () {
                    $scope.$parent.tt.Parameters.dynamicData = normalizedData.join(',');
                    $scope.$close();
                };

                $scope.onClick = function (points, evt) {
                    var col = getColFromEvent(evt);
                    var y = getYFromEvent(evt);

                    if (y < 0) y = 0;

                    if (col !== -1) {
                        $scope.data[0][col] = y;
                        $scope.normalizeData();
                    }
                };

                $scope.reset = function () {
                    $scope.data[0] = rawData ? rawData.split(',').map(Number) : new Array(24).fill(1);
                    $scope.normalizeData();
                };

                $scope.reset();
            }
        ])
        .controller('StaticTimerTypeController', ['$scope', function ($scope) {
            var rawData = $scope.$parent.tt.Parameters.staticData;
            $scope.days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
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