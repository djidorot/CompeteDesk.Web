using System.Collections.Generic;

namespace CompeteDesk.ViewModels.Metrics;

public sealed class MetricsViewModel
{
    public string SelectedTab { get; set; } = "overview";
    public string SelectedRange { get; set; } = "day";
    public string FromDate { get; set; } = "";
    public string ToDate { get; set; } = "";

    public List<MetricsKpiCard> Kpis { get; set; } = new();

    // Charts (one series each, rendered by _MetricsChart)
    public string LeftChartTitle { get; set; } = "";
    public List<int> LeftChartPoints { get; set; } = new();
    public List<string> LeftChartLabels { get; set; } = new();
    public bool LeftChartPercent { get; set; }

    public string RightChartTitle { get; set; } = "";
    public List<int> RightChartPoints { get; set; } = new();
    public List<string> RightChartLabels { get; set; } = new();
    public bool RightChartPercent { get; set; }

    // Tables
    public string LeftTableTitle { get; set; } = "";
    public string RightTableTitle { get; set; } = "";
    public List<MetricsRankRow> LeftTableRows { get; set; } = new();
    public List<MetricsRankRow> RightTableRows { get; set; } = new();

    // Info tooltip in UI
    public string InfoText { get; set; } = "Metrics are computed from your CompeteDesk data (workspaces, strategies, actions, war room, habits, reports, and AI history).";
}

public sealed class MetricsKpiCard
{
    public string Icon { get; set; } = "◎";
    public string Label { get; set; } = "";
    public string ValueText { get; set; } = "0";
    public double DeltaPct { get; set; } // vs prior period (based on new items in selected time range)
}

public sealed class MetricsRankRow
{
    public string Name { get; set; } = "";
    public string ValueText { get; set; } = "0";
    public double ChangePct { get; set; }
}
