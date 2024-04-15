using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.AI.Clu;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure.Tasks;
using FFXIVVenues.Veni.UserSupport;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;
using FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.Veni.VenueDiscovery.Commands;
using FFXIVVenues.Veni.VenueObservations;
using FFXIVVenues.Veni.VenueRendering;
using OfficeOpenXml;

var serviceCollection = new ServiceCollection();
var config = Bootstrap.LoadConfiguration(serviceCollection);
Bootstrap.ConfigureLogging(serviceCollection, config);
Bootstrap.ConfigureApiClient(serviceCollection, config);
Bootstrap.ConfigureRepository(serviceCollection, config);
Bootstrap.ConfigureDiscordClient(serviceCollection, config);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

serviceCollection.AddSingleton<ICommandBroker, CommandBroker>();
serviceCollection.AddSingleton<IComponentBroker, ComponentBroker>();
serviceCollection.AddSingleton<IApiService, ApiService>();
serviceCollection.AddSingleton<IAuthorizer, Authorizer>();
serviceCollection.AddSingleton<IGuildManager, GuildManager>();
serviceCollection.AddSingleton<IVenueApprovalService, VenueApprovalService>();
serviceCollection.AddSingleton<IAIHandler, AIHandler>();
serviceCollection.AddSingleton<IDavinciService, DavinciService>();
serviceCollection.AddSingleton<IAIContextBuilder, AiContextBuilder>();
serviceCollection.AddSingleton<IIntentHandlerProvider, IntentHandlerProvider>();
serviceCollection.AddSingleton<ISessionProvider, SessionProvider>();
serviceCollection.AddSingleton<IDiscordHandler, DiscordHandler>();
serviceCollection.AddSingleton<ICluClient, CluClient>();
serviceCollection.AddSingleton<IVenueAuditService, VenueAuditService>();
serviceCollection.AddSingleton<IVenueRenderer, VenueRenderer>();
serviceCollection.AddSingleton<IApiObservationService, ApiObservationService>();
serviceCollection.AddSingleton<IInteractionContextFactory, InteractionContextFactory>();
serviceCollection.AddSingleton<ICommandCartographer, CommandCartographer>();
serviceCollection.AddSingleton<IMassAuditService, MassAuditService>();
serviceCollection.AddSingleton<IMassAuditExporter, MassAuditExporter>();
serviceCollection.AddSingleton<MassNoticeService>();
serviceCollection.AddSingleton<IDiscordValidator, DiscordValidator>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var commandBroker = serviceProvider.GetService<ICommandBroker>();
commandBroker.AddFromAssembly();
commandBroker.AddVenueControlCommands();
commandBroker.Add<EscalateCommand.CommandFactory, EscalateCommand.CommandHandler>(EscalateCommand.COMMAND_NAME);
commandBroker.Add<FindCommand.CommandFactory, FindCommand.CommandHandler>(FindCommand.COMMAND_NAME);
commandBroker.Add<HelpCommand.CommandFactory, HelpCommand.CommandHandler>(HelpCommand.COMMAND_NAME);
commandBroker.Add<ShowOpenCommand.CommandFactory, ShowOpenCommand.CommandHandler>(ShowOpenCommand.COMMAND_NAME);
commandBroker.Add<ShowForCommand.CommandFactory, ShowForCommand.CommandHandler>(ShowForCommand.COMMAND_NAME);
commandBroker.Add<ShowMineCommand.CommandFactory, ShowMineCommand.CommandHandler>(ShowMineCommand.COMMAND_NAME);
commandBroker.Add<SetWelcomeJoinersCommand.CommandFactory, SetWelcomeJoinersCommand.CommandHandler>(SetWelcomeJoinersCommand.COMMAND_NAME);
commandBroker.Add<SetFormatNamesCommand.CommandFactory, SetFormatNamesCommand.CommandHandler>(SetFormatNamesCommand.COMMAND_NAME);
commandBroker.Add<ShowCountCommand.CommandFactory, ShowCountCommand.CommandHandler>(ShowCountCommand.COMMAND_NAME);
commandBroker.Add<GraphCommand.CommandFactory, GraphCommand.CommandHandler>(GraphCommand.COMMAND_NAME);
commandBroker.Add<GetUnapprovedCommand.CommandFactory, GetUnapprovedCommand.CommandHandler>(GetUnapprovedCommand.COMMAND_NAME);

serviceProvider.GetService<IComponentBroker>()
    .AddVenueObservationHandlers()
    .AddVenueAuditingHandlers()
    .AddVenueControlHandlers()
    .AddVenueRenderingHandlers();

_ = serviceProvider.GetService<IApiObservationService>()
    .AddVenueObservers()
    .ObserveAsync();

await serviceProvider.GetService<IDiscordHandler>().ListenAsync();
await Task.Delay(Timeout.Infinite);