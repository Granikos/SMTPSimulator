(function() {
    angular.module("Server", ["ui.grid", "ui.grid.edit", "ui.grid.rowEdit", "ui.grid.selection", "ui.bootstrap.modal"])

        .service("BindingService", ["$http", DataService("api/ServerBindings")])

        .service("SubnetService", ["$http", DataService("api/ServerSubnets")])

        .service("ServerService", [
            "$http", function($http) {
                this.get = function() {
                    return $http.get("api/Server");
                };

                this.set = function(config) {
                    return $http.post("api/Server", config);
                };
            }
        ])

        .controller("ServerController", [
            "$scope", "$modal", "$q", "ServerService", function($scope, $modal, $q, ServerService) {
                $scope.config = {};
                $scope.status = '???';

                ServerService.get().success(function(config) {
                    $scope.config = config;
                });

                $scope.submit = function() {
                    ServerService.set($scope.config)
                        .success(function(config) {
                            // TODO
                        })
                        .error(function(config) {
                            // TODO
                        });
                };
            }
        ])

        .controller("ServerBindingsController", [
            "$scope", "$modal", "$q", "BindingService", function ($scope, $modal, $q, BindingService) {
                $scope.bindings = [];

                BindingService.all()
                    .success(function (bindings) {
                        $scope.bindings = bindings;
                    });

                var columnDefs = [
                    { displayName: 'IP', field: 'Address', editableCellTemplate: simpleTemplate('required') },
                    { displayName: 'Port', field: 'Port', editableCellTemplate: simpleTemplate('required', 'number'), type: 'number' },
                    { displayName: 'Enable SSL?', field: 'EnableSsl', type: 'bool', editableCellTemplate: simpleTemplate('', 'checkbox') },
                    { displayName: 'Enforce TLS?', field: 'EnforceTLS', type: 'bool' }
                ];

                $scope.saveRow = function (rowEntity) {
                    var promise = BindingService.update(rowEntity);
                    $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
                };

                $scope.deleteSelected = function () {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                        BindingService.delete(data.Id).success(function () {
                            $scope.bindings.splice($scope.bindings.lastIndexOf(data), 1);
                        });
                    });
                };

                $scope.gridOptions = {
                    data: 'bindings',
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

        .controller("ServerSubnetsController", [
            "$scope", "$modal", "$q", "SubnetService", function ($scope, $modal, $q, SubnetService) {
                $scope.subnets = [];

                SubnetService.all()
                    .success(function (subnets) {
                        $scope.subnets = subnets;
                    });

                var columnDefs = [
                    { displayName: 'Network IP', field: 'Address', editableCellTemplate: simpleTemplate('required') },
                    { displayName: 'Size', field: 'Size', editableCellTemplate: simpleTemplate('required', 'number'), type: 'number' }
                ];

                $scope.saveRow = function (rowEntity) {
                    var promise = SubnetService.update(rowEntity);
                    $scope.gridApi.rowEdit.setSavePromise($scope.gridApi.grid, rowEntity, promise);
                };

                $scope.deleteSelected = function () {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (data, index) {
                        SubnetService.delete(data.Id).success(function () {
                            $scope.subnets.splice($scope.subnets.lastIndexOf(data), 1);
                        });
                    });
                };

                $scope.gridOptions = {
                    data: 'subnets',
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
        ]);
})();