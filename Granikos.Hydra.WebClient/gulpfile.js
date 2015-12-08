/// <vs BeforeBuild='default' Clean='clean' />
var gulp = require('gulp');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var del = require('del');
var minifyCSS = require('gulp-minify-css');
var copy = require('gulp-copy');
var bower = require('gulp-bower');
var sourcemaps = require('gulp-sourcemaps');
// var rewriteCSS = require('gulp-rewrite-css');

var config = {
    jquerysrc: [
        'bower_components/jquery/dist/jquery.js',
        'bower_components/moment/moment.js'
    ],
    jquerybundle: 'Scripts/jquery-bundle.min.js',

    bootstrapsrc: [
        'bower_components/bootstrap/dist/js/bootstrap.min.js',
        'bower_components/bootstrap-dialog/dist/js/bootstrap-dialog.min.js',
        // 'bower_components/bootstrap-select/dist/js/bootstrap-select.js'
    ],
    bootstrapbundle: 'Scripts/bootstrap-bundle.min.js',

    angularsrc: [
        'bower_components/angular/angular.js',
        'bower_components/angular-aria/angular-aria.js',
        'bower_components/angular-route/angular-route.js',
        'bower_components/angular-ui/build/angular-ui.js',
        'bower_components/angular-ui-grid/ui-grid.js',
        'bower_components/angular-bootstrap/ui-bootstrap.js',
        'bower_components/angular-bootstrap/ui-bootstrap-tpls.js',
        // 'bower_components/angular-bootstrap-select/build/angular-bootstrap-select.js',
        'bower_components/angular-enum-flag-directive/angular-enum-flag-directive.js',
        'bower_components/checklist-model/checklist-model.js',
        'bower_components/ng-file-upload/ng-file-upload.js',
        'bower_components/angular-moment-duration/src/angular-moment-duration.js',
        'bower_components/ngTouchSpin/src/js/ngTouchSpin.js',
        'bower_components/angular-rangeslider/angular.rangeSlider.js',
        'bower_components/ui-select/dist/select.js',
        'bower_components/angular-xeditable/dist/js/xeditable.min.js'
    ],
    angularbundle: 'Scripts/angular-bundle.min.js',

    papaparsesrc: ['bower_components/papaparse/papaparse.min.js'],
    papaparsebundle: 'Scripts/papaparse.min.js',

    bootstrapcss: [
        'bower_components/bootstrap/dist/css/bootstrap.min.css',
        'bower_components/bootstrap-dialog/dist/css/bootstrap-dialog.min.css',
        'bower_components/font-awesome/css/font-awesome.min.css',
        // 'bower_components/bootstrap-select/dist/css/bootstrap-select.css'
    ],
    boostrapfonts: [
        'bower_components/bootstrap/dist/fonts/*.*',
        'bower_components/font-awesome/fonts/*.*'
    ],

    angularuicss: [
        'bower_components/angular-ui/build/angular-ui.min.css',
        'bower_components/angular-ui-grid/ui-grid.min.css',
        'bower_components/angular-rangeslider/angular.rangeSlider.css',
        'bower_components/ui-select/dist/select.css',
        'bower_components/angular-xeditable/dist/css/xeditable.css'
    ],
    angularuifonts: [
        'bower_components/angular-ui-grid/ui-grid.eot',
        'bower_components/angular-ui-grid/ui-grid.svg',
        'bower_components/angular-ui-grid/ui-grid.ttf',
        'bower_components/angular-ui-grid/ui-grid.woff'
    ],

    appcss: 'Content/Site.css',
    fontsout: 'Content/dist/fonts',
    cssout: 'Content/dist/css'

}

gulp.task('clean-vendor-scripts', function (cb) {
    del([config.jquerybundle,
         config.bootstrapbundle,
         config.angularbundle,
         config.papaparsebundle], cb);
});

gulp.task('jquery-bundle', ['bower-restore'], function () {
    return gulp.src(config.jquerysrc)
     .pipe(sourcemaps.init())
        .pipe(uglify())
     .pipe(concat('jquery-bundle.min.js'))
     .pipe(sourcemaps.write('maps'))
     .pipe(gulp.dest('Scripts'));
});

gulp.task('bootstrap-bundle', ['bower-restore'], function () {
    return gulp.src(config.bootstrapsrc)
     .pipe(sourcemaps.init())
        .pipe(uglify())
     .pipe(concat('bootstrap-bundle.min.js'))
     .pipe(sourcemaps.write('maps'))
     .pipe(gulp.dest('Scripts'));
});

gulp.task('angular-bundle', ['bower-restore'], function () {
    return gulp.src(config.angularsrc)
        .pipe(sourcemaps.init())
        .pipe(uglify())
        .pipe(concat('angular-bundle.min.js'))
        .pipe(sourcemaps.write('maps'))
        .pipe(gulp.dest('Scripts'));
});

gulp.task('papaparse', ['bower-restore'], function () {
    return gulp.src(config.papaparsesrc)
        .pipe(sourcemaps.init())
        .pipe(uglify())
        .pipe(concat('papaparse.min.js'))
        .pipe(sourcemaps.write('maps'))
        .pipe(gulp.dest('Scripts'));
});

// Combine and the vendor files from bower into bundles (output to the Scripts folder)
gulp.task('vendor-scripts', ['jquery-bundle', 'bootstrap-bundle', 'angular-bundle', 'papaparse'], function () {

});

// Synchronously delete the output style files (css / fonts)
gulp.task('clean-styles', function (cb) {
    del([config.fontsout,
              config.cssout],cb);
});

gulp.task('bootstrap-css', ['bower-restore'], function () {
    return gulp.src(config.bootstrapcss)
     .pipe(concat('bootstrap.css'))
     .pipe(gulp.dest(config.cssout))
     .pipe(minifyCSS())
     .pipe(concat('bootstrap.min.css'))
     .pipe(gulp.dest(config.cssout));
});

gulp.task('angular-ui-css', ['bower-restore'], function () {
    return gulp.src(config.angularuicss)
     // .pipe(rewriteCSS({ destination: config.fontsout }))
     .pipe(concat('angular-ui.css'))
     .pipe(gulp.dest(config.cssout))
     .pipe(minifyCSS())
     .pipe(concat('angular-ui.min.css'))
     .pipe(gulp.dest(config.cssout));
});

gulp.task('app-css', ['bower-restore'], function () {
    return gulp.src(config.appcss)
     .pipe(concat('app.css'))
     .pipe(gulp.dest(config.cssout))
     .pipe(minifyCSS())
     .pipe(concat('app.min.css'))
     .pipe(gulp.dest(config.cssout));
});

// Combine and minify css files and output fonts
gulp.task('css', ['bootstrap-css', 'angular-ui-css', 'app-css'], function () {

});

gulp.task('fonts', ['bower-restore'], function () {

    // return gulp.src([].concat(config.boostrapfonts, config.angularuifonts))
    //     .pipe(gulp.dest(config.fontsout));
    gulp.src(config.boostrapfonts)
        .pipe(gulp.dest(config.fontsout));
    return gulp.src(config.angularuifonts)
        .pipe(gulp.dest(config.cssout));
});

// Combine and minify css files and output fonts
gulp.task('styles', ['css', 'fonts'], function () {

});

//Restore all bower packages
gulp.task('bower-restore', function() {
    return bower();
});


//Set a default tasks
gulp.task('clean', ['clean-vendor-scripts', 'clean-styles'], function () {

});

//Set a default tasks
gulp.task('default', ['vendor-scripts', 'styles'], function () {

});