var Observation = Model.setup('Observation',['Id', 'KidId', 'DateObserved', 'Hours','Minutes','Comment']);

Observation.include({
    KidId: null,
    DateObserved: null,
    Hours: null,
    Minutes:null,
    Comment:'',

    validate: function () {
        if (this.KidId == null) return "Пожалуйста, выберите ребенка";
        if (this.DateObserved == null) return "Пожалуйста, выберите дату";
        if (this.Hours == null) return "Пожалуйста, укажите время или выберите текущее время";
        if (this.Minutes == null) return "Пожалуйста, укажите время или выберите текущее время";

        return true;
    }
});