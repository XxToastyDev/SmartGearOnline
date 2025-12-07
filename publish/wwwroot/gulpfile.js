const gulp = require("gulp");
const sass = require("gulp-sass")(require("sass"));
const concat = require("gulp-concat");
const uglify = require("gulp-uglify");
const rename = require("gulp-rename");

// Compile SASS
gulp.task("sass", function() {
    return gulp.src("wwwroot/scss/**/*.scss")
        .pipe(sass({ outputStyle: "compressed" }).on("error", sass.logError))
        .pipe(rename("site.min.css"))
        .pipe(gulp.dest("wwwroot/css"));
});

// Minify JS
gulp.task("js", function() {
    return gulp.src("wwwroot/js/**/*.js")
        .pipe(concat("site.bundle.js"))
        .pipe(uglify())
        .pipe(gulp.dest("wwwroot/js"));
});

// Watch files
gulp.task("watch", function() {
    gulp.watch("wwwroot/scss/**/*.scss", gulp.series("sass"));
    gulp.watch("wwwroot/js/**/*.js", gulp.series("js"));
});

// Default task
gulp.task("default", gulp.series("sass", "js", "watch"));
