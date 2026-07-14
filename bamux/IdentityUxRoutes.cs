using Bam.Identity;
using Bam.Protocol.Data.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Bam.Ux;

/// <summary>
/// The result of a UX route computation: a response body and the HTTP status code to send it with.
/// <see cref="Body"/> is <c>object</c> deliberately — it holds either an <see cref="ErrorResponse"/> or the
/// route's own success-shape response (<see cref="RegisterPersonResponse"/>/<see cref="ProfileData"/>), matching
/// the polymorphic body type <c>Results.Json(object)</c> itself accepts.
/// </summary>
public class UxOutcome
{
    public object Body { get; set; } = null!;
    public int StatusCode { get; set; }
}

/// <summary>
/// Maps bamux's own <c>/api/register</c>/<c>/api/profile/{handle}</c> — the routes the registration UI
/// (<c>RegisterPage</c>) actually calls, same-origin — onto <see cref="IRegistrationService"/> calls against
/// <c>bamid</c>. Independent of bamsvc's identical-shaped <c>IdentityGatewayRoutes</c>; this is the route the
/// browser hits directly, not a proxy through bamsvc.
/// </summary>
public class IdentityUxRoutes
{
    private readonly IRegistrationService _registrationService;

    public IdentityUxRoutes(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public void MapRoutes(WebApplication app)
    {
        app.MapPost("/api/register", async (HttpContext ctx) =>
        {
            PersonRegistrationRequest? request = await ctx.Request.ReadFromJsonAsync<PersonRegistrationRequest>();
            UxOutcome outcome = BuildRegisterOutcome(request);
            return Results.Json(outcome.Body, statusCode: outcome.StatusCode);
        });

        app.MapGet("/api/profile/{handle}", (string handle) =>
        {
            UxOutcome outcome = BuildProfileOutcome(handle);
            return Results.Json(outcome.Body, statusCode: outcome.StatusCode);
        });
    }

    public UxOutcome BuildRegisterOutcome(PersonRegistrationRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return new UxOutcome
            {
                Body = new ErrorResponse { Error = "FirstName and LastName are required" },
                StatusCode = 400,
            };
        }

        try
        {
            AccountData accountData = _registrationService.RegisterPerson(
                request.FirstName, request.LastName, request.Email, request.Phone, request.Handle);
            return new UxOutcome
            {
                Body = new RegisterPersonResponse { PersonHandle = accountData.PersonHandle },
                StatusCode = 200,
            };
        }
        catch (Exception ex)
        {
            return new UxOutcome
            {
                Body = new ErrorResponse { Error = ex.Message },
                StatusCode = 500,
            };
        }
    }

    public UxOutcome BuildProfileOutcome(string handle)
    {
        ProfileData? result = _registrationService.GetProfile(handle);
        if (result == null)
        {
            return new UxOutcome
            {
                Body = new ErrorResponse { Error = "Profile not found" },
                StatusCode = 404,
            };
        }

        return new UxOutcome
        {
            Body = result,
            StatusCode = 200,
        };
    }
}
