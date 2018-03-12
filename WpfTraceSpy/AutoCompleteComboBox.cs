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
