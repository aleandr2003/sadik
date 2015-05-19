var ItemUsageDetails = Model.setup('ItemUsageDetails', ['KidId', 'ItemId', 'presPerformed', 'datePerformed', 'skillDegree']);

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