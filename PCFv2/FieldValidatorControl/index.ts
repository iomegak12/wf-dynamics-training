import { IInputs, IOutputs } from "./generated/ManifestTypes";

export class FieldValidatorControl implements ComponentFramework.StandardControl<IInputs, IOutputs> {
    private _context: ComponentFramework.Context<IInputs>;
    private _notifyOutputChanged: () => void;
    private _container: HTMLDivElement;
    private _inputElement: HTMLInputElement;
    private _errorContainer: HTMLDivElement;
    private _regexPattern: string;
    private _errorMessage: string;

    constructor() { }

    public init(
        context: ComponentFramework.Context<IInputs>,
        notifyOutputChanged: () => void,
        state: ComponentFramework.Dictionary,
        container: HTMLDivElement
    ): void {
        this._context = context;
        this._notifyOutputChanged = notifyOutputChanged;
        this._container = container;

        // Get input properties
        this._regexPattern = context.parameters.regexPattern.raw || "";
        this._errorMessage = context.parameters.errorMessage.raw || "";

        // Set up initial view
        this.createInputField();
        this.createErrorContainer();

        // Set previous value in the input field
        const previousValue = context.parameters.fieldToValidate.formatted || "";
        this._inputElement.value = previousValue;
    }

    private createInputField(): void {
        this._inputElement = document.createElement("input");
        this._inputElement.type = "text";
        this._inputElement.placeholder = "Enter value for validation";

        // Attach an event listener for real-time validation
        this._inputElement.addEventListener("input", (event) => this.handleInputChange(event));

        // Append the input element to the container
        this._container.appendChild(this._inputElement);
    }

    private createErrorContainer(): void {
        // Create icon label
        const errorIconLabelElement = document.createElement("label");
        errorIconLabelElement.innerHTML = "";
        errorIconLabelElement.classList.add("icon");

        // Create error message label
        const errorLabelElement = document.createElement("label");
        errorLabelElement.classList.add("errorMessage");

        // Wrap labels in a div (error container)
        this._errorContainer = document.createElement("div");
        this._errorContainer.classList.add("Error");
        this._errorContainer.appendChild(errorIconLabelElement);
        this._errorContainer.appendChild(errorLabelElement);

        // Append the error container to the container
        this._container.appendChild(this._errorContainer);
    }

    private handleInputChange(event: Event): void {
        const inputValue = (event.target as HTMLInputElement).value;

        // Validate input value using regex
        const isValid = this.validateField(inputValue, this._regexPattern);

        // Display or hide error message based on validation result
        this.displayErrorMessage(!isValid, inputValue);

        // Notify the framework about the change
        this._notifyOutputChanged();
    }

    private validateField(value: string, regexPattern: string): boolean {
        // Allow null or empty value
        if (!value) {
            return true;
        }

        const regex = new RegExp(regexPattern);
        return regex.test(value);
    }

    private displayErrorMessage(show: boolean, inputValue: string): void {
        // Add or remove CSS classes based on validation result
        this._inputElement.classList.toggle("incorrect", show);
        this._errorContainer.classList.toggle("inputError", show);

        // Set error message based on validation result
        const errorMessage = show ? (this._errorMessage || `Entered input '${inputValue}' is incorrect`) : "";
        this._errorContainer.querySelector(".errorMessage")!.innerHTML = errorMessage;
    }

    public updateView(context: ComponentFramework.Context<IInputs>): void {
        // Implement logic to update the view if needed
    }

    public getOutputs(): IOutputs {
        // Return the validated input value using the output interface
        const inputValue = this._inputElement.value;
        return {
            fieldToValidate: inputValue
        };
    }

    public destroy(): void {
        // Cleanup logic here
    }
}