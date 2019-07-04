var clientId = "f3031107-74e2-4be0-ae9a-e015c90c42c2";
function authCallback(errorDesc, token, error, tokenType) {
    if (token) {
    }
    else {
        log(error + ":" + errorDesc);
    }
}
var clientApplication = new Msal.UserAgentApplication(clientId, null, authCallback);

(function () {

    var twitterBot = angular.module('twitterBot', ['ngRoute']);

    twitterBot.config(function ($routeProvider, $httpProvider) {
        $routeProvider.when('/', {
            templateUrl: 'templates/home.html',
            controller: 'home',
            controllerAs: 'controller'
        });
    });

    twitterBot.controller("navcontroller", function ($rootScope, $http, $location, $scope) {
        $scope.loggedIn = false;
        $scope.user = "";

        $scope.login = function () {
            clientApplication.loginPopup().then(function (token) {
                console.log(token);
                var user = clientApplication.getUser();
                $scope.loggedIn = true;
                $scope.user = user.name;
                $scope.$apply();
            });
        };

        $scope.logout = function () {
            clientApplication.logout();
        };

        $scope.home = function () {
            $location.path("/");
        };
    });

    twitterBot.controller("home", function ($rootScope, $http, $location, $scope) {
    });


})();