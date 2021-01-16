window.identityServer = (function () {
    "use strict";

    var identityServer = {
        getModel: function () {
            var modelJson = document.getElementById("modelJson");
            var encodedJson = '';
            if (typeof (modelJson.textContent) !== undefined) {
                encodedJson = modelJson.textContent;
            } else {
                encodedJson = modelJson.innerHTML;
            }
            var json = Encoder.htmlDecode(encodedJson);
            var model = JSON.parse(json);
            return model;
        }
    };

    return identityServer;
})();

(function () {
    "use strict";

    (function () {
        var app = angular.module("app", []);
        app.controller("LayoutCtrl", function ($scope, $rootScope, $http, $sce, Model) {
            $scope.model = Model;
            $scope.havecaptcha = 0;

            $scope.reload = function () {
                $http.get("../api/v0/Captcha?isactive=wehavecaptcha").then(
                    function success(response) {
                        if (response.status == 200) {
                            $scope.havecaptcha = response.data;
                            if (response.data != 0) {
                                let random = Math.floor(Math.random() * 99999999);
                                $scope.imageUrl = "/api/v0/Captcha?id=" + random;
                            }
                        }
                    });
            };
            $scope.showPassword = false;
            init();
            function init() {
                if ($scope.model.loginUrl == undefined) return;
                $scope.reload();

                $http.get("../api/v0/CenterInfo").then(
                    function success(response) {
                        if (response.status == 200) {
                            $scope.systemInfo = response.data;
                        }
                    });
            
                $http.get("../api/v0/ActiveNotification").then(
                    function success(response) {
                        $scope.Notifications = [];
                        if (response.status == 200) {
                            for (let i = 0; i <= response.data.length - 1; i++) {
                                let obj = { Title: response.data[i].Title, RegisterDate: response.data[i].RegisterDate, HtmlText: $sce.trustAsHtml(response.data[i].HtmlText) }
                                $scope.Notifications.push(obj);
                            }
                        }
                    });
            }
        });

        app.factory("loginHttpResponseInterceptor", ["$q", "$location", "$rootScope", function ($q, $location, $rootScope) {

            return {
                request: function (req) {
                    $rootScope.loading = true;
                    return req;
                },
                response: function (response) {
                    $rootScope.loading = false;
                    return response || $q.when(response);
                },
                responseError: function (rejection) {
                    if (rejection.data.Message == undefined)
                        $rootScope.AlertMessage = rejection.data;
                    else {
                        $rootScope.AlertMessage = rejection.data.Message;
                    }
                    if (rejection.status >= 400) {
                        $rootScope.AlertMessageTitle = "نتیجه";
                        $rootScope.AlertType = "warning";
                    }
                    if (rejection.status >= 500) {
                        $rootScope.AlertMessageTitle = "خطا";
                        $rootScope.AlertType = "danger";
                    }                    $rootScope.alertmessage = true;
                    $rootScope.loading = false;
                    
                    return $q.reject(rejection);
                }
            }
        }]).config(["$httpProvider", function ($httpProvider) {
            $httpProvider.interceptors.push("loginHttpResponseInterceptor");
        }]);

        app.directive("alertmessages", function () {
            return {
                restrict: "E",
                replace: true,
                link: function (scope, element) {
                    scope.$watch("alertmessages", function (val) {
                        if (val) {
                            if (val.toString() != "true") scope.msg = val;
                            element[0].style.display = "block";
                        } else
                            element[0].style.display = "none";
                    });
                },
                template: '<div class="row" style="padding: 15px 15px 3px 15px;"><div class="col-lg-12">' +
                    '<div class="alert alert-{{AlertType}} bg-alert-{{AlertType}}">' +
                    '<h4><i class="icon fa fa-warning"></i> {{AlertMessageTitle}}</h4>{{AlertMessage}}</div></div></div>'
            };
        });
        
        app.directive("antiForgeryToken", function () {
            return {
                restrict: "E",
                replace: true,
                scope: {
                    token: "="
                },
                template: "<input type='hidden' name='{{token.name}}' value='{{token.value}}'>"
            };
        });

        app.directive("focusIf", function ($timeout) {
            return {
                restrict: "A",
                scope: {
                    focusIf: "="
                },
                link: function (scope, elem, attrs) {
                    if (scope.focusIf) {
                        $timeout(function () {
                            elem.focus();
                        }, 100);
                    }
                }
            };
        });
    })();

    (function () {
        var model = identityServer.getModel();
        angular.module("app").constant("Model", model);
        if (model.autoRedirect && model.redirectUrl) {
            if (model.autoRedirectDelay < 0) {
                model.autoRedirectDelay = 0;
            }
            window.setTimeout(function () {
                window.location = model.redirectUrl;
            }, model.autoRedirectDelay * 1000);
        }
    })();

})();
