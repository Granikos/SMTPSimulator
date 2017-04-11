(function () {
    angular.module('Register', [])

        .factory('RegistrationFactory', [
            '$http', '$q', function($http, $q) {
                return function(emailAddress, password, confirmPassword) {

                    var deferredObject = $q.defer();

                    $http.put(
                            '/api/Account', {
                                Email: emailAddress,
                                Password: password,
                                ConfirmPassword: confirmPassword
                            }
                        ).then(
                            function (response) {
                                if (response.data == "True") {
                                    deferredObject.resolve({ success: true });
                                } else {
                                    deferredObject.resolve({ success: false, errors: response.data.Messages });
                                }
                            }, function() {
                                deferredObject.resolve({ success: false });
                            }
                        );

                    return deferredObject.promise;
                }
            }
        ])

        .controller('RegisterController', [
            '$scope', '$rootScope', '$location', 'RegistrationFactory', function ($scope, $rootScope, $location, RegistrationFactory) {
                $scope.registerForm = {
                    emailAddress: '',
                    password: '',
                    confirmPassword: '',
                    registrationFailure: false
                };

                $scope.register = function() {
                    RegistrationFactory($scope.registerForm.emailAddress, $scope.registerForm.password, $scope.registerForm.confirmPassword).then(
                        function (registerResult) {
                            $rootScope.auth.user = $scope.registerForm.emailAddress;
                            if (registerResult.success) {
                                $location.path('/');
                            } else {
                                $scope.registerForm.registrationFailure = true;
                                $scope.registerForm.errors = registerResult.errors;
                            }
                        }
                    );
                }
            }
        ]);
})();