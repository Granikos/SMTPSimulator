(function () {
    var mailRegexp = /^([\u00C0-\u017F\w\s'-]+<[\w\.]+@([\w\d-]+\.)+[\w]{2,4}>|[\w\.]+@([\w\d-]+\.)+[\w]{2,4})$/;

    angular.module('Mail', ['ui.bootstrap.typeahead', 'ui.bootstrap.modal'])

        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])

        .service('LocalUsersService', ['$http', DataService('api/LocalUsers')])

        .controller('MailController', [
            '$scope', '$http', '$uibModal', 'SendConnectorService', function ($scope, $http, $uibModal, SendConnectorService) {
                $scope.Mail = {
                    Sender: null,
                    Recipients: [],
                    Text: null,
                    Html: null,
                    Subject: null,
                    Connector: {}
                };
                $scope.connectors = [];
                $scope.MailRegexp = mailRegexp;
                $scope.emptyConnector = {};

                $scope.timespanGreaterThanZero = function (value) {
                    return value && value.asMilliseconds() > 0;
                }

                $scope.modes = ['Disabled', 'Enabled', 'Required', 'Full Tunnel'];

                $scope.policies = ['Require Encryption', 'Allow No Encryption', 'No Encryption'];

                $scope.authLevels = ['Encryption Only', 'Certificate Validation', 'Domain Validation'];

                $scope.connectorChanged = function () {
                    var conn = $scope.selectedConnector || $scope.emptyConnector;
                    console.log(conn);
                    angular.copy(conn, $scope.Mail.Connector);
                };

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

                $http.get("api/SendConnectors/Empty")
                    .then(function (data) {
                        data.data.RetryTimeDuration = moment.duration(data.data.RetryTime);
                        $scope.emptyConnector = data.data;
                        angular.copy(data.data, $scope.Mail.Connector);
                    });

                $scope.searchLocalUsers = function(search) {
                    return $http.get("api/LocalUsers/Search/" + search)
                        .then(function(data) {
                            return data.data;
                        });
                };

                $scope.send = function () {
                    var reenable = disableClickedButton($('#sendButton'));
                    return $http.post("api/Mail/Send", $scope.Mail)
                    .success(function (data) {
                            reenable();
                        BootstrapDialog.alert({
                            message: "The mail has been enqueued successfully",
                            title: "Success",
                            type: BootstrapDialog.TYPE_SUCCESS
                        });
                    })
                    .error(function (data) {
                        reenable();
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