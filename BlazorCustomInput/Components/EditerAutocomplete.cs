using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorCustomInput.Components
{
    /*
@using Microsoft.AspNetCore.Components.Web
@typeparam TVal
@inherits EditerText<TVal>
<input @attributes="AdditionalAttributes"
       class="@CssClass"
       @bind="Value"
       @bind:event="oninput"
       @bind:after="OnInputAsync"
       @onfocusout="OnFocusoutAsync" />
    @if (IsLoading)
{
    <br />
    @LoadingTemplate
}
else if (autocomplete is not null)
{
    <br />
    @AutocompleteFrame(autocomplete)
}
    */
    public partial class EditerAutocomplete<TVal> : EditerText<TVal>
    {
        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        public RenderFragment<IEnumerable<AutocompleteArg<TVal>>> AutocompleteFrame { get; set; } = default!;
        /// <summary>
        /// 読み込み中
        /// </summary>
        [Parameter]
        public RenderFragment? LoadingTemplate { get; set; } = default!;
        /// <summary>
        /// オートコンプリートのリストを返す
        /// </summary>
        [Parameter]
        public Func<TVal?, Task<TVal[]>> GetAutocomleteItems { get; set; } = default!;
        /// <summary>
        /// オートコンプリートが検索開始する文字数
        /// </summary>
        [Parameter]
        public int MinTextLength { get; set; } = 3;
        /// <summary>
        /// TValから文字列を取り出し/生成
        /// </summary>
        [Parameter]
        public Func<TVal?, string> GetText { get; set; } = (x) => x is string val ? val : x?.ToString() ?? string.Empty;
        /// <summary>
        /// インプットボックスにキーボード入力
        /// </summary>
        [Parameter]
        public EventCallback<KeyboardEventArgs> KeyEventCallback { get; set; }
        /// <summary>
        /// コンプリート完了したときに呼び出しされる｡
        /// </summary>
        [Parameter]
        public EventCallback CompleteCallBack { get; set; } = default!;

        /// <summary>
        /// オートコンプリートリスト
        /// </summary>
        List<AutocompleteArg<TVal>>? autocomplete { get; set; }
        /// <summary>
        /// 読み込み中フラグ
        /// </summary>
        bool IsLoading = false;

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
            index++;
            if (IsDisabled)
            {
                builder.AddAttribute(index, "diasabled");
            }
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "type", editType);
            builder.AddAttribute(++index, "class", CssClass);
            builder.AddAttribute(++index, "onfocusout", EventCallback.Factory.Create<FocusEventArgs>(this, OnFocusoutAsync));
            builder.AddAttribute(++index, "value", BindConverter.FormatValue(Value));
            builder.AddAttribute(
                ++index,
                "oninput",
                EventCallback.Factory.CreateBinder(
                    this,
                    RuntimeHelpers.CreateInferredBindSetter(
                        callback: __value =>
                        {
                            Value = __value;
                            return RuntimeHelpers.InvokeAsynchronousDelegate(callback: OnInputAsync);
                        },
                        value: Value),
                    Value)
            );

            builder.SetUpdatesAttributeName("value");
            builder.AddElementReferenceCapture(++index, __inputReference => Element = __inputReference);
            builder.CloseElement();
            
            if (IsLoading && LoadingTemplate is not null)
            {
                builder.AddMarkupContent(++index, "\r\n<br>\r\n");
                builder.AddContent(++index, LoadingTemplate);
                return;
            }
            else if (autocomplete is not null && AutocompleteFrame is not null)
            {
                builder.AddMarkupContent(++index, "\r\n<br>\r\n");
                builder.AddContent(++index, AutocompleteFrame(autocomplete));
            }
        }

        /// <summary>
        /// 確定
        /// </summary>
        async Task OnFocusoutAsync()
        {
            //System.Diagnostics.Debug.WriteLine("focusout");
            await Task.Delay(500);
            System.Diagnostics.Debug.WriteLine(Value);
            if (CompleteCallBack.HasDelegate) await CompleteCallBack.InvokeAsync();
            autocomplete = default;
        }
        /// <summary>
        /// リストの呼び出し
        /// </summary>
        async Task OnInputAsync()
        {
            await ValueChanged.InvokeAsync(Value);
            try
            {
                if (GetText(Value).Length >= MinTextLength && GetAutocomleteItems is not null)
                {
                    IsLoading = true;
                    autocomplete = new();
                    var autocompleteItems = await GetAutocomleteItems(Value);
                    autocomplete = autocompleteItems.Select(t => new AutocompleteArg<TVal>(t, EventCallback.Factory.Create<TVal>(this, OnSelectItemAsync))).ToList();
                }
                else
                {
                    autocomplete = default;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// 選択されたときに呼び出される
        /// </summary>
        /// <param name="value"></param>
        async Task OnSelectItemAsync(TVal value)
        {
            //System.Diagnostics.Debug.WriteLine("selected");
            Value = value;
            autocomplete = default;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
