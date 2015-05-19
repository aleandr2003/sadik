function Collapsible(control, box) {
    var self = this;
    var _control = control;
    var _box = box;
    var _text = _control.text();

    this.Collapse = function () {
        _box.hide();
        _control.text(_text + ' +');
    }

    this.Expand = function () {
        _box.show();
        _control.text(_text + ' -');
    }

    this.IsExpanded = function () {
        return _box.is(':visible');
    }

    _control.click(function () {
        if (self.IsExpanded()) {
            self.Collapse();
        } else {
            self.Expand();
        }
    });
}

Collapsible.prototype.constructor = Collapsible;