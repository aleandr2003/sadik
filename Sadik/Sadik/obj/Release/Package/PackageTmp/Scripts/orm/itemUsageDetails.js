var ItemUsageDetails = Model.setup('ItemUsageDetails', ['KidId', 'ItemId', 'presPerformed', 'datePerformed', 'skillDegree']);
ItemUsageDetails.extend({
    resubmitIntervalTime: 10 * 60 * 1000, //10 minutes
    remoteUrl: SadikGlobalSettings.itemUsageDetailsUrl,
    autoSaveRemote: true,
    resubmit: function () {
        this.each(function (record) {
            if (record.isDirty) {
                record.createRemote(); //раньше я вызывал destroy при получении ответа, но сейчас не делаю этого, потому что destroy автоматически вызывает destroyRemote
            }
        });
    }
});
ItemUsageDetails.include({
    KidId: null,
    ItemId: null,
    presPerformed: null,
    datePerformed: null,
    skillDegree: null,

    validate: function () {
        if (this.KidId == null) return "Пожалуйста, выберите ребенка";
        if (this.ItemId == null) return "Пожалуйста, выберите материал";
        return true;
    }
});

$(window).unload(function () {
    ItemUsageDetails.saveLocalDirtyOnly('ItemUsageDetails');
});
$(window).load(function () {
    setInterval(ItemUsageDetails.ItemUsageDetails, Emotion.resubmitIntervalTime);
    ItemUsageDetails.resubmit();
});