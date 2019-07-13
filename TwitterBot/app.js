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
        }).when('/management-portal', {
            templateUrl: 'templates/manage.html',
            controller: 'manage',
            controllerAs: 'controller'
        }).when('/add-account-redirect', {
            templateUrl: 'templates/twitter-redirect.html',
            controller: 'redirect',
            controllerAs: 'controller'
        });

        $locationProvider.html5Mode(true);
    });

    twitterBot.controller("navcontroller", function ($rootScope, $http, $location, $scope) {
        $scope.loggedIn = false;
        $scope.user = "";

        //on site load check if active token already exists in cache, then set ui auth state
        var cachedUser = clientApplication.getUser();
        if (cachedUser != null) {
            $scope.user = cachedUser.name;
            $scope.loggedIn = true;
        } 

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
    });

    twitterBot.controller("tweets", function ($rootScope, $http, $location, $scope, $routeParams) {
        $scope.tweetQueue = [];
        $scope.handles = [];
        $scope.tweetSubmitObject = {};

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

                $http.get("get-distinct-handles", config).then(function (response) {
                    $scope.handles = response.data;
                    $scope.$apply();
                });

            }, function (error) {
                clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                }, function (error) {

                });
            });
    });

    twitterBot.controller("manage", function ($rootScope, $http, $location, $scope, $routeParams, $window) {

        $scope.twitterSignIn = function () {
            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get("twitter-auth-token", config).then(function (response) {
                        $window.location.href = "https://api.twitter.com/oauth/authenticate?oauth_token=" + response.data;
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });
        };
    });

    twitterBot.controller("redirect", function ($http, $location, $scope, $routeParams) {

        //parses url?oauth_token=TOKEN&oauth_verifier=TOKEN
        var oauthToken = $routeParams.oauth_token;
        var oauthVerifier = $routeParams.oauth_verifier;
        console.log(oauthToken)
        console.log(oauthVerifier)

    });

})();