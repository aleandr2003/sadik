var Observation = Model.setup('Observation',['Id', 'KidId', 'DateObserved', 'TeacherId', 'TeacherName', /*'Hours','Minutes',*/'Comment']);

Observation.extend({
    resubmitIntervalTime : 10 * 60 * 1000, //10 minutes
    resubmit: function(){ 
        this.each(function (record) {
            if(record.isDirty){
                if (record.Id == '') {
                    record.createRemote();
                } else {
                    record.updateRemote();
                }
            }
        });
    }
});
Observation.include({
    KidId: null,
    DateObserved: null,
    //Hours: null,
    //Minutes:null,
    Comment: '',
    init: function (atts) {
        if (atts) this.load(atts);
        if (atts.DateObserved && atts.DateObserved.constructor !== Date) {
            if (typeof atts.DateObserved == 'number') {
                this.DateObserved = new Date(atts.DateObserved);
            } else if (typeof atts.DateObserved == 'string') {
                if (/^-?\d+$/.test(atts.DateObserved)) {
                    this.DateObserved = new Date(parseInt(/^-?\d+$/.exec(atts.DateObserved)[0]));
                } else if (atts.DateObserved.indexOf('STR') === 0) {
                    var dateParts = atts.DateObserved.substring(3).split('-');
                    var years = parseInt(dateParts[0]);
                    var months = parseInt(dateParts[1]);
                    var days = parseInt(dateParts[2]);
                    var hours = parseInt(dateParts[3]);
                    var minutes = parseInt(dateParts[4]);
                    var seconds = parseInt(dateParts[5]);
                    this.DateObserved = new Date(years, months - 1, days, hours, minutes, seconds);
                } else {
                    this.DateObserved = new Date(atts.DateObserved);
                }
            } 
        }
    },
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