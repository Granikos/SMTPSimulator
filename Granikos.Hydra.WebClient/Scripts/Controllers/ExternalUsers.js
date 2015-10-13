(function () {
    angular.module('ExternalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.grid.pagination', 'ui.bootstrap.modal', 'ngFileUpload', 'checklist-model'])
        .service('ExternalUsersService', ['$http', DataService('api/ExternalUsers')])
        .service("ExternalGroupsService", ["$http", DataService("api/ExternalGroups")])
        .controller('ExternalUsersController', [
            '$scope', '$modal', '$q', '$http', 'ExternalUsersService', 'Upload', 'ExternalGroupsService', 'uiGridConstants', function ($scope, $modal, $q, $http, ExternalUsersService, Upload, GroupService, uiGridConstants) {
                $scope.users = [];
                $scope.groups = {};

                var paginationOptions = {
                    PageSize: 25,
                    PageNumber: 1
                };

                $scope.columns = [
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
                    columnDefs: $scope.columns,
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

                $scope.refreshGroups = function () {
                    GroupService.all()
                        .success(function (groups) {
                            $scope.groups = groups;
                            if ($scope.columns.length > 3) {
                                $scope.columns.splice(3, $scope.columns.length - 3);
                            }
                            for (var j = 0; j < groups.length; j++) {
                                $scope.columns.push({
                                    name: 'group' + j,
                                    displayName: groups[j].Name,
                                    type: 'boolean',
                                    cellTemplate: '<input type="checkbox" checklist-model="grid.appScope.groups[' + j + '].MailboxIds" checklist-value="row.entity.Id" />'
                                });
                            }
                        })
                        .error(function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                function doImport(file, overwrite) {
                    $scope.importDone = false;
                    $modal.open({
                        templateUrl: 'Views/ExternalUsers/ImportDialog.html',
                        scope: $scope
                    });

                    Upload.upload({
                        url: 'api/ExternalUsers/Import' + (overwrite ? 'WithOverwrite' : ''),
                        file: file
                    }).success(function (data, status, headers, config) {
                        $scope.importDone = true;
                        $scope.importedUsers = data.ImportCount;
                        $scope.overwrittenUsers = data.OverwrittenCount;

                        $scope.refreshGroups();
                        $scope.refresh();
                    });
                }

                $scope.import = function (files) {
                    if (files && files.length > 0) {
                        var file = files[0];

                        if ($scope.gridOptions.totalItems > 0) {
                            BootstrapDialog.confirm({
                                type: BootstrapDialog.TYPE_PRIMARY,
                                title: 'Overwrite Existing Users',
                                message: 'Do you want to delete all existing users?',
                                btnCancelLabel: 'No',
                                btnOKLabel: 'Yes',
                                btnOKClass: 'btn-danger',
                                callback: function (overwrite) {
                                    doImport(file, overwrite);
                                }
                            });
                        } else {
                            doImport(file, false);
                        }
                    }
                };

                $scope.saveRow = function (rowEntity) {
                    var promise = ExternalUsersService.update(rowEntity);
                    $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
                };

                $scope.deleteSelected = function () {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                        ExternalUsersService.delete(data.Id).success(function () {
                            $scope.refresh();
                        });
                    });
                };

                var groupAddCtrl = ['$scope', '$modalInstance', function ($scope, $modalInstance) {
                    $scope.Name = null;

                    $scope.add = function () {
                        $modalInstance.close($scope.Name);
                    };
                }];

                $scope.addGroupDialog = function () {
                    $modal
                        .open({
                            templateUrl: 'Views/ExternalUsers/AddGroupDialog.html',
                            controller: groupAddCtrl
                        })
                        .result.then(function (name) {
                            $scope.addGroup(name);
                        });
                };

                $scope.addGroup = function (name) {
                    var p = $http.post('api/ExternalGroups/' + name);

                    p.success(function () {
                        $scope.refreshGroups();
                    });

                    return p;
                };

                $scope.deleteGroup = function (id) {
                    GroupService.delete(id)
                        .success(function () {
                            $scope.refreshGroups();
                            $scope.refresh();
                        })
                        .error(function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.refresh = function () {
                    setTimeout(function () {
                        ExternalUsersService.all(paginationOptions)
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

                $scope.refreshGroups();
                $scope.refresh();
            }
        ]);
})();