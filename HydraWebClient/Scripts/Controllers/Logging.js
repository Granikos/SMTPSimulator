(function () {
    angular.module('Logging', ['ui.bootstrap.modal'])

        .controller('LoggingController', [
            '$scope', '$http', function ($scope, $http) {
                $scope.logs = [];
                $scope.loading = false;
                $scope.log = null;
                $scope.logName = null;

                $http.get("api/Logs/List")
                    .success(function (data) {
                        $scope.logs = data;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $scope.load = function () {
                    $scope.loading = true;
                    $http.get("api/Logs/Get/" + $scope.logName)
                        .success(function (data) {
                            $scope.log = data;
                            $scope.loading = false;
                        })
                        .error(function (data) {
                            $scope.loading = false;
                            showError(data.Message || data.data.Message);
                        });
                }
            }
        ]);
})();