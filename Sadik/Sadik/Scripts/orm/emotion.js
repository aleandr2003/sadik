﻿var Emotion = Observation.setup('Emotion', Observation.attributes.concat(['Emotion']));

Emotion.include({
    Emotion:null,

    validate: function () {
        if (this.KidId == null) return "Пожалуйста, выберите ребенка";
        if (this.DateObserved == null) return "Пожалуйста, выберите дату";
        if (this.Hours == null) return "Пожалуйста, укажите время или выберите текущее время";
        if (this.Minutes == null) return "Пожалуйста, укажите время или выберите текущее время";
        if (this.Emotion == null) return "Пожалуйста, выберите эмоцию";

        return true;
    }
});