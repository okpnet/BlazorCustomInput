using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// タイムスパンコンポーネント
    /// </summary>
    public partial class EditerTimeSpan<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {//入力済みのものがあるとき、単位を上位に変更すると下位が切り捨てられる。
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
            if (tvalType == typeof(TimeSpan) ||
                tvalType == typeof(short) ||
                tvalType == typeof(int) ||
                tvalType == typeof(uint) ||
                tvalType == typeof(long) ||
                tvalType == typeof(ulong) ||
                tvalType == typeof(float) ||
                tvalType == typeof(double) ||
                tvalType == typeof(decimal))
            {
                return "any";
            }
            else
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported numeric type.");
            }
        }
        /// <summary>
        /// 表示する単位
        /// </summary>
        [Parameter, EditorRequired]
        public Unit TimeSpanUnit { get; set; }
        /// <summary>
        /// 最大値
        /// </summary>
        [Parameter]
        public int? IsMax { get; set; }
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
        /// <summary>
        /// Gets or sets the associated <see cref="ElementReference"/>.
        /// <para>
        /// May be <see langword="null"/> if accessed before the component is rendered.
        /// </para>
        /// </summary>
        [DisallowNull] public ElementReference? Element { get; protected set; }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            builder.OpenElement(index, "input");
            if (IsDisabled)
            {
                builder.AddAttribute(++index, "diasabled");
            }
            builder.AddAttribute(++index, "step", _stepAttrVal);
            builder.AddAttribute(++index, "min", BindConverter.FormatValue(0));
            builder.AddAttribute(++index, "max", GetMaxSize());
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", "number");
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
        /// <returns></returns>
        protected override string FormatValueAsString(Tval? value)
        {
            if (value is null) return string.Empty;
            var formatStr = $"{(Comma ? "F" : "N")}{Degit}";

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
                TimeSpan @timespan => TimeSpanUnit switch
                {
                    Unit.Day => @timespan.Days.ToString(),
                    Unit.Hour => @timespan.Hours.ToString(),
                    Unit.Min => @timespan.Minutes.ToString(),
                    _ => @timespan.Seconds.ToString()
                },
                _ => string.Empty
            };

        }
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>

        protected override bool ConvetTo(string? value, out Tval? result)
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType == typeof(TimeSpan) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @timespan))
            {
                result = (Tval)(object)(TimeSpanUnit switch
                {
                    Unit.Day => TimeSpan.FromDays(@timespan),
                    Unit.Hour => TimeSpan.FromHours(@timespan),
                    Unit.Min => TimeSpan.FromMinutes(@timespan),
                    _ => TimeSpan.FromSeconds(@timespan)
                });
                return true;
            }
            else if (tvalType == typeof(short) && int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @short))
            {
                result = (Tval)(object)@short; return true;
            }
            else if (tvalType == typeof(int) && int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @int))
            {
                result = (Tval)(object)@int; return true;
            }
            else if (tvalType == typeof(uint) && uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @uint))
            {
                result = (Tval)(object)@uint; return true;
            }
            else if (tvalType == typeof(long) && long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @long))
            {
                result = (Tval)(object)@long; return true;
            }
            else if (tvalType == typeof(ulong) && ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @ulong))
            {
                result = (Tval)(object)@ulong; return true;
            }
            else if (tvalType == typeof(float) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @float))
            {
                result = (Tval)(object)@float; return true;
            }
            else if (tvalType == typeof(double) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @double))
            {
                result = (Tval)(object)@double; return true;
            }
            else if (tvalType == typeof(decimal) && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var @decimal))
            {
                result = (Tval)(object)@decimal; return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
        /// <summary>
        /// 最大値の取得
        /// </summary>
        /// <returns></returns>
        string GetMaxSize()
        {
            if (IsMax is not null || IsMax > 0) return IsMax.Value.ToString();
            return NumberExtention.GetMaxsize<Tval>();
        }
    }
}
