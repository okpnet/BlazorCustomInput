using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCustomInput.Components
{
    public sealed class AutocompleteArg<Tval>
    {
        /// <summary>
        /// 候補に上がった
        /// </summary>
        public Tval? Value { get; }
        /// <summary>
        /// 読み込み中
        /// </summary>
        public bool IsLoading { get; }
        /// <summary>
        /// 親の/でコンテンツに設定する選択イベント
        /// </summary>
        public EventCallback Selected { get; }
        /// <summary>
        /// コントロールに通知するコールバック
        /// </summary>
        public EventCallback<Tval> SelectCallback { get; }

        public AutocompleteArg()
        {
            IsLoading= true;
            Value= default;
            SelectCallback = EventCallback<Tval>.Empty;
            Selected = EventCallback.Factory.Create(this, OnSelected);
        }

        public AutocompleteArg(Tval? value, bool isLoading, EventCallback<Tval> selectCallback):this()
        {
            Value = value;
            IsLoading = isLoading;
            SelectCallback = selectCallback;
        }
        /// <summary>
        /// 選択されたことをコントロールに通知
        /// </summary>
        void OnSelected()
        {
            if(!IsLoading && SelectCallback.HasDelegate)
            {
                SelectCallback.InvokeAsync(Value);
            }
        }
    }
}
