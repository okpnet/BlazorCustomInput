using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace BlazorCustomInput.Components.Tests
{
    public partial class EditorSelectTest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
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
        }

        protected bool IsValueNull => EqualityComparer<Tval>.Default.Equals(Value, default);

        protected bool Compare(Tval? x, Tval? y)
            => CompareFunc?.Invoke(x, y) ?? EqualityComparer<Tval>.Default.Equals(x, y);

        protected string? GetOptionKey(Tval? value)=> OptionValueSelector is not null? 
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
            result=selectedOption is not null && selectedOption.Value is not null ? selectedOption.Value : default!;
            return true;
        }

        protected override string? FormatValueAsString(Tval? value) => GetOptionKey(value);

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Tval result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            var resultVal = ConvetTo(value, out result);
            validationErrorMessage = resultVal ? string.Empty : ParsingErrorMessage;
            return resultVal;
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