var CameInClass = Observation.setup('CameInClass', Observation.attributes);
CameInClass.extend({
    remoteUrl: SadikGlobalSettings.cameInClassUrl,
    autoSaveRemote: true
});
CameInClass.include({
    Type: 'CameInClass'
});
$(window).unload(function () {
    CameInClass.saveLocalDirtyOnly('CameInClasses');
});
$(window).load(function () {
    setInterval(CameInClass.resubmit, CameInClass.resubmitIntervalTime);
    CameInClass.resubmit();
});