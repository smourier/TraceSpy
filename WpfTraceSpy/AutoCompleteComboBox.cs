using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TraceSpy
{
    public class AutoCompleteComboBox : ComboBox
    {
        public AutoCompleteComboBox()
        {
            IsEditable = true;
            ShouldPreserveUserEnteredPrefix = true;
        }
    }
}
