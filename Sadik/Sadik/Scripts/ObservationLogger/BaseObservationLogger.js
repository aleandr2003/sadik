function BaseObservationLogger(block, submitObservationUrl) {
    var self = this;

    this._kidsSource;
    
    this._block = block;
    this._form = this._block.find('form');
    this._observationId = this._block.find('.js_ObservationId');
    this._uniqueId = this._block.find('.js_UniqueId');
    this._kidIdSelector_common = $('.js_KidIdSelect');
    this._kidIdSelector = this._block.find('.js_KidIdSelect');
    this._kidIdField = $('.js_KidId');

    this._dateField = this._block.find('.js_DateObserved');
    this._hoursField = this._block.find('.js_HoursObserved');
    this._minutesField = this._block.find('.js_MinutesObserved');

    this._commentField = this._block.find('.js_commentTextArea');
    this._expandCommentLink = this._block.find('.js_addCommentLink');

    this._submitButton = this._block.find('.js_submitButton');
    this._resultMessage = this._block.find('.js_resultMessage');
    this._useCurrentTimeCheckBox = this._block.find('.js_useCurrentTime');
    this._inputTimeBlock = this._block.find('.js_inputTimeBlock');

    this._counter = this._block.find('.js_counter');

    this._submitObservationUrl = submitObservationUrl;
    this._resubmitIntervalTime = 10 * 60 * 1000; //10 minutes

    this.GetKidId = function () {
        return self._kidIdField.val();
    }

    this.SetKid = function (kid) {
        if (kid !== null) {
            //TODO: следующие 2 строки устанавливают значения выбранного ребенка для всех полей на странице.
            //Исправить, чтобы устанавливались для текущего модуля, а остальное рулилось через ивенты.
            self._kidIdSelector_common.val(kid.label);
            self._kidIdField.val(kid.value);
        } else {
            self._kidIdField.val('');
        }
    }

    this.SelectKidById = function (id) {
        var kid = SearchDictionary(self._kidsSource, function (k) { return k.value == id });
        self.SetKid(kid);
    }

    this.SelectKidByName = function (name) {
        var kid = SearchDictionary(self._kidsSource, function (k) { return k.label == name });
        self.SetKid(kid);
    }

    this.SetKidsSource = function (kidsSource) {
        var source = [];
        for (var i in kidsSource) {
            var kid = kidsSource[i];
            source.push({label: kid.FullName, value: kid.Id});
        }
        self._kidsSource = source;
        self._kidIdSelector.autocomplete("option", {
            source: self._kidsSource
        });
    }

    this.performValidation = function () {
        //if (self._dateField.val() != '' && !self._useCurrentTimeCheckBox.prop('checked')) {
        //    if (self._hoursField.val() == '') {
        //        alert("Не указано время. Поле час");
        //        return false;
        //    }
        //    if (self._minutesField.val() == '') {
        //        alert("Не указано время. Поле минуты");
        //        return false;
        //    }
        //}
        //return true;
    }

    this.reset = function () {
        self._commentField.val('');
        self._hoursField.val('');
        self._minutesField.val('');
    };

    this.saved = function () {
        self._resultMessage.find('span').text('Наблюдение сохранено');
        self._resultMessage.show();
        setTimeout(function () {
            self._resultMessage.fadeOut(function () {
                $(this).find('span').text('');
                $(this).show();
            });
        }, 3000);
        self.reset();
    }

    var _commentCollapsible = new Collapsible(self._expandCommentLink, self._commentField);
    if (self._commentField.val().length > 0) {
        _commentCollapsible.Expand();
    } else {
        _commentCollapsible.Collapse();
    }

    this.submitButtonOnClick = function (event) {
        //if (!self.performValidation()) {
        //    event.preventDefault();
        //}
    }

    this.OnSuccessSubmitObservation = function (data, status, xhr) {
        //self.saved(data);
    }
    //this.OnBeginSubmitObservation = function () {
    //    sadikAjaxStart();
    //}
    //this.OnCompleteSubmitObservation = function () {
    //    sadikAjaxStop();
    //}

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
        var observation;
        if (uniqueId != '') {
            observation = Observation.inst(attributes);
            observation.UniqueId = uniqueId;
            observation.update();
        } else {
            observation = Observation.create(attributes);
        }
        var valid = observation.validate();
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

    this._submitButton.click(function (event) { self.submitButtonOnClick.call(this, event) });

    this._form.submit(function (event) {
        event.preventDefault();
        if ($(this).find('.field-validation-error:visible').length > 0) return false;
        self.submitObservation.call(this, event);
    });

    this.kidAutocompleteOnSelect = function (event, ui) {
        event.preventDefault();
        self.SelectKidByName(ui.item.label);
    }

    //this.kidAutocompleteOnChange = function (event, ui) {
    //    self.SelectKidByName($(this).val());
    //}

    this._kidIdSelector.autocomplete({
        select: function (event, ui) { self.kidAutocompleteOnSelect.call(this, event, ui) }
        //change: function (event, ui) { self.kidAutocompleteOnChange.call(this, event, ui) }
    });
    
    this._useCurrentTimeCheckBox.change(function () {
        if ($(this).prop('checked')) {
            self._inputTimeBlock.hide();
            self._hoursField.prop('disabled', true);
            self._minutesField.prop('disabled', true);
            self._dateField.prop('disabled', true);
        } else {
            self._inputTimeBlock.show();
            self._hoursField.prop('disabled', false);
            self._minutesField.prop('disabled', false);
            self._dateField.prop('disabled', false);
        }
        var dt = new Date();
        self._dateField.val(self.getCurrentDate());
        self._hoursField.val(dt.getHours());
        self._minutesField.val(dt.getMinutes());
    });

    this.SetUseCurrentTime = function (use) {
        self._useCurrentTimeCheckBox.prop('checked', use);
        self._useCurrentTimeCheckBox.change();
    }

    this.HideUseCurrentTimeBlock = function () {
        self._block.find('.js_useCurrentTimeBlock').hide();
    }

    this.UpdateCounter = function (count) {
        self._counter.find('span').text('' + count + ' наблюдений не отправлено');
        if (count == 0) {
            self._counter.hide();
        } else {
            self._counter.show();
        }
    }

    this.getCurrentDate = function () {
        var dt = new Date();
        var day = dt.getDate(); day = day < 10 ? '0' + day : day;
        var month = (dt.getMonth() + 1); month = month < 10 ? '0' + month : month;
        return day + '-' + month + '-' + dt.getFullYear();
    }

    this.setDateTime = function (date, hour, minute) {
        self._hoursField.val(hour);
        self._minutesField.val(minute);

        var dt = new Date(parseInt(/-?\d+/.exec(date)[0]));
        self._dateField.val(printDate(dt));
    }
    function printDate(dt) {
        var dateStr = padStr(dt.getDate()) + '-' +
                      padStr(1 + dt.getMonth()) + '-' +
                      padStr(dt.getFullYear())
        return dateStr;
    }

    function padStr(i) {
        return (i < 10) ? "0" + i : "" + i;
    }

}

class_extend(BaseObservationLogger, PubSub);