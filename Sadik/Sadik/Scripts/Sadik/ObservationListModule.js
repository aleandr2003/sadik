var ObservationsApp = angular.module("ObservationsApp", []);
ObservationsApp.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.defaults.headers.common["X-Requested-With"] = 'XMLHttpRequest';
}]);
ObservationsApp.controller("ObservationsCtrl", function ($scope, $http) {
    $scope.activities = [];
    $scope.cameInClasses = [];
    $scope.emotions = [];
    $scope.itemsSource = [];
    $scope.fillObservationsList = function () {
        $scope.observations = [].concat(Activity.recordsValues(), CameInClass.recordsValues(), Emotion.recordsValues());
    }
    $scope.$watchCollection("activities", function () { $scope.fillObservationsList(); });
    $scope.$watchCollection("cameInClasses", function () { $scope.fillObservationsList(); });
    $scope.$watchCollection("emotions", function () { $scope.fillObservationsList(); });

    $scope.canEdit = false;
    $scope.canDelete = false;
    $scope.edit = function (id) {
        $scope.currentUniqueId = null;
        if ($scope.observationLoggerModule) {
            var observation = SearchDictionary($scope.observations, function (item) { return item.Id == id; });
            if (observation) {
                $scope.currentUniqueId = observation.UniqueId;
                $scope.observationLoggerModule.editObservation(observation, observation.Type);
                $('#editingDialog').dialog("open");
            }
        }
    }
    $scope.delete = function (id) {
        var observation = SearchDictionary($scope.observations, function (item) { return item.Id == id; });
        if (observation) {
            observation.destroyRemote(function () {
                this.destroy();
            });
        }
    }
    $scope.filterType = '';
    $scope.setFilterType = function (type) {
        $scope.filterType = type;
    };
    $scope.filteredObservations = function (observation) {
        if (!$scope.filterType) return true;
        switch ($scope.filterType) {
            case 'Activity':
                return observation.Type == 'Activity';
                break;
            case 'CameInClass':
                return observation.Type == 'CameInClass';
                break;
            case 'Emotion':
                return observation.Type == 'Emotion';
                break;
            case 'WithComments':
                return observation.Comment && observation.Comment.length > 0;
                break;
        }
    };
    $scope.loadObservationsList = function (KidId) {
        $http({
            url: SadikGlobalSettings.observationListUrl,
            params: { Id: KidId },
            type: "GET"
        }).success(function (data) {
            $scope.mergeObservations(data);
        });
    };

    $scope.mergeObservations = function (observations) {
        var activities = Where(observations, function (item) { return item.Type == 'Activity'; });
        var cameInClasses = Where(observations, function (item) { return item.Type == 'CameInClass'; });
        var emotions = Where(observations, function (item) { return item.Type == 'Emotion'; });

        Activity.merge(activities);
        CameInClass.merge(cameInClasses);
        Emotion.merge(emotions);
    }


    Activity.subscribe("create", function () {
        $scope.$digest();
    });
    Activity.subscribe("update", function () {
        $scope.$digest();
    });
    Activity.subscribe("destroy", function () {
        $scope.$digest();
    });
    CameInClass.subscribe("create", function () {
        $scope.$digest();
    });
    CameInClass.subscribe("update", function () {
        $scope.$digest();
    });
    CameInClass.subscribe("destroy", function () {
        $scope.$digest();
    });
    Emotion.subscribe("create", function () {
        $scope.$digest();
    });
    Emotion.subscribe("update", function () {
        $scope.$digest();
    });
    Emotion.subscribe("destroy", function () {
        $scope.$digest();
    });

    //чтобы обновлять крутилки в списке (крутилки будут привязаны к isDirty, чтобы видеть, какие изменения еще не сохранены);
    Activity.subscribe("afterSaveRemote", function () {
        $scope.$digest();
    });
    CameInClass.subscribe("afterSaveRemote", function () {
        $scope.$digest();
    });
    Emotion.subscribe("afterSaveRemote", function () {
        $scope.$digest();
    });
});
ObservationsApp.filter("textLimit", function () {
    return function (value, limit) {
        if (angular.isString(value)) {
            if (value.length < limit) {
                return value;
            }
            else {
                return value.substring(0, limit + 1) + "...";
            }
        } else {
            return value;
        }
    };
}).filter('padZeros', function () {
    return function (n, len) {
        var num = parseInt(n, 10);
        len = parseInt(len, 10);
        if (isNaN(num) || isNaN(len)) {
            return n;
        }
        num = '' + num;
        while (num.length < len) {
            num = '0' + num;
        }
        return num;
    };
}).filter('dateWithoutShift', function ($filter) {
    var standardDateFilterFn = $filter('date');
    return function (input) {
        var toUTCDate = function (date) {
            var _utc = new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(), date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds());
            return _utc;
        };

        var utcDate = toUTCDate(new Date(input));
        var newArgs = [];
        for (var i = 1; i < arguments.length; i++) {
            newArgs.push(arguments[i]);
        }
        return standardDateFilterFn(utcDate, newArgs);
    };
}).filter('ActivityDuration', function ($filter) {
    var IdToDisplayNameFn = $filter('IdToDisplayName');
    return function (input) {
        return IdToDisplayNameFn(input, SadikGlobalSettings.ActivityDurations);
    };
}).filter('EmotionName', function ($filter) {
    var IdToDisplayNameFn = $filter('IdToDisplayName');
    return function (input) {
        return IdToDisplayNameFn(input, SadikGlobalSettings.EmotionTypes);
    };
}).filter('IdToDisplayName', function () {
    return function (input, source) {
        var id = input;
        if (typeof id == 'string') {
            id = parseInt(id);
            if (isNaN(id)) {
                return input;
            }
        }
        if (typeof id == 'number' && source && source.constructor === Array) {
            for (var i in source) {
                var item = source[i];
                if (item.Id == id) return item.DisplayName;
            }
            return id;
        } else {
            return id;
        }
    };
}).filter('InventoryName', function () {
    return function (input, source) {
        var id = input;
        if (typeof id == 'string') {
            id = parseInt(id);
            if (isNaN(id)) {
                return input;
            }
        }
        if (typeof id == 'number' && source && source.constructor === Array) {
            //binary search algorithm. on 300+ items and 1000+ observations makes a serious impact.
            //the array is sorted on the server;
            var minIndex = 0;
            var maxIndex = source.length - 1;
            var currentIndex;
            var currentElement;

            while (minIndex <= maxIndex) {
                currentIndex = (minIndex + maxIndex) / 2 | 0;
                currentElement = source[currentIndex];

                if (currentElement.Id < id) {
                    minIndex = currentIndex + 1;
                }
                else if (currentElement.Id > id) {
                    maxIndex = currentIndex - 1;
                }
                else {
                    return currentElement.Title;
                }
            }

            return '';
        } else {
            return input;
        }
    };
});