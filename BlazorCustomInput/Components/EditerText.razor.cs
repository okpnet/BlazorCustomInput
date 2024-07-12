using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// テキストフォームコンポーネント
    /// </summary>
    public partial class EditerText<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        /// <summary>
        /// エディットタイプ
        /// </summary>
        [Parameter]
        public TextEditType EditType { get; set; } = TextEditType.Text;
        /// <summary>
        /// Gets or sets the associated <see cref="ElementReference"/>.
        /// <para>
        /// May be <see langword="null"/> if accessed before the component is rendered.
        /// </para>
        /// </summary>
        [DisallowNull] public ElementReference? Element { get=>componentElement; protected set=>componentElement=value; }
        /// <summary>
        /// コンストラクタ
        /// 型チェック｡GetStepAttrValがあれば評価いらない?
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public EditerText() : base()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType != typeof(string))
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported text type.");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender && AutoFocus) await FocusAsync();
        }
        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            var editType = EditType switch
            {
                TextEditType.Email => "email",
                TextEditType.Url => "url",
                TextEditType.Number => "number",
                TextEditType.Password => "password",
                _ => "text"
            };
            builder.OpenElement(index, "input");
            if (IsDisabled)
            {
                builder.AddAttribute(++index, "diasabled");
            }
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", editType);
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
            else
            {
                var regex = EditType switch
                {
                    TextEditType.Email => "^[\\w\\-\\._]+@[\\w\\-\\._]+\\.[A-Za-z]+",
                    TextEditType.Url => "https?://[\\w!\\?/\\+\\-_~=;\\.,\\*&@#\\$%\\(\\)'\\[\\]]+",
                    TextEditType.Number => "^[\\d]+",
                    _ => ""
                };
                if (regex is not (null or "") && !System.Text.RegularExpressions.Regex.IsMatch(value, regex))
                {
                    result = default;
                    return false;
                }
                result = (Tval)(object)value;
                return true;
            }
        }
    }
}
