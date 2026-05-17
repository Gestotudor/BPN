using AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;
using AuthService.Application.Features.ApiKeys.Commands.RevokeApiKey;
using AuthService.Application.Features.ApiKeys.Queries.GetApiKeys;
using AuthService.Application.Common.Results;
using BuildingBlocks.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/api-keys")]
public sealed class ApiKeysController : ControllerBase
{
    private readonly ISender _sender;

    public ApiKeysController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateApiKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateApiKeyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(ApiResponse<CreateApiKeyResponse>.Ok(result.Value!, "API key created successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<GetApiKeysResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApiKeysQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(ApiResponse<IReadOnlyCollection<GetApiKeysResponse>>.Ok(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAsync(
        Guid id,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RevokeApiKeyCommand(id, reason), cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(ApiResponse<object>.Ok(new { }, "API key revoked successfully."));
    }

    private ObjectResult MapFailure<T>(Result<T> result)
    {
        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.Validation => BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            _ => BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors)))
        };
    }

    private ObjectResult MapFailure(Result result)
    {
        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.Validation => BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            _ => BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors)))
        };
    }
}
