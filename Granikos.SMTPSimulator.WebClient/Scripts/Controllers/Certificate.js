(function () {

    angular.module('Certificate', ['ui.bootstrap.modal', 'ngFileUpload'])

        .service("CertificateService", ["$http", DataService("api/Certificates")])

        .controller('CertificateController', [
            '$scope', '$uibModal', '$q', '$http', 'CertificateService', 'Upload', '$rootScope', function ($scope, $uibModal, $q, $http, CertificateService, Upload, $rootScope) {
                $rootScope.pageTitle = 'Settings';
                $rootScope.pageSubtitle = 'Certificates';
                $scope.certificates = [];

                function refresh() {
                    $http.get("/api/Certificates/By-Type/file")
                        .then(function(response) {
                            $scope.certificates = response.data;
                        }, function(response) {
                            showError(response.data.Message);
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
                        }).then(function () {
                            refresh();
                        }, function (response) {
                            showError(response.data.Message);
                        });
                    }
                };

                $scope.delete = function (file) {
                    CertificateService.delete(file).then(
                                function () {
                                    for (var i = 0; i < $scope.certificates.length; i++) {
                                        if ($scope.certificates[i] === file) {
                                            $scope.certificates.splice(i, 1);
                                            break;
                                        }
                                    }
                                }, function (response) {
                                    showError(response.data.Message);
                                });
                };

                refresh();
            }
        ]);
})();