// Controllers/WorkOrdersController.cs
using Microsoft.AspNetCore.Authorization;
using AMSPM_BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly MongoDbContext _context;

    public WorkOrdersController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpPost("import")]
    public IActionResult ImportWorkOrder([FromForm] IFormFile file, [FromForm] WorkOrderInputModel model)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Geen bestand geüpload");

        ObjectId fileId;
        using (var stream = file.OpenReadStream())
        {
            fileId = _context.UploadFile(file.FileName, stream);
        }

        var workOrder = new WorkOrder
        {
            CustomerId = ObjectId.Parse(model.CustomerId),
            Description = model.Description,
            Date = model.Date,
            PdfFileId = fileId
        };

        _context.WorkOrders.InsertOne(workOrder);
        return Ok(new { Id = workOrder.Id.ToString() });
    }

    [HttpGet("export/{id}")]
    public IActionResult ExportWorkOrder(string id)
    {
        var workOrder = _context.WorkOrders
            .Find(w => w.Id == ObjectId.Parse(id))
            .FirstOrDefault();

        if (workOrder == null)
            return NotFound("Werkbon niet gevonden");

        try
        {
            var fileBytes = _context.DownloadFile(workOrder.PdfFileId);
            return File(fileBytes, "application/pdf", $"werkbon-{workOrder.Id}.pdf");
        }
        catch (Exception)
        {
            return NotFound("Bestand niet gevonden in GridFS");
        }
    }

    [HttpPut("edit/{id}")]
    public IActionResult EditWorkOrder(string id, [FromBody] WorkOrderInputModel model)
    {
        var update = Builders<WorkOrder>.Update
            .Set(w => w.Description, model.Description)
            .Set(w => w.Date, model.Date);

        var result = _context.WorkOrders.UpdateOne(w => w.Id == ObjectId.Parse(id), update);
        if (result.ModifiedCount == 0) return NotFound();

        return Ok();
    }

    [HttpPost("send/{id}")]
    public IActionResult SendWorkOrder(string id)
    {
        var workOrder = _context.WorkOrders.Find(w => w.Id == ObjectId.Parse(id)).FirstOrDefault();
        if (workOrder == null) return NotFound();

        // TODO: Implementeer MailKit met GridFS-bestand
        return Ok("E-mail functionaliteit komt later");
    }
}

public class WorkOrderInputModel
{
    public string CustomerId { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
}