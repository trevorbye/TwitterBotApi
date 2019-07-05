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

    twitterBot.config(function ($routeProvider, $httpProvider, $locationProvider) {
        $routeProvider.when('/', {
            templateUrl: 'templates/home.html',
            controller: 'home',
            controllerAs: 'controller'
        });

        $locationProvider.html5Mode(true);
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
        $scope.manage = function () {
            $location.path("/management-portal");
        }

        $scope.tweet = function () {
            $location.path("/tweet-portal");
        }

        $scope.httpTest = function () {
            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get("current-principal-admin", config).then(function (response) {
                        console.log(response.data)
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {
                        
                    }, function (error) {

                    });
                })

            
        }
    });


})();