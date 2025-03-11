// Controllers/QuotesController.cs
using Microsoft.AspNetCore.Authorization;
using AMSPM_BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly MongoDbContext _context;

    public QuotesController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpPost("import")]
    public IActionResult ImportQuote([FromForm] IFormFile file, [FromForm] QuoteInputModel model)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Geen bestand geüpload");

        ObjectId fileId;
        using (var stream = file.OpenReadStream())
        {
            fileId = _context.UploadFile(file.FileName, stream);
        }

        var quote = new Quote
        {
            CustomerId = ObjectId.Parse(model.CustomerId),
            ProjectName = model.ProjectName,
            Amount = model.Amount,
            Description = model.Description,
            ValidUntil = model.ValidUntil,
            PdfFileId = fileId
        };

        _context.Quotes.InsertOne(quote);
        return Ok(new { Id = quote.Id.ToString() });
    }

    [HttpGet("export/{id}")]
    public IActionResult ExportQuote(string id)
    {
        var quote = _context.Quotes
            .Find(q => q.Id == ObjectId.Parse(id))
            .FirstOrDefault();

        if (quote == null)
            return NotFound("Offerte niet gevonden");

        try
        {
            var fileBytes = _context.DownloadFile(quote.PdfFileId);
            return File(fileBytes, "application/pdf", $"{quote.ProjectName}.pdf");
        }
        catch (Exception)
        {
            return NotFound("Bestand niet gevonden in GridFS");
        }
    }

    [HttpPut("edit/{id}")]
    public IActionResult EditQuote(string id, [FromBody] QuoteInputModel model)
    {
        var update = Builders<Quote>.Update
            .Set(q => q.ProjectName, model.ProjectName)
            .Set(q => q.Amount, model.Amount)
            .Set(q => q.Description, model.Description)
            .Set(q => q.ValidUntil, model.ValidUntil);

        var result = _context.Quotes.UpdateOne(q => q.Id == ObjectId.Parse(id), update);
        if (result.ModifiedCount == 0) return NotFound();

        return Ok();
    }

    [HttpPost("send/{id}")]
    public IActionResult SendQuote(string id)
    {
        var quote = _context.Quotes.Find(q => q.Id == ObjectId.Parse(id)).FirstOrDefault();
        if (quote == null) return NotFound();

        // TODO: Implementeer MailKit met GridFS-bestand
        return Ok("E-mail functionaliteit komt later");
    }
}

