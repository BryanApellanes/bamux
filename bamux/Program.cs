using Bam.Data.Objects;
using Bam.Identity;
using Bam.Identity.Clients;
using Bam.Presentation;
using Bam.Protocol;
using Bam.Protocol.Client;
using Bam.Ux;
using Bam.Ux.Pages;
using Microsoft.AspNetCore.Builder;

int httpPort = 8082;
string? portArg = args.FirstOrDefault(a => a.StartsWith("--port="));
if (portArg != null && int.TryParse(portArg["--port=".Length..], out int parsedPort))
{
    httpPort = parsedPort;
}

// bamid's TCP endpoint. bamid derives this port deterministically from its server name ("bamid")
// via UseNameBasedPort — observed to be 24515 as long as bamid's server name stays "bamid".
// Override via --bamid-tcp-port= if bamid is deployed under a different name/port.
int bamidTcpPort = 24515;
string? bamidTcpPortArg = args.FirstOrDefault(a => a.StartsWith("--bamid-tcp-port="));
if (bamidTcpPortArg != null && int.TryParse(bamidTcpPortArg["--bamid-tcp-port=".Length..], out int parsedBamidTcpPort))
{
    bamidTcpPort = parsedBamidTcpPort;
}

BamClient bamidClient = new BamClient(new JsonObjectDataEncoder(), BamClient.DefaultHttpBaseAddress, new BamHostBinding("localhost", bamidTcpPort));
IRegistrationService registrationService = new RegistrationServiceClient(bamidClient, BamClientProtocols.Tcp);
IdentityUxRoutes identityUxRoutes = new IdentityUxRoutes(registrationService);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();
app.Urls.Add($"http://localhost:{httpPort}");

app.MapPages(new IndexPage(), new RegisterPage(), new RegisterResultPage());
identityUxRoutes.MapRoutes(app);

Console.WriteLine($"[bamux] Starting on http://localhost:{httpPort}...");
app.Run();
