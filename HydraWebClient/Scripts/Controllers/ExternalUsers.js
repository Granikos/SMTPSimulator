(function () {
    var DomainRegexp = /^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$/;

    angular.module('ExternalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.grid.pagination', 'ui.bootstrap.modal', 'ngFileUpload'])
        .service('ExternalUsersService', ['$http', DataService('api/ExternalUsers')])
        .service("DomainService", ["$http", DataService("api/Domains")])
        .service("SendConnectorService", ["$http", DataService("api/SendConnectors")])
        .controller('ExternalUsersController', [
            '$scope', '$modal', '$q', '$http', 'ExternalUsersService', 'Upload', 'DomainService', 'SendConnectorService', function($scope, $modal, $q, $http, ExternalUsersService, Upload, DomainService, SendConnectorService) {
                $scope.users = [];
                $scope.domains = {};
                $scope.connectors = [];

                var paginationOptions = {
                    PageSize: 25,
                    PageNumber: 1
                };

                var columnDefs = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'mailbox', field: 'getMailAddress()', displayName:'Email Address', editableCellTemplate: simpleEditTemplate('required', 'email'), type: 'email' }
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
                    onRegisterApi: function(gridApi) {
                        $scope.gridApi = gridApi;
                        gridApi.rowEdit.on.saveRow($scope, $scope.saveRow);

                        gridApi.pagination.on.paginationChanged($scope, function(newPage, pageSize) {
                            paginationOptions.PageNumber = newPage;
                            paginationOptions.PageSize = pageSize;
                            $scope.refresh();
                        });
                    }
                };

                SendConnectorService.all()
                    .success(function(connectors) {
                        $scope.connectors = connectors;
                    })
                    .error(function(data) {
                        showError(data.data.Message);
                    });


                $scope.refreshDomains = function() {
                    DomainService.all()
                        .success(function(domains) {
                            $scope.domains = {};
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

                $scope.updateDomain = function(domain) {
                    DomainService.update(domain)
                        .error(function(data) {
                            showError(data.data.Message);
                        });
                };

                $scope.import = function(files) {
                    if (files && files.length) {
                        var file = files[0];
                        Upload.upload({
                            url: 'api/ExternalUsers/Import',
                            // fields: { 'username': $scope.username },
                            file: file
                        }).progress(function(evt) {
                            var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                            console.log('progress: ' + progressPercentage + '% ' + evt.config.file.name);
                        }).success(function(data, status, headers, config) {
                            console.log('file ' + config.file.name + 'uploaded. Response: ' + data);
                            $scope.refreshDomains();
                            $scope.refresh();
                        });
                    }
                };

                $scope.saveRow = function(rowEntity) {
                    var promise = ExternalUsersService.update(rowEntity);
                    $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
                };

                $scope.deleteSelected = function() {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function(data, index) {
                        ExternalUsersService.delete(data.Id).success(function() {
                            $scope.refresh();
                        });
                    });
                };

                var domainAddCtrl = ['$scope', '$modalInstance', function($scope, $modalInstance) {
                    $scope.DomainName = null;
                    $scope.DomainRegexp = DomainRegexp;

                    $scope.add = function() {
                        $modalInstance.close($scope.DomainName);
                    };
                }];

                $scope.addDomainDialog = function() {
                    $modal
                        .open({
                            templateUrl: 'Views/ExternalUsers/AddDomainDialog.html',
                            controller: domainAddCtrl
                        })
                        .result.then(function(domain) {
                            $scope.addDomain(domain);
                        });
                };

                $scope.addDomain = function(domain) {
                    var p = $http.post('api/Domains/' + domain);

                    p.success(function() {
                        $scope.refreshDomains();
                    });

                    return p;
                };

                $scope.deleteDomain = function(id) {
                    DomainService.delete(id)
                        .success(function() {
                            $scope.refreshDomains();
                            $scope.refresh();
                        })
                        .error(function(data) {
                            showError(data.data.Message);
                        });
                };

                $scope.refresh = function() {
                    setTimeout(function() {
                        ExternalUsersService.all(paginationOptions)
                            .success(function(result) {
                                $scope.gridOptions.totalItems = result.Total;
                                $scope.users = result.Entities;

                                angular.forEach($scope.users, function(row) {
                                    row.getMailAddress = function() {
                                        var domain = $scope.domains[row.DomainId];
                                        return row.Mailbox + '@' + (domain ? domain.DomainName : '???');
                                    };
                                });
                            });
                    }, 100);
                };

                // watches
                $scope.$watch('pagingOptions', function(newVal, oldVal) {
                    if (newVal !== oldVal && newVal.currentPage !== oldVal.currentPage) {
                        $scope.refresh();
                    }
                }, true);

                $scope.refreshDomains();
                $scope.refresh();
            }
        ]);
})();