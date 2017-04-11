(function () {
    var mailRegexp = /^([\u00C0-\u017F\w\s'-]+<[\w\.]+@([\w\d-]+\.)+[\w]{2,4}>|[\w\.]+@([\w\d-]+\.)+[\w]{2,4})$/;

    angular.module('Mail', ['ui.bootstrap.typeahead', 'ui.bootstrap.modal'])

        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])

        .service('LocalUsersService', ['$http', DataService('api/LocalUsers')])

        .controller('MailController', [
            '$scope', '$http', '$uibModal', 'SendConnectorService', '$rootScope', function ($scope, $http, $uibModal, SendConnectorService, $rootScope) {
                $rootScope.pageTitle = 'Mail';
                $rootScope.pageSubtitle = '';
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
                    .then(function (response) {
                        $.each(response.data, function (i, c) {
                            c.RetryTimeDuration = moment.duration(c.RetryTime);
                        });
                        $scope.connectors = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $http.get("api/SendConnectors/Empty")
                    .then(function (response) {
                        response.data.RetryTimeDuration = moment.duration(response.data.RetryTime);
                        $scope.emptyConnector = response.data;
                        angular.copy(response.data, $scope.Mail.Connector);
                    });

                $scope.searchLocalUsers = function(search) {
                    return $http.get("api/LocalUsers/Search/" + search)
                        .then(function(response) {
                            return response.data;
                        });
                };

                $scope.send = function () {
                    var reenable = disableClickedButton($('#sendButton'));
                    return $http.post("api/Mail/Send", $scope.Mail)
                    .then(function () {
                        reenable();
                        BootstrapDialog.alert({
                            message: "The mail has been enqueued successfully",
                            title: "Success",
                            type: BootstrapDialog.TYPE_SUCCESS
                        });
                    }, function (response) {
                        reenable();
                        showError(response.data.Message);
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
                                            .then(function (response) {
                                                return response.data;
                                            });
                                    };

                                    $scope.submit = function () {
                                        $uibModalInstance.close($scope.Recipient);
                                    };
                                }
                            ]
                        })
                        .result.then(function (recipient) {
                            $scope.Mail.Recipients.push(recipient);
                        });
                };
            }
        ]);
})();