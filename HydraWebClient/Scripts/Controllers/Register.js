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
                        ).
                        success(function(data) {
                            if (data == "True") {
                                deferredObject.resolve({ success: true });
                            } else {
                                deferredObject.resolve({ success: false, errors: data.Messages });
                            }
                        }).
                        error(function() {
                            deferredObject.resolve({ success: false });
                        });

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
                    var result = RegistrationFactory($scope.registerForm.emailAddress, $scope.registerForm.password, $scope.registerForm.confirmPassword);
                    result.then(function (result) {
                        $rootScope.auth.user = $scope.registerForm.emailAddress;
                        if (result.success) {
                            $location.path('/');
                        } else {
                            $scope.registerForm.registrationFailure = true;
                            $scope.registerForm.errors = result.errors;
                        }
                    });
                }
            }
        ]);
})();