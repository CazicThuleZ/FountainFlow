"use strict";

const { series, src, dest, parallel, watch } = require("gulp");
const autoprefixer = require("gulp-autoprefixer");
const concat = require("gulp-concat");
const CleanCSS = require("gulp-clean-css");
const npmdist = require("gulp-npm-dist");
const rename = require("gulp-rename");
const rtlcss = require("gulp-rtlcss");
const sourcemaps = require("gulp-sourcemaps");
const sass = require("gulp-sass")(require("sass"));
const uglify = require("gulp-uglify");


const paths = {
    baseSrcAssets: "wwwroot/",   // source assets directory
    baseDistAssets: "wwwroot/",  // build assets directory
};


const vendor = function () {

    // vendor.min.css
    const outCSS = paths.baseDistAssets + "css/";

    src([
        "./node_modules/select2/dist/css/select2.min.css",
        "./node_modules/daterangepicker/daterangepicker.css",
        "./node_modules//bootstrap-touchspin/dist/jquery.bootstrap-touchspin.min.css",
        "./node_modules/bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css",
        "./node_modules/bootstrap-timepicker/css/bootstrap-timepicker.min.css",
        "./node_modules/flatpickr/dist/flatpickr.min.css",
    ])
        .pipe(concat("vendor.css"))
        .pipe(CleanCSS())
        .pipe(rename({ suffix: ".min" }))
        .pipe(dest(outCSS));

    const outJs = paths.baseDistAssets + "js/";

    // vendor.min.js
    src([
        "./node_modules/jquery/dist/jquery.min.js",
        "./node_modules/bootstrap/dist/js/bootstrap.bundle.js",
        "./node_modules/simplebar/dist/simplebar.min.js",
        "./node_modules/lucide/dist/umd/lucide.min.js",

        "./node_modules/select2/dist/js/select2.min.js",
        "./node_modules/daterangepicker/moment.min.js",
        "./node_modules/daterangepicker/daterangepicker.js",
        "./node_modules/bootstrap-datepicker/dist/js/bootstrap-datepicker.min.js",
        "./node_modules/bootstrap-timepicker/js/bootstrap-timepicker.min.js",
        "./node_modules/jquery-mask-plugin/dist/jquery.mask.min.js",
        "./node_modules/bootstrap-touchspin/dist/jquery.bootstrap-touchspin.min.js",
        "./node_modules/bootstrap-maxlength/dist/bootstrap-maxlength.min.js",
        "./node_modules/handlebars/dist/handlebars.min.js",
        "./node_modules/typeahead.js/dist/typeahead.bundle.min.js",
        "./node_modules/flatpickr/dist/flatpickr.min.js",
    ])

    
        .pipe(concat("vendor.js"))
        .pipe(uglify())
        .pipe(rename({ suffix: ".min" }))
        .pipe(dest(outJs));

    const out = paths.baseDistAssets + "vendor/";
    return src(npmdist(), { base: "./node_modules" })
        .pipe(rename(function (path) {
            path.dirname = path.dirname.replace(/\/dist/, '').replace(/\\dist/, '');
        }))
        .pipe(dest(out));
};

// CSS
const scss = function () {
    const out = paths.baseDistAssets + "css/";

    src(paths.baseSrcAssets + `scss/app-saas.scss`)
        .pipe(sourcemaps.init())
        .pipe(sass.sync().on('error', sass.logError)) // scss to css
        .pipe(
            autoprefixer({
                overrideBrowserslist: ["last 2 versions"],
            })
        )
        .pipe(dest(out))  // app.css
        .pipe(CleanCSS())
        .pipe(rename({ suffix: ".min" })) // app.min.css
        .pipe(sourcemaps.write("./")) // source maps
        .pipe(dest(out));

    // Generate RTL
    return src(paths.baseSrcAssets + `scss/app-saas.scss`)
        .pipe(sourcemaps.init())
        .pipe(sass.sync().on('error', sass.logError)) // scss to css
        .pipe(
            autoprefixer({
                overrideBrowserslist: ["last 2 versions"],
            })
        )
        .pipe(rtlcss())
        .pipe(rename({ suffix: "-rtl" }))
        .pipe(dest(out))
        .pipe(CleanCSS())
        .pipe(rename({ suffix: ".min" }))
        .pipe(sourcemaps.write("./")) // source maps
        .pipe(dest(out));
};

// Icons 
const icons = function () {
    const out = paths.baseDistAssets + "css/";
    return src(paths.baseSrcAssets + "scss/icons.scss")
        .pipe(sourcemaps.init())
        .pipe(sass.sync().on('error', sass.logError)) // scss to css
        .pipe(
            autoprefixer({
                overrideBrowserslist: ["last 2 versions"],
            })
        )
        .pipe(dest(out))
        .pipe(CleanCSS())
        .pipe(rename({ suffix: ".min" }))
        .pipe(sourcemaps.write("./")) // source maps
        .pipe(dest(out));
};

function watchFiles() {
    watch(paths.baseSrcAssets + "scss/icons.scss", series(icons));
    watch([paths.baseSrcAssets + "scss/**/*.scss", "!" + paths.baseSrcAssets + "scss/icons.scss"], series(scss));
}

// Production Tasks
exports.default = series(
    vendor,
    parallel(scss, icons),
    parallel(watchFiles)
);


// Build Tasks
exports.build = series(
    vendor,
    parallel(scss, icons)
);