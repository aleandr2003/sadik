function ObservationLoggerModule(options) {
    var self = this;

    var activityLogger;
    var cameInClassLogger;
    var emotionLogger;
    self._kidId = options.kidId;
    self._itemId;
    self._kidsSelectorBlock = options.kidsSelectorBlock;
    self._kidsSource = options.kidsSource;
    self._itemsSource = options.activityLogger ? options.activityLogger.itemsSource : null;

    if (options.activityLogger) {
        activityLogger = new ActivityLogger(options.activityLogger);
        self._itemId = options.activityLogger.itemId;
        if (self._kidsSource) {
            activityLogger.SetKidsSource(self._kidsSource);
        }
        if (self._itemsSource) {
            activityLogger.SetItemsSource(self._itemsSource);
        }
        if (self._kidId) {
            activityLogger.SelectKidById(self._kidId);
        }
        if (self._itemId) {
            activityLogger.SelectItem(self._itemId);
        }
    }

    if (options.cameInClassLogger) {
        cameInClassLogger = new CameInClassLogger(options.cameInClassLogger);
        if (self._kidsSource) {
            cameInClassLogger.SetKidsSource(self._kidsSource);
        }
        if (self._kidId) {
            cameInClassLogger.SelectKidById(self._kidId);
        }
    }

    if (options.emotionLogger) {
        emotionLogger = new EmotionLogger(options.emotionLogger);
        if (self._kidsSource) {
            emotionLogger.SetKidsSource(self._kidsSource);
        }
        if (self._kidId) {
            emotionLogger.SelectKidById(self._kidId);
        }
    }

    if (self._kidsSelectorBlock) {
        self._kidsSelectorBlock.change(function () {
            if (activityLogger) {
                activityLogger.SelectKidById($(this).val());
            }
            if (cameInClassLogger) {
                cameInClassLogger.SelectKidById($(this).val());
            }
            if (emotionLogger) {
                emotionLogger.SelectKidById($(this).val());
            }
        });
    }

    if (options.useCurrentTime !== undefined) {
        if (activityLogger) {
            activityLogger.SetUseCurrentTime(options.useCurrentTime);
        }
        if (cameInClassLogger) {
            cameInClassLogger.SetUseCurrentTime(options.useCurrentTime);
        }
        if (emotionLogger) {
            emotionLogger.SetUseCurrentTime(options.useCurrentTime);
        }
    }

    if (options.hideUseCurrentTime) {
        if (activityLogger) {
            activityLogger.HideUseCurrentTimeBlock();
        }
        if (cameInClassLogger) {
            cameInClassLogger.HideUseCurrentTimeBlock();
        }
        if (emotionLogger) {
            emotionLogger.HideUseCurrentTimeBlock();
        }
    }

    var renderKidsSelector = function () {
        if (self._kidsSelectorBlock && self._kidsSource) {
            self._kidsSelectorBlock.empty();
            for (var i in self._kidsSource) {
                var kid = self._kidsSource[i];
                $('<option></option>').text(kid.FullName).attr('value', kid.Id).appendTo(self._kidsSelectorBlock);
            }
            self._kidsSelectorBlock.attr('size', self._kidsSource.length);
            self._kidsSelectorBlock.val('');
        }
    }

    this.SetKidsSource = function (kidsSource) {
        this._kidsSource = kidsSource;
        if (activityLogger) {
            activityLogger.SetKidsSource(self._kidsSource);
            if (self._kidId) {
                activityLogger.SelectKidById(self._kidId);
            }
        }
        if (cameInClassLogger) {
            cameInClassLogger.SetKidsSource(self._kidsSource);
            if (self._kidId) {
                cameInClassLogger.SelectKidById(self._kidId);
            }
        }
        if (emotionLogger) {
            emotionLogger.SetKidsSource(self._kidsSource);
            if (self._kidId) {
                emotionLogger.SelectKidById(self._kidId);
            }
        }
        if (self._kidsSelectorBlock) {
            renderKidsSelector();
        }
    }

    this.SetItemsSource = function (itemsSource) {
        this._itemsSource = itemsSource;
        if (activityLogger) {
            activityLogger.SetItemsSource(self._itemsSource);
            if (self._itemId) {
                activityLogger.SelectItem(self._itemId);
            }
        }
    }

    this.SelectKid = function (id) {
        self._kidId = id;
        if (activityLogger) {
            activityLogger.SelectKidById(self._kidId);
        }
        if (cameInClassLogger) {
            cameInClassLogger.SelectKidById(self._kidId);
        }
        if (emotionLogger) {
            emotionLogger.SelectKidById(self._kidId);
        }
        if (self._kidsSelectorBlock) {
            self._kidsSelectorBlock.val(self._kidId);
        }
    }

    this.SelectItem = function (id) {
        self._itemId = id;
        if (activityLogger) {
            activityLogger.SelectItem(self._itemId);
        }
    }

    if (self._kidsSelectorBlock && self._kidsSource) {
        renderKidsSelector();
    }
}