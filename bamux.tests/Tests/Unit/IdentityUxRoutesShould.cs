using Bam.DependencyInjection;
using Bam.Identity;
using Bam.Protocol.Data.Server;
using Bam.Test;
using Bam.Ux;
using NSubstitute;

namespace Bam.Ux.Tests.Unit;

[UnitTestMenu("IdentityUxRoutes should", Selector = "iur")]
public class IdentityUxRoutesShould : UnitTestMenuContainer
{
    public IdentityUxRoutesShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void DelegateSuccessfulRegistrationToRegistrationService()
    {
        IRegistrationService registrationService = Substitute.For<IRegistrationService>();
        registrationService.RegisterPerson("Ada", "Lovelace", "ada@example.com", null, null)
            .Returns(new AccountData { PersonHandle = "ada-lovelace" });

        IdentityUxRoutes routes = new IdentityUxRoutes(registrationService);
        PersonRegistrationRequest request = new PersonRegistrationRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
        };

        When.A<IdentityUxRoutes>(
            "builds the register outcome for a valid request",
            routes,
            (r) => r.BuildRegisterOutcome(request))
        .TheTest
        .ShouldPass(because =>
        {
            UxOutcome outcome = routes.BuildRegisterOutcome(request);
            because.ItsTrue("status code is 200", outcome.StatusCode == 200);
            because.ItsTrue("body is a RegisterPersonResponse with the expected handle",
                outcome.Body is RegisterPersonResponse response && response.PersonHandle == "ada-lovelace");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RejectRegistrationMissingRequiredFields()
    {
        IRegistrationService registrationService = Substitute.For<IRegistrationService>();
        IdentityUxRoutes routes = new IdentityUxRoutes(registrationService);
        PersonRegistrationRequest request = new PersonRegistrationRequest
        {
            FirstName = "",
            LastName = "Lovelace",
        };

        When.A<IdentityUxRoutes>(
            "builds the register outcome for a request missing FirstName",
            routes,
            (r) => r.BuildRegisterOutcome(request))
        .TheTest
        .ShouldPass(because =>
        {
            UxOutcome outcome = routes.BuildRegisterOutcome(request);
            because.ItsTrue("status code is 400", outcome.StatusCode == 400);
            because.ItsTrue("body is an ErrorResponse", outcome.Body is ErrorResponse);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void DelegateProfileLookupToRegistrationService()
    {
        IRegistrationService registrationService = Substitute.For<IRegistrationService>();
        registrationService.GetProfile("ada-lovelace").Returns(new ProfileData
        {
            ProfileHandle = "profile-1",
            PersonHandle = "ada-lovelace",
            Name = "Ada Lovelace",
            DeviceHandle = "device-1",
        });

        IdentityUxRoutes routes = new IdentityUxRoutes(registrationService);

        When.A<IdentityUxRoutes>(
            "builds the profile outcome for a known handle",
            routes,
            (r) => r.BuildProfileOutcome("ada-lovelace"))
        .TheTest
        .ShouldPass(because =>
        {
            UxOutcome outcome = routes.BuildProfileOutcome("ada-lovelace");
            because.ItsTrue("status code is 200", outcome.StatusCode == 200);
            because.ItsTrue("body is the expected ProfileData",
                outcome.Body is ProfileData profile && profile.PersonHandle == "ada-lovelace");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ReturnNotFoundForUnknownProfileHandle()
    {
        IRegistrationService registrationService = Substitute.For<IRegistrationService>();
        registrationService.GetProfile("unknown-handle").Returns((ProfileData?)null);

        IdentityUxRoutes routes = new IdentityUxRoutes(registrationService);

        When.A<IdentityUxRoutes>(
            "builds the profile outcome for an unknown handle",
            routes,
            (r) => r.BuildProfileOutcome("unknown-handle"))
        .TheTest
        .ShouldPass(because =>
        {
            UxOutcome outcome = routes.BuildProfileOutcome("unknown-handle");
            because.ItsTrue("status code is 404", outcome.StatusCode == 404);
            because.ItsTrue("body is an ErrorResponse", outcome.Body is ErrorResponse);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
