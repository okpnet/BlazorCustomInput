using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// 日付フォームコンポーネント
    /// </summary>
    public partial class EditerDate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        #region"Const"
        private const string DateFormat = "yyyy-MM-dd";                     // Compatible with HTML 'date' inputs
        private const string DateTimeLocalFormat = "yyyy-MM-ddTHH:mm:ss";   // Compatible with HTML 'datetime-local' inputs
        private const string MonthFormat = "yyyy-MM";                       // Compatible with HTML 'month' inputs
        private const string TimeFormat = "HH:mm:ss";                       // Compatible with HTML 'time' inputs
        #endregion
        #region"Private field"
        /// <summary>
        /// アトリビュート
        /// </summary>
        string _typeAttributeValue = string.Empty;
        /// <summary>
        /// 表示フォーマット
        /// </summary>
        string _format = string.Empty;
        #endregion
        #region"Static Field"
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
            if (tvalType == typeof(DateTime) ||
                tvalType == typeof(DateTimeOffset) ||
                tvalType == typeof(DateOnly) ||
                tvalType == typeof(TimeOnly) ||
                tvalType == typeof(int) ||
                tvalType == typeof(long) ||
                tvalType == typeof(decimal) ||
                tvalType == typeof(string))
            {
                return "any";
            }
            else
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported numeric type.");
            }
        }
        #endregion

        /// <summary>
        /// 変換フォーマット
        /// </summary>
        [Parameter]
        public string? CustomParseFormat { get; set; } = string.Empty;
        /// <summary>
        /// 入力日付タイプ
        /// </summary>
        [Parameter]
        public InputDateType DateType { get; set; } = InputDateType.Date;
        /// <summary>
        /// コンストラクタ
        /// 型チェック｡GetStepAttrValがあれば評価いらない?
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public EditerDate()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType != typeof(DateTime) &&
                tvalType != typeof(DateTimeOffset) &&
                tvalType != typeof(DateOnly) &&
                tvalType != typeof(TimeOnly) &&
                tvalType != typeof(int) &&
                tvalType != typeof(long) &&
                tvalType != typeof(decimal) &&
                tvalType != typeof(string))
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported date type.");
            }
        }
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected override void OnParametersSet()
        {
            (_format, _typeAttributeValue, var msgDescription) = DateType switch
            {
                InputDateType.Date => (DateFormat, "date", "date"),
                InputDateType.DateTimeLocal => (DateTimeLocalFormat, "datetime-local", "date and time"),
                InputDateType.Time => (TimeFormat, "time", "time"),
                InputDateType.Month => (MonthFormat, "month", "year and month"),
                _ => throw new InvalidOperationException($"Unsupported {nameof(InputDateType)} '{DateType}'.")
            };

            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (CustomParseFormat is null or "" && CheckType())
            {
                throw new InvalidOperationException($"'CustomParseFormat' required for {tvalType.Name} .");
            }
            ParsingErrorMessage = ParsingErrorMessage is null or "" ? $"The {{0}} field must be a {msgDescription}." : ParsingErrorMessage;
        }

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
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", _typeAttributeValue);
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
            if (value is null or "")
            {
                result = default;
                return true;
            }

            DateTime dateTime;
            if (!DateTime.TryParseExact(value, _format, null, DateTimeStyles.None, out dateTime))
            {
                result = default;
                return false;
            }

            if (CustomParseFormat is not (null or "") && CheckType())
            {
                var numberStr = dateTime.ToString(CustomParseFormat);
                return BindConverter.TryConvertTo(numberStr, CultureInfo.InvariantCulture, out result);
            }
            return BindConverter.TryConvertTo(value, CultureInfo.InvariantCulture, out result);
        }
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override string? FormatValueAsString(Tval? value)
        {
            if (value is null) return string.Empty;

            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (CustomParseFormat is not (null or "") && tvalType == typeof(int) || tvalType == typeof(long) || tvalType == typeof(decimal) ||
                tvalType == typeof(string))
            {
                var dateStr = value switch
                {
                    int @int => @int.ToString(),
                    long @long => @long.ToString(),
                    string @string => @string,
                    _ => string.Empty
                };
                if (dateStr is null) return string.Empty;

                return DateTime.TryParseExact(dateStr, CustomParseFormat, null, DateTimeStyles.None, out var custom) ?
                                    BindConverter.FormatValue(custom, _format, CultureInfo.InvariantCulture) :
                                    string.Empty;
            }
            else
            {
                return value switch
                {
                    DateTime @datetime => BindConverter.FormatValue(@datetime, _format, CultureInfo.InvariantCulture),
                    DateTimeOffset @dateTimeOffset => BindConverter.FormatValue(@dateTimeOffset, _format, CultureInfo.InvariantCulture),
                    DateOnly @dateOnly => BindConverter.FormatValue(@dateOnly, _format, CultureInfo.InvariantCulture),
                    TimeOnly @timeOnly => BindConverter.FormatValue(@timeOnly, _format, CultureInfo.InvariantCulture),
                    _ => string.Empty
                };
            }
        }
        /// <summary>
        /// 日付=>文字列
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        string ConvertString(DateTime? date)
        {
            if (date is null) return string.Empty;
            return BindConverter.FormatValue(date, _format, CultureInfo.InvariantCulture) ?? string.Empty;
        }
        /// <summary>
        /// 型チェック
        /// </summary>
        /// <returns></returns>
        bool CheckType()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType == typeof(int) ||
            tvalType == typeof(long) ||
            tvalType == typeof(decimal) ||
            tvalType == typeof(string))
            {
                return true;
            }
            return false;
        }
    }
}
