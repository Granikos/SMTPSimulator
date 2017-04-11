(function () {
    var fieldsRegex = /^#Fields:\s*(.*)$/mi;

    angular.module('Logging', ['ui.bootstrap.modal', 'ui.grid', 'ui.grid.resizeColumns'])

        .controller('LoggingController', [
            '$scope', '$http', '$timeout', '$rootScope', function ($scope, $http, $timeout, $rootScope) {
                $rootScope.pageTitle = 'Logging';
                $rootScope.pageSubtitle = '';
                $scope.logs = [];
                $scope.loading = false;
                $scope.log = null;
                $scope.logName = null;

                $scope.gridOptions = {
                    data: [],
                    enableFiltering: true
                };

                $http.get("api/Logs/List")
                    .then(function (response) {
                        $scope.logs = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $scope.load = function () {
                    if (!$scope.logName) return;
                    $scope.loading = true;
                    $http.get("api/Logs/Get/" + $scope.logName)
                        .then(function (response) {
                            var ct = response.headers('Content-Type');
                            $scope.isCSV = ct === 'text/csv';
                            if ($scope.isCSV) {
                                var style = window.getComputedStyle($('#csvGrid')[0], null);
                                var fontSize = style.getPropertyValue('font-size');
                                var fontFamily = style.getPropertyValue('font-family');
                                var csvFields, match;
                                if ((match = fieldsRegex.exec(response.data)) !== null) {
                                    var fields = match[1].split(/,/);
                                    csvFields = fields.map(function(f, i) { return { field: 'values['+i+']', name: f }; });
                                }
                                var csv = Papa.parse(response.data, { comments: '#', skipEmptyLines: true });
                                $scope.gridOptions.columnDefs = angular.copy(csvFields);
                                $scope.gridOptions.data = angular.copy(csv.data.map(function (v) { return { values: v }; }));
                                calculateColumnAutoWidths($scope.gridOptions.columnDefs, $scope.gridOptions.data, fontFamily, fontSize, true);
                                $scope.refreshGrid = true;
                                $timeout(function () {
                                    $scope.refreshGrid = false;
                                }, 0);
                            }
                            $scope.log = response.data;
                            $scope.loading = false;
                        }, function (response) {
                            $scope.loading = false;
                            showError(response.data.Message);
                        });
                }
}
]);
})();