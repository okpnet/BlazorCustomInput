﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;

namespace BlazorCustomInput.Base
{
    /// <summary>
    /// A clone of the InputBase class. However, a null value for the parameter "EditContext" is allowed.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class CustomComponentBase<TValue> : ComponentBase, IDisposable
    {
        private bool _initialize = false;//初回の初期化後True
        private readonly EventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler;
        private bool _previousParsingAttemptFailed;
        private ValidationMessageStore? _parsingValidationMessages;
        private Type? _nullableUnderlyingType;
        /// <summary>
        /// this element.
        /// </summary>
        protected ElementReference? componentElement;
        /// <summary>
        /// JavaScriptモジュール
        /// </summary>
        IJSObjectReference? jsModule;
        /// <summary>
        /// JavascriotRuntimeインジェクション
        /// </summary>
        [Inject]
        protected IJSRuntime JsRuntime { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        [CascadingParameter]
        EditContext? CascadedEditContext { get; set; }

        /// <summary>
        /// input auto focus
        /// </summary>
        [Parameter]
        public bool AutoFocus { get; set; } = false;

        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        /// <summary>
        /// Gets or sets the value of the input. This should be used with two-way binding.
        /// </summary>
        /// <example>
        /// @bind-Value="model.PropertyName"
        /// </example>
        [Parameter]
        public TValue? Value { get; set; }

        /// <summary>
        /// Gets or sets a callback that updates the bound value.
        /// </summary>
        [Parameter]
        public EventCallback<TValue> ValueChanged { get; set; }

        /// <summary>
        /// Gets or sets an expression that identifies the bound value.
        /// </summary>
        [Parameter]
        public Expression<Func<TValue>>? ValueExpression { get; set; }

        /// <summary>
        /// Gets or sets the display name for this field.
        /// <para>This value is used when generating error messages when the input value fails to parse correctly.</para>
        /// </summary>
        [Parameter]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets the associated <see cref="Forms.EditContext"/>.
        /// </summary>
        protected EditContext? EditContext { get; set; } = default!;

        /// <summary>
        /// Gets the <see cref="FieldIdentifier"/> for the bound value.
        /// </summary>
        protected internal FieldIdentifier FieldIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the current value of the input.
        /// </summary>
        protected TValue? CurrentValue
        {
            get => Value;
            set
            {
                var hasChanged = !EqualityComparer<TValue>.Default.Equals(value, Value);
                //エディットコンテキストに関係無く入力できるようにした。
                Value = value;
                _ = ValueChanged.InvokeAsync(Value);
                if (EditContext is not null && hasChanged)
                {
                    //Value = value;
                    //_ = ValueChanged.InvokeAsync(Value);
                    EditContext?.NotifyFieldChanged(FieldIdentifier);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current value of the input, represented as a string.
        /// </summary>
        protected virtual string? CurrentValueAsString
        {
            get => FormatValueAsString(CurrentValue);
            set
            {
                _parsingValidationMessages?.Clear();

                bool parsingFailed;

                if (_nullableUnderlyingType != null && string.IsNullOrEmpty(value))
                {
                    // Assume if it's a nullable type, null/empty inputs should correspond to default(T)
                    // Then all subclasses get nullable support almost automatically (they just have to
                    // not reject Nullable<T> based on the type itself).
                    parsingFailed = false;
                    CurrentValue = default!;
                }
                else if (TryParseValueFromString(value, out var parsedValue, out var validationErrorMessage))
                {
                    parsingFailed = false;
                    CurrentValue = parsedValue!;
                }
                else
                {
                    parsingFailed = true;
                    if (EditContext is not null)
                    {
                        _parsingValidationMessages ??= new ValidationMessageStore(EditContext);

                        _parsingValidationMessages.Add(FieldIdentifier, validationErrorMessage);

                        // Since we're not writing to CurrentValue, we'll need to notify about modification from here
                        EditContext.NotifyFieldChanged(FieldIdentifier);
                    }
                }

                // We can skip the validation notification if we were previously valid and still are
                if (EditContext is not null && (parsingFailed || _previousParsingAttemptFailed))
                {
                    EditContext.NotifyValidationStateChanged();
                    _previousParsingAttemptFailed = parsingFailed;
                }
            }
        }

        /// <summary>
        /// Constructs an instance of <see cref="InputBase{TValue}"/>.
        /// </summary>
        protected CustomComponentBase()
        {
            _validationStateChangedHandler = OnValidateStateChanged;
        }

        /// <summary>
        /// Formats the value as a string. Derived classes can override this to determine the formating used for <see cref="CurrentValueAsString"/>.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A string representation of the value.</returns>
        protected virtual string? FormatValueAsString(TValue? value)
            => value?.ToString();

        /// <summary>
        /// Parses a string to create an instance of <typeparamref name="TValue"/>. Derived classes can override this to change how
        /// <see cref="CurrentValueAsString"/> interprets incoming values.
        /// </summary>
        /// <param name="value">The string value to be parsed.</param>
        /// <param name="result">An instance of <typeparamref name="TValue"/>.</param>
        /// <param name="validationErrorMessage">If the value could not be parsed, provides a validation error message.</param>
        /// <returns>True if the value could be parsed; otherwise false.</returns>
        protected abstract bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage);

        /// <summary>
        /// Gets a string that indicates the status of the field being edited. This will include
        /// some combination of "modified", "valid", or "invalid", depending on the status of the field.
        /// </summary>
        private string FieldClass
            => EditContext is not null ? EditContext.FieldCssClass(FieldIdentifier) : "";

        /// <summary>
        /// Gets a CSS class string that combines the <c>class</c> attribute and <see cref="FieldClass"/>
        /// properties. Derived components should typically use this value for the primary HTML element's
        /// 'class' attribute.
        /// </summary>
        protected string CssClass
        {
            get
            {
                if (AdditionalAttributes != null &&
                    AdditionalAttributes.TryGetValue("class", out var @class) &&
                    !string.IsNullOrEmpty(Convert.ToString(@class, CultureInfo.InvariantCulture)))
                {
                    return $"{@class} {FieldClass}";
                }

                return FieldClass; // Never null or empty
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        /// <summary>
        /// JSモジュールの読み込み
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender) await JsLoadFocusModuleAsync();
        }

        public async ValueTask FocusAsync()
        {
            if(jsModule is null) await JsLoadFocusModuleAsync();
            await jsModule!.InvokeVoidAsync(Define.JS_SET_FOCUS, componentElement);
        }
        /// <inheritdoc />
        /// OnInitializedよりも前｡最初に呼ばれる
        //[MemberNotNull(nameof(EditContext), nameof(CascadedEditContext))]
        public override Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (!_initialize && EditContext == null)
            {
                _initialize = true;
                // This is the first run
                // Could put this logic in OnInit, but its nice to avoid forcing people who override OnInit to call base.OnInit()

                //if (CascadedEditContext == null)
                //{
                //    throw new InvalidOperationException($"{GetType()} requires a cascading parameter " +
                //        $"of type {nameof(Forms.EditContext)}. For example, you can use {GetType().FullName} inside " +
                //        $"an {nameof(EditForm)}.");
                //}

                if (ValueExpression == null)
                {
                    throw new InvalidOperationException($"{GetType()} requires a value for the 'ValueExpression' " +
                        $"parameter. Normally this is provided automatically when using 'bind-Value'.");
                }

                EditContext = CascadedEditContext;
                FieldIdentifier = FieldIdentifier.Create(ValueExpression);
                _nullableUnderlyingType = Nullable.GetUnderlyingType(typeof(TValue));
                if (EditContext is not null)
                {
                    EditContext.OnValidationStateChanged += _validationStateChangedHandler;
                }
            }
            else if (CascadedEditContext != EditContext)
            {
                // Not the first run

                // We don't support changing EditContext because it's messy to be clearing up state and event
                // handlers for the previous one, and there's no strong use case. If a strong use case
                // emerges, we can consider changing this.
                throw new InvalidOperationException($"{GetType()} does not support changing the " +
                    $"{nameof(Microsoft.AspNetCore.Components.Forms.EditContext)} dynamically.");
            }

            UpdateAdditionalValidationAttributes();

            // For derived components, retain the usual lifecycle with OnInit/OnParametersSet/etc.
            return base.SetParametersAsync(ParameterView.Empty);
        }
        /// <summary>
        /// load and set focus jsavascript modules
        /// </summary>
        /// <returns></returns>
        private async Task JsLoadFocusModuleAsync()
        {
            var module = "export function " + Define.JS_SET_FOCUS + "(element) { return element !== null && typeof element === 'object' & '" + Define.JS_SET_FOCUS + "' in element ? element." + Define.JS_SET_FOCUS + "() : null;};";
            jsModule ??=await JsRuntime.InvokeAsync<IJSObjectReference>("import", "data:text/javascript;charset=utf-8," + Uri.EscapeDataString(module));
        }

        private void OnValidateStateChanged(object? sender, ValidationStateChangedEventArgs eventArgs)
        {
            UpdateAdditionalValidationAttributes();

            StateHasChanged();
        }

        private void UpdateAdditionalValidationAttributes()
        {
            var hasAriaInvalidAttribute = AdditionalAttributes != null && AdditionalAttributes.ContainsKey("aria-invalid");
            if (EditContext is not null && EditContext.GetValidationMessages(FieldIdentifier).Any())
            {
                if (hasAriaInvalidAttribute)
                {
                    // Do not overwrite the attribute value
                    return;
                }

                if (ConvertToDictionary(AdditionalAttributes, out var additionalAttributes))
                {
                    AdditionalAttributes = additionalAttributes;
                }

                // To make the `Input` components accessible by default
                // we will automatically render the `aria-invalid` attribute when the validation fails
                // value must be "true" see https://www.w3.org/TR/wai-aria-1.1/#aria-invalid
                additionalAttributes["aria-invalid"] = "true";
            }
            else if (hasAriaInvalidAttribute)
            {
                // No validation errors. Need to remove `aria-invalid` if it was rendered already

                if (AdditionalAttributes!.Count == 1)
                {
                    // Only aria-invalid argument is present which we don't need any more
                    AdditionalAttributes = null;
                }
                else
                {
                    if (ConvertToDictionary(AdditionalAttributes, out var additionalAttributes))
                    {
                        AdditionalAttributes = additionalAttributes;
                    }

                    additionalAttributes.Remove("aria-invalid");
                }
            }
        }

        /// <summary>
        /// Returns a dictionary with the same values as the specified <paramref name="source"/>.
        /// </summary>
        /// <returns>true, if a new dictrionary with copied values was created. false - otherwise.</returns>
        private bool ConvertToDictionary(IReadOnlyDictionary<string, object>? source, out Dictionary<string, object> result)
        {
            var newDictionaryCreated = true;
            if (source == null)
            {
                result = new Dictionary<string, object>();
            }
            else if (source is Dictionary<string, object> currentDictionary)
            {
                result = currentDictionary;
                newDictionaryCreated = false;
            }
            else
            {
                result = new Dictionary<string, object>();
                foreach (var item in source)
                {
                    result.Add(item.Key, item.Value);
                }
            }

            return newDictionaryCreated;
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
        }

        void IDisposable.Dispose()
        {
            // When initialization in the SetParametersAsync method fails, the EditContext property can remain equal to null
            if (EditContext is not null)
            {
                EditContext.OnValidationStateChanged -= _validationStateChangedHandler;
            }

            Dispose(disposing: true);
        }

        
    }
}
