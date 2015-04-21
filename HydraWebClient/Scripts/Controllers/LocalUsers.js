(function () {
    angular.module('LocalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.bootstrap.modal'])
        .service('LocalUserService', ['$http', DataService('api/LocalUsers')])
        .controller('LocalUsersController', [
            '$scope', '$modal', '$q', 'LocalUserService', function ($scope, $modal, $q, LocalUserService) {
                $scope.users = [];

                var columnDefs = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleTemplate('required') },
                    { name: 'mailbox', field: 'Mailbox', editableCellTemplate: simpleTemplate('required', 'email'), type: 'email' }
                ];

                $scope.import = function (name, content) {
                    if (name.lastIndexOf('.csv') !== name.length - 4) {
                        showError('Only .csv files can be imported!');
                        return;
                    }

                    $scope.importCurrent = 0;
                    $scope.importMax = 0;
                    $scope.importErrors = [];
                    $scope.importRunning = true;

                    var modal = $modal
                        .open({
                            templateUrl: 'Views/LocalUsersImportDialog.html',
                            backdrop: 'static',
                            keyboard: false,
                            scope: $scope
                        });

                    modal.opened.then(function () {
                        parseCSV(content, columnDefs, $q).then(function () {
                            $scope.importRunning = false;
                        }, function (errors) {
                            for (var i = 0; i < errors.length; i++) {
                                var error = errors[i];
                                $scope.importErrors.push({
                                    i: i,
                                    message: error.message + ' (in row ' + error.row + ')'
                                });
                            }
                            $scope.importRunning = false;
                        }, function (args) {
                            if (args.item) {
                                $scope.add(args.item).error(function (e) {
                                    $scope.importErrors.push({
                                        i: args.current,
                                        message: e.Message + ' (' + args.item.Mailbox + ')'
                                    });
                                });
                            }

                            $scope.importCurrent = args.current;
                            $scope.importMax = args.max;
                        });
                    });
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
                    var p = LocalUserService.add(user);

                    p.success(function (u) {
                        $scope.users.push(u);
                    });

                    return p;
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