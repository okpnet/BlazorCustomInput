using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCustomInput.Components
{
    public sealed class AutocompleteArg<Tval>
    {
        public Tval? Value { get; init; }
        public bool IsLoading { get; init; }

        public EventCallback<Tval> SelectCallback { get; init; }
        
    }
}
