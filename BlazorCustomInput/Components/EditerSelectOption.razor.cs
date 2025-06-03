using BlazorCustomInput.Components.Tests;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCustomInput.Components
{
    public class EditerSelectOption<Tval>:ComponentBase
    {
        [CascadingParameter]
        public EditorSelectTest<Tval>? Parent { get; set; }

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
