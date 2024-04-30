if (typeof (Redivac) == 'undefined') {
    Redivac = { __namespace: true };
}

if (typeof (Redivac.Customizations) == 'undefined') {
    Redivac.Customizations = { __namespace: true };
}

Redivac.Customizations.TransportServiceUtils = (function () {
    var getTransportationCharges = function (campusLocation) {
        if (!campusLocation) {
            console.log("Invalid Campus Location Specified!");

            return;
        };

        var req = new XMLHttpRequest();
        var result = null;
        var environmentVariableName = "TRANSPORT_SERVICE_URL";
        var clientBaseUrl = Redivac.Customizations.EnvironmentUtils.getEnvironmentVariable(environmentVariableName);
        var clientUrl = clientBaseUrl + "/" + campusLocation;

        req.open("GET", clientUrl, false);
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Accept", "application/json");

        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;

                if (this.status === 200) {
                    result = JSON.parse(this.response);
                } else {
                    console.log(this.responseText);
                }
            }
        };

        req.send();

        var transportationCharges = result.TransportationCharges;

        return transportationCharges;
    };

    return {
        getTransportationCharges: getTransportationCharges
    };
})();