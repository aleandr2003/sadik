var Activity = Observation.setup('Activity', Observation.attributes.concat(['ItemId', 'Duration', 'Polarization', 'ChoseHimSelf']));

Activity.include({
    ItemId: null,
    Duration: null,
    Polarization: null,
    ChoseHimSelf: null,

    validate: function () {
        if (this.KidId == null) return "Пожалуйста, выберите ребенка";
        if (this.DateObserved == null) return "Пожалуйста, выберите дату";
        if (this.Hours == null) return "Пожалуйста, укажите время или выберите текущее время";
        if (this.Minutes == null) return "Пожалуйста, укажите время или выберите текущее время";
        if (this.ItemId == null) return "Пожалуйста, выберите материал";
        if (this.Duration == null) return "Пожалуйста, выберите продолжительность";
        if (this.Polarization == null) return "Не установлено поле Поляризация";
        if (this.ChoseHimSelf == null) return "Не установлено поле Выбрал сам";

        return true;
    }
});