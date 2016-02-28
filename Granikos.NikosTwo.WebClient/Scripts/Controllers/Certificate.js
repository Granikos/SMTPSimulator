(function () {

    angular.module('Certificate', ['ui.bootstrap.modal', 'ngFileUpload'])

        .service("CertificateService", ["$http", DataService("api/Certificates")])

        .controller('CertificateController', [
            '$scope', '$uibModal', '$q', '$http', 'CertificateService', 'Upload', function ($scope, $uibModal, $q, $http, CertificateService, Upload) {
                $scope.certificates = [];

                function refresh() {
                    $http.get("/api/Certificates/By-Type/file")
                        .then(function(data) {
                            $scope.certificates = data.data;
                        }, function(data) {
                            showError(data.Message || data.data.Message);
                        });
                }

                $scope.upload = function (files) {
                    if (files && files.length > 0) {
                        var file = files[0];
                        if ($scope.certificates.indexOf(file.name) !== -1) {
                            showError("A certificate with the same name already exists.");
                            return;
                        }
                        Upload.upload({
                            url: 'api/Certificates/' + file.name,
                            file: file
                        }).then(function (data, status, headers, config) {
                            refresh();
                        }, function (data) {
                            showError(data.Message || data.data.Message);
                        });
                    }
                };

                $scope.delete = function (file) {
                    CertificateService.delete(file)
                                .success(function () {
                                    for (var i = 0; i < $scope.certificates.length; i++) {
                                        if ($scope.certificates[i] === file) {
                                            $scope.certificates.splice(i, 1);
                                            break;
                                        }
                                    }
                                })
                                .error(function (data) {
                                    showError(data.Message || data.data.Message);
                                });
                };

                refresh();
            }
        ]);
})();