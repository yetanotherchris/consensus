using Consensus.Api;

var builder = WebApplication.CreateBuilder(args);

var consensusBuilder = new ConsensusWebAppBuilder(builder);
consensusBuilder.RegisterServices(builder.Services);

var app = builder.Build();
consensusBuilder.SetupMiddleware(app);

app.Run();
