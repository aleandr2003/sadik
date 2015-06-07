DateCustom = {
    printDate: function (dt) {
        var dateStr = this.padStr(dt.getDate()) + '-' +
                      this.padStr(1 + dt.getMonth()) + '-' +
                      this.padStr(dt.getFullYear())
        return dateStr;
    },

    printDateTime: function (dt) {
        var dateStr = this.padStr(dt.getFullYear()) + '-' +
                        this.padStr(1 + dt.getMonth()) + '-' +
                        this.padStr(dt.getDate()) + ' ' +
                        this.padStr(dt.getHours()) + ':' +
                        this.padStr(dt.getMinutes()) + ':' +
                        this.padStr(dt.getSeconds())
        return dateStr;
    },

    padStr: function (i) {
        return (i < 10) ? "0" + i : "" + i;
    }
};