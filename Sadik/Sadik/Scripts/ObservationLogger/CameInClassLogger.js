function CameInClassLogger(options) {
    CameInClassLogger.superclass.constructor.call(this, options.block, options.saveObservationUrl);
    var self = this;

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
            TeacherId: SadikGlobalSettings.CurrentUser.Id,
            TeacherName: SadikGlobalSettings.CurrentUser.FirstName
        };
        var cameInClass;
        if (uniqueId != '' && uniqueId != Math.defaultGuid) {
            cameInClass = CameInClass.inst(attributes);
            cameInClass.UniqueId = uniqueId;
            cameInClass.update();
        } else {
            cameInClass = CameInClass.create(attributes);
        }
        var valid = cameInClass.validate();
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
        self.SelectKidById(observation.KidId);
        self._commentField.val(observation.Comment);
        self.setDateTime(observation.DateObserved);
    }

    self.OnSuccessSubmitObservation = function (data, status, xhr) {
        self.publish("observationSubmittedComplete", data.UniqueId);
        if (data.UniqueId) {
            self.publish("observationSubmittedSuccess");
        } else {
            self.publish("observationSubmittedError");
        }
    }

    CameInClass.subscribe("create", function (observation) {
        self.publish("observationSubmitted");
        observation.createRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });
    CameInClass.subscribe("update", function (observation) {
        self.publish("observationSubmitted");
        observation.updateRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });

    CameInClass.subscribe("create", function () {
        self.UpdateCounter(CameInClass.countDirty());
    });
    CameInClass.subscribe("update", function () {
        self.UpdateCounter(CameInClass.countDirty());
    });
    CameInClass.subscribe("destroy", function () {
        self.UpdateCounter(CameInClass.countDirty());
    });
    CameInClass.subscribe("afterSaveRemote", function () {
        self.UpdateCounter(CameInClass.countDirty());
    });

    $(window).unload(function () {
        CameInClass.saveLocalDirtyOnly('CameInClasses');
    });
    
    self.UpdateCounter(CameInClass.countDirty());

    self.ResubmitObservations = function () {
        CameInClass.each(function (observation) {
            if (observation.isDirty) {
                if (observation.Id == '') {
                    observation.createRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
                } else {
                    observation.updateRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
                }
            }
        });
    }
    setInterval(self.ResubmitObservations, self._resubmitIntervalTime);
    self.ResubmitObservations();
}

class_extend(CameInClassLogger, BaseObservationLogger);
