using Microsoft.AspNetCore.Components;

namespace BlazorCustomInput.Components
{
    public class EditorSelectOption<Tval>:ComponentBase
    {
        [CascadingParameter]
        public EditorSelect<Tval>? Parent { get; set; }

        [Parameter]
        public Tval? Value { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public bool IsPlaceholder { get; set; }

        protected override void OnInitialized()
        {
            Parent?.RegisterOption(this);
        }
    }
}
