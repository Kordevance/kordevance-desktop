namespace Kori;

public class AppConstants
{
    public static bool IsReleaseEnvironment = 
        #if DEBUG
            false;
        #else
            true;
        #endif
}