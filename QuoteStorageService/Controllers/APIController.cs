using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using QuoteStorageService.Models;
using QuoteStorageService.Storage;

namespace QuoteStorageService.Controllers;

[Route("api")]
[ApiController]
// ReSharper disable once InconsistentNaming
public class APIController : ControllerBase
{
    private readonly IStorage _storage;
    private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

    public APIController(IStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Метод для сохранения файла
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("saveFile")]
    public async Task<IActionResult> SaveFile(IFormFile formFile, CancellationToken cancellationToken)
    {
        async Task<BadRequestResult> GetBadRequestResult(string text, CancellationToken ct)
        {
            await HttpContext.Response.WriteAsync(text, ct);
            return new BadRequestResult();
        }

        try
        {
            Logger.Info("Start SaveFile. Validating file name");
            var fileName = formFile.FileName.Trim();
            //мы принимаем csv файл, поэтому требуем чтобы тип контента в соответствующем заголовке был CSV файл
            //https://developer.mozilla.org/ru/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
            if (!formFile.ContentType.Contains("text/csv", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Supports only text/csv content type");

            //Парсим формат файла, пример файла: ShanghaiFuturesExchange_20230708.csv
            var fileNameParts =
                fileName.Split('_', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (fileNameParts.Length != 2)
                return await GetBadRequestResult("Invalid uploaded file name", cancellationToken);

            if (!Enum.TryParse<QuoteProvider>(fileNameParts[0], out var quoteProvider))
                return await GetBadRequestResult("Unsupported exchange", cancellationToken);

            if (!fileNameParts[1].EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
                return await GetBadRequestResult("Supports only csv files", cancellationToken);

            var dateString = fileNameParts[1][..^4]; //range indexer C#
            if (!DateTime.TryParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date))
                return await GetBadRequestResult("Invalid filename. Invalid date", cancellationToken);

            Logger.Trace("Validating file name done. File valid. Start save file");

            await using var stream = formFile.OpenReadStream();
            await _storage.SaveFile(quoteProvider, date, stream, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error while save file");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("quoteList")]
    public async Task<IActionResult> GetQuoteList(QuoteProvider? quoteProvider, [FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, CancellationToken cancellationToken)
    {
        if (dateFrom > dateTo)
        {
            Logger.Info("dateFrom larger dateTo");
            return BadRequest();
        }

        Logger.Info("Start GetQuoteList");
        var quoteList = await _storage.GetQuoteList(
            quoteProvider
            , dateFrom
            , dateTo
            , cancellationToken
        );
        Logger.Info("Finish GetQuoteList");
        return new JsonResult(quoteList);
    }

    [HttpGet("quoteSingle/{quoteProvider}/{date}")]
    public async Task<IActionResult> GetQuoteSingle(QuoteProvider quoteProvider, DateTime date, CancellationToken cancellationToken)
    {
        Logger.Info("Start GetQuoteSingle");
        var stream = await _storage.GetFile(quoteProvider, date, cancellationToken);
        if (stream == null)
        {
            Logger.Info("File not found");
            return NotFound(); //404
        }

        return new FileStreamResult(stream, "text/csv");
    }
}