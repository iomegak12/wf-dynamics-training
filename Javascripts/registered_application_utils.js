if (typeof (Redivac) == 'undefined') {
    Redivac = { __namespace: true };
}

if (typeof (Redivac.Customizations) == 'undefined') {
    Redivac.Customizations = { __namespace: true };
}

Redivac.Customizations.RegisteredApplicationUtils = (function () {
    var onChangeUniversity = function (formExecutionContext) {
        if (!formExecutionContext) {
            console.error("Invalid Form Execution Context Specified!");

            return;
        }

        var formContext = formExecutionContext.getFormContext();
        var universityId = formContext.getAttribute("csp_university").getValue();

        if (!universityId) {
            formContext.getAttribute("csp_transportationfees").setValue(0);

            return;
        }

        var cleanedUniversityId = universityId[0].id.replace("{", "").replace("}", "");
        var campusLocation = Redivac.Customizations.UniversityUtils.getCampusLocation(cleanedUniversityId);
        var transportationCharges = Redivac.Customizations.TransportServiceUtils.getTransportationCharges(campusLocation);

        formContext.getAttribute("csp_transportationfees").setValue(transportationCharges);

        console.info("Transportation Charges Processing Completed Successfully ...");
    };

    return {
        onChangeUniversity: onChangeUniversity
    }
})();
