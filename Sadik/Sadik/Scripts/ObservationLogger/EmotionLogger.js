function EmotionLogger(options) {
    EmotionLogger.superclass.constructor.call(this, options.block, options.saveObservationUrl);
    var self = this;
    this._block = options.block;
    this._emotionRadio = this._block.find('.js_emotion_radio');

    this.reset = function () {
        self._commentField.val('');
        self._hoursField.val('');
        self._minutesField.val('');
        self._emotionRadio.prop('checked', false);
    };

    this.submitObservation = function () {
        //var settingDummy = jQuery.ajaxSettings.traditional;
        //jQuery.ajaxSettings.traditional = true;
        var hours = self._hoursField.val();
        var minutes = self._minutesField.val();
        var date = self._dateField.val();
        if (self._useCurrentTimeCheckBox.is(':visible') && self._useCurrentTimeCheckBox.prop('checked')) {
            var dt = new Date();
            hours = dt.getHours();
            minutes = dt.getMinutes();
            date = self.getCurrentDate();
            self._dateField.val(date);
        }
        var uniqueId = self._uniqueId.val().toUpperCase();
        var attributes = {
            Id: self._observationId.val(),
            KidId: self._kidIdField.val(),
            DateObserved: date,
            Hours: hours,
            Minutes: minutes,
            Comment: self._commentField.val(),
            Emotion: self._block.find('.js_emotion_radio:checked').val()
        };
        var emotion;
        if (uniqueId != '' && uniqueId != Math.defaultGuid) {
            emotion = Emotion.inst(attributes);
            emotion.UniqueId = uniqueId;
            emotion.update();
        } else {
            emotion = Emotion.create(attributes);
        }
        var valid = emotion.validate();
        if (valid !== true) {
            alert(valid);
            return;
        }
        self.saved();
        //$.ajax({
        //    'url': self._submitObservationUrl,
        //    'type': 'POST',
        //    'data': obs.toJSON(),
        //    'dataType': 'json',
        //    'success': self.OnSuccessSubmitObservation,
        //    'error': function () { },
        //    'beforeSubmit': self.OnBeginSubmitObservation,
        //    'complete': self.OnCompleteSubmitObservation
        //});
        //jQuery.ajaxSettings.traditional = settingDummy;
    }

    self.editObservation = function (observation) {
        self._observationId.val(observation.Id);
        self._uniqueId.val(observation.UniqueId);
        self._kidIdField.val(observation.KidId);
        self._commentField.val(observation.Comment);
        self._emotionRadio.prop('checked', false);
        self._block.find('.js_emotion_radio[value="' + observation.Emotion+ '"]').prop('checked', true);
        self.setDateTime(observation.DateObserved, observation.Hours, observation.Minutes);
    }

    self.OnSuccessSubmitObservation = function (data, status, xhr) {
        if (data.UniqueId) {
            var emotion = Emotion.exists(data.UniqueId.toUpperCase());
            if (emotion) emotion.destroy();
        }
    }

    Emotion.subscribe("create", function (observation) {
        observation.createRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });
    Emotion.subscribe("update", function (observation) {
        observation.updateRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });

    Emotion.subscribe("create", function () {
        self.UpdateCounter(Emotion.count());
    });
    Emotion.subscribe("update", function () {
        self.UpdateCounter(Emotion.count());
    });
    Emotion.subscribe("destroy", function () {
        self.UpdateCounter(Emotion.count());
    });

    $(window).unload(function () {
        Emotion.saveLocal('Emotions');
    });
    Emotion.loadLocal('Emotions');
    self.UpdateCounter(Emotion.count());
    self.ResubmitObservations = function () {
        Emotion.each(function (observation) {
            if (observation.Id == '') {
                observation.createRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
            } else {
                observation.updateRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
            }
        });
    }
    setInterval(self.ResubmitObservations, self._resubmitIntervalTime);
    self.ResubmitObservations();
}
class_extend(EmotionLogger, BaseObservationLogger);