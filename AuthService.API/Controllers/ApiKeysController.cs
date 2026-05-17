using AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;
using AuthService.Application.Features.ApiKeys.Commands.RevokeApiKey;
using AuthService.Application.Features.ApiKeys.Queries.GetApiKeys;
using AuthService.Application.Common.Results;
using BuildingBlocks.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

/// <summary>
/// Manages MoneyBee API keys for branches, internal services, and client applications.
/// </summary>
[ApiController]
[Route("api/api-keys")]
[Produces("application/json")]
public sealed class ApiKeysController : ControllerBase
{
    private readonly ISender _sender;

    public ApiKeysController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Creates a new API key for a branch, internal service, or client application.
    /// </summary>
    /// <remarks>
    /// Creates an API key and returns the plaintext key only once. The key is stored as a secure hash in the database. Requires the <c>auth.manage</c> scope.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateApiKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Lists API key metadata.
    /// </summary>
    /// <remarks>
    /// Returns API key metadata without exposing plaintext API keys.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<GetApiKeysResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApiKeysQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(ApiResponse<IReadOnlyCollection<GetApiKeysResponse>>.Ok(result.Value!));
    }

    /// <summary>
    /// Revokes an API key.
    /// </summary>
    /// <param name="id">Identifier of the API key to revoke.</param>
    /// <param name="reason">Optional operational reason recorded for the revocation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Soft revokes an API key so it can no longer authenticate requests.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
