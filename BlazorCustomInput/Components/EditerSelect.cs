using BlazorCustomInput.Argments;
using BlazorCustomInput.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorCustomInput.Components
{
    public class EditerSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Tval> : EditBase<Tval>
    {
        private readonly bool _isMultipleSelect;
        private IEnumerable<Tval> _source;

        /// <summary>
        /// Gets or sets the child content to be rendering inside the select element.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }
        /// <summary>
        /// Gets source to a select option. 
        /// </summary>
        [Parameter, EditorRequired]
        public Func<IEnumerable<Tval>> Source { get; set; } = default!;
        [Parameter]
        public RenderFragment? BeforeOptionContent { get; set; }

        [Parameter]
        public RenderFragment? AfterOptionContent { get; set; }
        /// <summary>
        /// Rendering string the options. 
        /// </summary>
        [Parameter, EditorRequired]
        public Func<Tval?, string> OptionString { get; set; } = default!;
        /// <summary>
        /// Action before rendering the options. 
        /// </summary>
        [Parameter]
        public Action<OptionArgment<Tval>> BeforeOptionRender { get; set; } = default!;

        public EditerSelect()
        {
            _isMultipleSelect = typeof(Tval).IsArray;
            _source=Enumerable.Empty<Tval>();
        }

        protected override void OnParametersSet()
        {
            _source = Source();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int index = 0;
            builder.OpenElement(++index, "select");
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "class", CssClass);
            builder.AddAttribute(++index, "multiple", _isMultipleSelect);

            if (_isMultipleSelect)
            {
                builder.AddAttribute(++index, "value", BindConverter.FormatValue(CurrentValue)?.ToString());
                builder.AddAttribute(++index, "onchange", EventCallback.Factory.CreateBinder<string?[]?>(this, SetCurrentValueAsStringArray, default));
                builder.SetUpdatesAttributeName("value");
            }
            else
            {
                builder.AddAttribute(++index, "value", CurrentValueAsString);
                builder.AddAttribute(++index, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, default));
                builder.SetUpdatesAttributeName("value");
            }

            builder.AddElementReferenceCapture(++index, __inputReference => Element = __inputReference);
            builder.AddElementReferenceCapture(++index, __selectReference => Element = __selectReference);
            
            if(BeforeOptionContent is not null)builder.AddContent(++index, BeforeOptionContent);

            foreach(var item in _source)
            {
                var arg = new OptionArgment<Tval>(item);
                if(BeforeOptionRender is not null)
                {
                    BeforeOptionRender(arg);
                }
                builder.OpenElement(++index, "option");
                ++index;
                if (arg.IsDisable)
                {
                    builder.AddAttribute(index, "diable", arg.IsDisable);
                }
                ++index;
                if (arg.CssClass is not (null or ""))
                {
                    builder.AddAttribute(index, "class", arg.CssClass);
                }
                ++index;
                if (arg.AdditionalAttributes.Count>0)
                {
                    builder.AddMultipleAttributes(index, arg.AdditionalAttributes);
                }
                builder.AddAttribute(++index, "value", item?.GetHashCode());
                builder.AddMarkupContent(++index, OptionString(arg.Value));
                builder.CloseElement();
            }

            if (AfterOptionContent is not null) builder.AddContent(10000, AfterOptionContent);

            builder.CloseElement();
        }

        protected override bool ConvetTo(string? value, out Tval? result)
        {
            if (value is null or "")
            {
                result = default;
                return true;
            }
            else
            {
                var enumrator=_source.GetEnumerator();
                while (enumrator.MoveNext())
                {
                    var current= enumrator.Current;
                    if (current is null) continue;
                    if (current.GetHashCode().ToString() == value)
                    {
                        result = current;
                        return true;
                    }
                }
                result= default;
                return false;
            }
        }

        protected override string? FormatValueAsString(Tval? value)=> value?.GetHashCode().ToString();

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Tval result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            var resultVal = ConvetTo(value, out result);
            validationErrorMessage = resultVal ? string.Empty : ParsingErrorMessage;
            return resultVal;
        }

        private void SetCurrentValueAsStringArray(string?[]? value)
        {
            if (value is null || value.Length == 0) return;

            CurrentValue = BindConverter.TryConvertTo<Tval>(_source.Where(t=>value.Contains(t!.GetHashCode().ToString())).ToArray(), CultureInfo.CurrentCulture, out var result)
                ? result
                : default;
        }

        private void OnChange(string? value)
        {

        }
    }
}
