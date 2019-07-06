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
        }).when('/tweet-portal', {
            templateUrl: 'templates/tweets.html',
            controller: 'tweets',
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
                $rootScope.authToken = token;
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
    });

    twitterBot.controller("tweets", function ($rootScope, $http, $location, $scope) {
        $scope.tweetQueue = null;

        var config = {
            headers: {
                'Content-type': 'application/json',
                'Authorization': 'Bearer ' + $rootScope.authToken
            }
        };

        $http.get("get-user-tweet-queue", config).then(function (response) {
            $scope.tweetQueue = response.data;
        });
        /*
        clientApplication.acquireTokenSilent([clientId])
            .then(function (token) {
                var config = {
                    headers: {
                        'Content-type': 'application/json',
                        'Authorization': 'Bearer ' + token
                    }
                };

                $http.get("get-user-tweet-queue", config).then(function (response) {
                    $scope.tweetQueue = response.data;
                    $scope.$apply();
                });
            }, function (error) {
                clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                }, function (error) {

                });
            });
            */
    });


})();