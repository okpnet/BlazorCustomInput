using Microsoft.AspNetCore.Components;
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
            @bind="@Value"
            @bind:event="oninput"
            @bind:after="OnInputAsync"
            @onblur="OnLostFocus" />
    
    @if (autocomplete is not null && autocomplete.Any())
    {
        <br />
        <span @onmouseover="(()=>isOut=false)" @onmouseleave="(()=>isOut=true)" >
            <Cascad value="IsLoading">
                <Cascad value="autocomplete">
                    <AutocompelteFrame/>
                </cascade>
            </cascade>
        </span>
    }
    */
    public partial class EditerAutocomplete<TVal> : EditerText<TVal>
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        public RenderFragment<AutocompleteFrame<TVal>> Frame { get; set; } = default!;
        [Parameter]
        public RenderFragment<IEnumerable<AutocompleteFrame<TVal>>> test { get; set; } = default!;
        /// <summary>
        /// 読み込み中
        /// </summary>
        [Parameter]
        public RenderFragment? LoadingTemplate { get; set; } = default!;
        /// <summary>
        /// オートコンプリートのリストを返す
        /// </summary>
        [Parameter]
        public Func<TVal?, Task<IEnumerable<TVal>>> GetAutocomleteItems { get; set; } = default!;
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
        /// 
        /// </summary>
        bool isOut = true;
        /// <summary>
        /// 
        /// </summary>
        bool isLoading = true;

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
            builder.AddAttribute(++index, "onblur", EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.FocusEventArgs>(this, OnLostFocus));
            builder.AddAttribute(++index, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(
                ++index,
                "oninput",
                EventCallback.Factory.CreateBinder(
                    this,
                    Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredBindSetter(
                        callback: __value =>
                        {
                            Value = __value;
                            return Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.InvokeAsynchronousDelegate(callback: OnInputAsync);
                        },
                        value: Value),
                    Value));
            builder.SetUpdatesAttributeName("value");
            builder.CloseElement();
            if (isLoading)
            {
                builder.AddContent(++index, LoadingTemplate);
                return;
            }
            if (autocomplete is not null && autocomplete.Any())
            {
                builder.AddMarkupContent(++index, "\r\n<br>\r\n");
                builder.OpenElement(++index, "span");
                builder.AddAttribute(++index, "onmouseover", EventCallback.Factory.Create<MouseEventArgs>(this, (() => isOut = false)));
                builder.AddAttribute(++index, "onmouseleave", EventCallback.Factory.Create<MouseEventArgs>(this, (() => isOut = true)));
                builder.OpenComponent<CascadingValue<bool>>(++index);
                builder.AddAttribute(++index, "Value", isLoading);
                builder.AddAttribute(++index, "Name", "IsLoading");
                builder.OpenComponent<CascadingValue<IEnumerable<AutocompleteArg<TVal>>>>(++index);
                builder.AddAttribute(++index, "Value", autocomplete);
                builder.AddAttribute(++index, "Name", "AutocompleteItems");
                builder.AddAttribute(++index, nameof(Frame), (RenderFragment)((builder2) =>
                {
                }));
                builder.CloseComponent();
                builder.CloseComponent();
                builder.CloseComponent();
                builder.CloseElement();
            }
        }
        /// <summary>
        /// 確定
        /// </summary>
        void OnLostFocus()
        {
            if (isOut)
            {
                autocomplete = default;
                return;
            }
        }
        /// <summary>
        /// リストの呼び出し
        /// </summary>
        async Task OnInputAsync()
        {
            try
            {
                if (GetText(Value).Length >= MinTextLength && GetAutocomleteItems is not null)
                {
                    isLoading = true;
                    autocomplete = new();
                    var autocompleteItems = await GetAutocomleteItems(Value);
                    autocomplete = new();
                    autocomplete = autocompleteItems.Select(t => new AutocompleteArg<TVal>(t, false, EventCallback.Factory.Create<TVal>(this, OnSelectItem))).ToList();
                }
                else
                {
                    autocomplete = default;
                }
            }
            finally
            {
                isLoading = false;
            }
        }
        /// <summary>
        /// 選択されたときに呼び出される
        /// </summary>
        /// <param name="value"></param>
        void OnSelectItem(TVal value)
        {
            autocomplete = default;
            Value = value;
        }
    }
}
