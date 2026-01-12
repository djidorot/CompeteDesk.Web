namespace CompeteDesk.ViewModels.Settings
{
    public class SettingsIndexViewModel
    {
        // Profile
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";

        // AI Preferences
        public string Verbosity { get; set; } = "Balanced";   // Short | Balanced | Detailed
        public string Tone { get; set; } = "Analytical";      // Executive | Analytical | Tactical

        public bool AutoDraftPlans { get; set; } = true;
        public bool AutoSummaries { get; set; } = true;
        public bool AutoRecommendations { get; set; } = true;
        public bool StoreDecisionTraces { get; set; } = true;
    }
}
