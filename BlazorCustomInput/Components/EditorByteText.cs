using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// GUID､バイト配列テキストコンポーネント
    /// </summary>
    public partial class EditorByteText<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        /// <summary>
        /// カスタムフォーマット
        /// </summary>
        [Parameter]
        public string CustomGuidFormat { get; set; } = "D";

        public EditorByteText()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType != typeof(byte[]) &&
                tvalType != typeof(Guid))
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported byte text type.");
            }
        }
        /// <summary>
        /// パラメータセット
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType == typeof(Guid) && CustomGuidFormat is not (null or "") && !System.Text.RegularExpressions.Regex.IsMatch(CustomGuidFormat, "[dnbpx]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {//カスタムフォーマットの検証
                throw new ArgumentException("format string can only be \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\", or \"x\" .");
            }
        }
        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            builder.OpenElement(index, "input");
            ++index;
            builder.AddAttribute(index, "disabled", IsDisabled);
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", "text");
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
            try
            {
                if (value is not null)
                {
                    var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
                    if (tvalType == typeof(byte[]) && !System.Text.RegularExpressions.Regex.IsMatch(value, "[^A-F0-9]"))
                    {
                        result = (Tval)(object)Convert.FromHexString(value);
                        return true;
                    }
                    else if (tvalType == typeof(Guid) && Guid.TryParse(value, out var @guid))
                    {
                        result = (Tval)(object)guid;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            result = default;
            return false;
        }
        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override string? FormatValueAsString(Tval? value)
        {
            if (value is null) return string.Empty;
            return value switch
            {
                byte[] @bytes => Convert.ToHexString(@bytes),
                Guid @guid => guid.ToString(CustomGuidFormat),
                _ => string.Empty
            };
        }
    }
}
