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
    public partial class EditorNumber<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
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
        /// <summary>
        /// 最小
        /// </summary>
        [Parameter]
        public decimal? MinValue { get; set; }
        /// <summary>
        /// 最大
        /// </summary>
        [Parameter]
        public decimal? MaxValue { get; set; }
        /// <summary>
        /// 小数点以下桁
        /// </summary>
        [Parameter]
        public int Degit { get; set; } = 0;
        /// <summary>
        /// カンマ区切り有効
        /// </summary>
        [Parameter]
        public bool Comma { get; set; } = false;

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            builder.OpenElement(index, "input");
            ++index;
            builder.AddAttribute(index, "disabled", IsDisabled);
            builder.AddAttribute(++index, "step", _stepAttrVal);
            ++index;
            if (MinValue is not null)
            {
                builder.AddAttribute(index, "min", BindConverter.FormatValue(MinValue.Value));
            }

            builder.AddAttribute(++index, "max", BindConverter.FormatValue(GetMaxSize()));
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            if (Comma)
            {
                builder.AddAttribute(++index, "type", "text");
            }
            else
            {
                builder.AddAttribute(++index, "type", "number");
            }

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
            var formatStr = $"{(Comma ? "N" : "F")}{Degit}";
            return value switch
            {
                int @int => @int.ToString(formatStr),
                short @short => @short.ToString(formatStr),
                uint @uint => @uint.ToString(formatStr),
                long @long => @long.ToString(formatStr),
                ulong @ulong => @ulong.ToString(formatStr),
                float @float => @float.ToString(formatStr),
                double @double => @double.ToString(formatStr),
                decimal @decimal => @decimal.ToString(formatStr),
                byte @byte => @byte.ToString(formatStr),
                _ => string.Empty
            };
        }
        /// <summary>
        /// 最大値の取得
        /// </summary>
        /// <returns></returns>
        string GetMaxSize()
        {
            if (MaxValue is not null || MaxValue > 0) return MaxValue.Value.ToString();
            return NumberExtention.GetMaxsize<Tval>();
        }
    }
}
