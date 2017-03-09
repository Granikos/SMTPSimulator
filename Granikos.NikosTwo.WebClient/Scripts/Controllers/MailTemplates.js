(function () {
    angular.module('MailTemplates', ['ui.bootstrap.modal'])

        .service('MailTemplateService', ['$http', DataService('api/MailTemplates')])

        .controller('MailTemplatesController', [
            '$scope', '$uibModal', '$q', '$http', 'MailTemplateService', 'Upload', '$rootScope', function ($scope, $uibModal, $q, $http, MailTemplateService, Upload, $rootScope) {
                $rootScope.pageTitle = 'Settings';
                $rootScope.pageSubtitle = 'Mail Templates';
                $scope.templates = [];
                $scope.encodings = ['ASCII', 'UTF8', 'UTF32'];
                $scope.behaviours = ['SMTP Compliant'];

                MailTemplateService.all()
                    .then(function (response) {
                        $scope.templates = response.data;
                    }, function (response) {
                        showError(response.data.Message);
                    });

                $scope.importing = false;

                $scope.import = function (files, $event) {
                    if (files && files.length > 0) {
                        var disabledButton = disableClickedButton($('#importButton'));
                        $('#importfile').prop('disabled', true);
                        $scope.importing = true;
                        var file = files[0];

                        Upload.upload({
                            url: 'api/MailTemplates/Import',
                            file: file
                        }).then(function (response) {
                            $scope.importing = false;
                            disabledButton();
                            $('#importfile').prop('disabled', false);
                            $scope.templates.push(response.data);
                        }, function (response) {
                            $scope.importing = false;
                            disabledButton();
                            $('#importfile').prop('disabled', false);
                            showError(response.data.Message);
                        });
                    }
                };

                $scope.update = function (template, $event) {
                    var button = $($event.currentTarget).find('button.add-button');
                    var disabledButton = disableClickedButton(button);

                    MailTemplateService
                        .update(template)
                        .then(function (response) {
                            disabledButton();
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates[index] = response.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + template.Id).addClass('in');
                            }, 10);
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.delete = function (template, $event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    MailTemplateService
                        .delete(template.Id)
                        .then(function () {
                            disabledButton();
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates.splice(index, 1);
                            }
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.startAdd = function () {
                    $http.get("api/MailTemplates/Empty")
                        .then(function (response) {
                            $scope.adding = true;
                            response.data.__adding__ = true;
                            response.data.Id = 'Add';
                            $scope.templates.push(response.data);
                            window.setTimeout(function () {
                                $('#collapseAdd').addClass('in');
                                $('#templateFormAdd :input').first().focus();
                            }, 10);
                        }, function (response) {
                            showError(response.data.Message);
                        });
                };

                $scope.add = function (template, $event) {
                    var button = $($event.currentTarget).find('button.add-button');
                    var disabledButton = disableClickedButton(button);

                    delete template.Id;

                    MailTemplateService
                        .add(template)
                        .then(function (response) {
                            disabledButton();
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates[index] = response.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + response.data.Id).addClass('in');
                            }, 10);
                            $scope.adding = false;
                        }, function (response) {
                            disabledButton();
                            showError(response.data.Message);
                        });
                };

                $scope.cancelAdd = function (template) {
                    var index = $scope.templates.indexOf(template);
                    if (index > -1) {
                        $scope.templates.splice(index, 1);
                    }
                    $scope.adding = false;
                };
            }
        ]);
})();