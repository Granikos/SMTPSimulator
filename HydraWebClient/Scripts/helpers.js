function DataService(urlBase) {
    return function ($http) {
        this.all = function () {
            return $http.get(urlBase);
        };

        this.get = function (id) {
            return $http.get(urlBase + "/" + id);
        };

        this.add = function (cust) {
            return $http.post(urlBase, cust);
        };

        this.update = function (cust) {
            return $http.put(urlBase + "/" + cust.Id, cust);
        };

        this.delete = function (id) {
            return $http.delete(urlBase + "/" + id);
        };
    };
}

function simpleTemplate(add, type) {
    return "<div><form name=\"inputForm\"><input type=\"" + (type || "INPUT_TYPE") + "\" ng- class=\"'colt' + col.uid\" ui-grid-editor ng-model=\"MODEL_COL_FIELD\"" + (add || "") + " validate-cell></form></div>";
}

function showError(error) {
    if (Object.prototype.toString.call(error) === "[object Array]") {
        var e = "";

        for (var i = 0; i < error.length; i++) {
            e += error[i];
        }

        error = e;
    }

    BootstrapDialog.alert({
        message: error,
        title: "Error",
        type: BootstrapDialog.TYPE_DANGER
    });
}

function parseCSV(content, columnDefs, $q) {
    var deferred = $q.defer();
    setTimeout(function () {
        var data = Papa.parse(content);
        if (data.errors && data.errors.length > 0) {
            deferred.reject(data.errors);
            return;
        }
        var header = data.data[0];
        var columns = {};
        for (var i = 0; i < header.length; i++) {
            var x = header[i].replace(/\s+/g, "").toLowerCase();
            columns[x] = i;
        }

        var max = data.data.length - 1;
        deferred.notify({
            current: 0,
            max: max
        });

        for (var i = 1; i < data.data.length; i++) {
            var item = {};
            var line = data.data[i];
            for (var j = 0; j < columnDefs.length; j++) {
                var field = columnDefs[j].field;
                item[field] = line[columns[field.toLowerCase()]];
            }
            deferred.notify({
                item: item,
                current: i,
                max: max
            });
        }

        deferred.resolve();
    }, 0);

    return deferred.promise;
};