namespace KtMobileTestManager.Models;

public class TestScenario
{
    public string Id { get; set; } = string.Empty;
    public string MainModule { get; set; } = string.Empty;
    public string SubScreen { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AcceptanceCriteria { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string Status { get; set; } = "Bekliyor"; // Geçti, Kaldı, Bekliyor
    public string TesterNote { get; set; } = string.Empty;
    public DateTime? ExecutedAt { get; set; }
}

public class TrackerViewModel
{
    public string? SelectedModule { get; set; }
    public string? SelectedScreen { get; set; }
    public bool HasFiltered { get; set; } = false;

    public string? ExecutedModuleFilter { get; set; }

    public List<TestScenario> FilteredScenarios { get; set; } = new();
    public List<TestScenario> ExecutedScenarios { get; set; } = new();

    public int TotalScenariosCount { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
}
