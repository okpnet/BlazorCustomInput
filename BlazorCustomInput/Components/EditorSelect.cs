using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace BlazorCustomInput.Components
{
    public class EditorSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public Func<Tval?, Tval?, bool>? CompareFunc { get; set; }

        [Parameter]
        public Func<Tval?, string>? OptionValueSelector { get; set; }

        internal List<OptionEntry<Tval>> Options { get; } = new();

        internal void RegisterOption(EditorSelectOption<Tval> option)
        {
            var key = option.Value?.GetHashCode().ToString() ?? "null";
            Options.Add(new OptionEntry<Tval>
            {
                Key = key,
                Value = option.Value,
                ChildContent = option.ChildContent,
                IsPlaceholder = option.IsPlaceholder
            });
            StateHasChanged();
        }

        protected bool IsValueNull => EqualityComparer<Tval>.Default.Equals(Value, default);

        protected bool Compare(Tval? x, Tval? y)
            => CompareFunc?.Invoke(x, y) ?? EqualityComparer<Tval>.Default.Equals(x, y);

        protected string? GetOptionKey(Tval? value) => OptionValueSelector is not null ?
            OptionValueSelector.Invoke(value) :
            (value is not null ? value.GetHashCode().ToString() : null);

        private async Task OnChange(ChangeEventArgs e)
        {
            var selectedKey = e.Value?.ToString();
            var match = Options.FirstOrDefault(o => o.Key == selectedKey);
            if (match is not null && !EqualityComparer<Tval?>.Default.Equals(Value, match.Value))
            {
                Value = match.Value;
                await ValueChanged.InvokeAsync(Value);
            }
        }

        private bool IsSelected(OptionEntry<Tval> entry)
            => EqualityComparer<Tval?>.Default.Equals(Value, entry.Value);

        /// <summary>
        /// オーバーライド
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override bool ConvetTo(string? value, out Tval? result)
        {
            var selectedOption = Options.FirstOrDefault(t => GetOptionKey(t.Value) == value);
            result = selectedOption is not null && selectedOption.Value is not null ? selectedOption.Value : default!;
            return true;
        }

        protected override string? FormatValueAsString(Tval? value) => GetOptionKey(value);

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Tval result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            var resultVal = ConvetTo(value, out result);
            validationErrorMessage = resultVal ? string.Empty : ParsingErrorMessage;
            return resultVal;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "select");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(1, "value", GetOptionKey(Value)); // ← これが重要！
            builder.AddAttribute(2, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this,OnChange));
            builder.SetUpdatesAttributeName("value");
            
            builder.OpenComponent<OptionBuilder>(7);//シーケンスを使わずにコンポーネント化。再描画に寄与
            builder.AddAttribute(8, nameof(OptionBuilder.RenderOptions), Options);
            builder.AddAttribute(9, nameof(OptionBuilder.CompareFunc), CompareFunc);
            builder.AddAttribute(10, nameof(OptionBuilder.Value), Value);
            builder.AddAttribute(11, nameof(OptionBuilder.OptionValueSelector), OptionValueSelector);
            builder.CloseComponent();

            builder.AddElementReferenceCapture(12, __inputReference => Element = __inputReference);
            builder.CloseElement();

            builder.OpenComponent<CascadingValue<EditorSelect<Tval>>>(13);
            builder.AddAttribute(14, "Value", this);
            builder.AddAttribute(15, "IsFixed", true);
            builder.AddAttribute(16, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(17, ChildContent);
            }));
            builder.CloseComponent();
        }

        internal class OptionEntry<TVal>
        {
            public string Key { get; set; } = "";
            public TVal? Value { get; set; }
            public RenderFragment? ChildContent { get; set; }
            public bool IsPlaceholder { get; set; }
        }

        internal class OptionBuilder:ComponentBase
        {
            [Parameter]
            public IEnumerable<OptionEntry<Tval>> RenderOptions { get; set; }

            [Parameter]
            public Func<Tval?, Tval?, bool>? CompareFunc { get; set; }

            [Parameter]
            public Tval? Value { get; set; }

            [Parameter]
            public Func<Tval?, string>? OptionValueSelector { get; set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                var seq = 0;
                foreach (var option in RenderOptions)
                {
                    var selected = Compare(option.Value, Value);
                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(3, "value", GetOptionKey(option.Value));
                    seq++;
                    if(option.Value is null)
                    {
                        builder.AddAttribute(seq, "disabled", "disabled");
                    }
                    if (selected)
                    {
                        builder.AddAttribute(seq++, "selected", "selected");
                    }
                    builder.AddContent(seq++, option.ChildContent);
                    builder.CloseElement();
                }
            }
            protected bool Compare(Tval? x, Tval? y)
            => CompareFunc?.Invoke(x, y) ?? EqualityComparer<Tval>.Default.Equals(x, y);

            protected string? GetOptionKey(Tval? value) => OptionValueSelector is not null ?
                OptionValueSelector.Invoke(value) :
                (value is not null ? value.GetHashCode().ToString() : null);
        }
    }
}
