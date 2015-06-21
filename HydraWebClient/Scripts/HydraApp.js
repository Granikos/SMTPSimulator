(function () {
    angular.module('Hydra', ['ngRoute', 'ui.bootstrap', 'Server', 'LocalUsers', 'ExternalUsers', 'Send', 'Recieve', 'Timer', 'Logging', 'Register', 'Login', 'enumFlag', 'checklist-model'])
        .config([
            '$routeProvider', '$httpProvider', '$locationProvider', function($routeProvider, $httpProvider, $locationProvider) {
                $locationProvider.hashPrefix('!').html5Mode(true);

                $routeProvider
                    .when('/Server', {
                        templateUrl: 'Views/Server/Index.html',
                        controller: 'ServerController'
                    })
                    .when('/LocalUsers', {
                        templateUrl: 'Views/LocalUsers/Index.html',
                        controller: 'LocalUsersController'
                    })
                    .when('/ExternalUsers', {
                        templateUrl: 'Views/ExternalUsers/Index.html',
                        controller: 'ExternalUsersController'
                    })
                    .when('/Send', {
                        templateUrl: 'Views/Send/Index.html',
                        controller: 'SendController'
                    })
                    .when('/Recieve', {
                        templateUrl: 'Views/Recieve/Index.html',
                        controller: 'RecieveController'
                    })
                    .when('/Timer', {
                        templateUrl: 'Views/Timer/Index.html',
                        controller: 'TimerController'
                    })
                    .when('/Logging', {
                        templateUrl: 'Views/Logging/Index.html',
                        controller: 'LoggingController'
                    })
                    .when('/login', {
                        templateUrl: '/Views/Login.html',
                        controller: 'LoginController'
                    })
                    .when('/logout', {
                        template: '',
                        controller: 'LogoutController'
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
            '$rootScope', '$location', function ($rootScope, $location) {
                $rootScope.auth = {};

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
        })

    .directive('onReadFile', ['$parse', function ($parse) {
        return {
            restrict: 'A',
            scope: false,
            link: function (scope, element, attrs) {
                var fn = $parse(attrs.onReadFile);

                element.on('change', function (onChangeEvent) {
                    var reader = new FileReader();
                    var file = (onChangeEvent.srcElement || onChangeEvent.target).files[0];

                    reader.onload = function (onLoadEvent) {
                        scope.$apply(function () {
                            fn(scope, {
                                $fileContent: onLoadEvent.target.result,
                                $fileName: file.name,
                                $fileSize: file.size
                            });
                        });
                    };

                    reader.readAsText(file);
                });
            }
        };
    }]);
})();