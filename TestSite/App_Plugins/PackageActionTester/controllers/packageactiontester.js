angular.module("umbraco")
    .controller("PackageActionTester.PackageActionTesterController", function ($scope, $http) {

        $scope.bindData = function () {
            $scope.packageActionsXML = '';
            $http.get('/umbraco/backoffice/PackageActionTester/PackageActionTesterAPI/GetALL').then(function (res) {
                $scope.packageActions = res.data;
            });
        };

        $scope.selectAction = function () {
            var action = $scope.selectedPackageAction;
            if (action != '') {
                $scope.packageActionsXML += action.SampleXMl + '\r\n\r\n';
            }
        };

        $scope.install = function () {
            $scope.executePackageActions('install');
        };

        $scope.uninstall = function () {
            $scope.executePackageActions('uninstall');
        };

        $scope.executePackageActions = function (installAction) {
            var data = { InstallAction: installAction, Xml: $scope.packageActionsXML };

            $http.post('/umbraco/backoffice/PackageActionTester/PackageActionTesterAPI/ExecutePackageActions', data).then(function (res) {
                $scope.installResult = res.data;
            });
        };

        //Initialize
        $scope.bindData();
    });