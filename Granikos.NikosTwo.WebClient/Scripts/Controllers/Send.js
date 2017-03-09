(function () {
    var IPRegexp = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
    var DomainRegexp = /^(\*\.?)?([a-z0-9]+(-[a-z0-9]+)*\.)*[a-z0-9]+(-[a-z0-9]+)*$/;

    angular.module('Send', ['ui.bootstrap.modal', 'enumFlag', 'checklist-model', 'ui.moment-duration', 'jkuri.touchspin'])

        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])

        .controller('SendController', [
            '$scope', '$uibModal', '$q', '$http', 'SendConnectorService', '$rootScope', function ($scope, $uibModal, $q, $http, SendConnectorService, $rootScope) {
                $rootScope.pageTitle = 'Connectors';
                $rootScope.pageSubtitle = 'Send';
                $scope.connectors = [];
                $scope.adding = false;
                $scope.IPRegexp = IPRegexp;
                $scope.defaultId = 0;

                $scope.sslProtocols = {
                    0: 'None',
                    12: 'SSL 2',
                    48: 'SSL 3',
                    192: 'TTL',
                    240: 'Default',
                    768: 'TLS 1.1',
                    3072: 'TLS 1.2'
                }

                $scope.timespanGreaterThanZero = function (value) {
                    return value && value.asMilliseconds() > 0;
                }

                $scope.modes = ['Disabled', 'Enabled', 'Required', 'Full Tunnel'];

                $scope.policies = ['Require Encryption', 'Allow No Encryption', 'No Encryption'];

                $scope.authLevels = ['Encryption Only', 'Certificate Validation', 'Domain Validation'];

                $http.get("api/SendConnectors/Default")
                    .then(function (response) {
                        $scope.defaultId = response.data.Id;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                SendConnectorService.all()
                    .then(function (response) {
                        $.each(response.data, function (i, c) {
                            c.RetryTimeDuration = moment.duration(c.RetryTime);
                        });
                        $scope.connectors = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $scope.makeDefault = function (connector, $event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    $http.post("api/SendConnectors/Default/" + connector.Id)
                        .then(function () {
                            disabledButton();
                            $scope.defaultId = connector.Id;
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.addDomain = function (connector) {
                    $uibModal
                        .open({
                            templateUrl: 'Views/ExternalUsers/SelectDomainDialog.html',
                            controller: [
                                '$scope', '$uibModalInstance', function ($scope, $uibModalInstance) {
                                    $scope.Domain = null;
                                    $scope.DomainRegexp = DomainRegexp;

                                    $scope.searchDomains = function (search) {
                                        return $http.get("api/ExternalUsers/SearchDomains/" + search)
                                            .then(function (response) {
                                                return response.data;
                                            });
                                    };

                                    $scope.submit = function () {
                                        $uibModalInstance.close($scope.Domain);
                                    };
                                }
                            ]
                        })
                        .result.then(function (domain) {
                            connector.Domains.push(domain);
                        });
                };

                $scope.removeDomain = function (connector, domain) {
                    for (var i = connector.Domains.length - 1; i >= 0; i--) {
                        if (connector.Domains[i] === domain) {
                            connector.Domains.splice(i, 1);
                        }
                    }
                };

                $scope.update = function (connector, $event) {
                    var button = $($event.currentTarget).find('button.update-button');
                    var disabledButton = disableClickedButton(button);

                    connector.RetryTime = connector.RetryTimeDuration.hours() + ':' +
                        connector.RetryTimeDuration.minutes() + ':' +
                        connector.RetryTimeDuration.seconds();

                    SendConnectorService
                        .update(connector)
                        .then(function (response) {
                            disabledButton();
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                response.data.RetryTimeDuration = moment.duration(response.data.RetryTime);
                                $scope.connectors[index] = response.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + connector.Id).addClass('in');
                            }, 10);
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.delete = function (connector, $event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    SendConnectorService
                        .delete(connector.Id)
                        .then(function () {
                            disabledButton();
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                $scope.connectors.splice(index, 1);
                            }
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.startAdd = function () {
                    $http.get("api/SendConnectors/Empty")
                        .then(function (response) {
                            $scope.adding = true;
                            response.data.__adding__ = true;
                            response.data.Id = 'Add';
                            response.data.RetryTimeDuration = moment.duration(response.data.RetryTime);
                            $scope.connectors.push(response.data);
                            window.setTimeout(function () {
                                $('#collapseAdd').addClass('in');
                                $('#connectorFormAdd :input').first().focus();
                            }, 10);
                        }, function (response) {
                            showError(response.data.Message);
                        });
                };

                $scope.add = function (connector, $event) {
                    var button = $($event.currentTarget).find('button.add-button');
                    var disabledButton = disableClickedButton(button);

                    delete connector.Id;
                    connector.RetryTime = connector.RetryTimeDuration.hours() + ':' +
                        connector.RetryTimeDuration.minutes() + ':' +
                        connector.RetryTimeDuration.seconds();

                    SendConnectorService
                        .add(connector)
                        .then(function (response) {
                            disabledButton();
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                response.data.RetryTimeDuration = moment.duration(response.data.RetryTime);
                                $scope.connectors[index] = response.data;
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

                $scope.cancelAdd = function (connector) {
                    var index = $scope.connectors.indexOf(connector);
                    if (index > -1) {
                        $scope.connectors.splice(index, 1);
                    }
                    $scope.adding = false;
                };
            }
        ]);
})();