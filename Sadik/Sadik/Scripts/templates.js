angular.module('ObservationsApp').run(['$templateCache', function($templateCache) {
  'use strict';

  $templateCache.put('ViewsAngular/Observation/ActivityListItem.html',
    "<div class=\"ActivityListItemContainer_WaitUpload observationCell\"><img src=\"/Content/images/spinner.gif\" ng-show=\"observation.isDirty\" width=\"15\" height=\"15\" alt=\"wait\"><div class=\"dummy20\">&nbsp;</div></div><div class=\"ActivityListItemContainer_DateObserved observationCell\">{{observation.DateObserved | date:'dd MMMM yyyy HH:mm'}}</div><div class=\"ActivityListItemContainer_InventoryItem observationCell\">{{observation.ItemId | InventoryName:itemsSource}}</div><div class=\"ActivityListItemContainer_ChoseHimSelf observationCell\" ng-switch=\"observation.ChoseHimSelf\"><span ng-switch-when=\"true\">Сам</span><div ng-switch-default class=\"nbsp\"></div></div><div class=\"ActivityListItemContainer_Polarization observationCell\" ng-switch=\"observation.Polarization\"><span ng-switch-when=\"true\">*</span><div ng-switch-default class=\"nbsp\"></div></div><div class=\"ActivityListItemContainer_Duration observationCell\">{{observation.Duration | ActivityDuration}}</div><div class=\"ActivityListItemContainer_TeacherName observationCell\">{{observation.TeacherName}}</div><div class=\"ActivityListItemContainer_DeleteButton observationCell\"><button ng-click=\"delete(observation.Id)\" ng-if=\"canDelete\">Удалить</button></div><div class=\"ActivityListItemContainer_EditButton observationCell\"><button ng-click=\"edit(observation.Id)\" ng-if=\"canEdit\">Редактировать</button></div><div class=\"clearFloatBoth\"></div><div class=\"ActivityListItemContainer_Comment js_comment_container\" ng-if=\"observation.Comment\"><span class=\"commentText\">{{observation.Comment | textLimit:140}}</span></div>"
  );


  $templateCache.put('ViewsAngular/Observation/CameInClassListItem.html',
    "<div class=\"CameInClassListItemContainer_WaitUpload observationCell\"><img src=\"/Content/images/spinner.gif\" ng-show=\"observation.isDirty\" width=\"15\" height=\"15\" alt=\"wait\"><div class=\"dummy20\">&nbsp;</div></div><div class=\"CameInClassListItemContainer_DateObserved observationCell\">{{observation.DateObserved | date:'dd MMMM yyyy HH:mm'}}</div><div class=\"CameInClassListItemContainer_Dummy observationCell\">Пришел в класс<div class=\"nbsp\"></div></div><div class=\"CameInClassListItemContainer_TeacherName observationCell\">{{observation.TeacherName}}</div><div class=\"CameInClassListItemContainer_DeleteButton observationCell\"><button ng-click=\"delete(observation.Id)\" ng-if=\"canDelete\">Удалить</button></div><div class=\"CameInClassListItemContainer_EditButton observationCell\"><button ng-click=\"edit(observation.Id)\" ng-if=\"canEdit\">Редактировать</button></div><div class=\"clearFloatBoth\"></div><div class=\"CameInClassListItemContainer_Comment js_comment_container\" ng-if=\"observation.Comment\"><span class=\"commentText\">{{observation.Comment | textLimit:140}}</span></div>"
  );


  $templateCache.put('ViewsAngular/Observation/EmotionListItem.html',
    "<div class=\"EmotionListItemContainer_WaitUpload observationCell\"><img src=\"/Content/images/spinner.gif\" ng-show=\"observation.isDirty\" width=\"15\" height=\"15\" alt=\"wait\"><div class=\"dummy20\">&nbsp;</div></div><div class=\"EmotionListItemContainer_DateObserved observationCell\">{{observation.DateObserved | date:'dd MMMM yyyy HH:mm'}}</div><div class=\"EmotionListItemContainer_EmotionType observationCell\">{{observation.Emotion | EmotionName}}</div><div class=\"EmotionListItemContainer_Dummy observationCell\"><div class=\"nbsp\"></div></div><div class=\"EmotionListItemContainer_TeacherName observationCell\">{{observation.TeacherName}}</div><div class=\"EmotionListItemContainer_DeleteButton observationCell\"><button ng-click=\"delete(observation.Id)\" ng-if=\"canDelete\">Удалить</button></div><div class=\"EmotionListItemContainer_EditButton observationCell\"><button ng-click=\"edit(observation.Id)\" ng-if=\"canEdit\">Редактировать</button></div><div class=\"clearFloatBoth\"></div><div class=\"EmotionListItemContainer_Comment js_comment_container\" ng-if=\"observation.Comment\"><span class=\"commentText\">{{observation.Comment | textLimit:140}}</span></div>"
  );


  $templateCache.put('ViewsAngular/Observation/ObservationList.html',
    "<ul class=\"observationList\"><li ng-repeat=\"observation in observations | orderBy:'-DateObserved' | filter:filteredObservations\" ng-switch=\"observation.Type\"><div ng-switch-when=\"Activity\" class=\"ActivityListItemContainer js_ObservationItem js_ObservationItemActivity\"><ng-include src=\"'/ViewsAngular/Observation/ActivityListItem.html'\"></ng-include></div><div ng-switch-when=\"CameInClass\" class=\"CameInClassListItemContainer js_ObservationItem js_ObservationItemCameInClass\"><ng-include src=\"'/ViewsAngular/Observation/CameInClassListItem.html'\"></ng-include></div><div ng-switch-when=\"Emotion\" class=\"EmotionListItemContainer js_ObservationItem js_ObservationItemEmotion\"><ng-include src=\"'/ViewsAngular/Observation/EmotionListItem.html'\"></ng-include></div></li></ul>"
  );


  $templateCache.put('ViewsAngular/Observation/ObservationListFilter.html',
    "<div class=\"observationListFilter\"><a href=\"javascript:void(0)\" class=\"js_filterObservationsAll filterLink\" ng-click=\"setFilterType('')\">Все</a> <a href=\"javascript:void(0)\" class=\"js_filterObservationsActivities filterLink\" ng-click=\"setFilterType('Activity')\">Работа с материалами</a> <a href=\"javascript:void(0)\" class=\"js_filterObservationsCameInClass filterLink\" ng-click=\"setFilterType('CameInClass')\">Вход в класс</a> <a href=\"javascript:void(0)\" class=\"js_filterObservationsEmotion filterLink\" ng-click=\"setFilterType('Emotion')\">Настроение</a> <a href=\"javascript:void(0)\" class=\"js_filterObservationsComments filterLink\" ng-click=\"setFilterType('WithComments')\">Комментарии</a><div class=\"clearFloatBoth\"></div></div>"
  );

}]);
