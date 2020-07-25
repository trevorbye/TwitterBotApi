var clientIdString = "f3031107-74e2-4be0-ae9a-e015c90c42c2";
var authority = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47";
var clientApplication = new Msal.UserAgentApplication(clientIdString, authority);

(function () {
    var twitterBot = angular.module('twitterBot', ['ngRoute']);

    // directive for binding file input element to ng-model
    twitterBot.directive("selectNgFiles", function () {
        return {
            require: "ngModel",
            link: function postLink(scope, elem, attrs, ngModel) {
                elem.on("change", function (e) {
                    var files = elem[0].files;
                    var modifiedFileList = [];
                    
                    Array.from(files).forEach(function (file) {
                        let reader = new FileReader();
                        reader.onload = function (e) {
                            modifiedFileList.push(e.target.result);
                            ngModel.$setViewValue(modifiedFileList);
                        }
                        reader.readAsDataURL(file);
                    });
                    console.log(modifiedFileList);
                    elem.val(null);
                })
            }
        }
    });

    twitterBot.directive('croppedImage', function () {
        return {
            restrict: "E",
            replace: true,
            template: "<div class='center-cropped'></div>",
            link: function (scope, element, attrs) {
                var width = attrs.width;
                var height = attrs.height;
                element.css('width', width + "px");
                element.css('height', height + "px");
                element.css('backgroundPosition', 'center center');
                element.css('backgroundRepeat', 'no-repeat');
                element.css('backgroundImage', "url('" + attrs.src + "')");
            }
        }
    });

    twitterBot.config(function ($routeProvider, $httpProvider, $locationProvider) {
        var routeResolve = {
            "auth": function ($window, $q) {
                var defer = $q.defer();

                clientApplication.acquireTokenSilent([clientIdString])
                    .then(function (token) {
                        defer.resolve();
                        return defer.promise;
                    }, function (error) {
                        $window.location.href = "http://localhost:52937/";
                        //$window.location.href = "https://mstwitterbot.azurewebsites.net/";
                    });  
            }
        };

        const cacheBustSuffix = Date.now();

        $routeProvider.when('/', {
            templateUrl: `templates/login.html?t-${cacheBustSuffix}`,
            controller: 'login',
            controllerAs: 'controller'
        }).when('/home', {
            templateUrl: `templates/home.html?t-${cacheBustSuffix}`,
            controller: 'home',
            resolve: routeResolve
        }).when('/tweet-portal', {
            templateUrl: `templates/tweets.html?t-${cacheBustSuffix}`,
            controller: 'tweets',
            resolve: routeResolve
        }).when('/management-portal', {
            templateUrl: `templates/manage.html?t-${cacheBustSuffix}`,
            controller: 'manage',
            resolve: routeResolve
        }).when('/development', {
            templateUrl: `templates/dev.html?t-${cacheBustSuffix}`,
            controller: 'dev',
            resolve: routeResolve
        }).when('/account', {
            templateUrl: `templates/account.html?t-${cacheBustSuffix}`,
            controller: 'account',
            resolve: routeResolve
        }).when('/tweet-queue', {
            templateUrl: `templates/tweet-queue.html?t-${cacheBustSuffix}`,
            controller: 'queue',
            resolve: routeResolve
        }).when('/add-account-redirect', {
            templateUrl: 'templates/twitter-redirect.html',
            controller: 'redirect',
            resolve: routeResolve
        }).when('/not-found', {
            templateUrl: `templates/404.html?t-${cacheBustSuffix}`,
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

        $rootScope.accountText = "Account";
        $rootScope.queueText = "Tweet Queue";
        $rootScope.tweetText = "Compose Tweet";
        $rootScope.logoutText = "Logout";
        $rootScope.devText = "Info";

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

        $scope.account = function () {
            $location.path("/account");
        };

        $scope.queue = function () {
            $location.path("/tweet-queue");
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
        $rootScope.accountActive = false;
        $rootScope.queueActive = false;
        $rootScope.devActive = false;

        $scope.account = function () {
            $location.path("/account");
        };

        $scope.tweet = function () {
            $location.path("/tweet-portal");
        };
    });

    twitterBot.controller("dev", function ($rootScope, $http, $location, $scope) {
        $rootScope.tweetActive = false;
        $rootScope.accountActive = false;
        $rootScope.queueActive = false;
        $rootScope.devActive = true;
    });

    twitterBot.controller("tweets", function ($rootScope, $http, $location, $scope, $routeParams, $window) {
        // set navbar active classes
        $rootScope.tweetActive = true;
        $rootScope.accountActive = false;
        $rootScope.queueActive = false;
        $rootScope.devActive = false;

        $scope.isLoadingQueue = true;
        $scope.tweetQueue = [];
        $scope.handles = [];        
        $scope.tweetSubmitObject = {};
        $scope.error = false;
        $scope.errorMessage = "";
        $scope.isValidTweet = false;
        $scope.imageFileList = [];

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
                        $scope.isLoadingQueue = false;
                        $scope.apply;
                    });
                });

                $http.get("api/get-distinct-handles", config).then(function (response) {
                    $scope.handles = response.data;
                    $scope.apply;
                });

            }, function (error) {
                    $scope.isLoadingQueue = false;
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                });
            });

        $scope.checkTweetValidity = function (e) {
            const value = $("#body-text").val();
            const result = twttr.txt.parseTweet(value);
            if (result) {
                const maxCharacters = 280;
                const span = $("#charsRemaining");
                span.text(`${result.weightedLength} / ${maxCharacters}`);
                if (result.weightedLength > maxCharacters - 20 &&
                    result.weightedLength <= maxCharacters) {
                    span.removeClass("badge-danger").addClass("badge-warning");
                } else if (result.weightedLength > maxCharacters) {
                    span.removeClass("badge-warning").addClass("badge-danger");
                } else {
                    span.removeClass("badge-warning badge-danger");
                }

                const isValidLength = result.weightedLength <= maxCharacters;
                $scope.isValidTweet = isValidLength
                    && result.valid
                    && $scope.tweetSubmitObject.handle;
            } else {
                $scope.isValidTweet = false;
            }
        };

        $scope.submitTweet = function() {
            $scope.error = false;

            // client side validation for null fields
            if ($scope.tweetSubmitObject.handle === null) {
                $scope.error = true;
                $scope.errorMessage = "Handle cannot be empty.";
            } else if ($scope.tweetSubmitObject.body === null) {
                $scope.error = true;
                $scope.errorMessage = "Tweet body cannot be empty.";
            } 

            var date;
            if (!$scope.tweetSubmitObject.date || !$scope.tweetSubmitObject.time) {
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
                    "StatusBody": $scope.tweetSubmitObject.body,
                    "ImageBase64Strings": $scope.imageFileList
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
                            "IsPostedByWebJob": false,
                            "ImageBase64Strings": $scope.imageFileList
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

        $scope.clearImages = function () {
            $scope.imageFileList = [];
        };

        $scope.openImageInNewWindow = function (file) {
            var newTab = $window.open("about:blank", "_blank");
            newTab.document.write("<img src='" + file + "' />");
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

    twitterBot.controller("account", function ($http, $location, $scope, $window, $rootScope) {
        $rootScope.tweetActive = false;
        $rootScope.accountActive = false;
        $rootScope.queueActive = false;
        $rootScope.devActive = false;

        $scope.isLoadingHandles = true;
        $scope.handles = [];

        $('#delete').on('show.bs.modal', function (event) {
            const button = $(event.relatedTarget);
            const handle = button.data('handle');
            const index = button.data('index');

            var modal = $(this);
            modal.find('.modal-body')
                .html(`<p>Are you sure you want to delete the <span class="twitter">${handle}</span> account 😲?</p>`);

            const deleteBtn = modal.find('.modal-footer .btn-danger');
            deleteBtn.data('handle', handle);
            deleteBtn.data('index', index);
        });

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
                    var uiList = [];
                    for (let index = 0; index < accounts.length; ++index) {
                        var account = accounts[index];
                        var uiHandleObject = {
                            handle: account.TwitterHandle,
                            settings: false,
                            retweet: account.IsAutoRetweetEnabled,
                            isPrivate: account.IsPrivateAccount
                        };
                        uiList.push(uiHandleObject);
                    }

                    $scope.handles = uiList;
                    $scope.isLoadingHandles = false;
                }, _ => $scope.isLoadingHandles = false);
            });

        $scope.expandSettings = function (index) {
            $scope.handles[index].settings = true;
            $('[data-toggle="tooltip"]').tooltip();
        };

        $scope.hideSettings = function (index) {
            $scope.handles[index].settings = false;
            $('[data-toggle="tooltip"]').tooltip();
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

        $scope.togglePrivateAccount = function (handle, index, isPrivate) {
            $scope.handles[index].isPrivate = isPrivate;

            clientApplication.acquireTokenSilent([clientIdString])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.get(`api/toggle-private-account?handle=${handle}&isPrivate=${isPrivate}`, config).then(function (response) {

                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });
        };

        $scope.deleteAccount = function () {            
            const deleteBtn = $('.modal-footer .btn-danger');
            const handle = deleteBtn.data('handle');
            const index = deleteBtn.data('index');

            clientApplication.acquireTokenSilent([clientIdString])
                .then(function (token) {
                    var config = {
                        headers: {
                            'Content-type': 'application/json',
                            'Authorization': 'Bearer ' + token
                        }
                    };

                    $http.delete(`api/delete-twitter-account?handle=${handle}`, config).then(function (response) {
                        $scope.handles.splice(index, 1);
                    });
                }, function (error) {
                    clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                    }, function (error) {

                    });
                });

            $('#delete').modal('hide');
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

    twitterBot.controller("queue", function ($http, $location, $scope, $window, $rootScope) {
        $rootScope.tweetActive = false;
        $rootScope.accountActive = false;
        $rootScope.queueActive = false;
        $rootScope.devActive = false;

        $scope.isLoadingHandles = true;
        $scope.isLoadingQueue = true;
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
                    var uiList = [];
                    for (let index = 0; index < accounts.length; ++index) {
                        var account = accounts[index];
                        var uiHandleObject = {
                            handle: account.TwitterHandle,
                            settings: false,
                            retweet: account.IsAutoRetweetEnabled,
                            isPrivate: account.IsPrivateAccount
                        };
                        uiList.push(uiHandleObject);
                    }

                    $scope.handles = uiList;
                    $scope.isLoadingHandles = false;
                }, _ => $scope.isLoadingHandles = false);

                $http.get("api/get-utc-now").then(function (utcRes) {
                    $http.get("api/get-handles-tweet-queue", config).then(function (response) {

                        var utcNow = Date.parse(utcRes.data);
                        var tweets = response.data;
                        for (let index = 0; index < tweets.length; ++index) {
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
                        $scope.isLoadingQueue = false;
                        $scope.apply;
                    });
                });

            }, function (error) {
                $scope.isLoadingQueue = false;
                clientApplication.acquireTokenPopup([clientIdString]).then(function (token) {

                }, function (error) {

                });
            });

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

        $scope.deleteTweet = function (tweetId, index) {

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
