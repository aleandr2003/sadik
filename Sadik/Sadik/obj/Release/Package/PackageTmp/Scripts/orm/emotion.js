var Emotion = Observation.setup('Emotion', Observation.attributes.concat(['Emotion']));
Emotion.extend({
    remoteUrl: SadikGlobalSettings.emotionUrl,
    autoSaveRemote: true
});
Emotion.include({
    Emotion:null,
    Type: 'Emotion',
    validate: function () {
        if (this.KidId == null) return "Пожалуйста, выберите ребенка";
        if (this.DateObserved == null) return "Пожалуйста, выберите дату";
        //if (this.Hours == null) return "Пожалуйста, укажите время или выберите текущее время";
        //if (this.Minutes == null) return "Пожалуйста, укажите время или выберите текущее время";
        if (this.Emotion == null) return "Пожалуйста, выберите эмоцию";

        return true;
    }
});

$(window).unload(function () {
    Emotion.saveLocalDirtyOnly('Emotions');
});
$(window).load(function () {
    setInterval(Emotion.resubmit, Emotion.resubmitIntervalTime);
    Emotion.resubmit();
});