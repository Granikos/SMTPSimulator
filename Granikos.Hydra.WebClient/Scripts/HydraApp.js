(function () {
    angular.module('Hydra', ['ngRoute', 'ui.bootstrap', 'ui-rangeSlider', 'Server', 'LocalUsers', 'ExternalUsers', 'Send', 'Receive', 'Timer', 'Mail', 'Logging', 'Register', 'Login', 'enumFlag', 'checklist-model', 'ui.select'])
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
                    .when('/Receive', {
                        templateUrl: 'Views/Receive/Index.html',
                        controller: 'ReceiveController'
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

                $httpProvider.defaults.transformResponse.push(function (responseData) {
                    convertDateStringsToDates(responseData);
                    return responseData;
                });
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
        .directive('validateCell', ['uiGridEditConstants', function(uiGridEditConstants) {
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
        }])
        .directive('ngTab', ['uiGridEditConstants', function(uiGridEditConstants) {
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
        }])

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
          var ModalInstanceCtrl = ['$scope', '$modalInstance', function ($scope, $modalInstance) {
                $scope.ok = function () {
                    $modalInstance.close();
                };

                $scope.cancel = function () {
                    $modalInstance.dismiss('cancel');
                };
            }];

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
    })
    .filter('typeaheadHighlightSafe', ['$sce', '$injector', '$log', function ($sce, $injector, $log) {
            var isSanitizePresent = $injector.has('$sanitize');

        function escapeRegexp(queryToEscape) {
            // Regex: capture the whole query string and replace it with the string that will be used to match
            // the results, for example if the capture is "a" the result will be \a
            return queryToEscape.replace(/([.?*+^$[\]\\(){}|-])/g, '\\$1');
        }

        function containsHtml(matchItem) {
            return /<.*>/g.test(matchItem);
        }

        return function (matchItem, query) {
            if (!isSanitizePresent && containsHtml(matchItem)) {
                $log.warn('Unsafe use of typeahead please use ngSanitize'); // Warn the user about the danger
            }


            if (query) {
                var escapedQuery = escapeRegexp(query);

                if (/\W/.test(query)) {
                    // Replaces the capture string with a the same string inside of a "strong" tag
                    matchItem = ('' + matchItem).replace(new RegExp(escapedQuery, 'gi'), '<strong>$&</strong>');
                } else {
                    var re1 = new RegExp(escapedQuery + '(?!\\w*;)', 'gi');
                    var re2 = new RegExp('([^&\\w]\w*)(' + escapedQuery + ')(\\w*;)', 'gi');

                    // Replaces the capture string with a the same string inside of a "strong" tag
                    matchItem = ('' + matchItem).replace(re1, '<strong>$&</strong>');
                    matchItem = matchItem.replace(re2, '$1<strong>$2</strong>$2');
                }

                if (!isSanitizePresent) {
                    matchItem = $sce.trustAsHtml(matchItem); // If $sanitize is not present we pack the string in a $sce object for the ng-bind-html directive
                }
            }

            return matchItem;
        };
    }])
    .directive('selectpicker', function () {
        return {
            restrict: 'C',
            link: function (scope, element, attrs) {
                $(element).selectpicker();
            }
        };
    });
})();