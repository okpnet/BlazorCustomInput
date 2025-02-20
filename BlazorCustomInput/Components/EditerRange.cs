using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// 数コンポーネント
    /// </summary>
    public partial class EditerRange<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {

        /// <summary>
        /// InputNumber引用
        /// </summary>
        static readonly string _stepAttrVal = GetStepAttrVal();
        /// <summary>
        /// InputNumber引用
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        static string GetStepAttrVal()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType == typeof(short) ||
                tvalType == typeof(int) ||
                tvalType == typeof(uint) ||
                tvalType == typeof(long) ||
                tvalType == typeof(ulong) ||
                tvalType == typeof(float) ||
                tvalType == typeof(double) ||
                tvalType == typeof(decimal) ||
                tvalType == typeof(byte))
            {
                return "any";
            }
            else
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported numeric type.");
            }
        }


        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            builder.OpenElement(index, "input");
            ++index;
            builder.AddAttribute(index, "disabled", IsDisabled);
            builder.AddAttribute(++index, "step", _stepAttrVal);
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", "range");

            builder.AddAttribute(++index, "class", CssClass);
            builder.AddAttribute(++index, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(++index, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.AddElementReferenceCapture(++index, __inputReference => Element = __inputReference);

            builder.CloseElement();
        }
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override bool ConvetTo(string? value, out Tval? result)
        {
            return BindConverter.TryConvertTo<Tval>(value, CultureInfo.InvariantCulture, out result);
        }
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override string? FormatValueAsString(Tval? value)
        {
            if (value is null) return string.Empty;
            return value.ToString();
        }
    }
}
