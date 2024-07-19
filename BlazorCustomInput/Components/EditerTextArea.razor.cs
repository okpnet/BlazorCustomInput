using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// テキストフォームコンポーネント
    /// </summary>
    public partial class EditerTextArea<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        /// <summary>
        /// コンストラクタ
        /// 型チェック｡GetStepAttrValがあれば評価いらない?
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public EditerTextArea()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType != typeof(string))
            {
                throw new InvalidOperationException($"The type '{tvalType}' is not a supported text type.");
            }
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var index = 0;
            builder.OpenElement(index, "textarea");
            if (IsDisabled)
            {
                builder.AddAttribute(++index, "diasabled");
            }
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
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
                result = (Tval)(object)value;
                return true;
            }
        }
    }
}
