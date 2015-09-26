﻿/*
 * jQuery wizard plug-in 3.0.7 (18-SEPT-2012)
 *
 *
 * Copyright (c) 2012 Jan Sundman (jan.sundman[at]aland.net)
 *
 * http://www.thecodemine.org
 *
 * Licensed under the MIT licens:
 *   http://www.opensource.org/licenses/mit-license.php
 *
 */
(function (e) { e.widget("ui.formwizard", { _init: function () { var t = this; var n = this.options.formOptions.success; var r = this.options.formOptions.complete; var i = this.options.formOptions.beforeSend; var s = this.options.formOptions.beforeSubmit; var o = this.options.formOptions.beforeSerialize; this.options.formOptions = e.extend(this.options.formOptions, { success: function (e, r, i) { if (n) { n(e, r, i) } if (t.options.formOptions && t.options.formOptions.resetForm || !t.options.formOptions) { t._reset() } }, complete: function (e, n) { if (r) { r(e, n) } t._enableNavigation() }, beforeSubmit: function (e, n, r) { if (s) { var i = s(e, n, r); if (!i) t._enableNavigation(); return i } }, beforeSend: function (e) { if (i) { var n = i(e); if (!n) t._enableNavigation(); return n } }, beforeSerialize: function (e, n) { if (o) { var r = o(e, n); if (!r) t._enableNavigation(); return r } } }); if (this.options.historyEnabled) { e.bbq.removeState("_" + e(this.element).attr("id")) } this.steps = this.element.find(".step").hide(); this.firstStep = this.steps.eq(0).attr("id"); this.activatedSteps = new Array; this.isLastStep = false; this.previousStep = undefined; this.currentStep = this.steps.eq(0).attr("id"); this.nextButton = this.element.find(this.options.next).click(function () { return t._next() }); this.nextButtonInitinalValue = this.nextButton.val(); this.nextButton.val(this.options.textNext); this.backButton = this.element.find(this.options.back).click(function () { t._back(); return false }); this.backButtonInitinalValue = this.backButton.val(); this.backButton.val(this.options.textBack); if (this.options.validationEnabled && jQuery().validate == undefined) { this.options.validationEnabled = false; if (window["console"] !== undefined) { console.log("%s", "validationEnabled option set, but the validation plugin is not included") } } else if (this.options.validationEnabled) { this.element.validate(this.options.validationOptions) } if (this.options.formPluginEnabled && jQuery().ajaxSubmit == undefined) { this.options.formPluginEnabled = false; if (window["console"] !== undefined) { console.log("%s", "formPluginEnabled option set but the form plugin is not included") } } if (this.options.disableInputFields == true) { e(this.steps).find(":input:not('.wizard-ignore')").attr("disabled", "disabled") } if (this.options.historyEnabled) { e(window).bind("hashchange", undefined, function (n) { var r = n.getState("_" + e(t.element).attr("id")) || t.firstStep; if (r !== t.currentStep) { if (t.options.validationEnabled && r === t._navigate(t.currentStep)) { if (!t.element.valid()) { t._updateHistory(t.currentStep); t.element.validate().focusInvalid(); return false } } if (r !== t.currentStep) t._show(r) } }) } this.element.addClass("ui-formwizard"); this.element.find(":input").addClass("ui-wizard-content"); this.steps.addClass("ui-formwizard-content"); this.backButton.addClass("ui-formwizard-button ui-wizard-content"); this.nextButton.addClass("ui-formwizard-button ui-wizard-content"); if (!this.options.disableUIStyles) { this.element.addClass("ui-helper-reset ui-widget ui-widget-content ui-helper-reset ui-corner-all"); this.element.find(":input").addClass("ui-helper-reset ui-state-default"); this.steps.addClass("ui-helper-reset ui-corner-all"); this.backButton.addClass("ui-helper-reset ui-state-default"); this.nextButton.addClass("ui-helper-reset ui-state-default") } this._show(undefined); return e(this) }, _next: function () { if (this.options.validationEnabled) { if (!this.element.valid()) { this.element.validate().focusInvalid(); return false } } if (this.options.remoteAjax != undefined) { var t = this.options.remoteAjax[this.currentStep]; var n = this; if (t !== undefined) { var r = t.success; var i = t.beforeSend; var s = t.complete; t = e.extend({}, t, { success: function (e, t) { if (r !== undefined && r(e, t) || r == undefined) { n._continueToNextStep() } }, beforeSend: function (t) { n._disableNavigation(); if (i !== undefined) i(t); e(n.element).trigger("before_remote_ajax", { currentStep: n.currentStep }) }, complete: function (t, r) { if (s !== undefined) s(t, r); e(n.element).trigger("after_remote_ajax", { currentStep: n.currentStep }); n._enableNavigation() } }); this.element.ajaxSubmit(t); return false } } return this._continueToNextStep() }, _back: function () { if (this.activatedSteps.length > 0) { if (this.options.historyEnabled) { this._updateHistory(this.activatedSteps[this.activatedSteps.length - 2]) } else { this._show(this.activatedSteps[this.activatedSteps.length - 2], true) } } return false }, _continueToNextStep: function () { if (this.isLastStep) { for (var e = 0; e < this.activatedSteps.length; e++) { this.steps.filter("#" + this.activatedSteps[e]).find(":input").not(".wizard-ignore").removeAttr("disabled") } if (!this.options.formPluginEnabled) { return true } else { this._disableNavigation(); this.element.ajaxSubmit(this.options.formOptions); return false } } var t = this._navigate(this.currentStep); if (t == this.currentStep) { return false } if (this.options.historyEnabled) { this._updateHistory(t) } else { this._show(t, true) } return false }, _updateHistory: function (t) { var n = {}; n["_" + e(this.element).attr("id")] = t; e.bbq.pushState(n) }, _disableNavigation: function () { this.nextButton.attr("disabled", "disabled"); this.backButton.attr("disabled", "disabled"); if (!this.options.disableUIStyles) { this.nextButton.removeClass("ui-state-active").addClass("ui-state-disabled"); this.backButton.removeClass("ui-state-active").addClass("ui-state-disabled") } }, _enableNavigation: function () { if (this.isLastStep) { this.nextButton.val(this.options.textSubmit) } else { this.nextButton.val(this.options.textNext) } if (e.trim(this.currentStep) !== this.steps.eq(0).attr("id")) { this.backButton.removeAttr("disabled"); if (!this.options.disableUIStyles) { this.backButton.removeClass("ui-state-disabled").addClass("ui-state-active") } } this.nextButton.removeAttr("disabled"); if (!this.options.disableUIStyles) { this.nextButton.removeClass("ui-state-disabled").addClass("ui-state-active") } }, _animate: function (e, t, n) { this._disableNavigation(); var r = this.steps.filter("#" + e); var i = this.steps.filter("#" + t); r.find(":input").not(".wizard-ignore").attr("disabled", "disabled"); i.find(":input").not(".wizard-ignore").removeAttr("disabled"); var s = this; r.animate(s.options.outAnimation, s.options.outDuration, s.options.easing, function () { i.animate(s.options.inAnimation, s.options.inDuration, s.options.easing, function () { if (s.options.focusFirstInput) i.find(":input:first").focus(); s._enableNavigation(); n.apply(s) }); return }) }, _checkIflastStep: function (t) { this.isLastStep = false; if (e("#" + t).hasClass(this.options.submitStepClass) || this.steps.filter(":last").attr("id") == t) { this.isLastStep = true } }, _getLink: function (t) { var n = undefined; var r = this.steps.filter("#" + t).find(this.options.linkClass); if (r != undefined) { if (r.filter(":radio,:checkbox").size() > 0) { n = r.filter(this.options.linkClass + ":checked").val() } else { n = e(r).val() } } return n }, _navigate: function (e) { var t = this._getLink(e); if (t != undefined) { if (t != "" && t != null && t != undefined && this.steps.filter("#" + t).attr("id") != undefined) { return t } return this.currentStep } else if (t == undefined && !this.isLastStep) { var n = this.steps.filter("#" + e).next().attr("id"); return n } }, _show: function (t) { var n = false; var r = t !== undefined; if (t == undefined || t == "") { this.activatedSteps.pop(); t = this.firstStep; this.activatedSteps.push(t) } else { if (e.inArray(t, this.activatedSteps) > -1) { n = true; this.activatedSteps.pop() } else { this.activatedSteps.push(t) } } if (this.currentStep !== t || t === this.firstStep) { this.previousStep = this.currentStep; this._checkIflastStep(t); this.currentStep = t; var i = function () { if (r) { e(this.element).trigger("step_shown", e.extend({ isBackNavigation: n }, this._state())) } }; if (r) { e(this.element).trigger("before_step_shown", e.extend({ isBackNavigation: n }, this._state())) } this._animate(this.previousStep, t, i) } }, _reset: function () { this.element.resetForm(); e("label,:input,textarea", this).removeClass("error"); for (var t = 0; t < this.activatedSteps.length; t++) { this.steps.filter("#" + this.activatedSteps[t]).hide().find(":input").attr("disabled", "disabled") } this.activatedSteps = new Array; this.previousStep = undefined; this.isLastStep = false; if (this.options.historyEnabled) { this._updateHistory(this.firstStep) } else { this._show(this.firstStep) } }, _state: function (e) { var t = { settings: this.options, activatedSteps: this.activatedSteps, isLastStep: this.isLastStep, isFirstStep: this.currentStep === this.firstStep, previousStep: this.previousStep, currentStep: this.currentStep, backButton: this.backButton, nextButton: this.nextButton, steps: this.steps, firstStep: this.firstStep }; if (e !== undefined) return t[e]; return t }, show: function (e) { if (this.options.historyEnabled) { this._updateHistory(e) } else { this._show(e) } }, state: function (e) { return this._state(e) }, reset: function () { this._reset() }, next: function () { this._next() }, back: function () { this._back() }, destroy: function () { this.element.find("*").removeAttr("disabled").show(); this.nextButton.unbind("click").val(this.nextButtonInitinalValue).removeClass("ui-state-disabled").addClass("ui-state-active"); this.backButton.unbind("click").val(this.backButtonInitinalValue).removeClass("ui-state-disabled").addClass("ui-state-active"); this.backButtonInitinalValue = undefined; this.nextButtonInitinalValue = undefined; this.activatedSteps = undefined; this.previousStep = undefined; this.currentStep = undefined; this.isLastStep = undefined; this.options = undefined; this.nextButton = undefined; this.backButton = undefined; this.formwizard = undefined; this.element = undefined; this.steps = undefined; this.firstStep = undefined }, update_steps: function () { this.steps = this.element.find(".step").addClass("ui-formwizard-content"); this.firstStep = this.steps.eq(0).attr("id"); this.steps.not("#" + this.currentStep).hide().find(":input").addClass("ui-wizard-content").attr("disabled", "disabled"); this._checkIflastStep(this.currentStep); this._enableNavigation(); if (!this.options.disableUIStyles) { this.steps.addClass("ui-helper-reset ui-corner-all"); this.steps.find(":input").addClass("ui-helper-reset ui-state-default") } }, options: { historyEnabled: false, validationEnabled: false, validationOptions: undefined, formPluginEnabled: false, linkClass: ".link", submitStepClass: "submit_step", back: ":reset", next: ":submit", textSubmit: "Submit", textNext: "Next", textBack: "Back", remoteAjax: undefined, inAnimation: { opacity: "show" }, outAnimation: { opacity: "hide" }, inDuration: 400, outDuration: 400, easing: "swing", focusFirstInput: false, disableInputFields: true, formOptions: { reset: true, success: function (e) { if (window["console"] !== undefined) { console.log("%s", "form submit successful") } }, disableUIStyles: false } } }) })(jQuery);

