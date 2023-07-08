namespace QuoteStorageService.Configuration;

public enum StorageType
{
    //TODO: комментарии на всех полях и классах
    /// <summary>
    /// 
    /// </summary>
    DB, 
    /// <summary>
    /// 
    /// </summary>
    FileSystem
}

public class StorageConfiguration
{
    public StorageType Type { get; init; }
    // ReSharper disable once InconsistentNaming
    public DataBaseConfiguration? DB { get; init; }
    public FileSystemConfiguration? FileSystem { get; init; }

    internal bool SelfValidate()
    {
        switch (Type)
        {
            case StorageType.DB:
                if (string.IsNullOrEmpty(DB?.ConnectionString))
                    throw new InvalidOperationException("Connection string must be specified");
                break;
            case StorageType.FileSystem:
                if (string.IsNullOrEmpty(FileSystem?.BaseDirectory))
                    throw new InvalidOperationException("BaseDirectory must be specified");

                if (!Directory.Exists(FileSystem.BaseDirectory))
                    throw new InvalidOperationException("BaseDirectory not exists");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, "Value not supported");
        }

        return true;
    }
}

public class DataBaseConfiguration
{
    public string? ConnectionString { get; init; }
}

public class FileSystemConfiguration
{
    public string? BaseDirectory { get; init; }
} 

