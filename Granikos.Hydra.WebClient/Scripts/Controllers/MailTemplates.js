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

                $scope.import = function (files) {
                    if (files && files.length > 0) {
                        var file = files[0];

                        Upload.upload({
                            url: 'api/MailTemplates/Import',
                            file: file
                        }).success(function (data, status, headers, config) {
                            $scope.templates.push(data);
                        });
                    }
                };

                $scope.update = function (template) {
                    MailTemplateService
                        .update(template)
                        .then(function (data) {
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + template.Id).addClass('in');
                            }, 10);
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                };

                $scope.delete = function (template) {
                    MailTemplateService
                        .delete(template.Id)
                        .then(function (success) {
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates.splice(index, 1);
                            }
                        }, function (data) {
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

                $scope.add = function (template) {
                    delete template.Id;
                    delete template.__adding__;

                    MailTemplateService
                        .add(template)
                        .then(function (data) {
                            var index = $scope.templates.indexOf(template);
                            if (index > -1) {
                                $scope.templates[index] = data.data;
                            }
                            window.setTimeout(function () {
                                $('#collapse' + data.data.Id).addClass('in');
                            }, 10);
                            $scope.adding = false;
                        }, function (data) {
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