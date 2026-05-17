using AuthService.Application.Common.Results;
using AuthService.Application.Features.Auth.Queries.ValidateApiKey;
using BuildingBlocks.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

/// <summary>
/// Validates API keys for internal MoneyBee services.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Validates an API key and optionally checks required scopes.
    /// </summary>
    /// <remarks>
    /// Used by Customer and Transfer services to authenticate incoming requests. If <c>requiredScopes</c> is supplied, the Auth service also checks authorization.
    /// </remarks>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ValidateApiKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateAsync(
        [FromBody] ValidateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ValidateApiKeyQuery(request.ApiKey, request.RequiredScopes),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Errors.Any(x => x.Contains("Rate limit exceeded.", StringComparison.OrdinalIgnoreCase)))
            {
                return StatusCode(
                    StatusCodes.Status429TooManyRequests,
                    ApiResponse<object>.Fail("Rate limit exceeded."));
            }

            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors)));
        }

        return Ok(ApiResponse<ValidateApiKeyResponse>.Ok(result.Value!));
    }
}
