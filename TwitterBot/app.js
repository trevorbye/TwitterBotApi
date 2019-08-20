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
            templateUrl: 'templates/login.html',
            controller: 'login',
            controllerAs: 'controller'
        }).when('/home', {
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
        }).when('/not-found', {
            templateUrl: 'templates/404.html',
            controller: 'redirect',
            controllerAs: 'controller'
        }).otherwise({
                redirectTo: "/not-found"
        });

        $locationProvider.html5Mode(true);
    });

    // locking down templates. "real" auth is on back end, this just makes sure unauthenticated users
    // don't see empty html templates
    twitterBot.run(['$rootScope', '$location', "$window", function ($rootScope, $location, $window) {
        $rootScope.$on('$routeChangeStart', function (event) {

            var deployType = "test";
            var redirectUri;

            if (deployType == "test") {
                redirectUri = "http://localhost:52937/";
            } else {
                redirectUri = "";
            }
           
            if ($location.path() != "/") {
                if ($rootScope.loggedIn == false) {
                    console.log('DENY');
                    event.preventDefault();
                    //redirect actual window rather than route, otherwise AAD callback uri won't match
                    $window.location.href = redirectUri;
                }
            }
        });
    }]);

    twitterBot.controller("login", function ($rootScope, $http, $location, $scope) {

        //on site load check if active token already exists in cache, then set ui auth state
        var cachedUser = clientApplication.getUser();
        if (cachedUser != null) {
            $location.path("/home");
            $scope.$apply();
            $rootScope.$apply();
        }

        $scope.login = function () {
            clientApplication.loginPopup().then(function (token) {
                var user = clientApplication.getUser();
                $rootScope.loggedIn = true;
                $rootScope.user = user.name;
                
                $location.path("/home");
                $scope.$apply();
                $rootScope.$apply();
            }).catch(function (error) {
                console.log(error);
            });
        };
    });

    twitterBot.controller("navcontroller", function ($rootScope, $http, $location, $scope) {
        $rootScope.loggedIn = false;
        $rootScope.user = "";

        //on site load check if active token already exists in cache, then set ui auth state
        var cachedUser = clientApplication.getUser();
        if (cachedUser != null) {
            $rootScope.user = cachedUser.name;
            $rootScope.loggedIn = true;
        } 

        $scope.logout = function () {
            clientApplication.logout();
        };

        $scope.home = function () {
            $location.path("/");
        };

        $scope.manage = function () {
            $location.path("/management-portal");
        };

        $scope.tweet = function () {
            $location.path("/tweet-portal");
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
        $scope.error = false;
        $scope.errorMessage = "";

        clientApplication.acquireTokenSilent([clientId])
            .then(function (token) {
                var config = {
                    headers: {
                        'Content-type': 'application/json',
                        'Authorization': 'Bearer ' + token
                    }
                };

                $http.get("api/get-utc-now").then(function (utcRes) {
                    $http.get("api/get-user-tweet-queue", config).then(function (response) {

                        var utcNow = Date.parse(utcRes.data);
                        var tweets = response.data;
                        var index;

                        for (index = 0; index < tweets.length; ++index) {
                            var tweet = tweets[index]
                            var utcTweetTime = Date.parse(tweet.CreatedTime + "Z");
                            var elapsedTimeSeconds = (utcNow - utcTweetTime) / 1000;

                            var humanReadableTime;
                            if (elapsedTimeSeconds < 60) {
                                humanReadableTime = "Just now";
                            } else if (elapsedTimeSeconds < 3600) {
                                var elapsedMinRounded = Math.round(elapsedTimeSeconds / 60);
                                humanReadableTime = elapsedMinRounded.toString() + " min ago";
                            } else if (elapsedTimeSeconds < 86400) {
                                var elapsedHoursRounded = Math.round(elapsedTimeSeconds / 3600);
                                humanReadableTime = elapsedHoursRounded.toString() + " hours ago";
                            } else {
                                var elapsedDaysRounded = Math.round(elapsedTimeSeconds / 86400);
                                humanReadableTime = elapsedDaysRounded.toString() + " days ago";
                            }
                            tweet.CreatedTime = humanReadableTime;
                            
                            var statusTimeUtc = tweet.ScheduledStatusTime + "Z";
                            tweet.ScheduledStatusTime = new Date(statusTimeUtc).toLocaleString();
                        }

                        $scope.tweetQueue = tweets;
                        $scope.apply;
                    });
                });

                $http.get("api/get-distinct-handles", config).then(function (response) {
                    $scope.handles = response.data;
                    $scope.apply;
                });

            }, function (error) {
                clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                }, function (error) {

                });
            });

        $scope.submitTweet = function () {
            // client side validation for null fields
            if ($scope.tweetSubmitObject.handle == null) {
                $scope.error = true;
                $scope.errorMessage = "Handle cannot be empty.";
            } else if ($scope.tweetSubmitObject.body == null) {
                $scope.error = true;
                $scope.errorMessage = "Tweet body cannot be empty.";
            } else if ($scope.tweetSubmitObject.date == null) {
                $scope.error = true;
                $scope.errorMessage = "Date cannot be empty.";
            } else if ($scope.tweetSubmitObject.time == null) {
                $scope.error = true;
                $scope.errorMessage = "Time cannot be empty.";
            }

            if ($scope.tweetSubmitObject.body.length > 280) {
                $scope.error = true;
                $scope.errorMessage = "Tweet body exceeds 280 character limit.";
            }

            var date = new Date($scope.tweetSubmitObject.date);
            var time = new Date($scope.tweetSubmitObject.time);
            date.setHours(time.getHours(), time.getMinutes());

            if (date < Date.now()) {
                $scope.error = true;
                $scope.errorMessage = "Scheduled Tweet time must be in the future.";
            }

            var tweetQueueObject = {
                "TwitterHandle": $scope.tweetSubmitObject.handle,
                "ScheduledStatusTime": date,
                "StatusBody": $scope.tweetSubmitObject.body
            }

            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.post("api/post-new-tweet", tweetQueueObject, config).then(function (response) {
                        var tweetObject = response.data;
                        tweetObject.CreatedTime = "Just now";
                        tweetObject.ScheduledStatusTime = date.toLocaleString();

                        $scope.tweetQueue.unshift(tweetObject);
                        $scope.tweetSubmitObject = {};
                        $scope.apply;
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });

        };

        $scope.cancelTweet = function (tweetId, index) {

            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.delete("api/delete-tweet?id=" + tweetId, config).then(function (response) {
                        $scope.tweetQueue.splice(index, 1);
                        $scope.apply;
                    });

                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

    });

    twitterBot.controller("manage", function ($http, $location, $scope, $window) {
        $scope.handles = [];
        $scope.tweetQueue = [];

        clientApplication.acquireTokenSilent([clientId])
            .then(function (token) {
                var config = {
                    headers: {
                        'Content-type': 'application/json',
                        'Authorization': 'Bearer ' + token
                    }
                };

                $http.get("api/get-user-twitter-accounts", config).then(function (response) {
                    var accounts = response.data;
                    var index = 0;
                    var uiList = [];
                    for (index = 0; index < accounts.length; index++) {
                        var account = accounts[index];
                        var uiHandleObject = { "handle": account.TwitterHandle, "settings": false, "retweet": account.IsAutoRetweetEnabled };
                        uiList.push(uiHandleObject);
                    }

                    $scope.handles = uiList;
                });

                $http.get("api/get-utc-now").then(function (utcRes) {
                    $http.get("api/get-handles-tweet-queue", config).then(function (response) {

                        var utcNow = Date.parse(utcRes.data);
                        var tweets = response.data;
                        var index;

                        for (index = 0; index < tweets.length; ++index) {
                            var tweet = tweets[index]
                            var utcTweetTime = Date.parse(tweet.CreatedTime + "Z");
                            var elapsedTimeSeconds = (utcNow - utcTweetTime) / 1000;

                            var humanReadableTime;
                            if (elapsedTimeSeconds < 60) {
                                humanReadableTime = "Just now";
                            } else if (elapsedTimeSeconds < 3600) {
                                var elapsedMinRounded = Math.round(elapsedTimeSeconds / 60);
                                humanReadableTime = elapsedMinRounded.toString() + " min ago";
                            } else if (elapsedTimeSeconds < 86400) {
                                var elapsedHoursRounded = Math.round(elapsedTimeSeconds / 3600);
                                humanReadableTime = elapsedHoursRounded.toString() + " hours ago";
                            } else {
                                var elapsedDaysRounded = Math.round(elapsedTimeSeconds / 86400);
                                humanReadableTime = elapsedDaysRounded.toString() + " days ago";
                            }
                            tweet.CreatedTime = humanReadableTime;

                            var statusTimeUtc = tweet.ScheduledStatusTime + "Z";
                            tweet.ScheduledStatusTime = new Date(statusTimeUtc).toLocaleString();
                        }

                        $scope.tweetQueue = tweets;
                        $scope.apply;
                    });
                });
                
            }, function (error) {
                clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                }, function (error) {

                });
            });

        $scope.expandSettings = function (index) {
            $scope.handles[index].settings = true;
        };

        $scope.hideSettings = function (index) {
            $scope.handles[index].settings = false;
        };

        $scope.enableAutoTweets = function (handle, index) {
            $scope.handles[index].retweet = true;

            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get("api/enable-auto-tweets?handle=" + handle, config).then(function (response) {
                        
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.disableAutoTweets = function (handle, index) {
            $scope.handles[index].retweet = false;

            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get("api/disable-auto-tweets?handle=" + handle, config).then(function (response) {
                        
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.deleteAccount = function (handle, index) {
            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.delete("api/delete-twitter-account?handle=" + handle, config).then(function (response) {
                        $scope.handles.splice(index, 1);
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.approveTweet = function (tweetId, index) {
            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get("api/approve-or-cancel?approveById=" + tweetId + "&cancelById=0", config).then(function (response) {
                        $scope.tweetQueue[index].IsApprovedByHandle = true;
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.cancelTweet = function (tweetId, index) {
            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get("api/approve-or-cancel?cancelById=" + tweetId + "&approveById=0", config).then(function (response) {
                        $scope.tweetQueue[index].IsApprovedByHandle = false;
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.twitterSignIn = function () {
            clientApplication.acquireTokenSilent([clientId])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get("api/twitter-auth-token", config).then(function (response) {
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
        $scope.waiting = true;
        $scope.handle = "";
        $scope.success = false;
        $scope.failure = false;

        //parses url?oauth_token=TOKEN&oauth_verifier=TOKEN
        var oauthToken = $routeParams.oauth_token;
        var oauthVerifier = $routeParams.oauth_verifier;
        console.log(oauthToken)
        console.log(oauthVerifier)

        var params = {
            token: oauthToken,
            verifier: oauthVerifier
        };

        clientApplication.acquireTokenSilent([clientId])
            .then(function (token) {
                var config = {
                    headers: {
                        'Content-type': 'application/json',
                        'Authorization': 'Bearer ' + token
                    },
                    params: params
                };

                $http.get("api/convert-to-access-token", config).then(function (response) {
                    $scope.handle = response.data;
                    $scope.waiting = false;
                    $scope.success = true;
                    
                }, function (response) {
                    $scope.handle = response.data.Message;
                    $scope.waiting = false;
                    $scope.failure = true;
                    
                });
                
            }, function (error) {
                clientApplication.acquireTokenPopup([clientId]).then(function (token) {

                }, function (error) {

                });
            });

        $scope.backToPortal = function () {
            // remove token query params
            $location.search({});
            $location.path("/management-portal");
        };

    });

})();