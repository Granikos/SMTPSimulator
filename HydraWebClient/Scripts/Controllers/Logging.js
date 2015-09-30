(function () {
    angular.module('Logging', ['ui.bootstrap.modal', 'ui.grid'])

        .controller('LoggingController', [
            '$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
                $scope.logs = [];
                $scope.loading = false;
                $scope.log = null;
                $scope.logName = null;

                $scope.gridOptions = {
                    data: [],
                    enableHorizontalScrollbar: false
                };

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
                        .success(function (data, status, headers) {
                            var ct = headers('Content-Type');
                            $scope.isCSV = ct === 'text/csv';
                            if ($scope.isCSV) {
                                var csv = Papa.parse(data, { header: true, skipEmptyLines: true });
                                var csvFields = csv.meta.fields;
                                $scope.gridOptions.columnDefs = angular.copy(csvFields.map(function (f) { return { field: f, name: f }; }));
                                $scope.gridOptions.data = angular.copy(csv.data);
                                $scope.refreshGrid = true;
                                $timeout(function () {
                                    $scope.refreshGrid = false;
                                }, 0);
                            }
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