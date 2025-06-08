namespace VAYTIEN.Extensions;

public class StartupTrackerService
{
    private bool _hasForcedLogout = false;

    public bool HasForcedLogout() => _hasForcedLogout;

    public void MarkAsLoggedOut() => _hasForcedLogout = true;
}
