(function () {
    angular.module('Server', ['ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.selection', 'ui.bootstrap.modal'])

        .service('BindingService', ['$http', DataService('api/ServerBindings')])

        .controller('ServerController', [
            '$scope', '$modal', '$q', 'BindingService', function ($scope, $modal, $q, BindingService) {
            }
        ]);
})();