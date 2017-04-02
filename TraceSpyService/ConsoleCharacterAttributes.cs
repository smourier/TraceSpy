using System;

namespace TraceSpyService
{
	[Flags]
    public enum ConsoleCharacterAttributes
	{
		ForegroundBlue = 0x0001,
        ForegroundGreen = 0x0002,
        ForegroundRed = 0x0004,
        ForegroundIntensity = 0x0008,
		BackgroundBlue = 0x0010,
		BackgroundGreen = 0x0020,
		BackgroundRed = 0x0040,
		BackgroundIntensity = 0x0080,
		ForegroundWhite = ForegroundBlue | ForegroundGreen | ForegroundRed,
		BackgroundWhite = BackgroundBlue | BackgroundGreen | BackgroundRed,
	}
}
