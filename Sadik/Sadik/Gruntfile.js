module.exports = function (grunt) {

    // Project configuration.
    grunt.initConfig({
        ngtemplates: {
            ObservationsApp: {
                src: 'ViewsAngular/**/**.html',
                dest: 'Scripts/templates.js',
                options: {
                    htmlmin: { collapseWhitespace: true}
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-angular-templates');

    grunt.registerTask('combine-templates', ['ngtemplates']);
    // Default task(s).
    grunt.registerTask('default', ['ngtemplates']);

};