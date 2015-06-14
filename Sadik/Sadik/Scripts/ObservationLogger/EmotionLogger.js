function EmotionLogger(options) {
    EmotionLogger.superclass.constructor.call(this, options.block);
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
        //var hours = self._hoursField.val();
        //var minutes = self._minutesField.val();
        var date = self.getSelectedDate();
        if (self._useCurrentTimeCheckBox.is(':visible') && self._useCurrentTimeCheckBox.prop('checked')) {
            date = new Date();
            self._dateField.val(self.getCurrentDate());
        }
        //var uniqueId = self._uniqueId.val().toUpperCase(); //не помню, зачем сделал тут upperCase. возможно иначе сервер не парсил Guid.
        var uniqueId = self._uniqueId.val();
        var attributes = {
            Id: self._observationId.val(),
            KidId: self._kidIdField.val(),
            DateObserved: date,
            //Hours: hours,
            //Minutes: minutes,
            Comment: self._commentField.val(),
            Emotion: self._block.find('.js_emotion_radio:checked').val(),
            TeacherId: SadikGlobalSettings.CurrentUser.Id,
            TeacherName: SadikGlobalSettings.CurrentUser.FirstName
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
       
    }

    self.editObservation = function (observation) {
        self._observationId.val(observation.Id);
        self._uniqueId.val(observation.UniqueId);
        self.SelectKidById(observation.KidId);
        self._commentField.val(observation.Comment);
        self._emotionRadio.prop('checked', false);
        self._block.find('.js_emotion_radio[value="' + observation.Emotion+ '"]').prop('checked', true);
        self.setDateTime(observation.DateObserved);
    }

    self.OnSuccessSubmitObservation = function (observation) {
        self.publish("observationSubmittedComplete", observation.UniqueId);
        if (observation.UniqueId) {
            self.publish("observationSubmittedSuccess");
        } else {
            self.publish("observationSubmittedError");
        }
    }

    Emotion.subscribe("afterCreateRemote", function (observation) {
        self.publish("observationSubmitted");
        self.OnSuccessSubmitObservation(observation);
    });
    Emotion.subscribe("afterUpdateRemote", function (observation) {
        self.publish("observationSubmitted");
        self.OnSuccessSubmitObservation(observation);
    });

    Emotion.subscribe("create", function () {
        self.UpdateCounter(Emotion.countDirty());
    });
    Emotion.subscribe("update", function () {
        self.UpdateCounter(Emotion.countDirty());
    });
    Emotion.subscribe("destroy", function () {
        self.UpdateCounter(Emotion.countDirty());
    });
    Emotion.subscribe("afterSaveRemote", function () {
        self.UpdateCounter(Emotion.countDirty());
    });

    //$(window).unload(function () {
    //    Emotion.saveLocalDirtyOnly('Emotions');
    //});
    
    self.UpdateCounter(Emotion.countDirty());
    //self.ResubmitObservations = function () {
    //    Emotion.each(function (observation) {
    //        if (observation.isDirty) {
    //            if (observation.Id == '') {
    //                observation.createRemote(self.OnSuccessSubmitObservation);
    //            } else {
    //                observation.updateRemote(self.OnSuccessSubmitObservation);
    //            }
    //        }
    //    });
    //}
    //setInterval(self.ResubmitObservations, self._resubmitIntervalTime);
    //self.ResubmitObservations();
}
class_extend(EmotionLogger, BaseObservationLogger);