(function () {
    var IPRegexp = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;

    angular.module('Send', ['ui.bootstrap.modal', 'enumFlag', 'checklist-model'])

        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])

        .controller('SendController', [
            '$scope', '$modal', '$q', '$http', 'SendConnectorService', function ($scope, $modal, $q, $http, SendConnectorService) {
                $scope.connectors = [];
                $scope.certificates = [];
                $scope.domains = [];
                $scope.adding = false;
                $scope.IPRegexp = IPRegexp;

                SendConnectorService.all()
                    .success(function (connectors) {
                        $scope.connectors = connectors;
                    })
                    .error(function (data) {
                        showError(data.Message);
                    });

                $http.get("api/SendConnectors/Certificates")
                    .success(function (certificates) {
                        $scope.certificates = certificates;
                    })
                    .error(function (data) {
                        showError(data.Message);
                    });

                $http.get("api/SendConnectors/Domains")
                    .success(function (domains) {
                        $scope.domains = domains;
                    })
                    .error(function (data) {
                        showError(data.Message);
                    });


                $scope.update = function (connector) {
                    if (connector.TLSSettings.CertificateName === '-- None --')
                        connector.TLSSettings.CertificateName = null;

                    SendConnectorService
                        .update(connector)
                        .then(function (data) {
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                $scope.connectors[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + connector.Id).addClass('in');
                            }, 10);
                        }, function (data) {
                            showError(data.Message);
                        });
                };

                $scope.delete = function (connector) {
                    SendConnectorService
                        .delete(connector.Id)
                        .then(function (success) {
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                $scope.connectors.splice(index, 1);
                            }
                        }, function (data) {
                            showError(data.Message);
                        });
                };

                $scope.startAdd = function () {
                    $http.get("api/SendConnectors/Default")
                        .then(function (data) {
                            $scope.adding = true;
                            data.data.__adding__ = true;
                            data.data.Id = 'Add';
                            $scope.connectors.push(data.data);
                            window.setTimeout(function () {
                                $('#collapseAdd').addClass('in');
                                $('#connectorFormAdd :input').first().focus();
                            }, 10);
                        }, function (data) {
                            showError(data.Message);
                        });
                };

                $scope.add = function (connector) {
                    if (connector.TLSSettings.CertificateName === '-- None --')
                        connector.TLSSettings.CertificateName = null;

                    delete connector.Id;

                    SendConnectorService
                        .add(connector)
                        .then(function (data) {
                            var index = $scope.connectors.indexOf(connector);
                            if (index > -1) {
                                $scope.connectors[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + data.data.Id).addClass('in');
                            }, 10);
                            $scope.adding = false;
                        }, function (data) {
                            showError(data.Message);
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