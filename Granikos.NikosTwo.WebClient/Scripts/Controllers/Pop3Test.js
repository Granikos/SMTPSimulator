(function () {
    angular.module('Pop3Test', [])

        .controller('Pop3TestController', [
            '$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
                $rootScope.pageTitle = 'Pop3 Test';
                $rootScope.pageSubtitle = '';
                $scope.settings = {
                    Port: 110,
                    AuthMethod: 'Login',
                    Ssl: false
                };
                $scope.result = null;

                $scope.updatePort = function() {
                    $scope.settings.Port = $scope.settings.Ssl ? 995 : 110;
                }

                $scope.test = function () {
                    $scope.result = null;
                    var reenable = disableClickedButton($('#testButton'));
                    return $http.post("api/ConnectionTest/Pop3", $scope.settings)
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