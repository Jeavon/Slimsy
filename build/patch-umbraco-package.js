var jsonfile = require('jsonfile');
var semver = require('semver');

var file = '../src/Slimsy/wwwroot/umbraco-package.json';

var buildVersion = process.env.semver;
var semversion = semver.valid(buildVersion);

console.log("semversion: " + semversion);

jsonfile.readFile(file, function (err, project) {

	console.log(project);

	project.version = semversion;
	jsonfile.writeFile(file, project, {spaces: 2}, function(err) {
		console.error(err);
	});
})