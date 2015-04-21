(function () {
    angular.module('LocalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.bootstrap.modal'])

    .service('LocalUserService', ['$http', DataService('api/LocalUsers')])

    .controller('LocalUsersController', [
        '$scope', '$modal', 'LocalUserService', function ($scope, $modal, LocalUserService) {
            $scope.users = [];
            $scope.adding = false;

                var columnDefs = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleTemplate('required') },
                    { name: 'mailbox', field: 'Mailbox', editableCellTemplate: simpleTemplate('required', 'email'), type: 'email' }
                ];

            function simpleTemplate(add, type) {
                return "<div><form name=\"inputForm\"><input type=\"" + (type || "INPUT_TYPE") + "\" ng- class=\"'colt' + col.uid\" ui-grid-editor ng-model=\"MODEL_COL_FIELD\"" + (add || "") + " validate-cell></form></div>";
            }

            function showError(error) {
                if (Object.prototype.toString.call(error) === '[object Array]') {
                    var e = '';

                    for (var i = 0; i < error.length; i++) {
                        e += error[i];
                    }

                    error = e;
                }

                BootstrapDialog.alert({
                    message: error,
                    title: 'Error',
                    type: BootstrapDialog.TYPE_DANGER
                });
                
            }

            $scope.readFile = function (name, content) {
                if (name.lastIndexOf('.csv') !== name.length - 4) {
                    showError('Only .csv files can be imported!');
                    return;
                }
                var data = Papa.parse(content);
                if (data.errors && data.errors.length > 0) {
                    showError(data.errors);
                    return;
                }
                var header = data.data[0];
                var columns = {
                    
                }
                for (var i = 0; i < header.length; i++) {
                    var x = header[i].replace(/\s+/g, '').toLowerCase();
                    columns[x] = i;
                }

                function get(n, line) {
                    return line[columns[n.toLowerCase()]];
                }

                for (var i = 1; i < data.data.length; i++) {
                    var user = {};
                    var line = data.data[i];
                    for (var j = 0; j < columnDefs.length; j++) {
                        var field = columnDefs[j].field;
                        user[field] = get(field, line);
                    }
                    $scope.add(user);
                }
            };

            LocalUserService.all()
                .success(function (users) {
                    $scope.users = users;
                });

            $scope.saveRow = function (rowEntity) {
                var promise = LocalUserService.update(rowEntity);
                $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
            };

            $scope.addDialog = function () {
                $modal
                    .open({
                        templateUrl: 'Views/LocalUsersAddDialog.html',
                        controller: 'LocalUsersAddDialogController'
                    })
                    .result.then(function (user) {
                        $scope.add(user);
                    });
            };

            $scope.add = function (user) {
                LocalUserService.add(user).success(function (u) {
                    $scope.users.push(u);
                });
            };

            $scope.deleteSelected = function () {
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                    LocalUserService.delete(data.Id).success(function () {
                        $scope.users.splice($scope.users.lastIndexOf(data), 1);
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
                enableHorizontalScrollbar: 0,
                columnDefs: columnDefs,
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                    gridApi.rowEdit.on.saveRow($scope, $scope.saveRow);
                }
            };
        }
    ])

    .controller('LocalUsersAddDialogController', [
        '$scope', '$modalInstance', function ($scope, $modalInstance) {
            $scope.user = {
                FirstName: '',
                LastName: '',
                Mailbox: ''
            }

            $scope.submit = function () {
                $modalInstance.close($scope.user);
            };
        }
    ]);
})();