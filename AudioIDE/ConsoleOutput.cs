using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AudioIDE
{
    public class ConsoleOutput : TextBlock
    {
        public void AddLine(string Text)
        {
            Inlines.Add(Text + "\n");
        }
    }
}
