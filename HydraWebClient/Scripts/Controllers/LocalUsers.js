(function () {
    angular.module('LocalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.grid.pagination', 'ui.bootstrap.modal'])

        .service('LocalUsersService', ['$http', DataService('api/LocalUsers')])

        .controller('LocalUsersController', [
            '$scope', '$modal', '$q', '$http', 'LocalUsersService', 'Upload', function ($scope, $modal, $q, $http, LocalUserService, Upload) {
                $scope.users = [];
                $scope.templates = [];

                var paginationOptions = {
                    PageSize: 25,
                    PageNumber: 1
                };

                var columnDefs = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'mailbox', field: 'Mailbox', displayName: 'Email Address', editableCellTemplate: simpleEditTemplate('required', 'email'), type: 'email' }
                ];

                $scope.gridOptions = {
                    data: 'users',
                    enablePaging: true,
                    paginationPageSizes: [25, 50, 100],
                    paginationPageSize: 25,
                    useExternalPagination: true,
                    totalServerItems: "totalServerItems",
                    enableSorting: false,
                    enableColumnMenus: false,
                    useExternalSorting: false,
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

                        gridApi.pagination.on.paginationChanged($scope, function (newPage, pageSize) {
                            paginationOptions.PageNumber = newPage;
                            paginationOptions.PageSize = pageSize;
                            $scope.refresh();
                        });
                    }
                };

                $http.get("api/LocalUsers/Templates")
                    .success(function (data) {
                        $scope.templates = data;
                    })
                    .error(function (data) {
                        showError(data.data.Message);
                    });

                $scope.import = function (files) {
                    if (files && files.length) {
                        var file = files[0];
                        Upload.upload({
                            url: 'api/LocalUsers/Import',
                            file: file
                        }).success(function (data, status, headers, config) {
                            $scope.refresh();
                        });
                    }
                };

                $scope.generate = function (template) {
                    $scope.generate = {
                        numberOfUsers: 1,
                        pattern: '%g.%s',
                        domain: '',
                        supportsPattern: template.SupportsPattern
                    };

                    var modal = $modal
                        .open({
                            templateUrl: 'Views/LocalUsers/GenerateDialog.html',
                            scope: $scope
                        });

                    $scope.startGeneration = function () {
                        $http.post("api/LocalUsers/Generate/" + $scope.generate.numberOfUsers, {
                            pattern: $scope.generate.pattern,
                            template: template.Name,
                            domain: $scope.generate.domain
                        })
                            .success(function (data) {
                                modal.close();
                                $scope.refresh();
                            })
                            .error(function (data) {
                                showError(data.data.Message);
                            });
                    };
                };

                $scope.saveRow = function (rowEntity) {
                    var promise = LocalUserService.update(rowEntity);
                    $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
                };

                $scope.addDialog = function () {
                    $modal
                        .open({
                            templateUrl: 'Views/LocalUsers/AddDialog.html',
                            controller: 'LocalUsersAddDialogController'
                        })
                        .result.then(function (user) {
                            $scope.add(user);
                        });
                };

                $scope.add = function (user) {
                    var p = LocalUserService.add(user);

                    p.success(function (u) {
                        $scope.refresh();
                        // $scope.users.push(u);
                    });

                    return p;
                };

                $scope.deleteSelected = function () {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                        LocalUserService.delete(data.Id).success(function () {
                            // $scope.users.splice($scope.users.lastIndexOf(data), 1);
                            $scope.refresh();
                        });
                    });
                };

                $scope.refresh = function () {
                    setTimeout(function () {
                        LocalUserService.all(paginationOptions)
                            .success(function (result) {
                                $scope.gridOptions.totalItems = result.Total;
                                $scope.users = result.Entities;
                            });
                    }, 100);
                };

                // watches
                $scope.$watch('pagingOptions', function (newVal, oldVal) {
                    if (newVal !== oldVal && newVal.currentPage !== oldVal.currentPage) {
                        $scope.refresh();
                    }
                }, true);

                $scope.refresh();
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