using Microsoft.AspNetCore.Mvc;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Controllers;

[ApiController]
[Route("api/templates")]
public sealed class TemplatesController(ITemplateService templates) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<GameTemplateResponse>> GetAll() => Ok(templates.GetAll());
}
