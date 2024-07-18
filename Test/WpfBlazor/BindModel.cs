using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBlazor
{
    public class BindModel
    {
        public string Name { get; set; }
        public  int Id { get; set; }

        public BindModel? Child { get; set; }
    }
}
