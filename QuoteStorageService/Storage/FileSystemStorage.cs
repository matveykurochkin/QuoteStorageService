﻿using NLog;
using QuoteStorageService.Configuration;
using QuoteStorageService.Models;

namespace QuoteStorageService.Storage;

public class FileSystemStorage : IStorage
{
    private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly FileSystemConfiguration _conf;

    public FileSystemStorage(FileSystemConfiguration conf)
    {
        _conf = conf;
    }

    /// <summary>
    /// Метод, получающий файл по дате
    /// </summary>
    /// <param name="quoteProvider"></param>
    /// <param name="date"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Stream?> GetFile(QuoteProvider quoteProvider, DateTime date, CancellationToken cancellationToken)
    {
        Logger.Info("Start get file");
        var fullPath = Path.Combine(_conf.BaseDirectory!, quoteProvider.ToString(), $"{date:yyyyMMdd}.csv");
        if (File.Exists(fullPath))
        {
            Logger.Info("File found {fullPath}", fullPath);
            var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult(fs as Stream)!;
        }

        Logger.Info("File not exist {fullPath}", fullPath);
        return Task.FromResult(null as Stream);
    }

    /// <summary>
    /// Метод, сохраняющий файл 
    /// </summary>
    /// <param name="quoteProvider"></param>
    /// <param name="date"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    public async Task SaveFile(QuoteProvider quoteProvider, DateTime date, Stream stream, CancellationToken cancellationToken)
    {
        Logger.Info("Start save file");
        var fullPath = Path.Combine(_conf.BaseDirectory!, quoteProvider.ToString(), $"{date:yyyyMMdd}.csv");

        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        Logger.Trace("Try save file: {fullPath}", fullPath);
        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fs, cancellationToken);
        Logger.Info("Saved to file: {fullPath}", fullPath);
    }

    /// <summary>
    /// Метод возвращающий список файлов, лежащих в промежутке указанных дат
    /// </summary>
    /// <param name="quoteProvider"></param>
    /// <param name="dateFrom"></param>
    /// <param name="dateTo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<List<FileDescription>> GetQuoteList(QuoteProvider? quoteProvider, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken)
    {
        Logger.Info("Start save file");
        //TODO add log messages

        List<FileDescription> fileDescriptions = new();
        for (var currentDate = dateFrom; currentDate <= dateTo; currentDate = currentDate.AddDays(1))
        {
            // ShanghaiFuturesExchange,
            if (!quoteProvider.HasValue || quoteProvider.Value == QuoteProvider.ShanghaiFuturesExchange)
            {
                var fullPath = Path.Combine(_conf.BaseDirectory!, QuoteProvider.ShanghaiFuturesExchange.ToString(), $"{currentDate:yyyyMMdd}.csv");
                if (File.Exists(fullPath))
                    fileDescriptions.Add(new()
                    {
                        QuoteProvider = QuoteProvider.ShanghaiFuturesExchange,
                        Date = currentDate
                    });
            }

            // WienerBoerse
            if (!quoteProvider.HasValue || quoteProvider.Value == QuoteProvider.WienerBoerse)
            {
                var fullPath = Path.Combine(_conf.BaseDirectory!, QuoteProvider.WienerBoerse.ToString(), $"{currentDate:yyyyMMdd}.csv");
                if (File.Exists(fullPath))
                    fileDescriptions.Add(new()
                    {
                        QuoteProvider = QuoteProvider.WienerBoerse,
                        Date = currentDate
                    });
            }
        }

        return Task.FromResult(fileDescriptions);
    }
}