using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// ブーリアンチェックボックスコンポーネント
    /// </summary>
    /// <typeparam name="Tval"></typeparam>
    public partial class EditerBool<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        #region"Protected Field"
        /// <summary>
        /// チェックボックスチェック
        /// </summary>
        private bool _isCheck;
        protected bool IsCheck
        {
            get => _isCheck;
            set
            {
                if (_isCheck != value)
                {
                    _isCheck = value;
                    CurrentValue = _isCheck
                        ? (TrueValue is not null ? (Tval)(object)TrueValue : default)
                        : (FalseValue is not null ? (Tval)(object)FalseValue : default);
                }
            }
        }
        #endregion
        /// <summary>
        /// True値
        /// </summary>
        [Parameter]
        public Tval? TrueValue { get; set; }
        /// <summary>
        /// False値
        /// </summary>
        [Parameter]
        public Tval? FalseValue { get; set; }
        /// <summary>
        /// Indeterminate
        /// </summary>
        [Parameter]
        public bool IsIndeterminate { get; set; } = true;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EditerBool()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType != typeof(short) &&
                tvalType != typeof(int) &&
                tvalType != typeof(uint) &&
                tvalType != typeof(long) &&
                tvalType != typeof(ulong) &&
                tvalType != typeof(float) &&
                tvalType != typeof(double) &&
                tvalType != typeof(decimal) &&
                tvalType != typeof(byte) &&
                tvalType != typeof(bool) &&
                tvalType != typeof(string))
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported bool checkbox type.");
            }
        }
        /// <summary>
        /// パラメータセットオーバーライド
        /// True値とFalse値の検証
        /// Bool型かつTrue値False値ともにNullのとき､強制的にTrue値をTrueにする
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType == typeof(bool) && TrueValue is null && FalseValue is null)
            {
                TrueValue = (Tval)(object)true;
                FalseValue = (Tval)(object)false;
            }
            else if (!Verify())
            {
                throw new ArgumentException($"The values true and false cannot be equal.");
            }
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            //描画ループの原因
            //if (IsCheck)
            //{
            //    if (TrueValue is not null)
            //        CurrentValue = (Tval)(object)TrueValue;
            //    else
            //        CurrentValue = default;
            //}
            //else
            //{
            //    if (FalseValue is not null)
            //        CurrentValue = (Tval)(object)FalseValue;
            //    else
            //        CurrentValue = default;
            //}
            //indeterminateを結合
            var cssClass = string.Join(' ', CssClass, IsIndeterminate ? "is-indeterminate" : "");

            builder.OpenElement(index, "input"); 
            ++index;
            builder.AddAttribute(index, "disabled", IsDisabled);
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", "checkbox");
            builder.AddAttribute(++index, "class", cssClass);
            builder.AddAttribute(++index, "value", BindConverter.FormatValue(CurrentValue));
            //builder.AddAttribute(++index, "checked", BindConverter.FormatValue(IsCheck));
            builder.AddAttribute(++index, "onchange", EventCallback.Factory.CreateBinder<bool>(this, __value => IsCheck = __value, IsCheck));
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
                result = default!;
                return true;
            }
            return BindConverter.TryConvertTo<Tval>(value, CultureInfo.InvariantCulture, out result);
        }
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override string? FormatValueAsString(Tval? value)
        {
            if (value is null)
            {
                IsIndeterminate = true;
                return string.Empty;
            }
            IsIndeterminate = false;
            return IsCheck ? ConvertFromCheck(TrueValue) : ConvertFromCheck(FalseValue);
        }
        /// <summary>
        /// ベリファイ
        /// </summary>
        /// <returns></returns>
        bool Verify()
        {
            if (TrueValue is null && FalseValue is null)
            {
                return false;
            }
            else if (TrueValue is not null && FalseValue is not null)
            {
                return TrueValue.Equals(TrueValue);
            }
            else if (TrueValue is not null && FalseValue is null || TrueValue is null && FalseValue is not null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// チェックに応じた値の変更
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string? ConvertFromCheck(Tval? value)
        {
            if (value is null) return string.Empty;
            return value switch
            {
                int @int => BindConverter.FormatValue(@int, CultureInfo.InvariantCulture),
                short @short => BindConverter.FormatValue(@short, CultureInfo.InvariantCulture),
                uint @uint => BindConverter.FormatValue(@uint, CultureInfo.InvariantCulture)?.ToString() ?? string.Empty,
                long @long => BindConverter.FormatValue(@long, CultureInfo.InvariantCulture),
                ulong @ulong => BindConverter.FormatValue(@ulong, CultureInfo.InvariantCulture)?.ToString() ?? string.Empty,
                float @float => BindConverter.FormatValue(@float, CultureInfo.InvariantCulture),
                double @double => BindConverter.FormatValue(@double, CultureInfo.InvariantCulture),
                decimal @decimal => BindConverter.FormatValue(@decimal, CultureInfo.InvariantCulture),
                byte @byte => BindConverter.FormatValue(@byte, CultureInfo.InvariantCulture)?.ToString() ?? string.Empty,
                bool @bool => @bool.ToString(),
                string @string => @string,
                _ => string.Empty
            };
        }
    }
}
