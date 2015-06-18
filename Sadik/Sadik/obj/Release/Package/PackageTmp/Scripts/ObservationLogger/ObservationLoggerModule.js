function ObservationLoggerModule(options) {
    ObservationLoggerModule.superclass.constructor.call(this);

    var self = this;

    var activityLogger;
    var cameInClassLogger;
    var emotionLogger;
    self._kidId = options.kidId;
    self._itemId;
    self._kidsSelectorBlock = options.kidsSelectorBlock;
    self._kidsSource = options.kidsSource;
    self._block = options.block;
    //$ обозначает jquery объекты. Нужно найти и пометить так все переменные с JQuery объектами. Пока что этого не сделано.
    self.$_activityLoggerBlock = self._block.find('.js_logObservationBlock_activity');
    self.$_cameInClassLoggerBlock = self._block.find('.js_logObservationBlock_cameInClass');
    self.$_emotionLoggerBlock = self._block.find('.js_logObservationBlock_emotion');

    if (self.$_activityLoggerBlock.length > 0) {
        activityLogger = new ActivityLogger({ block: self.$_activityLoggerBlock });
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

    if (self.$_cameInClassLoggerBlock.length > 0) {
        cameInClassLogger = new CameInClassLogger({ block: self.$_cameInClassLoggerBlock });
        if (self._kidsSource) {
            cameInClassLogger.SetKidsSource(self._kidsSource);
        }
        if (self._kidId) {
            cameInClassLogger.SelectKidById(self._kidId);
        }
    }

    if (self.$_emotionLoggerBlock.length > 0) {
        emotionLogger = new EmotionLogger({ block: self.$_emotionLoggerBlock });
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
        activityLogger && activityLogger.SetUseCurrentTime(options.useCurrentTime);
        cameInClassLogger && cameInClassLogger.SetUseCurrentTime(options.useCurrentTime);
        emotionLogger && emotionLogger.SetUseCurrentTime(options.useCurrentTime);
    }

    if (options.hideUseCurrentTime) {
        activityLogger && activityLogger.HideUseCurrentTimeBlock();
        cameInClassLogger && cameInClassLogger.HideUseCurrentTimeBlock();
        emotionLogger && emotionLogger.HideUseCurrentTimeBlock();
    }

    if (!options.showKidsSelectorInLoggers) {
        activityLogger && activityLogger.HideKidsSelector();
        cameInClassLogger && cameInClassLogger.HideKidsSelector();
        emotionLogger && emotionLogger.HideKidsSelector();
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

    this.editObservation = function (observation, type) {
        self.currentObservation = observation;
        activityLogger && activityLogger.hide();
        cameInClassLogger && cameInClassLogger.hide();
        emotionLogger && emotionLogger.hide();
        if (activityLogger && type == 'Activity') {
            activityLogger.editObservation(observation);
            activityLogger.show();
        } else if (cameInClassLogger && type == 'CameInClass') {
            cameInClassLogger.editObservation(observation);
            cameInClassLogger.show();
        } else if (emotionLogger && type == 'Emotion') {
            emotionLogger.editObservation(observation);
            emotionLogger.show();
        }
    }

    if (self._kidsSelectorBlock && self._kidsSource) {
        renderKidsSelector();
    }

    self.OnObservationSubmittedComplete = function (UniqueId) {
        if (self.currentObservation && UniqueId && self.currentObservation.UniqueId == UniqueId) {
            self.currentObservation = null;
        }
        self.publish("observationSubmittedComplete", UniqueId);
    };

    activityLogger.subscribe("observationSubmittedComplete", self.OnObservationSubmittedComplete);
    cameInClassLogger.subscribe("observationSubmittedComplete", self.OnObservationSubmittedComplete);
    emotionLogger.subscribe("observationSubmittedComplete", self.OnObservationSubmittedComplete);
}
class_extend(ObservationLoggerModule, PubSubBase);