(function () {
    angular.module('ImapTest', [])

        .controller('ImapTestController', [
            '$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
                $rootScope.pageTitle = 'Imap Test';
                $rootScope.pageSubtitle = '';
                $scope.settings = {
                    Port: 143,
                    AuthMethod: 'Auto',
                    Ssl: false
                };
                $scope.result = null;

                $scope.updatePort = function() {
                    $scope.settings.Port = $scope.settings.Ssl ? 993 : 143;
                }

                $scope.test = function () {
                    $scope.result = null;
                    var reenable = disableClickedButton($('#testButton'));
                    return $http.post("api/ImapTest", $scope.settings)
                    .success(function (data) {
                        $scope.result = data;
                        reenable();
                    })
                    .error(function (data) {
                        reenable();
                        showError(data.Message || data.data.Message);
                    });
                };
            }
        ]);
})();