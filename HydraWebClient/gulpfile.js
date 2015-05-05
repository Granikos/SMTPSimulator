var gulp = require('gulp');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var del = require('del');
var minifyCSS = require('gulp-minify-css');
var copy = require('gulp-copy');
var bower = require('gulp-bower');
var sourcemaps = require('gulp-sourcemaps');

var config = {
    jquerysrc: [
        'bower_components/jquery/dist/jquery.min.js',
    ],
    jquerybundle: 'Scripts/jquery-bundle.min.js',

    bootstrapsrc: [
        'bower_components/bootstrap/dist/js/bootstrap.min.js',
        'bower_components/bootstrap-dialog/dist/js/bootstrap-dialog.min.js'
    ],
    bootstrapbundle: 'Scripts/bootstrap-bundle.min.js',

    angularsrc: [
        'bower_components/angular/angular.min.js',
        'bower_components/angular-aria/angular-aria.min.js',
        'bower_components/angular-route/angular-route.min.js',
        'bower_components/angular-ui/build/angular-ui.min.js',
        'bower_components/angular-ui-grid/ui-grid.min.js',
        'bower_components/angular-bootstrap/ui-bootstrap.min.js'
    ],
    angularbundle: 'Scripts/angular-bundle.min.js',

    papaparsesrc: ['bower_components/papaparse/papaparse.min.js'],
    papaparsebundle: 'Scripts/papaparse.min.js',

    bootstrapcss: [
        'bower_components/bootstrap/dist/css/bootstrap.min.css',
        'bower_components/bootstrap-dialog/dist/css/bootstrap-dialog.min.css'
    ],
    boostrapfonts: 'bower_components/bootstrap/dist/fonts/*.*',

    angularuicss: [
        'bower_components/angular-ui/build/angular-ui.min.css',
        'bower_components/angular-ui-grid/ui-grid.min.css'
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

gulp.task('jquery-bundle', ['clean-vendor-scripts', 'bower-restore'], function () {
    return gulp.src(config.jquerysrc)
     .pipe(concat('jquery-bundle.min.js'))
     .pipe(gulp.dest('Scripts'));
});

gulp.task('bootstrap-bundle', ['clean-vendor-scripts', 'bower-restore'], function () {
    return gulp.src(config.bootstrapsrc)
     .pipe(sourcemaps.init())
     .pipe(concat('bootstrap-bundle.min.js'))
     .pipe(sourcemaps.write('maps'))
     .pipe(gulp.dest('Scripts'));
});

gulp.task('angular-bundle', ['clean-vendor-scripts', 'bower-restore'], function () {
    return gulp.src(config.angularsrc)
        .pipe(sourcemaps.init())
        .pipe(uglify())
        .pipe(concat('angular-bundle.min.js'))
        .pipe(sourcemaps.write('maps'))
        .pipe(gulp.dest('Scripts'));
});

gulp.task('papaparse', ['clean-vendor-scripts', 'bower-restore'], function () {
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

gulp.task('bootstrap-css', ['clean-styles', 'bower-restore'], function () {
    return gulp.src(config.bootstrapcss)
     .pipe(concat('bootstrap.css'))
     .pipe(gulp.dest(config.cssout))
     .pipe(minifyCSS())
     .pipe(concat('bootstrap.min.css'))
     .pipe(gulp.dest(config.cssout));
});

gulp.task('angular-ui-css', ['clean-styles', 'bower-restore'], function () {
    return gulp.src(config.angularuicss)
     .pipe(concat('angular-ui.css'))
     .pipe(gulp.dest(config.cssout))
     .pipe(minifyCSS())
     .pipe(concat('angular-ui.min.css'))
     .pipe(gulp.dest(config.cssout));
});

gulp.task('app-css', ['clean-styles', 'bower-restore'], function () {
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

gulp.task('fonts', ['clean-styles', 'bower-restore'], function () {
    return gulp.src(config.boostrapfonts)
        .pipe(gulp.dest(config.fontsout));
});

// Combine and minify css files and output fonts
gulp.task('styles', ['css', 'fonts'], function () {

});

//Restore all bower packages
gulp.task('bower-restore', function() {
    return bower();
});

//Set a default tasks
gulp.task('default', ['vendor-scripts', 'styles'], function () {

});