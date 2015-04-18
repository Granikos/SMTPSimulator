(function() {
    angular.module('LocalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection'])

    .service('LocalUserService', ['$http', DataService('api/LocalUsers')])

    .controller('LocalUsersController', [
        '$scope', 'LocalUserService', function ($scope, LocalUserService) {
            $scope.users = [];

            function simpleTemplate(add, type) {
                return "<div><form name=\"inputForm\"><input type=\"" + (type || "INPUT_TYPE") + "\" ng- class=\"'colt' + col.uid\" ui-grid-editor ng-model=\"MODEL_COL_FIELD\"" + (add || "") + " validate-cell></form></div>";
            }

            LocalUserService.all()
                .success(function (users) {
                    $scope.users = users;
                });

            $scope.saveRow = function (rowEntity) {
                var promise = LocalUserService.update(rowEntity);
                $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
            };

            $scope.deleteSelected = function () {
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                    LocalUserService.delete(data.Id).success(function () {
                        $scope.gridOptions.data.splice($scope.gridOptions.data.lastIndexOf(data), 1);
                    });
                });
            };

            $scope.gridOptions = {
                data: 'users',
                enableCellSelection: false,
                enableRowSelection: true,
                multiSelect: true,
                showSelectionCheckbox: true,
                enableSelectAll: true,
                selectionRowHeaderWidth: 35,
                columnDefs: [
                    // { name: 'id', field: 'Id', enableCellEdit: false },
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleTemplate('required') },
                    { name: 'mailbox', field: 'Mailbox', editableCellTemplate: simpleTemplate('required', 'email'), type: 'email' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                    gridApi.rowEdit.on.saveRow($scope, $scope.saveRow);
                }
            };
        }
    ]);
})();