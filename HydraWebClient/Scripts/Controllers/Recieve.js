(function () {
    angular.module('Recieve', ['ui.bootstrap.modal', 'enumFlag'])

        .service("RecieveConnectorService", ["$http", DataService("api/RecieveConnectors")])

        .controller('RecieveController', [
            '$scope', '$modal', '$q', 'RecieveConnectorService', function ($scope, $modal, $q, RecieveConnectorService) {
                $scope.connectors = [];

                $scope.sslProtocols = {
                    0 : 'None',
                    12: 'SSL 2',
                    48: 'SSL 3',
                    192: 'TTL',
                    240: 'Default',
                    768: 'TLS 1.1',
                    3072: 'TLS 1.2'
                }

                RecieveConnectorService.all()
                    .success(function (connectors) {
                        $scope.connectors = connectors;
                    });
            }
        ]);
})();