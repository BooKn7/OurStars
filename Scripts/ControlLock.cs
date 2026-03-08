// P1・P2の同時拘束を防ぐためのロック管理
public static class ControlLock
{
    private static bool isP1Active = false;
    private static bool isP2Active = false;

    public static bool TryActivateP1()
    {
        if (isP2Active) return false;
        isP1Active = true;
        return true;
    }

    public static bool TryActivateP2()
    {
        if (isP1Active) return false;
        isP2Active = true;
        return true;
    }

    public static void ReleaseP1() => isP1Active = false;
    public static void ReleaseP2() => isP2Active = false;
}
