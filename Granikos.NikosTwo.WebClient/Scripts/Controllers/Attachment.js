(function () {

    angular.module('Attachment', ['ui.bootstrap.modal', 'ngFileUpload'])

        .service("AttachmentService", ["$http", DataService("api/Attachments")])

        .controller('AttachmentController', [
            '$scope', '$uibModal', '$q', '$http', 'AttachmentService', 'Upload', '$rootScope', function ($scope, $uibModal, $q, $http, AttachmentService, Upload, $rootScope) {
                $rootScope.pageTitle = 'Settings';
                $rootScope.pageSubtitle = 'Attachments';
                $scope.attachments = [];

                AttachmentService.all().then(
                    function (response) {
                        $scope.attachments = response.data;
                    },
                    function (response) {
                        showError(response.data.Message);
                    });

                $scope.upload = function (files) {
                    if (files && files.length > 0) {
                        var file = files[0];
                        Upload.upload({
                            url: 'api/Attachments/' + file.name + '?size=' + file.size,
                            file: file
                        }).then(function () {
                            AttachmentService.all().then(
                                function (response) {
                                    $scope.attachments = response.data;
                                },
                                function (response) {
                                    showError(response.data.Message);
                                });
                        });
                    }
                };

                $scope.delete = function (file) {
                    AttachmentService.delete(file).then(
                                function () {
                                    for (var i = 0; i < $scope.attachments.length; i++) {
                                        if ($scope.attachments[i].Name === file) {
                                            $scope.attachments.splice(i, 1);
                                            break;
                                        }
                                    }
                                },function (response) {
                                    showError(response.data.Message);
                                });
                };

                $scope.rename = function (oldName, newName) {
                    return $http.put("api/Attachments/" + oldName + '?newName=' + newName);
                };

                $scope.fileIcon = fontawesomeFileIconClassForFileExtension;
            }
        ]);
})();