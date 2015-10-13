(function () {
    angular.module('Timer', ['ui.bootstrap.modal', 'ui-rangeSlider'])

        .service("TimeTableService", ["$http", DataService("api/TimeTables")])

        .controller('TimerController', [
            '$scope', '$modal', 'TimeTableService', function ($scope, $modal, TimeTableService) {
                $scope.timeTables = [];

                TimeTableService.all()
                    .success(function (timeTables) {
                        $scope.timeTables = timeTables;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });
            }
        ]);
})();