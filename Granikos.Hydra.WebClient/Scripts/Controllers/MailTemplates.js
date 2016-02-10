(function () {
    angular.module('MailTemplates', ['ui.bootstrap.modal'])

        .service('MailTemplateService', ['$http', DataService('api/MailTemplates')])

        .controller('MailTemplatesController', [
            '$scope', '$modal', '$q', '$http', 'MailTemplateService', 'Upload', function ($scope, $modal, $q, $http, MailTemplateService, Upload) {
                $scope.templates = [];
                $scope.encodings = ['ASCII', 'UTF8', 'UTF32'];
                $scope.behaviours = ['SMTP Compliant'];

                MailTemplateService.all()
                    .success(function (data) {
                        $scope.templates = data;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
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
                        }).success(function (data, status, headers, config) {
                            $scope.importing = false;
                            disabledButton();
                            $('#importfile').prop('disabled', false);
                            $scope.templates.push(data);
                        }).error(function (data) {
                            $scope.importing = false;
                            disabledButton();
                            $('#importfile').prop('disabled', false);
                            showError(data.Message || data.data.Message);
                        });
                    }
                };

                $scope.update = function (template, $event) {
                    var button = $($event.currentTarget).find('button.add-button');
                    var disabledButton = disableClickedButton(button);

                    MailTemplateService
                        .update(template)
                        .then(function (data) {
                            disabledButton();
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + template.Id).addClass('in');
                            }, 10);
                        }, function (data) {
                            disabledButton();
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.delete = function (template, $event) {
                    var disabledButton = disableClickedButton($event.currentTarget);
                    MailTemplateService
                        .delete(template.Id)
                        .then(function (success) {
                            disabledButton();
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates.splice(index, 1);
                            }
                        }, function (data) {
                            disabledButton();
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.startAdd = function () {
                    $http.get("api/MailTemplates/Empty")
                        .then(function (data) {
                            $scope.adding = true;
                            data.data.__adding__ = true;
                            data.data.Id = 'Add';
                            $scope.templates.push(data.data);
                            window.setTimeout(function () {
                                $('#collapseAdd').addClass('in');
                                $('#templateFormAdd :input').first().focus();
                            }, 10);
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.add = function (template, $event) {
                    var button = $($event.currentTarget).find('button.add-button');
                    var disabledButton = disableClickedButton(button);

                    delete template.Id;

                    MailTemplateService
                        .add(template)
                        .then(function (data) {
                            disabledButton();
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + data.data.Id).addClass('in');
                            }, 10);
                            $scope.adding = false;
                        }, function (data) {
                            disabledButton();
                            showError(data.Message || data.data.Message);
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