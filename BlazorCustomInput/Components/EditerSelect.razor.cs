using BlazorCustomInput.Base;
using BlazorCustomInput.Components.Tests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace BlazorCustomInput.Components
{
    public class EditerSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public RenderFragment? ChoseTemplate { get; set; }

        [Parameter]
        public Func<Tval?, Tval?, bool>? CompareFunc { get; set; }

        [Parameter]
        public Func<Tval?, string>? OptionValueSelector { get; set; }

        internal List<OptionEntry<Tval>> Options { get; } = new();

        internal void RegisterOption(OptionTest<Tval> option)
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
            builder.AddAttribute(2, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this,OnChange));
            if (ChoseTemplate != null && IsValueNull)
            {
                builder.OpenElement(3, "option");
                builder.AddAttribute(4, "value", "");
                builder.AddContent(5, ChoseTemplate);

                builder.CloseElement();
            }
            foreach(var option in Options)
            {
                var selected = Compare(option.Value, Value);
                builder.OpenElement(6, "option");
                builder.AddAttribute(7, "value", GetOptionKey(option.Value));
                builder.SetUpdatesAttributeName("value");
                builder.AddAttribute(8, "selected", selected);
                builder.AddContent(9, option.ChildContent);
                builder.CloseElement();
            }
            builder.AddElementReferenceCapture(10, __inputReference => Element = __inputReference);
            builder.CloseElement();
            builder.OpenComponent<CascadingValue<EditerSelect<Tval>>>(11);
            builder.AddAttribute(12, "Value", this);
            builder.AddAttribute(13, "IsFixed", true);
            builder.AddContent(14, ChildContent);

            builder.CloseComponent();
        }

        internal class OptionEntry<TVal>
        {
            public string Key { get; set; } = "";
            public TVal? Value { get; set; }
            public RenderFragment? ChildContent { get; set; }
            public bool IsPlaceholder { get; set; }
        }
    }
}
