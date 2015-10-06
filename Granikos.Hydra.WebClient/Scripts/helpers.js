function DataService(urlBase) {
    return function ($http) {
        this.urlBase = urlBase;
        this.$http = $http;

        this.all = function (params) {
            return $http.get(urlBase, params && { params: params });
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

function simpleEditTemplate(add, type) {
    return "<div><form name=\"inputForm\"><input type=\"" + (type || "INPUT_TYPE") + "\" ng- class=\"'colt' + col.uid\" ui-grid-editor ng-model=\"MODEL_COL_FIELD\"" + (add || "") + " validate-cell></form></div>";
}

function checkboxTemplate() {
    return '<div class="ngCellText" ng-class="col.colIndex()"><span ng-cell-text><input type="checkbox" ng-checked="row.entity[col.field]" disabled="disabled" /></span></div>';
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
}

function exportToCsv(filename, rows) {
    var processRow = function (row) {
        var finalVal = '';
        for (var j = 0; j < row.length; j++) {
            var innerValue = row[j] === null ? '' : row[j].toString();
            if (row[j] instanceof Date) {
                innerValue = row[j].toLocaleString();
            };
            var result = innerValue.replace(/"/g, '""');
            if (result.search(/("|,|\n)/g) >= 0)
                result = '"' + result + '"';
            if (j > 0)
                finalVal += ',';
            finalVal += result;
        }
        return finalVal + '\n';
    };

    var csvFile = '';
    for (var i = 0; i < rows.length; i++) {
        csvFile += processRow(rows[i]);
    }

    var blob = new Blob(['\ufeff' + csvFile], { type: 'text/csv;charset=utf-8;' });
    if (navigator.msSaveBlob) { // IE 10+
        navigator.msSaveBlob(blob, filename);
    } else {
        var link = document.createElement("a");
        if (link.download !== undefined) { // feature detection
            // Browsers that support HTML5 download attribute
            var url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", filename);
            link.style = "visibility:hidden";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }
}

var regexIso8601 = /^(\d{4}|\+\d{6})(?:-(\d{2})(?:-(\d{2})(?:T(\d{2}):(\d{2}):(\d{2})(?:\.(\d{1,}))?(Z|([\-+])(\d{2}):(\d{2}))?)?)?)?$/;

function convertDateStringsToDates(input) {
    // Ignore things that aren't objects.
    if (typeof input !== "object") return;

    for (var key in input) {
        if (!input.hasOwnProperty(key)) continue;

        var value = input[key];
        var match;
        // Check for string properties which look like dates.
        if (typeof value === "string" && (match = value.match(regexIso8601))) {
            var milliseconds = Date.parse(match[0])
            if (!isNaN(milliseconds)) {
                input[key] = new Date(milliseconds);
            }
        } else if (typeof value === "object") {
            // Recurse into object
            convertDateStringsToDates(value);
        }
    }
}

function getTextWidth(text, font) {
    // re-use canvas object for better performance
    var canvas = getTextWidth.canvas || (getTextWidth.canvas = document.createElement("canvas"));
    var context = canvas.getContext("2d");
    context.font = font;
    var metrics = context.measureText(text);
    return metrics.width;
};

function getTextWidth2(text, fontFamily, fontSize) {
    var span;
    if (!getTextWidth2.span) {
        span = document.createElement("span");

        span.style.position = 'absolute';
        span.style.visibility = 'hidden';
        span.style.height = 'auto';
        span.style.width = 'auto';
        span.style.whiteSpace = 'nowrap';

        getTextWidth2.span = span;
        document.body.appendChild(span);
    } else {
        span = getTextWidth2.span;
    }

    span.style.fontFamily = fontFamily;
    span.style.fontSize = fontSize;
    $(span).text(text);
    return span.clientWidth;
};