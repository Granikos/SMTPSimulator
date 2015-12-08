(function () {

    angular.module('Attachment', ['ui.bootstrap.modal', 'ngFileUpload'])

        .service("AttachmentService", ["$http", DataService("api/Attachments")])

        .controller('AttachmentController', [
            '$scope', '$modal', '$q', '$http', 'AttachmentService', 'Upload', function ($scope, $modal, $q, $http, AttachmentService, Upload) {
                $scope.attachments = [];

                AttachmentService.all()
                    .success(function (attachments) {
                        $scope.attachments = attachments;
                    })
                    .error(function (data) {
                        showError(data.Message || data.data.Message);
                    });

                $scope.upload = function (files) {
                    if (files && files.length > 0) {
                        var file = files[0];
                        Upload.upload({
                            url: 'api/Attachments/' + file.name + '?size=' + file.size,
                            file: file
                        }).success(function (data, status, headers, config) {
                            AttachmentService.all()
                                .success(function (attachments) {
                                    $scope.attachments = attachments;
                                })
                                .error(function (data) {
                                    showError(data.Message || data.data.Message);
                                });
                        });
                    }
                };

                $scope.delete = function (file) {
                    AttachmentService.delete(file)
                                .success(function () {
                                    for (var i = 0; i < $scope.attachments.length; i++) {
                                        if ($scope.attachments[i].Name === file) {
                                            $scope.attachments.splice(i, 1);
                                            break;
                                        }
                                    }
                                })
                                .error(function (data) {
                                    showError(data.Message || data.data.Message);
                                });
                };

                $scope.rename = function (oldName, newName) {
                    return $http.put("api/Attachments/" + oldName + '?newName=' + newName);
                };

                $scope.fileIcon = fontawesomeFileIconClassForFileExtension;
            }
        ]);
})();