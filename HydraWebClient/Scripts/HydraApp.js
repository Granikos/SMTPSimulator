(function () {
    angular.module('Hydra', ['ngRoute', 'ui.bootstrap', 'Server', 'LocalUsers', 'ExternalUsers', 'Send', 'Recieve', 'Timer', 'Mail', 'Logging', 'Register', 'Login', 'enumFlag', 'checklist-model'])
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
                    .when('/Mail', {
                        templateUrl: 'Views/Mail/Index.html',
                        controller: 'MailController'
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
            '$rootScope', '$location', '$http', function ($rootScope, $location, $http) {
                $rootScope.auth = {};

                $http.get('/api/Account').
                       success(function (data) {
                        if (data) $rootScope.auth.user = data;
                    });

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
    }])
      .directive('ngReallyClick', ['$modal', function ($modal) {
            var ModalInstanceCtrl = function ($scope, $modalInstance) {
                $scope.ok = function () {
                    $modalInstance.close();
                };

                $scope.cancel = function () {
                    $modalInstance.dismiss('cancel');
                };
            };

            return {
                restrict: 'A',
                scope: {
                    ngReallyClick: "&",
                    item: "="
                },
                link: function (scope, element, attrs) {
                    element.bind('click', function () {
                        var message = attrs.ngReallyMessage || "Are you sure ?";
                        var okCls = attrs.ngReallyOkClass || "btn-primary";
                        var okText = attrs.ngReallyOkText || "OK";
                        var cancelCls = attrs.ngReallyCancelClass || "btn-warning";
                        var cancelText = attrs.ngReallyCancelText || "Cancel";

                        var modalHtml = '<div class="modal-body">' + message + '</div>';
                        modalHtml += '<div class="modal-footer"><button class="btn ' + okCls + '" ng-click="ok()">' + okText + '</button>';
                        modalHtml += '<button class="btn ' + cancelCls + '" ng-click="cancel()">' + cancelText + '</button></div>';

                        var modalInstance = $modal.open({
                            template: modalHtml,
                            controller: ModalInstanceCtrl
                        });

                        modalInstance.result.then(function () {
                            scope.ngReallyClick({ item: scope.item });
                        }, function () {
                            //Modal dismissed
                        });

                    });

                }
            }
        }
      ])
    .filter('escapeHTML', function() {
            return function(text) {
                if (text) {
                    return text.
                        replace(/&/g, '&amp;').
                        replace(/</g, '&lt;').
                        replace(/>/g, '&gt;').
                        replace(/'/g, '&#39;').
                        replace(/"/g, '&quot;');
                }
                return '';
            };
        });
})();