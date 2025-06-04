using BlazorCustomInput.Argments;
using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// The convertible select is custom selectbox suported.
    /// it also get property value from class object, and set it value to component.
    /// </summary>
    /// <typeparam name="Tval"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    public class EditorConvertibleSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSource> : EditBase<Tval>
    {
        private readonly bool _isMultipleSelect;
        private IEnumerable<TSource> _source;
        /// <summary>
        /// Gets or sets the child content to be rendering inside the select element.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }
        /// <summary>
        /// Gets source to a select option. 
        /// </summary>
        [Parameter, EditorRequired]
        public Func<IEnumerable<TSource>> Source { get; set; } = default!;
        /// <summary>
        /// The delegate is TSource convert to TVal.
        /// </summary>
        [Parameter, EditorRequired]
        public Func<TSource,Tval> ValueConverter { get; set; } = default!;
        /// <summary>
        /// Argment null value contents.
        /// </summary>
        [Parameter]
        public string? ChoosePromptContents { get; set; }
        /// <summary>
        /// Rendering string the options. 
        /// </summary>
        [Parameter, EditorRequired]
        public Func<TSource?, string> OptionContents { get; set; } = default!;

        public EditorConvertibleSelect()
        {
            _isMultipleSelect = typeof(TSource).IsArray;
            _source=Enumerable.Empty<TSource>();
        }

        protected override void OnParametersSet()
        {
            _source = Source();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int index = 0;
            var selectedItem = _source.FirstOrDefault(t => Equals(ValueConverter(t),Value));
            builder.OpenElement(index++, "select");
            builder.AddMultipleAttributes(index++, AdditionalAttributes);
            builder.AddAttribute(index++, "class", CssClass);
            builder.AddAttribute(index, "disabled", IsDisabled);
            builder.AddAttribute(index++, "multiple", _isMultipleSelect);

            if (_isMultipleSelect)
            {
                builder.AddAttribute(index++, "value", BindConverter.FormatValue(CurrentValue)?.ToString());
                builder.AddAttribute(index++, "onchange", EventCallback.Factory.CreateBinder<string?[]?>(this, SetCurrentValueAsStringArray, default));
                builder.SetUpdatesAttributeName("value");
            }
            else
            {
                index += 3;
                builder.AddAttribute(index++, "value", CurrentValueAsString);
                builder.AddAttribute(index++, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, default));
                builder.SetUpdatesAttributeName("value");
            }

            builder.AddElementReferenceCapture(index++, __inputReference => Element = __inputReference);
            builder.AddElementReferenceCapture(index++, __selectReference => Element = __selectReference);

            if(ChoosePromptContents is not (null or ""))
            {
                builder.OpenElement(index++, "option");
                builder.AddAttribute(index++, "hidden");
                builder.AddMarkupContent(index++, ChoosePromptContents);
                builder.CloseElement();
            }
            else
            {
                index += 4;
            }

            foreach(var item in _source)
            {
                if(item is null)
                {
                    continue;
                }
                var value = ValueConverter(item);
                var arg = new OptionArgment<TSource>(item);
                builder.OpenElement(index++, "option");
                builder.AddAttribute(index++, "value", item.GetHashCode());
                if (selectedItem is not null && Equals(selectedItem, item))
                {
                    builder.AddAttribute(++index, "selected");
                }
                builder.AddMarkupContent(index++, OptionContents(item));
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        protected override bool ConvetTo(string? value, out Tval? result)
        {
            if (value is null or "")
            {
                result = default;
                return true;
            }
            else
            {
                var enumrator=_source.GetEnumerator();
                while (enumrator.MoveNext())
                {
                    var current= enumrator.Current;
                    if (current is null) continue;
                    if (current.GetHashCode().ToString() == value)
                    {
                        result = ValueConverter(current);
                        return true;
                    }
                }
                result= default;
                return false;
            }
        }

        protected override string? FormatValueAsString(Tval? value)=> _source.FirstOrDefault(t=>Equals(ValueConverter(t),value))?.GetHashCode().ToString();

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Tval result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            var resultVal = ConvetTo(value, out result);
            validationErrorMessage = resultVal ? string.Empty : ParsingErrorMessage;
            return resultVal;
        }

        private void SetCurrentValueAsStringArray(string?[]? value)
        {
            if (value is null || value.Length == 0) return;

            CurrentValue = BindConverter.TryConvertTo<Tval>(_source.Where(t=>value.Contains(t!.GetHashCode().ToString())).ToArray(), CultureInfo.CurrentCulture, out var result)
                ? result
                : default;
        }
    }
}
