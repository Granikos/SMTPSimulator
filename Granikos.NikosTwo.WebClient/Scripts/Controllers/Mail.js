(function () {
    var mailRegexp = /^([\u00C0-\u017F\w\s'-]+<[\w\.]+@([\w\d-]+\.)+[\w]{2,4}>|[\w\.]+@([\w\d-]+\.)+[\w]{2,4})$/;

    angular.module('Mail', ['ui.bootstrap.modal'])

        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])

        .service('LocalUsersService', ['$http', DataService('api/LocalUsers')])

        .controller('MailController', [
            '$scope', '$http', '$uibModal', 'SendConnectorService', function ($scope, $http, $uibModal, SendConnectorService) {
                $scope.Mail = {
                    Sender: null,
                    Recipients: [],
                    ConnectorId: null,
                    Content: null
                };
                $scope.connectors = [];
                $scope.MailRegexp = mailRegexp;

                SendConnectorService.all()
                    .success(function (connectors) {
                        $scope.connectors = connectors;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $scope.searchLocalUsers = function(search) {
                    return $http.get("api/LocalUsers/Search/" + search)
                        .then(function(data) {
                            return data.data;
                        });
                };

                $scope.send = function () {
                    return $http.post("api/Mail/Send", $scope.Mail)
                    .success(function (data) {
                        // TODO
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });
                };

                $scope.removeRecipient = function (index) {
                    $scope.Recipients.splice(index, 1);
                }

                $scope.addRecipientDialog = function () {
                    $uibModal
                        .open({
                            templateUrl: 'Views/Mail/AddUserDialog.html',
                            controller: [
                                '$scope', '$uibModalInstance', function ($scope, $uibModalInstance) {
                                    $scope.Recipient = null;
                                    $scope.MailRegexp = mailRegexp;

                                    $scope.searchExternalUsers = function (search) {
                                        return $http.get("api/ExternalUsers/Search/" + search)
                                            .then(function (data) {
                                                return data.data;
                                            });
                                    };

                                    $scope.submit = function () {
                                        $uibModalInstance.close($scope.Recipient);
                                    };
                                }
                            ]
                        })
                        .result.then(function (user) {
                            $scope.Mail.Recipients.push(user);
                        });
                };
            }
        ]);
})();