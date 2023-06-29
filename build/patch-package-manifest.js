var jsonfile = require('jsonfile');
var semver = require('semver');

var file = '../src/Slimsy/wwwroot/package.manifest';

var buildVersion = process.env.semver;
var semversion = semver.valid(buildVersion);

console.log("semversion" + semversion);

jsonfile.readFile(file, function (err, project) {
	project.version = semversion;
	jsonfile.writeFile(file, project, {spaces: 2}, function(err) {
		console.error(err);
	});
})