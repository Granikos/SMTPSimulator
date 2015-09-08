(function () {
    angular.module('Mail', ['ui.bootstrap.modal'])

        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])

        .service('LocalUsersService', ['$http', DataService('api/LocalUsers')])

        .controller('MailController', [
            '$scope', '$http', '$modal', 'LocalUsersService', 'SendConnectorService', function ($scope, $http, $modal, LocalUsersService, SendConnectorService) {
                $scope.From = null;
                $scope.Recipients = [];
                $scope.SendConnector = null;
                $scope.Content = null;
                $scope.connectors = [];
                $scope.localUsers = [];

                LocalUsersService.all({ PageSize: 9999, PageNumber: 1 })
                    .success(function (result) {
                        $scope.localUsers =  result.Entities;
                    })
                    .error(function (data) {
                        showError(data.data.Message);
                    });

                SendConnectorService.all()
                    .success(function (connectors) {
                        $scope.connectors = connectors;
                    })
                    .error(function (data) {
                        showError(data.data.Message);
                    });

                $scope.searchLocalUsers = function(search) {
                    return $http.get("api/LocalUsers/Search/" + search)
                        .then(function(data) {
                            return $.map(data.data, function(item) {
                                return '"' + item.FirstName + ' ' + item.LastName + '" <' + item.Mailbox + '>';
                            });
                        });
                };

                $scope.removeRecipient = function (index) {
                    $scope.Recipients.splice(index, 1);
                }

                $scope.addRecipientDialog = function () {
                    $modal
                        .open({
                            templateUrl: 'Views/Mail/AddUserDialog.html',
                            controller: [
                                '$scope', '$modalInstance', function ($scope, $modalInstance) {
                                    $scope.Recipient = null;

                                    $scope.searchExternalUsers = function (search) {
                                        return $http.get("api/ExternalUsers/Search/" + search)
                                            .then(function (data) {
                                                return data.data;
                                            });
                                    };

                                    $scope.submit = function () {
                                        $modalInstance.close($scope.Recipient);
                                    };
                                }
                            ]
                        })
                        .result.then(function (user) {
                            $scope.Recipients.push(user);
                        });
                };
            }
        ]);
})();