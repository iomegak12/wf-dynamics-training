if (typeof (Redivac) == 'undefined') {
    Redivac = { __namespace: true };
}

if (typeof (Redivac.Customizations) == 'undefined') {
    Redivac.Customizations = { __namespace: true };
}

Redivac.Customizations.AdmissionUtils = (function () {
    var processAdmission = function (primaryControl, primaryItemId, selectedItemId) {
        var confirmStrings = {
            text: "Would you like to confirm the current application for admission?",
            title: "Admission Confirmation"
        };

        var confirmOptions = {
            height: 200,
            width: 450
        };

        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions)
            .then(
                function (success) {
                    if (success.confirmed) {
                        var formContext = primaryControl;
                        var validation = formContext != null &&
                            primaryItemId != null;

                        if (!validation) {
                            console.error("Invalid Registered Application Data Specified!");

                            return;
                        }

                        var applicationId = formContext.getControl("csp_applicationname").getAttribute().getValue();
                        var applicationGuid = primaryItemId.toString().replace("{", "").replace("}", "");

                        validation = applicationGuid && applicationId;

                        if (!validation) {
                            Xrm.Navigation.openAlertDialog({
                                text: "Invalid Application ID Specified for Admission!"
                            });

                            return;
                        }

                        var requestBody = {
                            applicationId: applicationId,
                            applicationGuid: applicationGuid
                        };

                        var request = new XMLHttpRequest();
                        var flowServiceUrlVariable = "ADMISSION_PROCESSING_FLOW_URL";
                        var flowUrl = Redivac.Customizations.EnvironmentUtils.getEnvironmentVariable(flowServiceUrlVariable);
                        var results = null;

                        request.open("POST", flowUrl, true);
                        request.setRequestHeader("Content-Type", "application/json");
                        request.setRequestHeader("Accept", "application/json");

                        request.onreadystatechange = function () {
                            if (this.readyState === 4) {
                                request.onreadystatechange = null;

                                if (this.status === 200) {
                                    results = JSON.parse(this.response);

                                    console.info("Request Processing Completed Successfully ... " +
                                        results.toString());
                                }
                                else {
                                    console.error("Invalid Response ... " + this.statusText);
                                }
                            }
                        };

                        request.send(JSON.stringify(requestBody));

                        Xrm.Navigation.openAlertDialog({
                            text: "Student Admission Processing Successfully Initiated ..."
                        });
                    }
                }
            )
    };

    return {
        processAdmission: processAdmission
    };
})();