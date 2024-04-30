if (typeof (Redivac) == 'undefined') {
    Redivac = { __namespace: true };
}

if (typeof (Redivac.Customizations) == 'undefined') {
    Redivac.Customizations = { __namespace: true };
}

Redivac.Customizations.EnvironmentUtils = (function () {
    var prepareFetchXmlString = function (environmentVariableName) {
        var fetchXmlString =
            `<fetch top="1" distinct="true">
                <entity name="environmentvariablevalue">
                    <attribute name="value" />
                    <attribute name="environmentvariablevalueid" />
                    <link-entity name="environmentvariabledefinition" from="environmentvariabledefinitionid" to="environmentvariabledefinitionid" link-type="inner" alias="environmentvariabledefinition">
                    <attribute name="displayname" />
                    <filter>
                        <condition attribute="displayname" operator="eq" value="${environmentVariableName}" />
                    </filter>
                    </link-entity>
                </entity>
            </fetch>`;

        return fetchXmlString;
    };

    var getEnvironmentVariable = function (environmentVariableName) {
        var originalFetchXML = prepareFetchXmlString(environmentVariableName);
        var escapedFetchXML = encodeURIComponent(originalFetchXML);
        var req = new XMLHttpRequest();
        var results = null;

        var clientUrl = Xrm.Utility.getGlobalContext().getClientUrl() + 
            "/api/data/v9.2/environmentvariablevalues?fetchXml=" + 
            escapedFetchXML;

        req.open("GET", clientUrl, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Prefer", "odata.include-annotations=*");

        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;

                if (this.status === 200) {
                    results = JSON.parse(this.response);
                } else {
                    console.log(this.responseText);
                }
            }
        };

        req.send();

        var environmentVariableValue = results.value[0].value;

        return environmentVariableValue;
    };

    return {
        getEnvironmentVariable: getEnvironmentVariable
    }
})();