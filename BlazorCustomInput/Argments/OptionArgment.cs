using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCustomInput.Argments
{
    public sealed class OptionArgment<TVal>
    {
        public string CssClass { get; set; }=string.Empty;
        public bool IsDisable { get; set; } = false;

        public IReadOnlyDictionary<string,object> AdditionalAttributes { get; set; }=new Dictionary<string,object>();
        public TVal? Value { get; }

        public OptionArgment(TVal? value)
        {
            Value = value;
        }
    }
}
