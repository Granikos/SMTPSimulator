(function () {
    angular.module('ExternalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.grid.pagination', 'ui.bootstrap.modal', 'ngFileUpload'])

        .service('ExternalUsersService', ['$http', DataService('api/ExternalUsers')])

        .service("DomainService", ["$http", DataService("api/Domains")])

        .controller('ExternalUsersController', [
            '$scope', '$modal', '$q', '$http', 'ExternalUsersService', 'Upload', 'DomainService', function ($scope, $modal, $q, $http, ExternalUsersService, Upload, DomainService) {
                $scope.users = [];
                $scope.domains = {};

                var paginationOptions = {
                    PageSize: 25,
                    PageNumber: 1
                };

                var columnDefs = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'mailbox', field: 'getMailAddress()', editableCellTemplate: simpleEditTemplate('required', 'email'), type: 'email' }
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

                $scope.refreshDomains = function() {
                    DomainService.all()
                        .success(function(domains) {
                            angular.forEach(domains, function(domain) {
                                $scope.domains[domain.Id] = domain;
                            });
                            angular.forEach($scope.users, function(user) {
                                user.__changed__ = true;
                            });
                        })
                        .error(function(data) {
                            showError(data.data.Message);
                        });
                };

                $scope.import = function (files) {
                    if (files && files.length) {
                        var file = files[0];
                        Upload.upload({
                            url: 'api/ExternalUsers/Import',
                            // fields: { 'username': $scope.username },
                            file: file
                        }).progress(function (evt) {
                            var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                            console.log('progress: ' + progressPercentage + '% ' + evt.config.file.name);
                        }).success(function (data, status, headers, config) {
                            console.log('file ' + config.file.name + 'uploaded. Response: ' + data);
                            $scope.refreshDomains();
                            $scope.refresh();
                        });
                    }
                };

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
                            $scope.refresh();
                            // $scope.users.splice($scope.users.lastIndexOf(data), 1);
                        });
                    });
                };

                $scope.refresh = function () {
                    setTimeout(function () {
                        ExternalUsersService.all(paginationOptions)
                            .success(function (result) {
                                $scope.gridOptions.totalItems = result.Total;
                                $scope.users = result.Entities;

                                angular.forEach($scope.users, function (row) {
                                    row.getMailAddress = function () {
                                        var domain = $scope.domains[row.DomainId];
                                        return row.Mailbox + '@' + (domain ? domain.DomainName : '???');
                                    };
                                });
                            });
                    }, 100);
                };

                // watches
                $scope.$watch('pagingOptions', function (newVal, oldVal) {
                    if (newVal !== oldVal && newVal.currentPage !== oldVal.currentPage) {
                        $scope.refresh();
                    }
                }, true);

                $scope.refreshDomains();
                $scope.refresh();
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