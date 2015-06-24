﻿(function () {
    angular.module('ExternalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.bootstrap.modal', 'ngFileUpload'])

        .service('ExternalUsersService', ['$http', DataService('api/ExternalUsers')])

        .service("DomainService", ["$http", DataService("api/Domains")])

        .controller('ExternalUsersController', [
            '$scope', '$modal', '$q', '$http', 'ExternalUsersService', 'Upload', 'DomainService', function ($scope, $modal, $q, $http, ExternalUsersService, Upload, DomainService) {
                $scope.users = [];
                $scope.domains = {};

                var columnDefs = [
                    { name: 'firstName', field: 'FirstName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'lastName', field: 'LastName', editableCellTemplate: simpleEditTemplate('required') },
                    { name: 'mailbox', field: 'getMailAddress()', editableCellTemplate: simpleEditTemplate('required', 'email'), type: 'email' }
                ];

                DomainService.all()
                    .success(function (domains) {
                        angular.forEach(domains, function (domain) {
                            $scope.domains[domain.Id] = domain;
                        });
                        angular.forEach($scope.users, function (user) {
                            user.__changed__ = true;
                        });
                    })
                    .error(function (data) {
                        showError(data.data.Message);
                    });

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
                        });
                    }
                };

                $scope.export = function () {
                    $http.get("api/ExternalUsers/Export")
                    .success(function (data) {
                            alert(data);
                        })
                    .error(function (data) {
                        showError(data.data.Message);
                    });
                };

                ExternalUsersService.all()
                    .success(function (users) {
                        $scope.users = users;

                        angular.forEach($scope.users, function (row) {
                            row.getMailAddress = function () {
                                var domain = $scope.domains[row.DomainId];
                                return row.Mailbox + '@' + (domain? domain.DomainName : '???');
                            };
                        });
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