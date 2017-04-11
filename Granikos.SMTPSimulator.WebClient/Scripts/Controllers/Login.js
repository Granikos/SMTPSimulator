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
                            var path = $location.path();
                            $location.path('/login').search('returnUrl', path);
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
                        ).then(
                        function (response) {
                            if (response.data) {
                                deferredObject.resolve({ success: true });
                            } else {
                                deferredObject.resolve({ success: false });
                            }
                        }, function () {
                            deferredObject.resolve({ success: false });
                        });

                    return deferredObject.promise;
                }
            }
        ])

        .controller('LoginController', [
            '$scope', '$rootScope', '$routeParams', '$location', 'LoginFactory', function ($scope, $rootScope, $routeParams, $location, LoginFactory) {
                $scope.loginForm = {
                    emailAddress: '',
                    password: '',
                    rememberMe: false,
                    returnUrl: $routeParams.returnUrl,
                    loginFailure: false
                };

                $scope.login = function () {
                    LoginFactory($scope.loginForm.emailAddress, $scope.loginForm.password, $scope.loginForm.rememberMe).then(
                        function (loginResult) {
                            if (loginResult.success) {
                                $rootScope.auth.user = $scope.loginForm.emailAddress;
                                if ($scope.loginForm.returnUrl !== undefined) {
                                    $location.path($scope.loginForm.returnUrl);
                                } else {
                                    $location.path('/');
                                }
                            } else {
                                $scope.loginForm.loginFailure = true;
                            }
                        }
                    );
                }
            }
        ])

        .controller('LogoutController', [
            '$rootScope', '$http', '$routeParams', '$location', function ($rootScope, $http, $routeParams, $location) {

                $http.delete('/api/Account').
                    then(function (response) {
                        if (response.data) {
                            $rootScope.auth.user = null;
                        }
                        if ($routeParams.returnUrl !== undefined) {
                            $location.path($routeParams.returnUrl);
                        } else {
                            $location.path('/');
                        }
                    }, function () {
                        // TODO
                    });
            }
        ]);
})();