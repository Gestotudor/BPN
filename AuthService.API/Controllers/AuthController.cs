using AuthService.Application.Common.Results;
using AuthService.Application.Features.Auth.Queries.ValidateApiKey;
using BuildingBlocks.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ValidateApiKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
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
