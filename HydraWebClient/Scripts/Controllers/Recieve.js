(function () {
    var IPRegexp = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;

    angular.module('Recieve', ['ui.bootstrap.modal', 'enumFlag'])

        .service("RecieveConnectorService", ["$http", DataService("api/RecieveConnectors")])

        .controller('RecieveController', [
            '$scope', '$modal', '$q', '$http', 'RecieveConnectorService', function ($scope, $modal, $q, $http, RecieveConnectorService) {
                $scope.connectors = [];
                $scope.adding = false;

                $scope.sslProtocols = {
                    0: 'None',
                    12: 'SSL 2',
                    48: 'SSL 3',
                    192: 'TTL',
                    240: 'Default',
                    768: 'TLS 1.1',
                    3072: 'TLS 1.2'
                }

                $scope.IPRegexp = IPRegexp;

                RecieveConnectorService.all()
                    .success(function (connectors) {
                        $scope.connectors = connectors;
                    })
                    .error(function (data) {
                        showError(data.Message);
                    });

                $http.get("api/RecieveConnectors/Certificates")
                    .success(function (certificates) {
                        $scope.certificates = certificates;
                    })
                    .error(function (data) {
                        showError(data.Message);
                    });


                $scope.update = function (connector) {
                    RecieveConnectorService
                        .update(connector)
                        .then(function (data) {
                            for (var i = 0; i < $scope.connectors.length; i++) {
                                if ($scope.connectors[i].Id === connector.Id) {
                                    $scope.connectors[i] = data.data;
                                    break;
                                }
                            }
                            window.setTimeout(function () {
                                $('#collapse' + connector.Id).addClass('in');
                            }, 10);
                        }, function (data) {
                            showError(data.Message);
                        });
                };

                $scope.delete = function (connector) {
                    RecieveConnectorService
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
                    $http.get("api/RecieveConnectors/Default")
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
                    RecieveConnectorService
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

                $scope.showIPRangeDialog = function (connector) {
                    $modal
                        .open({
                            templateUrl: 'Views/Recieve/AddIPRangeDialog.html',
                            controller: 'AddIPRangeDialogController'
                        })
                        .result.then(function (range) {
                            $scope.addIPRange(connector, range);
                        });
                };
                $scope.addIPRange = function (connector, range) {
                    connector.RemoteIPRanges.push(range);
                };
                $scope.removeIPRange = function (connector, range) {
                    var index = connector.RemoteIPRanges.indexOf(range);
                    if (index > -1) {
                        connector.RemoteIPRanges.splice(index, 1);
                    }
                };
            }
        ])
        .controller('AddIPRangeDialogController', [
            '$scope', '$modalInstance', function ($scope, $modalInstance) {
                $scope.range = {
                    StartString: '',
                    EndString: ''
                }

                $scope.IPRegexp = IPRegexp;

                $scope.submit = function () {
                    $modalInstance.close($scope.range);
                };
            }
        ]);
})();