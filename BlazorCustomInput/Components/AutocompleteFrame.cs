using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCustomInput.Components
{
    public class AutocompleteFrame<TVal>:ComponentBase
    {
        [CascadingParameter]
        public bool IsLoading { get; set; }
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        [CascadingParameter]
        public IEnumerable<AutocompleteArg<TVal>> AutocompleteItems { get; set; } = default!;
        /// <summary>
        /// オートコンプリートアイテムを表示するコンテンツ
        /// </summary>
        [Parameter]
        public RenderFragment<AutocompleteArg<TVal>> Template { get; set; } = default!;


        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int index = 0;
            foreach(var item in AutocompleteItems)
            {
                builder.AddContent(++index, AutocomleteNodes(item));
            }
        }
    }
}
