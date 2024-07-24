using Microsoft.AspNetCore.Mvc;
using TvShow.Domain;

namespace TvShow.Api.Controllers;

[ApiController]
[Route("api/show")]
public class ShowsController : ControllerBase
{
    private readonly ITvShowRepository _repository;

    public ShowsController(ITvShowRepository repository)
    {
        _repository = repository;
    }

    [HttpGet()]
    public Task<IEnumerable<Domain.Models.TvShow>> Index()
    {
        return Get(0, 20);
    }

    [HttpGet("page/{pageNumber:int=0}")]
    public Task<IEnumerable<Domain.Models.TvShow>> Index(int pageNumber)
    {
        return Get(pageNumber, 20);
    }

    [HttpGet("page/{pageNumber:int=0}/pageSize/{pageSize:int=20}")]
    public async Task<IEnumerable<Domain.Models.TvShow>> Get(int pageNumber, int pageSize)
    {
        var data = await _repository.GetShowsPaged(pageNumber, pageSize, CancellationToken.None);
        return data;
    }
}
