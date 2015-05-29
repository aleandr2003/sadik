function ActivityLogger(options) {
    ActivityLogger.superclass.constructor.call(this, options.block, options.saveObservationUrl);

    var self = this;
    var _getPresentationUrl = options.getPresentationUrl;
    var _updatePresentationUrl = options.updatePresentationUrl;
    var _getSkillUrl = options.getSkillUrl;
    var _updateSkillUrl = options.updateSkillUrl;
    var _getItemUsageDetailsUrl = options.getItemUsageDetailsUrl;
    this._updateItemUsageDetailsUrl = options.updateItemUsageDetailsUrl;

    this._itemsSource;
    this._block = options.block;
    
    this._isPresentationPerformedField = this._block.find('.js_presPerformed');
    this._presentationDateField = this._block.find('.js_datePerformed');
    this._skillDegreeField = this._block.find('.js_skillDegree');
    this._itemIdSelector = this._block.find('.js_ItemIdSelect');
    this._itemIdField = this._block.find('.js_ItemId');
    this._durationField = this._block.find('.js_Duration');
    this._polarizationField = this._block.find('.js_Polarization');
    this._choseHimselfField = this._block.find('.js_ChoseHimSelf');

    this._presentationBlock = this._block.find('.js_presentationBlock');
    this._skillBlock = this._block.find('.js_skillBlock');

    this._itemUsageDetailsRequest = {
        kidId: 0,
        itemId: 0,
        timeSent: new Date()
    };

    this.SelectKidById = function (id) {
        var kid = SearchDictionary(self._kidsSource, function (k) { return k.value == id });
        self.SetKid(kid);
        self.loadPresSkillBlock();
    }

    this.SelectKidByName = function (name) {
        var kid = SearchDictionary(self._kidsSource, function (k) { return k.label == name });
        self.SetKid(kid);
        self.loadPresSkillBlock();
    }

    this.GetItemId = function () {
        return self._itemIdField.val();
    }

    this.SetItem = function (item) {
        if (item !== null) {
            self._itemIdSelector.val(item.label);
            self._itemIdField.val(item.value);
        } else {
            self._itemIdField.val('');
        }
    }

    this.SelectItem = function (id) {
        var item = SearchDictionary(self._itemsSource, function (i) { return i.value == id });
        if (item !== null) {
            //$('#ItemIdSelect').val(item.label);
            self.SetItem(item);
        }
        self.loadPresSkillBlock();
    }

    this.SetItemsSource = function (itemsSource) {
        var source = [];
        for (var i in itemsSource) {
            var item = itemsSource[i];
            source.push({ label: item.FullTitle, value: item.Id });
        }
        self._itemsSource = source;
        self._itemIdSelector.autocomplete('option', {
            source: self._itemsSource
        });
    }

    this.loadPresSkillBlock = function () {
        if (self.GetKidId() != '' && self.GetItemId() != '') {
            //self.getPresentation(_getPresentationUrl);
            //self.getSkill(_getSkillUrl);
            self.getItemUsageDetails(_getItemUsageDetailsUrl);
        } else {
            self.hidePresSkillBlock();
        }
    }

    this.hidePresSkillBlock = function () {
        self._presentationBlock.hide();
        self._skillBlock.hide();
        self._isPresentationPerformedField.prop('checked',false);
        self._presentationDateField.val('');
        self._presentationDateField.prop('disabled', true);
        self._skillDegreeField.val('0');
    }

    this.getItemUsageDetails = function (url) {
        var kidId = self.GetKidId();
        var itemId = self.GetItemId();

        if (self._itemUsageDetailsRequest.kidId == kidId && 
            self._itemUsageDetailsRequest.itemId == itemId &&
            (new Date()).getTime() - self._itemUsageDetailsRequest.timeSent < 500) {
            return; //чтобы не делать один и тот же запрос дважды
        }
        self.hidePresSkillBlock();
        self._itemUsageDetailsRequest = {
            kidId: kidId,
            itemId: itemId,
            timeSent: (new Date()).getTime()
        }; //сохраняем предыдущий запрос
        data = { KidId: kidId, ItemId: itemId }
        if (data.KidId != '' && data.ItemId != '') {
            $.ajax({
                'url': url,
                'type': 'POST',
                'dataType': 'json',
                'data': data,
                'success': function (result) {
                    if (self.GetKidId() == result.KidId && self.GetItemId() == result.ItemId)
                    {
                        if (result.presentation == true) {
                            self._isPresentationPerformedField.prop('checked', true);
                            self._presentationDateField.val(result.date);
                            self._presentationDateField.prop('disabled', false);
                        } else {
                            self._isPresentationPerformedField.prop('checked', false);
                            self._presentationDateField.val('');
                            self._presentationDateField.prop('disabled', true);
                        }
                        self._presentationBlock.show();
                        self._skillDegreeField.val(result.skill);
                        self._skillBlock.show();
                    }
                },
                'error': function () { },
            });
        }
    }

    this.getPresentation = function (url) {
        data = { KidId: self.GetKidId(), ItemId: self.GetItemId() }
        if (data.KidId != '' && data.ItemId != '') {
            $.ajax({
                'url': url,
                'type': 'POST',
                'dataType': 'json',
                'data': data,
                'success': function (result) {
                    if (result.presentation == true) {
                        self._isPresentationPerformedField.prop('checked', true);
                        self._presentationDateField.val(result.date);
                    } else {
                        self._isPresentationPerformedField.prop('checked', false);
                        self._presentationDateField.val('');
                        self._presentationDateField.prop('disabled', true);
                    }
                    self._presentationBlock.show();
                },
                'error': function () { },
            });
        }
    }

    this.updatePresentation = function (url) {
        data = {
            KidId: self.GetKidId(),
            ItemId: self.GetItemId(),
            presPerformed: self._isPresentationPerformedField.prop('checked'),
            datePerformed: self._presentationDateField.val()
        };
        if (data.KidId != '' && data.ItemId != '') {
            $.ajax({
                'url': url,
                'type': 'POST',
                'dataType': 'json',
                'data': data,
                'success': function () { },
                'error': function () { },
            });
        }
    }

    this.getSkill = function (url) {
        data = { KidId: self.GetKidId(), ItemId: self.GetItemId() }
        if (data.KidId != '' && data.ItemId != '') {
            $.ajax({
                'url': url,
                'type': 'POST',
                'dataType': 'json',
                'data': data,
                'success': function (result) {
                    self._skillDegreeField.val(result.skill);
                    self._skillBlock.show();
                },
                'error': function () { },
            });
        }
    }

    this.updateSkill = function (url) {
        data = {
            KidId: self.GetKidId(),
            ItemId: self.GetItemId(),
            skillDegree: self._skillDegreeField.val()
        };
        if (data.KidId != '' && data.ItemId != '') {
            $.ajax({
                'url': url,
                'type': 'POST',
                'dataType': 'json',
                'data': data,
                'success': function () { },
                'error': function () { },
            });
        }
    }

    this.updateItemUsageDetails = function () {
        var details = ItemUsageDetails.create({
            KidId: self.GetKidId(),
            ItemId: self.GetItemId(),
            presPerformed: self._isPresentationPerformedField.prop('checked'),
            datePerformed: self._presentationDateField.val(),
            skillDegree: self._skillDegreeField.val()
        });
        
    }

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
            ItemId: self._itemIdField.val(),
            Duration: self._durationField.val(),
            Polarization: self._polarizationField.is(':checked'),
            ChoseHimSelf: self._choseHimselfField.is(':checked')
        };
        var activity;
        if (uniqueId != '' && uniqueId != Math.defaultGuid) {
            activity = Activity.inst(attributes);
            activity.UniqueId = uniqueId;
            activity.update();
        } else {
            activity = Activity.create(attributes);
        }

        var valid = activity.validate();
        if (valid !== true) {
            alert(valid);
            return;
        }

        if (activity.validate() === true) {
            if (self._presentationBlock.is(':visible') && self._skillBlock.is(':visible')) {
                self.updateItemUsageDetails();
            }
            else {
                if (self._presentationBlock.is(':visible')) {
                    self.updatePresentation(_updatePresentationUrl);
                }
                if (self._skillBlock.is(':visible')) {
                    self.updateSkill(_updateSkillUrl);
                }
            }
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

        self._itemIdField.val(observation.ItemId); +

        self._durationField.val(observation.Duration);
        self._polarizationField.prop('checked', observation.Polarization);
        self._choseHimselfField.prop('checked', observation.ChoseHimSelf);

        self.setDateTime(observation.DateObserved, observation.Hours, observation.Minutes);
        self.loadPresSkillBlock();
    }


    this.reset = function () {
        self._commentField.val('');
        self._hoursField.val('');
        self._minutesField.val('');
        self._itemIdSelector.val('');
        self._itemIdField.val('');
        var firstOptionValue = self._durationField.find('option').first().attr('value');
        self._durationField.val(firstOptionValue);
        self._polarizationField.prop('checked', false);
        self._choseHimselfField.prop('checked', true);
        self._isPresentationPerformedField.prop('checked', false);
        self._presentationDateField.val('');
        self._skillDegreeField.val('');
        self._presentationBlock.hide();
        self._skillBlock.hide();
    };

    this.submitButtonOnClick = function (event) {
        //if (!self.performValidation()) {
        //    event.preventDefault();
        //    return;
        //}
    }

    this._isPresentationPerformedField.change(function () {
        if (self._isPresentationPerformedField.prop('checked')) {
            self._presentationDateField.prop('disabled', false);
        } else {
            self._presentationDateField.val('');
            self._presentationDateField.prop('disabled', true);
        }
    });

    //мы используем и OnSelect, и OnChange чтобы обрабатывать и выбор из выпадающего списка и набор текста пользователем в textbox
    //Временно убрали onChange потому что он срабатывает не на нажатия клавиш, а на onBlur-е и неожиданно заставляет перезагружать поле скилла.

    this.kidAutocompleteOnSelect = function (event, ui) {
        event.preventDefault();
        self.SelectKidByName(ui.item.label);
    }

    //this.kidAutocompleteOnChange = function (event, ui) {
    //    self.SelectKidByName($(this).val());
    //}

    this.itemAutocompleteOnSelect = function (event, ui) {
        self.SetItem(ui.item);
        self.loadPresSkillBlock();
        event.preventDefault();
    }

    //this.itemAutocompleteOnChange = function (event, ui) {
    //    var text = self._itemIdSelector.val();
    //    self.SetItem(null);
    //    var item = SearchDictionary(self._itemsSource, function (i) { return i.label == text });
    //    if (item !== null) {
    //        self.SetItem(item);
    //    }
    //    self.loadPresSkillBlock();
    //}

    this._itemIdSelector.autocomplete({
        select: function (event, ui) { self.itemAutocompleteOnSelect.call(this, event, ui) }
       // change: function (event, ui) { self.itemAutocompleteOnChange.call(this, event, ui) }
    });

    self.OnSuccessSubmitObservation = function (data, status, xhr) {
        if (data.UniqueId) {
            var activity = Activity.exists(data.UniqueId.toUpperCase());
            if (activity) activity.destroy();
        }
    }

    Activity.subscribe("create", function (observation) {
        observation.createRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });
    Activity.subscribe("update", function (observation) {
        console.log('before remote update');
        observation.updateRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
    });

    Activity.subscribe("create", function () {
        self.UpdateCounter(Activity.count());
    });
    Activity.subscribe("update", function () {
        self.UpdateCounter(Activity.count());
    });
    Activity.subscribe("destroy", function () {
        self.UpdateCounter(Activity.count());
    });

    ItemUsageDetails.subscribe("create", function (details) {
        details.createRemote(self._updateItemUsageDetailsUrl, function (data, status, xhr) {
            var itemUsageDetails = ItemUsageDetails.exists(data.UniqueId);
            if (itemUsageDetails) itemUsageDetails.destroy();
        });
    });

    $(window).unload(function () {
        Activity.saveLocal('Activities');
        ItemUsageDetails.saveLocal('ItemUsageDetails');
    });

    Activity.loadLocal('Activities');
    ItemUsageDetails.loadLocal('ItemUsageDetails');
    self.UpdateCounter(Activity.count());

    self.ResubmitObservations = function () {
        Activity.each(function (observation) {
            if (observation.Id == '') {
                observation.createRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
            } else {
                observation.updateRemote(self._submitObservationUrl, self.OnSuccessSubmitObservation);
            }
        });
        ItemUsageDetails.each(function (observation) {
            observation.createRemote(self._updateItemUsageDetailsUrl, function (data, status, xhr) {
                var itemUsageDetails = ItemUsageDetails.exists(data.UniqueId.toUpperCase());
                if (itemUsageDetails) itemUsageDetails.destroy();
            });
        });
    }

    setInterval(self.ResubmitObservations, self._resubmitIntervalTime);
    self.ResubmitObservations();
    
}
class_extend(ActivityLogger, BaseObservationLogger);


