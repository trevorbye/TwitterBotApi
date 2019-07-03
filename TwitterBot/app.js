var clientApplication;

(function () {

    window.config = {
        clientID: 'f3031107-74e2-4be0-ae9a-e015c90c42c2',
    };

    function authCallback(errorDesc, token, error, tokenType) {
        if (token) {
        }
        else {
            log(error + ":" + errorDesc);
        }
    }

    if (!clientApplication) {
        clientApplication = new Msal.UserAgentApplication(window.config.clientID, null, authCallback);
    }

    var $userDisplay = $(".app-user");
    var $signInButton = $(".app-login");
    var $signOutButton = $(".app-logout");
    var $errorMessage = $(".app-error");
    onSignin(null);

    $signOutButton.click(function () {
        clientApplication.logout();
    });

    $signInButton.click(function () {
        clientApplication.loginPopup().then(onSignin);
    });

    $(".auth-test").click(function () {
        scope = [window.config.clientID];

        clientApplication.acquireTokenSilent(scope)
            .then(function (token) {
                authTest(token);
            }, function (error) {
                clientApplication.acquireTokenPopup(scope).then(function (token) {
                    authTest(token);
                }, function (error) {
                    
                });
            })
    });

    function authTest(accessToken) {
        $.ajax({
            type: "GET",
            url: "current-principal-admin",
            headers: {
                'Authorization': 'Bearer ' + accessToken,
                'Accept': "application/json"
            },
            data: {
               
            },
        }).done(function (response) {
            console.log(response);
        }).fail(function () {
        }).always(function () {
        });
    }

    function onSignin(idToken) {
        // Check Login Status, Update UI
        var user = clientApplication.getUser();
        if (user) {
            console.log(user)
            $userDisplay.html(user.name);
            $userDisplay.show();
            $signInButton.hide();
            $signOutButton.show();
        } else {
            $userDisplay.empty();
            $userDisplay.hide();
            $signInButton.show();
            $signOutButton.hide();
        }
    }

    // client-side routing logic
    let templateDiv = document.getElementById('templates');
    let routes = {
        '/': homepage
    };

    window.onpopstate = () => {
        templateDiv.innerHTML = routes[window.location.pathname];
    }

    onNavItemClick = function(pathName) {
        window.history.pushState({}, pathName, window.location.origin + pathName);
        templateDiv.innerHTML = routes[pathName];
    }

    templateDiv.innerHTML = routes[window.location.pathname];

}());