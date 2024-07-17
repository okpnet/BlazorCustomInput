using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;

namespace BlazorCustomInput.Components
{
    public class EditerSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        private readonly bool _isMultipleSelect;

        public EditerSelect()
        {
            _isMultipleSelect = typeof(Tval).IsArray;
        }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "select");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(++index, "class", CssClass);
            builder.AddAttribute(4, "multiple", _isMultipleSelect);

            if (_isMultipleSelect)
            {
                builder.AddAttribute(5, "value", BindConverter.FormatValue(CurrentValue)?.ToString());
                builder.AddAttribute(6, "onchange", EventCallback.Factory.CreateBinder<string?[]?>(this, SetCurrentValueAsStringArray, default));
                builder.SetUpdatesAttributeName("value");
            }
            else
            {
                builder.AddAttribute(7, "value", CurrentValueAsString);
                builder.AddAttribute(8, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, default));
                builder.SetUpdatesAttributeName("value");
            }
            builder.AddElementReferenceCapture(++index, __inputReference => Element = __inputReference);
            builder.AddElementReferenceCapture(9, __selectReference => Element = __selectReference);
            builder.AddContent(10, ChildContent);
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
                result = (Tval)(object)value;
                return true;
            }
        }

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Tval result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            var resultVal = ConvetTo(value, out result);
            validationErrorMessage = resultVal ? string.Empty : ParsingErrorMessage;
            return resultVal;
        }
        private void SetCurrentValueAsStringArray(string?[]? value)
        {
            CurrentValue = BindConverter.TryConvertTo<Tval>(value, CultureInfo.CurrentCulture, out var result)
                ? result
                : default;
        }
    }
}
