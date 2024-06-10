using Microsoft.AspNetCore.Components;

namespace BlazorCustomInput.Components
{
    public sealed class AutocompleteArg<Tval>
    {
        /// <summary>
        /// 候補に上がった
        /// </summary>
        public Tval? Value { get; }
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
            Value = default;
            SelectCallback = EventCallback<Tval>.Empty;
            Selected = EventCallback.Factory.Create(this, OnSelected);
        }

        public AutocompleteArg(Tval? value, EventCallback<Tval> selectCallback) : this()
        {
            Value = value;
            SelectCallback = selectCallback;
        }
        /// <summary>
        /// 選択されたことをコントロールに通知
        /// </summary>
        void OnSelected()
        {
            SelectCallback.InvokeAsync(Value);
        }
    }
}
