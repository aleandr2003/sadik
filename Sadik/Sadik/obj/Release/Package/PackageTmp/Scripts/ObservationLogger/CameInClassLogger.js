function CameInClassLogger(options) {
    CameInClassLogger.superclass.constructor.call(this, options.block, options.saveObservationUrl);
    var self = this;

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
            Comment: self._commentField.val()
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

    self.OnSuccessSubmitObservation = function (data, status, xhr) {
        if (data.UniqueId) {
            var cameInClass = CameInClass.exists(data.UniqueId.toUpperCase());
            if (cameInClass) cameInClass.destroy();
        }
    }

    CameInClass.subscribe("create", function (observation) {
        observation.createRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });
    CameInClass.subscribe("update", function (observation) {
        observation.updateRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });

    CameInClass.subscribe("create", function () {
        self.UpdateCounter(CameInClass.count());
    });
    CameInClass.subscribe("update", function () {
        self.UpdateCounter(CameInClass.count());
    });
    CameInClass.subscribe("destroy", function () {
        self.UpdateCounter(CameInClass.count());
    });

    $(window).unload(function () {
        CameInClass.saveLocal('CameInClasses');
    });
    CameInClass.loadLocal('CameInClasses');
    self.UpdateCounter(CameInClass.count());

    self.ResubmitObservations = function () {
        CameInClass.each(function (observation) {
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

class_extend(CameInClassLogger, BaseObservationLogger);
