namespace DeWaveFreeAPI.Enums
{
    namespace DeWaveFreeAPI.Enums
    {
        public static class EventTypeValues
        {
            public const string Online = "online";
            public const string Offline = "offline";
            public const string Seminar = "seminar";

            public static readonly string[] All = { Online, Offline, Seminar };

            public static bool IsValid(string value)
            {
                return All.Contains(value?.ToLower());
            }
        }

        public static class VisibilityValues
        {
            public const string Course = "course";
            public const string Public = "public";
            public const string Invite = "invite";

            public static readonly string[] All = { Course, Public, Invite };

            public static bool IsValid(string value)
            {
                return All.Contains(value?.ToLower());
            }
        }
    }
}
