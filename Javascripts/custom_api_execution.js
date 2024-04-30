if (typeof (Redivac) == 'undefined') {
    Redivac = { __namespace: true };
}

if (typeof (Redivac.Customizations) == 'undefined') {
    Redivac.Customizations = { __namespace: true };
}

Redivac.Customizations.CustomAPIUtils = (function () {
    var executeCustomAPI = function () {
        var parameters = {};
        parameters.csp_universityid = "UNIV10001"; // Edm.String
        parameters.csp_universityName = "Madurai Kamarajar University"; // Edm.String

        var req = new XMLHttpRequest();
        req.open("POST", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/csp_simplecustomapi", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Accept", "application/json");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200 || this.status === 204) {
                    var result = JSON.parse(this.response);
                    console.log(result);
                    // Return Type: mscrm.csp_simplecustomapiResponse
                    // Output Parameters
                    var csp_response = result["csp_response"]; // Edm.String

                    Xrm.Navigation.openAlertDialog({
                        text: csp_response
                    });
                } else {
                    console.log(this.responseText);
                }
            }
        };
        req.send(JSON.stringify(parameters));
    };

    return {
        executeCustomAPI: executeCustomAPI
    };
})();