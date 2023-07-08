using RESTMicroservice.Models;

namespace RESTMicroservice.Storage;

public interface IStorage
{
    /// <summary>
    /// Получить файл по переданным параметрам
    /// </summary>
    /// <param name="quoteProvider"></param>
    /// <param name="date"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Возврщается stream на файл. Если файл не найден, будет возвращен null.
    /// Предполагается что вызывающая сторона должна завернуть stream в using, либо гарантировать вызов Dispose()
    /// </returns>
    Task<Stream?> GetFile(QuoteProvider quoteProvider, DateTime date, CancellationToken cancellationToken);
    
    /// <summary>
    /// Сохранить файл в хранилище
    /// </summary>
    /// <param name="quoteProvider">Провайдер котировок</param>
    /// <param name="date">Дата</param>
    /// <param name="stream">Stream с данными файла</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveFile(QuoteProvider quoteProvider, DateTime date, Stream stream, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить список доступных файлов с котировками
    /// </summary>
    /// <param name="quoteProvider">Провайдер котировок, если null то все доступные провайдеры</param>
    /// <param name="dateFrom">Дата С</param>
    /// <param name="dateTo">Дата По</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<FileDescription>> GetQuoteList(QuoteProvider? quoteProvider, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken);
}