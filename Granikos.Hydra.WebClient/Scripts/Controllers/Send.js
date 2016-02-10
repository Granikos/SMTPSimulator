(function () {
    var IPRegexp = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
    var DomainRegexp = /^(\*\.?)?([a-z0-9]+(-[a-z0-9]+)*\.)*[a-z0-9]+(-[a-z0-9]+)*$/;

    angular.module('Send', ['ui.bootstrap.modal', 'enumFlag', 'checklist-model', 'ui.moment-duration', 'jkuri.touchspin'])

        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])

        .controller('SendController', [
            '$scope', '$modal', '$q', '$http', 'SendConnectorService', function ($scope, $modal, $q, $http, SendConnectorService ) {
                $scope.connectors = [];
                $scope.certificates = [];
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
                    .success(function (connector) {
                        $scope.defaultId = connector.Id;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                SendConnectorService.all()
                    .success(function (connectors) {
                        $.each(connectors, function (i, c) {
                            c.RetryTimeDuration = moment.duration(c.RetryTime);
                        });
                        $scope.connectors = connectors;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $http.get("api/SendConnectors/Certificates")
                    .success(function (certificates) {
                        $scope.certificates = certificates;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $scope.makeDefault = function (connector, $event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    $http.post("api/SendConnectors/Default/" + connector.Id)
                        .success(function () {
                            disabledButton();
                            $scope.defaultId = connector.Id;
                        })
                        .error(function (data) {
                            disabledButton();
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.addDomain = function (connector) {
                    $modal
                        .open({
                            templateUrl: 'Views/ExternalUsers/SelectDomainDialog.html',
                            controller: [
                                '$scope', '$modalInstance', function ($scope, $modalInstance) {
                                    $scope.Domain = null;
                                    $scope.DomainRegexp = DomainRegexp;

                                    $scope.searchDomains = function (search) {
                                        return $http.get("api/ExternalUsers/SearchDomains/" + search)
                                            .then(function (data) {
                                                return data.data;
                                            });
                                    };

                                    $scope.submit = function () {
                                        $modalInstance.close($scope.Domain);
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

                    if (connector.TLSSettings.CertificateName === '-- None --')
                        connector.TLSSettings.CertificateName = null;

                    connector.RetryTime = connector.RetryTimeDuration.hours() + ':' +
                        connector.RetryTimeDuration.minutes() + ':' +
                        connector.RetryTimeDuration.seconds();

                    SendConnectorService
                        .update(connector)
                        .then(function (data) {
                            disabledButton();
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                data.data.RetryTimeDuration = moment.duration(data.data.RetryTime);
                                $scope.connectors[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + connector.Id).addClass('in');
                            }, 10);
                        }, function (data) {
                            disabledButton();
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.delete = function (connector, $event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    SendConnectorService
                        .delete(connector.Id)
                        .then(function (success) {
                            disabledButton();
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                $scope.connectors.splice(index, 1);
                            }
                        }, function (data) {
                            disabledButton();
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.startAdd = function () {
                    $http.get("api/SendConnectors/Empty")
                        .then(function (data) {
                            $scope.adding = true;
                            data.data.__adding__ = true;
                            data.data.Id = 'Add';
                            data.data.RetryTimeDuration = moment.duration(data.data.RetryTime);
                            $scope.connectors.push(data.data);
                            window.setTimeout(function () {
                                $('#collapseAdd').addClass('in');
                                $('#connectorFormAdd :input').first().focus();
                            }, 10);
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.add = function (connector, $event) {
                    var button = $($event.currentTarget).find('button.add-button');
                    var disabledButton = disableClickedButton(button);

                    if (connector.TLSSettings.CertificateName === '-- None --')
                        connector.TLSSettings.CertificateName = null;

                    delete connector.Id;
                    connector.RetryTime = connector.RetryTimeDuration.hours() + ':' +
                        connector.RetryTimeDuration.minutes() + ':' +
                        connector.RetryTimeDuration.seconds();

                    SendConnectorService
                        .add(connector)
                        .then(function (data) {
                            disabledButton();
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                data.data.RetryTimeDuration = moment.duration(data.data.RetryTime);
                                $scope.connectors[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + data.data.Id).addClass('in');
                            }, 10);
                            $scope.adding = false;
                        }, function (data) {
                            disabledButton();
                            showError(data.Message || data.data.Message);
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