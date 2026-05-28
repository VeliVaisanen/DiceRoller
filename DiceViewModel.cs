using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace DiceRoller;

public partial class DiceViewModel : ObservableObject
{
    private readonly Random _random = new();

    // --- Säätöparametrit ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DiceCountLabel))]
    private int _diceCount = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SidesLabel))]
    private int _sides = 6;

    // --- Tulokset ---
    [ObservableProperty]
    private string _totalResult = "—";

    [ObservableProperty]
    private string _averageResult = "—";

    [ObservableProperty]
    private string _diceDisplay = "🎲  🎲";

    [ObservableProperty]
    private bool _isRolling = false;

    // --- Historia ---
    public ObservableCollection<RollHistoryItem> History { get; } = new();

    // --- Label-apuominaisuudet ---
    public string DiceCountLabel => $"{DiceCount} noppaa";
    public string SidesLabel => $"d{Sides}";

    // --- Heittokomento ---
    [RelayCommand]
    private async Task RollAsync()
    {
        if (IsRolling) return;
        IsRolling = true;

        // Pieni animaatioviive
        DiceDisplay = "🎲 ...";
        await Task.Delay(300);

        // Heitetään nopat
        var results = Enumerable
            .Range(0, DiceCount)
            .Select(_ => _random.Next(1, Sides + 1))
            .ToList();

        int sum = results.Sum();
        double avg = (double)sum / results.Count;

        TotalResult = sum.ToString();
        AverageResult = avg.ToString("F1");
        DiceDisplay = string.Join("  ", results.Select(r => GetDieFace(r, Sides)));

        // Lisää historiaan (max 5 viimeisintä)
        History.Insert(0, new RollHistoryItem
        {
            Rolls = string.Join(", ", results),
            Sum = sum,
            Sides = Sides
        });
        while (History.Count > 5)
            History.RemoveAt(History.Count - 1);

        IsRolling = false;
    }

    // --- Apufunktiot ---
    private static string GetDieFace(int value, int sides)
    {
        if (sides != 6) return $"[{value}]";
        return value switch
        {
            1 => "⚀", 2 => "⚁", 3 => "⚂",
            4 => "⚃", 5 => "⚄", 6 => "⚅",
            _ => "?"
        };
    }
}

public class RollHistoryItem
{
    public string Rolls { get; init; } = "";
    public int Sum { get; init; }
    public int Sides { get; init; }
    public string Label => $"{Rolls}  (d{Sides})";
    public string SumLabel => Sum.ToString();
}
