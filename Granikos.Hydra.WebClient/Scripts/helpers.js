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



function calculateColumnAutoWidths(columns, data, fontFamily, fontSize, hasMenu) {
    var maximumsPixels = {};

    var cell = $('<div class="ui-grid-cell-contents" />');
    var padding = cell.innerWidth() - cell.width();

    var buttonWidth = 0;

    if (hasMenu) {
        // TODO: Cleanup
        var menuButton = $('<div class="ui-grid-column-menu-button" style="width: 2.2em"></div>');
        buttonWidth = menuButton.outerWidth();
    }

    var detector = new FontDetector();
    var fonts = fontFamily.split(/,/);
    var font;
    for (var i = 0; i < fonts.length; i++) {
        var f = fonts[i];
        if (f.indexOf('"') === 0) f = f.substr(1, f.length - 2);
        if (detector.detect(f)) {
            font = f;
            break;
        }
    }

    for (var i = 0; i < data.length; i++) {
        for (var j = 0; j < columns.length; j++) {
            var field = columns[j].field;
            if (field !== undefined) {
                var name = columns[j].name || columns[j].field;
                var v = new String(eval('data[i].' + field));

                var pixels = getTextWidth(v, fontSize + ' ' + font);
                if ((maximumsPixels[name] || 0) < pixels)
                    maximumsPixels[name] = pixels;
            }
        }
    }

    for (var j = 0; j < columns.length; j++) {
        var name = columns[j].name || columns[j].field;
        var text = columns[j].displayName || columns[j].name;

        var pixels = getTextWidth(text, fontSize + ' ' + font);
        pixels += buttonWidth;
        if ((maximumsPixels[name] || 0) < pixels)
            maximumsPixels[name] = pixels;

    }

    for (var j = 0; j < columns.length; j++) {
        var name = columns[j].name || columns[j].field;
        var pixels = maximumsPixels[name];
        var minWidth = Math.ceil(pixels + padding + 1);
        if ((columns[j].absMinWidth || 0) > minWidth) {
            minWidth = columns[j].absMinWidth;
        }
        columns[j].minWidth = minWidth;
    }
}

function fontawesomeFileIconClassForMimeType( mimeType ) {
    var classes = {
        'image' : 'fa-file-image-o',
        'audio' : 'fa-file-audio-o',
        'video' : 'fa-file-video-o',
        'application/pdf' : 'fa-file-pdf-o',
        'text/plain' : 'fa-file-text-o',
        'text/html' : 'fa-file-code-o',
        'application/json' : 'fa-file-code-o',
        'application/gzip' : 'fa-file-archive-o',
        'application/zip' : 'fa-file-archive-o',
        'application/octet-stream' : 'fa-file-o'
    };

    if (classes[mimeType]) return classes[mimeType];

    mimeType = mimeType.split('/')[0];

    if (classes[mimeType]) return classes[mimeType];

    return "fa-file-o";
}

function fontawesomeFileIconClassForFileExtension(fileName) {
    var exts = {
        'pdf': 'fa-file-pdf-o',

        'txt': 'fa-file-text-o',

        'doc': 'fa-file-word-o',
        'docx': 'fa-file-word-o',

        'xls': 'fa-file-excel-o',
        'xlsx': 'fa-file-excel-o',

        'ppt': 'fa-file-powerpoint-o',
        'pptx': 'fa-file-powerpoint-o',

        'bmp': 'fa-file-image-o',
        'gif': 'fa-file-image-o',
        'jpeg': 'fa-file-image-o',
        'jpg': 'fa-file-image-o',
        'png': 'fa-file-image-o',

        'bz2': 'fa-file-archive-o',
        'dmg': 'fa-file-archive-o',
        'gz': 'fa-file-archive-o',
        'gzip': 'fa-file-archive-o',
        'iso': 'fa-file-archive-o',
        'rar': 'fa-file-archive-o',
        'tar': 'fa-file-archive-o',
        'tgz': 'fa-file-archive-o',
        'zip': 'fa-file-archive-o',
        '7z': 'fa-file-archive-o',

        'mp3': 'fa-file-audio-o',
        'wav': 'fa-file-audio-o',
        'flac': 'fa-file-audio-o',
        'm4a': 'fa-file-audio-o',
        'cda': 'fa-file-audio-o',
        'wma': 'fa-file-audio-o',
        'ogg': 'fa-file-audio-o',
        'midi': 'fa-file-audio-o',

        'mov': 'fa-file-video-o',
        'webm': 'fa-file-video-o',
        'mkv': 'fa-file-video-o',
        'flv': 'fa-file-video-o',
        'ogv': 'fa-file-video-o',
        'vob': 'fa-file-video-o',
        'avi': 'fa-file-video-o',
        'wmv': 'fa-file-video-o',
        'mp4': 'fa-file-video-o',
        'mpg': 'fa-file-video-o',
        'mpeg': 'fa-file-video-o',
        'mp2': 'fa-file-video-o',
        'mpv': 'fa-file-video-o',
        'm4v': 'fa-file-video-o',
        '3gp': 'fa-file-video-o',
        '3g2': 'fa-file-video-o',

        'java': 'fa-file-code-o',
        'c': 'fa-file-code-o',
        'cpp': 'fa-file-code-o',
        'h': 'fa-file-code-o',
        'hpp': 'fa-file-code-o',
        'py': 'fa-file-code-o',
        'js': 'fa-file-code-o',
        'cs': 'fa-file-code-o',
        'aspx': 'fa-file-code-o',
        'php': 'fa-file-code-o',
        'html': 'fa-file-code-o',
        'xml': 'fa-file-code-o',
        'htm': 'fa-file-code-o',
        'pl': 'fa-file-code-o',
        'bas': 'fa-file-code-o',
        'sh': 'fa-file-code-o',
        'fs': 'fa-file-code-o',
        'dtd': 'fa-file-code-o',
        'xsl': 'fa-file-code-o',
        'bat': 'fa-file-code-o',
        'cgi': 'fa-file-code-o',
        'xsd': 'fa-file-code-o',
        'cshtml': 'fa-file-code-o',
        'lua': 'fa-file-code-o',
        'cmake': 'fa-file-code-o',
        'csx': 'fa-file-code-o',
        'php3': 'fa-file-code-o',
        'cc': 'fa-file-code-o',
        'csproj': 'fa-file-code-o',
        'vb': 'fa-file-code-o',
        'vbproj': 'fa-file-code-o',
        'less': 'fa-file-code-o',
        'xslt': 'fa-file-code-o',
        'xaml': 'fa-file-code-o',
        'sql': 'fa-file-code-o',
        'ps1': 'fa-file-code-o'
    };

    var parts = fileName.split('.');

    return exts[parts[parts.length - 1]] || "fa-file-o";
}

function disableClickedButton(btn) {
    btn = $(btn);
    var icon = btn.find('i.glyphicon');

    var exclusive = btn.hasClass('exclusive-action');

    var oldClass = icon.attr("class");
    icon.attr('class', 'glyphicon glyphicon-spin glyphicon-refresh');

    btn.prop('disabled', true);
    btn.addClass('disabled');

    var others;

    if (exclusive) {
        others = btn.parent('.btn-group').find('button.exclusive-action');
        others.prop('disabled', true);
    }

    return function () {
        btn.prop('disabled', false);
        btn.removeClass('disabled');
        icon.attr('class', oldClass);

        if (others) {
            others.prop('disabled', false);
        }
    };
}

