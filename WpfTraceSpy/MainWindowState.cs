namespace TraceSpy
{
    public class MainWindowState : DictionaryObject
    {
        public MainWindowState()
        {
            ResolveProcessName = true;
            RemoveEmptyLines = true;
            ShowProcessId = true;
            ShowEtwDescription = true;
            AutoScroll = true;
        }

        public bool ResolveProcessName { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool RemoveEmptyLines { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool ShowProcessId { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool ShowEtwDescription { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool AutoScroll { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        public bool EtwStarted
        {
            get => DictionaryObjectGetPropertyValue<bool>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(EtwText));
                }
            }
        }

        public bool? OdsStarted
        {
            get => DictionaryObjectGetPropertyValue<bool?>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(OdsText));
                }
            }
        }

        public string EtwText => EtwStarted ? "Stop ETW Traces" : "Start ETW Traces";
        public string OdsText
        {
            get
            {
                switch (OdsStarted)
                {
                    case true:
                        return "Stop ODS Traces";

                    case false:
                        return "Start ODS Traces";

                    default:
                        return "ODS Unavailable";
                }
            }
        }
    }
}
