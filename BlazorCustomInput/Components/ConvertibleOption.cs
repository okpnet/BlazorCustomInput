using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorCustomInput.Components
{
    /// <summary>
    /// it is child content from EditerConvertibleSelectClass.
    /// </summary>
    /// <typeparam name="TVal"></typeparam>
    public class ConvertibleOption<TVal> : ComponentBase
    {
        /// <summary>
        /// object
        /// </summary>
        [CascadingParameter]
        public TVal? Value { get; set; }
        /// <summary>
        /// content of option tag 
        /// </summary>
        [Parameter]
        public RenderFragment<TVal>? ChildContent { get; set; }
        /// <summary>
        /// attribute to option tags.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }
        /// <summary>
        /// Hash from object.
        /// component get value from hash.
        /// </summary>
        [CascadingParameter]
        public int ValueId { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int index = 0;
            builder.OpenElement(++index, "option");
            builder.AddMultipleAttributes(++index, AdditionalAttributes);
            builder.AddAttribute(++index, "value", ValueId);
            builder.AddContent(++index, ChildContent!, Value);
            builder.CloseComponent();
        }
    }
}
