var Observation = Model.setup('Observation',['Id', 'KidId', 'DateObserved', 'TeacherId', 'TeacherName', /*'Hours','Minutes',*/'Comment']);

Observation.include({
    KidId: null,
    DateObserved: null,
    Hours: null,
    Minutes:null,
    Comment:'',
    attributes: function () {
        var result = {};
        for (var i in this.parent.attributes) {
            var attr = this.parent.attributes[i];
            result[attr] = this[attr];
        }
        result.UniqueId = this.UniqueId;
        result.DateObservedStr = DateCustom.printDateTime(this.DateObserved);
        return result;
    },
    validate: function () {
        if (this.KidId == null) return "Пожалуйста, выберите ребенка";
        if (this.DateObserved == null) return "Пожалуйста, выберите дату";
        //if (this.Hours == null) return "Пожалуйста, укажите время или выберите текущее время";
        //if (this.Minutes == null) return "Пожалуйста, укажите время или выберите текущее время";

        return true;
    }
});