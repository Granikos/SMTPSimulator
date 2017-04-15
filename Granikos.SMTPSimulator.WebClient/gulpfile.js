/// <vs BeforeBuild='default' Clean='clean' />
var gulp = require('gulp');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var del = require('del');
var cleanCss = require('gulp-clean-css');
var copy = require('gulp-copy');
var bower = require('gulp-bower');
var sourcemaps = require('gulp-sourcemaps');
var replace = require('gulp-replace');
// var rewriteCSS = require('gulp-rewrite-css');
var mainBowerFiles = require('main-bower-files');

gulp.task("bower-js", ['bower-restore'], function () {
    return gulp.src(mainBowerFiles('**/*.js'))
        .pipe(gulp.dest("./Scripts/lib"));
});

gulp.task("bower-css", ['bower-restore'], function () {
    return gulp.src(mainBowerFiles('**/*.css'))
        .pipe(replace('../fonts/', ''))
        .pipe(gulp.dest("./Content/dist"));
});

gulp.task("fonts", ['bower-restore'], function () {
    var files = mainBowerFiles({
        filter: new RegExp('.*(otf|eot|svg|ttf|woff2?)$', 'i')
    });

    files.push('bower_components/bootstrap/dist/fonts/*');
    files.push('bower_components/font-awesome/fonts/*');

    return gulp.src(files)
        .pipe(gulp.dest("./Content/dist"));
});

// Synchronously delete the output style files (css / fonts)
gulp.task('clean', function (cb) {
    del(['./Content/dist', './Scripts/lib'], cb);
});

gulp.task('css', function () {
    return gulp.src('./Content/*.css')
     .pipe(concat('app.css'))
     .pipe(gulp.dest('./Content/dist'))
     .pipe(cleanCss())
     .pipe(concat('app.min.css'))
     .pipe(gulp.dest('./Content/dist'));
});

//Restore all bower packages
gulp.task('bower-restore', function () {
    return bower();
});

//Restore all bower packages
gulp.task('bower-dist', ['bower-js', 'bower-css', 'fonts'], function () {});

//Set a default tasks
gulp.task('default', ['bower-dist'], function () {

});