(function () {
    angular.module('Login', [])

        .factory('AuthHttpResponseInterceptor', [
            '$q', '$location', function ($q, $location) {
                return {
                    response: function (response) {
                        if (response.status === 401) {
                            console.log("Response 401");
                        }
                        return response || $q.when(response);
                    },
                    responseError: function (rejection) {
                        if (rejection.status === 401) {
                            console.log("Response Error 401", rejection);
                            $location.path('/login').search('returnUrl', $location.path());
                        }
                        return $q.reject(rejection);
                    }
                }
            }
        ])

        .factory('LoginFactory', [
            '$http', '$q', function ($http, $q) {
                return function (emailAddress, password, rememberMe) {
                    var deferredObject = $q.defer();

                    $http.post(
                            '/api/Account', {
                                Email: emailAddress,
                                Password: password,
                                RememberMe: rememberMe
                            }
                        ).
                        success(function (data) {
                            if (data) {
                                deferredObject.resolve({ success: true });
                            } else {
                                deferredObject.resolve({ success: false });
                            }
                        }).
                        error(function () {
                            deferredObject.resolve({ success: false });
                        });

                    return deferredObject.promise;
                }
            }
        ])

        .controller('LoginController', [
            '$scope', '$routeParams', '$location', 'LoginFactory', function ($scope, $routeParams, $location, LoginFactory) {
                $scope.loginForm = {
                    emailAddress: '',
                    password: '',
                    rememberMe: false,
                    returnUrl: $routeParams.returnUrl,
                    loginFailure: false
                };

                $scope.login = function () {
                    var result = LoginFactory($scope.loginForm.emailAddress, $scope.loginForm.password, $scope.loginForm.rememberMe);
                    result.then(function (result) {
                        if (result.success) {
                            if ($scope.loginForm.returnUrl !== undefined) {
                                $location.path($scope.loginForm.returnUrl);
                            } else {
                                $location.path('/');
                            }
                        } else {
                            $scope.loginForm.loginFailure = true;
                        }
                    });
                }
            }
        ]);
})();