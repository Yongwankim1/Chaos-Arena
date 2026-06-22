using System.Collections.Generic;

public static class RoomSessionData
{
    public static string RoomName;
    public static bool IsHost;
    public static MatchType MatchType;
    public static Dictionary<int, TeamSelectType> TeamSelections = new Dictionary<int, TeamSelectType>();
    public static TeamType HostTeam;

    public static void Clear()
    {
        RoomName = "";

        IsHost = false;

        HostTeam = TeamType.None;

        MatchType = MatchType.None;

        TeamSelections.Clear();
    }
}