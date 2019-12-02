var clientIdString = "f3031107-74e2-4be0-ae9a-e015c90c42c2";
var authority = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47";
var clientApplication = new Msal.UserAgentApplication(clientIdString, authority);

(function () {
    var twitterBot = angular.module('twitterBot', ['ngRoute']);
    twitterBot.config(function ($routeProvider, $httpProvider, $locationProvider) {
        var routeResolve = {
            "auth": function ($window, $q) {
                var defer = $q.defer();

                clientApplication.acquireTokenSilent([clientIdString])
                    .then(function (token) {
                        defer.resolve();
                        return defer.promise;
                    }, function (error) {
                        //$window.location.href = "http://localhost:52937/";
                        $window.location.href = "https://mstwitterbot.azurewebsites.net/";
                    });  
            }
        };

        $routeProvider.when('/', {
            templateUrl: 'templates/login.html',
            controller: 'login',
            controllerAs: 'controller'
        }).when('/home', {
            templateUrl: 'templates/home.html',
            controller: 'home',
            resolve: routeResolve
        }).when('/tweet-portal', {
            templateUrl: 'templates/tweets.html',
            controller: 'tweets',
            resolve: routeResolve
        }).when('/management-portal', {
            templateUrl: 'templates/manage.html',
            controller: 'manage',
            resolve: routeResolve
        }).when('/development', {
            templateUrl: 'templates/dev.html',
            controller: 'dev',
            resolve: routeResolve
        }).when('/add-account-redirect', {
            templateUrl: 'templates/twitter-redirect.html',
            controller: 'redirect',
            resolve: routeResolve
        }).when('/not-found', {
            templateUrl: 'templates/404.html',
            controller: 'redirect',
            resolve: routeResolve
        }).otherwise({
                redirectTo: "/not-found"
        });

        $locationProvider.html5Mode(true);
    });

    twitterBot.controller("login", function ($rootScope, $http, $location, $scope) {

        //on site load check if active token already exists in cache, then set ui auth state
        clientApplication.acquireTokenSilent([clientIdString])
            .then(function (token) {
                $location.path("/home");
            }, function (error) {
            });

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

        $rootScope.manageText = "Manage";
        $rootScope.tweetText = "Tweet";
        $rootScope.logoutText = "Logout";
        $rootScope.devText = "Development";

        // db init request 
        $http.get("api/init-loader").then(function (response) {
        });

        //on site load check if active token already exists in cache, then set ui auth state
        var cachedUser = clientApplication.getUser();
        if (cachedUser !== null) {
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

        $scope.dev = function () {
            $location.path("/development");
        };
    });

    twitterBot.controller("home", function ($rootScope, $http, $location, $scope) {
        $rootScope.tweetActive = false;
        $rootScope.manageActive = false;
        $rootScope.devActive = false;

        $scope.manage = function () {
            $location.path("/management-portal");
        };

        $scope.tweet = function () {
            $location.path("/tweet-portal");
        };
    });

    twitterBot.controller("dev", function ($rootScope, $http, $location, $scope) {
        $rootScope.tweetActive = false;
        $rootScope.manageActive = false;
        $rootScope.devActive = true;
    });

    twitterBot.controller("tweets", function ($rootScope, $http, $location, $scope, $routeParams) {
        // set navbar active classes
        $rootScope.tweetActive = true;
        $rootScope.manageActive = false;
        $rootScope.devActive = false;

        $scope.tweetQueue = [];
        $scope.handles = [];
        $scope.tweetSubmitObject = {};
        $scope.error = false;
        $scope.errorMessage = "";

        clientApplication.acquireTokenSilent([clientIdString])
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
                            var tweet = tweets[index];
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
                clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                }, function (error) {

                });
            });

        $scope.submitTweet = function () {
            $scope.error = false;

            // client side validation for null fields
            if ($scope.tweetSubmitObject.handle === null) {
                $scope.error = true;
                $scope.errorMessage = "Handle cannot be empty.";
            } else if ($scope.tweetSubmitObject.body === null) {
                $scope.error = true;
                $scope.errorMessage = "Tweet body cannot be empty.";
            } 

            if ($scope.tweetSubmitObject.body.length > 280) {
                $scope.error = true;
                $scope.errorMessage = "Tweet body exceeds 280 character limit.";
            }

            var date;
            if ($scope.tweetSubmitObject.date === null || $scope.tweetSubmitObject.time === null) {
                date = new Date(Date.now());
            } else {
                date = new Date($scope.tweetSubmitObject.date);
                var time = new Date($scope.tweetSubmitObject.time);
                date.setHours(time.getHours(), time.getMinutes());

                if (date < Date.now()) {
                    $scope.error = true;
                    $scope.errorMessage = "Scheduled Tweet time must be in the future.";
                }
            }

            if ($scope.error === false) {

                var tweetQueueObject = {
                    "TwitterHandle": $scope.tweetSubmitObject.handle,
                    "ScheduledStatusTime": date,
                    "StatusBody": $scope.tweetSubmitObject.body
                };

                clientApplication.acquireTokenSilent([clientIdString])
                    .then(function (token) {
                        var config = {
                            headers: {
                                'Content-type': 'application/json',
                                'Authorization': 'Bearer ' + token
                            }
                        };

                        var tweetObject = {
                            "CreatedTime": "Just now",
                            "ScheduledStatusTime": date.toLocaleString(),
                            "TwitterHandle": $scope.tweetSubmitObject.handle,
                            "StatusBody": $scope.tweetSubmitObject.body,
                            "IsApprovedByHandle": false,
                            "IsPostedByWebJob": false
                        };
                        $scope.tweetQueue.unshift(tweetObject);
                        $scope.tweetSubmitObject = {};
                        $scope.apply;

                        $http.post("api/post-new-tweet", tweetQueueObject, config).then(function (response) {
                            $scope.tweetQueue[0].TweetUser = response.data.TweetUser;
                            $scope.tweetQueue[0].Id = response.data.Id;
                        });
                    }, function (error) {
                        clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                        }, function (error) {

                        });
                    });
            }
        };

        $scope.cancelTweet = function (tweetId, index) {

            clientApplication.acquireTokenSilent([clientIdString])
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
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

    });

    twitterBot.controller("manage", function ($http, $location, $scope, $window, $rootScope) {
        $rootScope.tweetActive = false;
        $rootScope.manageActive = true;
        $rootScope.devActive = false;

        $scope.handles = [];
        $scope.tweetQueue = [];

        clientApplication.acquireTokenSilent([clientIdString])
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
                    for (index = 0; index < accounts.length; ++index) {
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
                            var tweet = tweets[index];
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
                            tweet.EditPaneExpanded = false;
                            tweet.error = false;
                        }

                        $scope.tweetQueue = tweets;
                        $scope.apply;
                    });
                });
                
            }, function (error) {
                clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

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

            clientApplication.acquireTokenSilent([clientIdString])
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
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.disableAutoTweets = function (handle, index) {
            $scope.handles[index].retweet = false;

            clientApplication.acquireTokenSilent([clientIdString])
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
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.deleteAccount = function (handle, index) {
            clientApplication.acquireTokenSilent([clientIdString])
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
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.editTweet = function (tweet, index) {

            if (tweet.StatusBody.length > 280) {
                $scope.tweetQueue[index].error = true;
            } else {
                $scope.tweetQueue[index].EditPaneExpanded = false;

                clientApplication.acquireTokenSilent([clientIdString])
                    .then(function (token) {
                        var config = {
                            headers: {
                                'Content-type': 'application/json',
                                'Authorization': 'Bearer ' + token
                            }
                        };

                        $http.post("api/edit-tweet-body", tweet, config).then(function (response) {
                        });
                    }, function (error) {
                        clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                        }, function (error) {

                        });
                    });
            }
        };

        $scope.approveTweet = function (tweetId, index) {
            clientApplication.acquireTokenSilent([clientIdString])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $scope.tweetQueue[index].IsApprovedByHandle = true;

                    $http.get("api/approve-or-cancel?approveById=" + tweetId + "&cancelById=0", config).then(function (response) {
                        
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.cancelTweet = function (tweetId, index) {
            clientApplication.acquireTokenSilent([clientIdString])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $scope.tweetQueue[index].IsApprovedByHandle = false;

                    $http.get("api/approve-or-cancel?cancelById=" + tweetId + "&approveById=0", config).then(function (response) {
                        
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.twitterSignIn = function () {
            clientApplication.acquireTokenSilent([clientIdString])
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
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

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
        console.log(oauthToken);
        console.log(oauthVerifier);

        var params = {
            token: oauthToken,
            verifier: oauthVerifier
        };

        clientApplication.acquireTokenSilent([clientIdString])
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
                clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

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