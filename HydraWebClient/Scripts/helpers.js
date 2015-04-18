function DataService(urlBase) {
    return function ($http) {
        this.all = function () {
            return $http.get(urlBase);
        };

        this.get = function (id) {
            return $http.get(urlBase + '/' + id);
        };

        this.add = function (cust) {
            return $http.post(urlBase, cust);
        };

        this.update = function (cust) {
            return $http.put(urlBase + '/' + cust.Id, cust);
        };

        this.delete = function (id) {
            return $http.delete(urlBase + '/' + id);
        };
    }
}