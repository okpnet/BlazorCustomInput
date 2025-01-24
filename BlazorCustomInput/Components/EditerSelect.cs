using BlazorCustomInput.Argments;
using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorCustomInput.Components
{
    public class EditerSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        private readonly bool _isMultipleSelect;
        private IEnumerable<Tval> _source;

        /// <summary>
        /// Gets source to a select option. 
        /// </summary>
        [Parameter, EditorRequired]
        public Func<IEnumerable<Tval>> Source { get; set; } = default!;
        /// <summary>
        /// Rendering string the options. 
        /// </summary>
        [Parameter, EditorRequired]
        public Func<Tval?, string> OptionContents { get; set; } = default!;
        /// <summary>
        /// Argment null value contents.
        /// </summary>
        [Parameter]
        public string? SelectOptionContents { get; set; }

        public EditerSelect()
        {
            _isMultipleSelect = typeof(Tval).IsArray;
            _source=Enumerable.Empty<Tval>();
        }

        protected override void OnParametersSet()
        {
            _source = Source();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int index = 0;
            var selectedItem = _source.FirstOrDefault(t => Equals(t, Value));

            builder.OpenElement(++index, "select");
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "class", CssClass);
            builder.AddAttribute(++index, "multiple", _isMultipleSelect);

            if (_isMultipleSelect)
            {
                builder.AddAttribute(++index, "value", BindConverter.FormatValue(CurrentValue)?.ToString());
                builder.AddAttribute(++index, "onchange", EventCallback.Factory.CreateBinder<string?[]?>(this, SetCurrentValueAsStringArray, default));
                builder.SetUpdatesAttributeName("value");
            }
            else
            {
                builder.AddAttribute(++index, "value", CurrentValueAsString);
                builder.AddAttribute(++index, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, default));
                builder.SetUpdatesAttributeName("value");
            }

            builder.AddElementReferenceCapture(++index, __inputReference => Element = __inputReference);
            builder.AddElementReferenceCapture(++index, __selectReference => Element = __selectReference);
            
            if (SelectOptionContents is not (null or ""))
            {
                builder.OpenElement(index++, "option");
                builder.AddAttribute(index++, "hidden");
                builder.AddMarkupContent(index++, SelectOptionContents);
                builder.CloseElement();
            }
            else
            {
                index += 4;
            }

            foreach (var item in _source)
            {
                builder.OpenElement(++index, "option");
                builder.AddAttribute(++index, "value", item?.GetHashCode());
                if (selectedItem is not null && Equals(selectedItem, item))
                {
                    builder.AddAttribute(++index, "selected");
                }
                builder.AddMarkupContent(++index, OptionContents(item));
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
                        result = current;
                        return true;
                    }
                }
                result= default;
                return false;
            }
        }

        protected override string? FormatValueAsString(Tval? value)=> value?.GetHashCode().ToString();

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
