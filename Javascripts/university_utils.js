if (typeof (Redivac) == 'undefined') {
    Redivac = { __namespace: true };
}

if (typeof (Redivac.Customizations) == 'undefined') {
    Redivac.Customizations = { __namespace: true };
}

Redivac.Customizations.UniversityUtils = (function () {
    var getCampusLocation = function (universityId) {
        if (!universityId) {
            console.error("Invalid University Id Specified!");

            return;
        }

        var req = new XMLHttpRequest();
        var clientUrl = Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/csp_universities(" +
            universityId + ")?$select=csp_campuslocation"

        req.open("GET", clientUrl, false);

        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Prefer", "odata.include-annotations=*");

        var result = null;
        var campusLocation = null;

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

        campusLocation = result["csp_campuslocation"];

        return campusLocation;
    };

    return {
        getCampusLocation: getCampusLocation
    };
})();