(function () {
    var fieldsRegex = /^#Fields:\s*(.*)$/mi;

    angular.module('Logging', ['ui.bootstrap.modal', 'ui.grid', 'ui.grid.resizeColumns'])

        .controller('LoggingController', [
            '$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
                $scope.logs = [];
                $scope.loading = false;
                $scope.log = null;
                $scope.logName = null;

                $scope.gridOptions = {
                    data: [],
                    enableFiltering: true
                };

                $http.get("api/Logs/List")
                    .success(function (data) {
                        $scope.logs = data;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                function calculateColumnAutoWidths(columns, data, fontFamily, fontSize) {
                    var maximumsPixels = {};

                    var cell = $('<div class="ui-grid-cell-contents" />');
                    var padding = cell.innerWidth() - cell.width();

                    // TODO: Cleanup
                    var menuButton = $('<div class="ui-grid-column-menu-button" style="width: 2.2em"></div>');
                    var buttonWidth = menuButton.outerWidth();

                    var detector = new FontDetector();
                    var fonts = fontFamily.split(/,/);
                    var font;
                    for (var i = 0; i < fonts.length; i++) {
                        var f = fonts[i];
                        if (f.indexOf('"') === 0) f = f.substr(1, f.length - 2);
                        if (detector.detect(f)) {
                            font = f;
                            break;
                        }
                    }

                    for (var i = 0; i < data.length; i++) {
                        for (var j = 0; j < columns.length; j++) {
                            var field = columns[j].field;
                            var v = new String(eval('data[i].'+field));

                            var pixels = getTextWidth(v, fontSize + ' ' + font);
                            if ((maximumsPixels[field] || 0) < pixels)
                                maximumsPixels[field] = pixels;
                        }
                    }

                    for (var j = 0; j < columns.length; j++) {
                        var field = columns[j].field;

                        var pixels = getTextWidth(columns[j].name, fontSize + ' ' + font);
                        pixels += buttonWidth;
                        if ((maximumsPixels[field] || 0) < pixels)
                            maximumsPixels[field] = pixels;

                    }

                    for (var j = 0; j < columns.length; j++) {
                        var pixels = maximumsPixels[columns[j].field];
                        columns[j].minWidth = pixels + padding + 1;
                    }
                }

                $scope.load = function () {
                    $scope.loading = true;
                    $http.get("api/Logs/Get/" + $scope.logName)
                        .success(function (data, status, headers) {
                            var ct = headers('Content-Type');
                            $scope.isCSV = ct === 'text/csv';
                            if ($scope.isCSV) {
                                var style = window.getComputedStyle($('#csvGrid')[0], null);
                                var fontSize = style.getPropertyValue('font-size');
                                var fontFamily = style.getPropertyValue('font-family');
                                var csvFields, match;
                                if ((match = fieldsRegex.exec(data)) !== null) {
                                    var fields = match[1].split(/,/);
                                    csvFields = fields.map(function(f, i) { return { field: 'values['+i+']', name: f }; });
                                }
                                var csv = Papa.parse(data, { comments: '#', skipEmptyLines: true });
                                $scope.gridOptions.columnDefs = angular.copy(csvFields);
                                $scope.gridOptions.data = angular.copy(csv.data.map(function (v) { return { values: v }; }));
                                calculateColumnAutoWidths($scope.gridOptions.columnDefs, $scope.gridOptions.data, fontFamily, fontSize);
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