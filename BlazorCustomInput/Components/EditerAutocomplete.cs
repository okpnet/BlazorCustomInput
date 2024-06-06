using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Rendering;

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
    <br />
    <span @onmouseover="(()=>isOut=false)" @onmouseleave="(()=>isOut=true)" >
        @if (autocomplete is not null && autocomplete.Any())
        {
            @foreach (var item in autocomplete)
            {
                @AutocomleteNodes(item)
            }
        }
    </span>
     * */
    public partial class EditerAutocomplete<TVal>:EditerText<TVal>
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;
        /// <summary>
        /// オートコンプリートアイテムを表示するコンテンツ
        /// </summary>
        [Parameter]
        public RenderFragment<AutocompleteArg<TVal>> AutocomleteNodes { get; set; } = default!;
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
        bool isOut = true;

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
            builder.AddAttribute(++index, "onblur", EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.FocusEventArgs>(this,OnLostFocus));
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
                            return Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.InvokeAsynchronousDelegate(callback:OnInputAsync);
                        }, 
                        value: Value),
                    Value));
            builder.SetUpdatesAttributeName("value");
            builder.CloseElement();
            builder.AddMarkupContent(++index, "\r\n<br>\r\n");
            builder.OpenElement(++index, "span");
            builder.AddAttribute(++index, "onmouseover", EventCallback.Factory.Create<MouseEventArgs>(this,(() => isOut = false)));
            builder.AddAttribute(++index, "onmouseleave", EventCallback.Factory.Create<MouseEventArgs>(this,(() => isOut = true)));
            if (autocomplete is not null && autocomplete.Any())
            {
                foreach (var item in autocomplete)
                {
                    builder.AddContent(++index,AutocomleteNodes(item));
                }
            }
            builder.CloseElement();
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
            if (GetText(Value).Length >= MinTextLength && GetAutocomleteItems is not null)
            {
                autocomplete = new();
                autocomplete.Add(new AutocompleteArg<TVal>());
                var autocompleteItems = await GetAutocomleteItems(Value);
                autocomplete = new();
                autocomplete = autocompleteItems.Select(t => new AutocompleteArg<TVal>(t, false, EventCallback.Factory.Create<TVal>(this, OnSelectItem))).ToList();
            }
            else
            {
                autocomplete = default;
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
