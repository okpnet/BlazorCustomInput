using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
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
        /// 入力中の即時更新をOnにする
        /// 入力中にEditContext?.NotifyFieldChangedが呼ばれるので注意。
        /// </summary>
        [Parameter]
        public bool IsImmediateUpdate { get; set; } = true;
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
            var editType = EditType.GetTypeString();
            builder.OpenElement(index, "input");
            index+=1;
            builder.AddAttribute(index, "disabled", IsDisabled);
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", editType);
            builder.AddAttribute(++index, "class", CssClass);
            builder.AddAttribute(++index, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(++index, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            index += 1;
            if (IsImmediateUpdate)
            {
                //フォーカスが外れなくても変更がわかるように追加
                builder.AddAttribute(
                    index,
                    "oninput",
                    EventCallback.Factory.CreateBinder(
                        this,
                        RuntimeHelpers.CreateInferredBindSetter(
                            callback: __value =>
                            {
                                CurrentValue = __value;
                                return Task.CompletedTask;
                            },
                            value: Value),
                        Value)
                );
            }
            
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
