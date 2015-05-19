(function () {
    angular.module('ExternalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.bootstrap.modal'])

        .service('ExternalUsersService', ['$http', DataService('api/ExternalUsers')])

        .controller('ExternalUsersController', [
            '$scope', '$modal', '$q', 'ExternalUsersService', function ($scope, $modal, $q, ExternalUsersService) {
                $scope.users = [];

                var columnDefs = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'mailbox', field: 'Mailbox', editableCellTemplate: simpleEditTemplate('required', 'email'), type: 'email' }
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
                            templateUrl: 'Views/ExternalUsers/ImportDialog.html',
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

                $scope.export = function () {
                    var rows = [['First Name', 'Last Name', 'Mailbox']];

                    for (var i = 0; i < $scope.users.length; i++) {
                        var user = $scope.users[i];

                        rows.push([user.FirstName, user.LastName, user.Mailbox]);
                    }

                    exportToCsv('ExternalUsers.csv', rows);
                };

                ExternalUsersService.all()
                    .success(function (users) {
                        $scope.users = users;
                    });

                $scope.saveRow = function (rowEntity) {
                    var promise = ExternalUsersService.update(rowEntity);
                    $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
                };

                $scope.addDialog = function () {
                    $modal
                        .open({
                            templateUrl: 'Views/ExternalUsers/AddDialog.html',
                            controller: 'ExternalUsersAddDialogController'
                        })
                        .result.then(function (user) {
                            $scope.add(user);
                        });
                };

                $scope.add = function (user) {
                    var p = ExternalUsersService.add(user);

                    p.success(function (u) {
                        $scope.users.push(u);
                    });

                    return p;
                };

                $scope.deleteSelected = function () {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                        ExternalUsersService.delete(data.Id).success(function () {
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
        .controller('ExternalUsersAddDialogController', [
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