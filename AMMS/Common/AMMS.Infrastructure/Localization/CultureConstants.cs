namespace AMMS.Infrastructure.Localization
{

    public static class CultureConstants
    {
        public const string HttpContextItemKey = "AmmsCulture";
        public const string DefaultCulture = "tr";

        public static readonly IReadOnlyList<string> SupportedCultures = ["tr", "en", "ar"];
    }
}
