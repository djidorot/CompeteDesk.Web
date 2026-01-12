using System.Collections.Generic;

namespace CompeteDesk.ViewModels.Metrics;

public class MetricsViewModel
{
    public int WorkspacesCount { get; set; }
    public int StrategiesCount { get; set; }
    public int ActionsCount { get; set; }

    public int HabitsCount { get; set; }
    public int ActiveHabitsCount { get; set; }

    public int WarIntelCount { get; set; }
    public int WarPlansCount { get; set; }

    public int WebsiteReportsCount { get; set; }
    public int BusinessReportsCount { get; set; }

    public int AiTracesCount { get; set; }

    public List<StatusCount> ActionStatuses { get; set; } = new();

    public class StatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
