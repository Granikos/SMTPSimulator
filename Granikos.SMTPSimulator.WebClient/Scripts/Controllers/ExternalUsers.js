(function () {
    var DomainRegexp = /^(\*\.?)?([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$/;

    angular.module('ExternalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.grid.pagination', 'ui.grid.resizeColumns', 'ui.bootstrap.modal', 'ngFileUpload', 'checklist-model'])
        .service('ExternalUsersService', ['$http', DataService('api/ExternalUsers')])
        .service("ExternalGroupsService", ["$http", DataService("api/ExternalGroups")])
        .controller('ExternalUsersController', [
            '$scope', '$uibModal', '$q', '$http', '$timeout', 'ExternalUsersService', 'Upload', 'ExternalGroupsService', 'uiGridConstants', '$rootScope', function ($scope, $uibModal, $q, $http, $timeout, ExternalUsersService, Upload, GroupService, uiGridConstants, $rootScope) {
                $rootScope.pageTitle = 'Users';
                $rootScope.pageSubtitle = 'External';
                $scope.users = [];
                $scope.groups = {};

                var paginationOptions = {
                    PageSize: 25,
                    PageNumber: 1
                };

                $scope.columns = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleEditTemplate('required'), enableColumnMenu: false },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleEditTemplate('required'), enableColumnMenu: false },
                    { name: 'mailbox', field: 'Mailbox', displayName: 'Email Address', editableCellTemplate: simpleEditTemplate('required', 'email'), type: 'email', width: '*', enableColumnMenu: false }
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

                function getColumnDef(id) {
                    return {
                        name: 'group' + id,
                        type: 'boolean',
                        width: '1%',
                        enableSorting: false,
                        enableHiding: false,
                        absMinWidth: 100,
                        _groupId: id,
                        headerCellTemplate: 'Views/ExternalUsers/GroupHeaderCellTemplate.html',
                        cellTemplate: 'Views/ExternalUsers/GroupCellTemplate.html'
                    };
                }

                $scope.refreshGridSizing = function () {
                    var style = window.getComputedStyle($('#usersGrid')[0], null);
                    var fontSize = style.getPropertyValue('font-size');
                    var fontFamily = style.getPropertyValue('font-family');
                    calculateColumnAutoWidths($scope.columns, $scope.users, fontFamily, fontSize, true);
                    $scope.refreshGrid = true;
                    $timeout(function () {
                        $scope.refreshGrid = false;
                    }, 0);

                }

                $scope.refreshGroups = function () {
                    GroupService.all()
                        .then(function (response) {
                            var groups = response.data
                            $scope.groups = groups;
                            $scope.groupsById = {};
                            if ($scope.columns.length > 3) {
                                $scope.columns.splice(3, $scope.columns.length - 3);
                            }
                            for (var j = 0; j < groups.length; j++) {
                                var id = groups[j].Id;
                                $scope.groupsById[id] = groups[j];
                                $scope.columns.push(getColumnDef(id));
                            }
                            $scope.refreshGridSizing();
                        }, function (response) {
                            showError(response.data.Message);
                        });
                };

                function doImport(file, overwrite) {
                    $scope.importDone = false;
                    $uibModal.open({
                        templateUrl: 'Views/ExternalUsers/ImportDialog.html',
                        scope: $scope
                    });

                    Upload.upload({
                        url: 'api/ExternalUsers/Import' + (overwrite ? 'WithOverwrite' : ''),
                        file: file
                    }).then(function (response) {
                        $scope.importDone = true;
                        $scope.importedUsers = response.data.ImportCount;
                        $scope.overwrittenUsers = response.data.OverwrittenCount;

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
                    $scope.gridApi.rowEdit.setSavePromise(rowEntity, promise);
                };

                $scope.deleteSelected = function () {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                        ExternalUsersService.delete(data.Id).then(function () {
                            $scope.refresh();
                        });
                    });
                };

                $scope.addGroupDialog = function () {
                    $uibModal
                        .open({
                            templateUrl: 'Views/ExternalUsers/AddGroupDialog.html',
                            controller: ['$scope', '$uibModalInstance', function ($scope, $uibModalInstance) {
                                $scope.Name = null;

                                $scope.add = function () {
                                    $uibModalInstance.close($scope.Name);
                                };
                            }]
                        })
                        .result.then(function (name) {
                            $scope.addGroup(name);
                        });
                };

                $scope.addGroup = function (name) {
                    $http.post('api/ExternalGroups/' + name)
                        .then(function (response) {
                            var group = response.data;
                            $scope.groups.push(group);
                            $scope.groupsById[group.Id] = group;
                            $scope.columns.push(getColumnDef(group.Id));
                            $scope.refreshGridSizing();
                        }, function (response) {
                            showError(response.data.Message);
                        });
                };

                $scope.deleteGroupWithConfirm = function (id) {
                    var name = $scope.groupsById[id].Name;
                    BootstrapDialog.confirm({
                        type: BootstrapDialog.TYPE_PRIMARY,
                        title: 'Delete User Group',
                        message: 'Do you want to delete the user group "' + name + '"?',
                        btnCancelLabel: 'No',
                        btnOKLabel: 'Yes',
                        btnOKClass: 'btn-danger',
                        callback: function (success) {
                            if (success) {
                                $scope.deleteGroup(id);
                            }
                        }
                    });
                };

                $scope.deleteGroup = function (id) {
                    var off;
                    for (off = 0; off < $scope.groups.length; off++)
                        if ($scope.groups[off].Id === id)
                            break;
                    GroupService.delete(id)
                        .then(function () {
                            $scope.groups.splice(off, 1);
                            $scope.columns.splice(off + 3, 1);
                            delete $scope.groupsById[id];
                        }, function (response) {
                            showError(response.data.Message);
                        });
                };

                $scope.removeAllFromGroup = function (id) {
                    var name = $scope.groupsById[id].Name;
                    BootstrapDialog.confirm({
                        type: BootstrapDialog.TYPE_PRIMARY,
                        title: 'Empty User Group',
                        message: 'Do you want to remove all users from the group "' + name + '"?',
                        btnCancelLabel: 'No',
                        btnOKLabel: 'Yes',
                        btnOKClass: 'btn-danger',
                        callback: function (success) {
                            if (success) {
                                var group = $scope.groupsById[id];
                                group.UserIds.length = 0;
                                $scope.$apply();
                            }
                        }
                    });
                };

                $scope.addToGroup = function (id) {
                    $uibModal
                        .open({
                            templateUrl: 'Views/ExternalUsers/SelectDomainDialog.html',
                            controller: [
                                '$scope', '$uibModalInstance', function ($scope, $uibModalInstance) {
                                    $scope.Domain = null;
                                    $scope.DomainRegexp = DomainRegexp;

                                    $scope.searchDomains = function (search) {
                                        return $http.get("api/ExternalUsers/SearchDomains/" + search)
                                            .then(function (response) {
                                                return response.data;
                                            });
                                    };

                                    $scope.submit = function () {
                                        $uibModalInstance.close($scope.Domain);
                                    };
                                }
                            ]
                        })
                        .result.then(function (domain) {
                            $http.get("api/ExternalUsers/ByDomain/" + domain)
                                .then(function (response) {
                                    var mbs = $scope.groupsById[id].UserIds;
                                    var temp = {};
                                    for (var i = 0; i < mbs.length; i++)
                                        temp[mbs[i]] = true;
                                    for (var i = 0; i < response.data.length; i++)
                                        if (!temp[response.data[i]])
                                            mbs.push(response.data[i]);
                                    $scope.groupsById[id]._dirty = true;
                                });
                        });
                };

                $scope.saveGroup = function (id) {
                    var group = $scope.groupsById[id];

                    GroupService.update(group)
                        .then(function (response) {
                            var index = $scope.groups.indexOf(group);
                            if (index > -1) {
                                $scope.groups[index] = response.data;
                            }
                            $scope.groupsById[id] = response.data;
                            // $scope.$apply();
                        }, function (response) {
                            showError(response.data.Message);
                        });
                };

                var usersResized = false;

                $scope.refresh = function () {
                    setTimeout(function () {
                        ExternalUsersService.all(paginationOptions)
                            .then(function (response) {
                                $scope.gridOptions.totalItems = response.data.Total;
                                $scope.users = response.data.Entities;
                                if (!usersResized) {
                                    $scope.refreshGridSizing();
                                    usersResized = true;
                                }
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