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
            OdsStarted = false;
            ShowTicksMode = ShowTicksMode.AsTicks;
        }

        public bool ResolveProcessName { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool RemoveEmptyLines { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool ShowProcessId { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool ShowEtwDescription { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool AutoScroll { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool WrapText { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool DontSplitText { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        public ShowTicksMode ShowTicksMode
        {
            get => DictionaryObjectGetPropertyValue<ShowTicksMode>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(ShowTicksAsTicks));
                    OnPropertyChanged(nameof(ShowTicksAsTime));
                    OnPropertyChanged(nameof(ShowTicksAsSeconds));
                    OnPropertyChanged(nameof(ShowTicksAsMilliseconds));
                    OnPropertyChanged(nameof(ShowTicksAsDeltaTicks));
                    OnPropertyChanged(nameof(ShowTicksAsDeltaSeconds));
                    OnPropertyChanged(nameof(ShowTicksAsDeltaMilliseconds));
                }
            }
        }
        public bool ShowTicksAsTicks { get => ShowTicksMode == ShowTicksMode.AsTicks; set { if (value) { ShowTicksMode = ShowTicksMode.AsTicks; } } }
        public bool ShowTicksAsTime { get => ShowTicksMode == ShowTicksMode.AsTime; set { if (value) { ShowTicksMode = ShowTicksMode.AsTime; } } }
        public bool ShowTicksAsSeconds { get => ShowTicksMode == ShowTicksMode.AsSeconds; set { if (value) { ShowTicksMode = ShowTicksMode.AsSeconds; } } }
        public bool ShowTicksAsMilliseconds { get => ShowTicksMode == ShowTicksMode.AsMilliseconds; set { if (value) { ShowTicksMode = ShowTicksMode.AsMilliseconds; } } }
        public bool ShowTicksAsDeltaTicks { get => ShowTicksMode == ShowTicksMode.AsDeltaTicks; set { if (value) { ShowTicksMode = ShowTicksMode.AsDeltaTicks; } } }
        public bool ShowTicksAsDeltaSeconds { get => ShowTicksMode == ShowTicksMode.AsDeltaSeconds; set { if (value) { ShowTicksMode = ShowTicksMode.AsDeltaSeconds; } } }
        public bool ShowTicksAsDeltaMilliseconds { get => ShowTicksMode == ShowTicksMode.AsDeltaMilliseconds; set { if (value) { ShowTicksMode = ShowTicksMode.AsDeltaMilliseconds; } } }

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
