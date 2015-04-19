(function () {
    angular.module('Hydra', ['ngRoute', 'ui.bootstrap', 'LocalUsers', 'Register', 'Login'])
        .config([
            '$routeProvider', '$httpProvider', '$locationProvider', function($routeProvider, $httpProvider, $locationProvider) {
                $locationProvider.hashPrefix('!').html5Mode(true);

                $routeProvider
                    .when('/Server', {
                    
                    })
                    .when('/LocalUsers', {
                        templateUrl: 'Views/LocalUsers.html',
                        controller: 'LocalUsersController'
                    })
                    .when('/ExternalUsers', {
                    
                    })
                    .when('/login', {
                        templateUrl: '/Views/Login.html',
                        controller: 'LoginController'
                    })
                    .when('/register', {
                        templateUrl: '/Views/Register.html',
                        controller: 'RegisterController'
                    })
                    .otherwise({ redirectTo: '/Server' });

                $httpProvider.interceptors.push('AuthHttpResponseInterceptor');
            }
        ])
        .run([
            '$rootScope', '$location', function($rootScope, $location) {
                $rootScope.$on('$locationChangeSuccess', function() {
                    var url = $location.url();
                    $('nav a:not(.navbar-brand)').each(function(i, el) {
                        $e = $(el);
                        $e.parent('li').toggleClass('active', url.indexOf($e.attr('href')) === 0);
                    });
                });
            }
        ])
        .controller('LandingPageController', [
            '$scope', function($scope) {
                $scope.test = 'Fubar';
            }
        ])
        .directive('validateCell', function(uiGridEditConstants) {
            return {
                restrict: 'A',
                scope: false,
                link: function(scope, element, attrs) {
                    element.bind('blur', function(evt) {
                        if (scope.inputForm && !scope.inputForm.$valid) {
                            evt.stopImmediatePropagation();
                        }
                    });
                }
            };
        })
        .directive('ngTab', function(uiGridEditConstants) {
            return {
                restrict: 'A',
                scope: false,
                priority: 0,
                compile: function compile(tElement, tAttrs, transclude) {
                    return {
                        pre: function preLink(scope, iElement, iAttrs, controller) {
                            var link = iElement.find('a').attr('href');

                            iElement.attr('ng-class', "{ active: '$location.url().indexOf(\\'" + link + "\\') === 0 }");
                        }
                    }
                }
            };
        });
})();