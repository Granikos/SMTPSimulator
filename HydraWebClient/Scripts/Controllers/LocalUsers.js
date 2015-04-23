(function () {
    angular.module('LocalUsers', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.bootstrap.modal'])

        .service('LocalUserService', ['$http', DataService('api/LocalUsers')])

        .service('UserTemplateService', ['$http', '$q', function ($http, $q) {
            this.locales = ['en', 'de'];

            function technicalGenerator(n, d) {
                var deferred = $q.defer();
                setTimeout(function () {
                    for (var i = 1; i <= n; i++) {
                        deferred.notify({
                            FirstName: 'User ' +i,
                            LastName: 'Test',
                            Mailbox: 'test' + i + '@' + d
                        });
                    }
                    deferred.resolve();
                }, 0);
                return deferred.promise;
            }

            function randomGenerator(l) {
                return function(n, d) {
                    var deferred = $q.defer();
                    $http.get('/Scripts/UserTemplates/' + l + '.json').
                        success(function(data, status, headers, config) {
                            var mailboxes = [''];
                            for (var i = 0; i < n; i++) {
                                var givenname, surname, mailbox = '';
                                while (mailboxes.indexOf(mailbox) >= 0) {
                                    givenname = data.givennames[Math.floor(Math.random() * data.givennames.length)];
                                    surname = data.surnames[Math.floor(Math.random() * data.surnames.length)];
                                    mailbox = (givenname + '.' + surname).toLowerCase() + '@' + d;
                                }
                                deferred.notify({
                                    FirstName: givenname,
                                    LastName: surname,
                                    Mailbox: mailbox
                                });
                            }
                            deferred.resolve();
                        }).
                        error(function(data, status, headers, config) {
                            deferred.reject(data);
                        });
                    return deferred.promise;
                };
            }

            this.templates = [
                {
                    name: 'Technical',
                    generator: technicalGenerator
                },
                {
                    name: 'Random Names (en)',
                    generator: randomGenerator('en')
                }
            ];
        }])

        .controller('LocalUsersController', [
            '$scope', '$modal', '$q', 'LocalUserService', 'UserTemplateService', function ($scope, $modal, $q, LocalUserService, UserTemplateService) {
                $scope.users = [];
                $scope.templates = UserTemplateService.templates;

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
                            templateUrl: 'Views/LocalUsers/ImportDialog.html',
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

                $scope.generate = function(template) {
                    $scope.generate = {
                        current: 0,
                        max: 0,
                        errors: [],
                        started: false,
                        done: false,
                        numberOfUsers: 1,
                        domain: ''
                    };

                    var modal = $modal
                        .open({
                            templateUrl: 'Views/LocalUsers/GenerateDialog.html',
                            backdrop: 'static',
                            keyboard: false,
                            scope: $scope
                        });

                    $scope.startGeneration = function () {
                        $scope.generate.max = $scope.generate.numberOfUsers;
                        $scope.generate.started = true;

                        template.generator($scope.generate.numberOfUsers, $scope.generate.domain)
                            .then(function() {
                                $scope.generate.done = true;
                            }, function(e) {
                                $scope.generate.done = true;
                                $scope.generate.errors.push({
                                    i: $scope.generate.current,
                                    message: 'An error occured.'
                                });
                            }, function(user) {
                                $scope.generate.current++;
                                $scope.add(user).error(function(e) {
                                    $scope.generate.errors.push({
                                        i: $scope.generate.current,
                                        message: e.Message + ' (' + args.item.Mailbox + ')'
                                    });
                                });
                            });
                    };
                };

                $scope.export = function () {
                    var rows = [['First Name', 'Last Name', 'Mailbox']];

                    for (var i = 0; i < $scope.users.length; i++) {
                        var user = $scope.users[i];

                        rows.push([user.FirstName, user.LastName, user.Mailbox]);
                    }

                    exportToCsv('LocalUsers.csv', rows);
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