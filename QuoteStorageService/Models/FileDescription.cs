namespace QuoteStorageService.Models;

/// <summary>
/// Класс, представляющий структуру файла, например: WienerBoerse_20230708.csv
/// </summary>
public class FileDescription
{
    public QuoteProvider QuoteProvider { get; init; }
    public DateTime Date { get; init; }
}