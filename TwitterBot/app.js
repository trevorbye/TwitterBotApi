﻿// the AAD application
var clientApplication;

(function () {

    // Enter Global Config Values & Instantiate MSAL Client application
    window.config = {
        clientID: 'f3031107-74e2-4be0-ae9a-e015c90c42c2',
    };

    function authCallback(errorDesc, token, error, tokenType) {
        //This function is called after loginRedirect and acquireTokenRedirect. Not called with loginPopup
        // msal object is bound to the window object after the constructor is called.
        if (token) {
        }
        else {
            log(error + ":" + errorDesc);
        }
    }

    if (!clientApplication) {
        clientApplication = new Msal.UserAgentApplication(window.config.clientID, null, authCallback);
    }

    // Get UI jQuery Objects
    var $panel = $(".panel-body");
    var $userDisplay = $(".app-user");
    var $signInButton = $(".app-login");
    var $signOutButton = $(".app-logout");
    var $errorMessage = $(".app-error");
    onSignin(null);


    // Handle Navigation Directly to View
    window.onhashchange = function () {
        loadView(stripHash(window.location.hash));
    };
    window.onload = function () {
        $(window).trigger("hashchange");
    };

    // Register NavBar Click Handlers
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
                    printErrorMessage(error);
                });
            })
    });

    function authTest(accessToken) {
        $.ajax({
            type: "GET",
            url: "/api/AdminManagers",
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

    // Route View Requests To Appropriate Controller
    function loadCtrl(view) {
        switch (view.toLowerCase()) {
            case 'home':
                return homeCtrl;
            case 'todolist':
                return todoListCtrl;
            case 'userdata':
                return userDataCtrl;
        }
    }

    // Show a View
    function loadView(view) {

        $errorMessage.empty();
        var ctrl = loadCtrl(view);

        if (!ctrl)
            return;

        // Check if View Requires Authentication
        if (ctrl.requireADLogin && !clientApplication.getUser()) {
            clientApplication.loginPopup().then(onSignin);
            return;
        }

        // Load View HTML
        $.ajax({
            type: "GET",
            url: "App/Views/" + view + '.html',
            dataType: "html",
        }).done(function (html) {

            // Show HTML Skeleton (Without Data)
            var $html = $(html);
            $html.find(".data-container").empty();
            $panel.html($html.html());
            ctrl.postProcess(html);

        }).fail(function () {
            $errorMessage.html('Error loading page.');
        }).always(function () {

        });
    };

    function stripHash(view) {
        return view.substr(view.indexOf('#') + 1);
    }

}());