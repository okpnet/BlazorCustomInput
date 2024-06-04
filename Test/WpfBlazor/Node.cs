using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBlazor
{
    internal class Node
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public Node? Parent { get; set; }
    }
}
