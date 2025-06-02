using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCustomInput.Components.Tests
{
    public partial class EditorSelectTest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        private readonly List<EditorSelectOptionTest<Tval>> _options = new();

        [Parameter] public Tval? Value { get; set; }
        [Parameter] public EventCallback<Tval?> ValueChanged { get; set; }

        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment? ChoseTemplate { get; set; }

        [Parameter] public Func<Tval?, Tval?, bool>? CompareFunc { get; set; }
        [Parameter] public Func<Tval?, string>? OptionValueSelector { get; set; }

        internal void RegisterOption(EditorSelectOptionTest<Tval> option)
        {
            _options.Add(option);
        }

        protected override void OnParametersSet()
        {
            _options.Clear(); // 再構築のため
        }

        protected List<EditorSelectOptionTest<Tval>> Options => _options;

        protected bool IsValueNull => EqualityComparer<Tval>.Default.Equals(Value, default);

        protected bool Compare(Tval? x, Tval? y)
            => CompareFunc?.Invoke(x, y) ?? EqualityComparer<Tval>.Default.Equals(x, y);

        protected string GetOptionKey(Tval? value)
            => OptionValueSelector?.Invoke(value) ?? value?.ToString() ?? "";

        protected async Task OnChanged(ChangeEventArgs e)
        {
            var newValueStr = e.Value?.ToString();
            foreach (var option in _options)
            {
                if (GetOptionKey(option.Value) == newValueStr)
                {
                    Value = option.Value;
                    await ValueChanged.InvokeAsync(Value);
                    StateHasChanged();
                    break;
                }
            }
        }
    }

}