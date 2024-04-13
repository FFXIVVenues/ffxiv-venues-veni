Veni (or Veni Ki) is a Discord AI with language understanding and modern slash commands for creating and managing venues with FFXIV Venues. She has full lore for her creation and growth as an Indexer within the team. You are able to create new venue, edits existing venues, delete them 🥲, open and close them. 


![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/FFXIVVenues/ffxiv-venues-veni/main_veni-release.yml)


# Permisions Required

## Scopes
- bot
- applications.commands

## Bot permissions
- Send Messages
- Send Messages in Threads

## Permissions needed for optional features
- Manage Nicknames
- Manage Roles

# Configuration

## Via User Secrets / Json

You can get a basic and complete User Secrets Json for Veni from the [Engineering Staff information forum](https://discord.com/channels/768923191073701909/1079181729337716808/1079181729337716808).
Where `.` appears in a Setting Name it indicates a value within an object. For example, `Clu.RuntimeKey` would imply a `Clu` object (`{}`) with a `RuntimeKey` value within. 
Also, note that setting names are case-sensitive.

### Example User Settings
```json
{
  "DiscordBotToken": <your-discord-token>,
  "Clu": {
    "RuntimeKey": <your-microsoft-clu-key>,
    "PredictionEndpoint": <your-microsoft-clu-url>,
    "Project": "VeniKi",
    "Deployment": "uat"
  },
  "Logging": { 
    "MinimumLevel": "Debug",
    "BetterStackToken": <your-betterstack-token>
  },
  "Davinci3": {
    "ApiKey": <your-openai-key>
  },
  "Persistence": {
    "Provider": "MongoDb",
    "ConnectionString": "mongodb://localhost:27017"
  },
  "Ui": {
    "BaseUrl": "http://localhost:3000"
  },
  "Api": {
    "BaseUrl": "http://localhost:5001",
    "AuthorizationKey": <your-api-access-token>
  },
  "Notifications": {
    "Approvals": {
      "Europe": [
          132458461334807484
      ]
    },
    "MissingSplash": {
      "NorthAmerica": [
        132458461334807484
      ]
    },
    "Help": [
        132458461334807484
    ]
  },
  "Authorisation": {
    "Master": [ 
      132458461334807484
    ],
    "ManagerPermissions": [
      "EditVenue",
      "EditPhotography",
      "OpenVenue",
      "CloseVenue",
      "DeleteVenue",
    ],
    "PermissionSets": [
      {
        "Name": "Auditor",
        "Permissions": [
          "AuditVenue",
          "ViewAuditHistory"
        ],
        "Members": [
          154896150846843284,
          976681355509043357
        ]
      }
    ]
  }
}
```

## Via Environment Variables

Environment variables must be prefixed with `FFXIV_VENUES_VENI_` to be recognised. 
Where `.` appears in a Setting Name it indicates a value subsection and should be replaced with two underscores (`__`).
For example, `Clu.RuntimeKey` would become `FFXIV_VENUES_VENI_Clu__RuntimeKey`. 

## Root

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| DiscordBotToken  | This is the token used to authenticate Veni to the Discord API. Create your own bot at discord.com/developers, or ask Kana for a Developer token. |  String |

## Clu

This section configures Veni's Conversational Language Understanding for intent classification, if this is miss configured Veni will fail to understand intent from natural language and users will need to rely on commands only. 

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Clu.RuntimeKey | This is the token used to authenticate Veni to the Discord API. Create your own bot at discord.com/developers, or ask Kana for a Developer token. |  String |
| Clu.PredictionEndpoint | The url for your instance of Microsoft's prediction API, most likely ending with `cognitiveservices.azure.com`. |  String |
| Clu.Project | This is the name of the project configured in the CLU hub. |  String |
| Clu.Deployment | This is the name of the training deployment to use. |  String |

## Logging

This section configures the logging in Veni. Veni has the ability to log to a BetterStack feed. 

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Logging.MinimumLevel | The verbosity of the logging Veni should output. |  Verbose, Debug, Information, Warning, Error, Fatal |
| Logging.BetterStackToken | The source token from Better stack for logging. |  String |

## Davinci3

This section configures the Davinci3 API from OpenAI.

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Davinci3.ApiKey | The API key for the OpenAI API. |  String |

## Ui
This section provides some basic information about the web UI available to users. 

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Ui.BaseUrl | The base URL of the user interface. |  String |

## Api
This section configures the FFXIV Venues API that Veni will use to work with Venues. The key which Veni is given needs full scopes and permissions for Veni to operate at with full functionality.

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Api.BaseUrl | The base URL of the FFXIV Venues API. |  String |
| Api.AuthorizationKey | The authorization key for the FFXIV Venues API. |  String |

## Persistence
This section configures the persistence.

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Persistence.Provider | The type of persistence provider. |  String |
| Persistence.ConnectionString | The connection string to connect to the persistence provider. |  String |

## Notifications

This section configures you will receive notifications for certain events in Venues. 

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Notifications.Approvals.Global | The list of discord user ids to receive notifications for any venue that requires approval before publishing. | Array\<ulong> |
| Notifications.Approvals.NorthAmerica | The list of discord user ids to receive notifications for any venue created in North American data centers that requires approval before publishing. | Array\<ulong> |
| Notifications.Approvals.Oceania | The list of discord user ids to receive notifications for any venue created in Oceania data centers that requires approval before publishing. | Array\<ulong> |
| Notifications.Approvals.Europe | The list of discord user ids to receive notifications for any venue created in European data centers that requires approval before publishing. | Array\<ulong> |
| Notifications.Approvals.Japan | The list of discord user ids to receive notifications for any venue created in Japanese data centers that requires approval before publishing. | Array\<ulong> |
| Notifications.MissingSplash.Global | The list of discord user ids to receive notifications for any venue that is created without a banner image. | Array\<ulong> |
| Notifications.MissingSplash.NorthAmerica | The list of discord user ids to receive notifications for any venue created in North American data centers without a banner image. | Array\<ulong> |
| Notifications.MissingSplash.Oceania | The list of discord user ids to receive notifications for any venue created in Oceania data centers without a banner image. | Array\<ulong> |
| Notifications.MissingSplash.Europe | The list of discord user ids to receive notifications for any venue created in Europe data centers without a banner image. | Array\<ulong> |
| Notifications.MissingSplash.Japan | The list of discord user ids to receive notifications for any venue created in Japan data centers without a banner image. | Array\<ulong> |
| Notifications.Help | The discord user id to notify when a user asks for help. | Array\<ulong> |

## Authorization

This section configures the authorization of discord users. 

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Authorization.Master.UserId | The discord user id which to grant all permissions. |  String |
| Authorization.ManagerPermissions | The permissions granted to managers to their own venues. | Array<[Permission](#Permissions)> |

## Authorization Permission Sets

Permissions Sets is an object array, wherein each object specifies a set of permissions and a set of discord user ids to which to grant those permissions.

| Setting Name | Description | Value Type |
|--------------|-------------|------------|
| Authorization.PermissionSets[0].Name | The name of the permission set, this field is for your anotation only and is not used by Veni. | String |
| Authorization.PermissionSets[0].Permissions | The array of permission to grant to the discord users in the Members array of this permission set. |  Array\<[Permission](#Permissions)> |
| Authorization.PermissionSets[0].Members | The discord user id's to which to grant the permissions in the Permissions array of this permission set. |  Array\<ulong> |

## Permissions

| Permission Name | Description |
| -------------- | ----------- |
| ApproveVenue | Allows the user to approve any venue and publish it. |
| ApproveNaVenue | Allows the user to approve venues located in North American data centers and publish them. |
| ApproveEuVenue | Allows the user to approve venues located in European data centers and publish them. |
| ApproveOceVenue | Allows the user to approve venues located in Oceania data centers and publish them. |
| ApproveJpnVenue | Allows the user to approve venues located in Japanese data centers and publish them. |
| AuditVenue | Allows the user to send audits to any venue. |
| AuditNaVenue | Allows the user to send audits to any venue located in the North American data centers. |
| AuditEuVenue | Allows the user to send audits to any venue located in the European data centers. |
| AuditOceVenue | Allows the user to send audits to any venue located in the Oceania data centers. |
| AuditJpnVenue | Allows the user to send audits to any venue located in the Japanese data centers. |
| ViewAuditHistory | Allows the user to view the audit history of any venue. |
| ViewNaAuditHistory | Allows the user to view the audit history of venue located in the North American data centers. |
| ViewEuAuditHistory | Allows the user to view the audit history of venue located in the European data centers. |
| ViewOceAuditHistory | Allows the user to view the audit history of venue located in the Oceania data centers. |
| ViewJpnAuditHistory | Allows the user to view the audit history of venue located in the Japanese data centers. |
| EditVenue | Allows the user to edit details of any venue. |
| EditNaVenue | Allows the user to edit details of venues located in the North American data centers. |
| EditEuVenue | Allows the user to edit details of venues located in the European data centers. |
| EditOceVenue | Allows the user to edit details of venues located in the Oceania data centers. |
| EditJpnVenue | Allows the user to edit details of venues located in the Japanese data centers. |
| EditPhotography | Allows the user to edit the splash banner of any venue. |
| EditNaPhotography | Allows the user to edit the splash banner of any venue located in the North American data centers. |
| EditEuPhotography | Allows the user to edit the splash banner of any venue located in the European data centers. |
| EditOcePhotography | Allows the user to edit the splash banner of any venue located in the Oceania data centers. |
| EditJpnPhotography | Allows the user to edit the splash banner of any venue located in the Japanese data centers. |
| EditManagers | Allows the user to modify the managers of any venue. |
| EditNaManagers | Allows the user to modify the managers of any venue located in North American data centers. |
| EditEuManagers | Allows the user to modify the managers of any venue located in European data centers. |
| EditOceManagers | Allows the user to modify the managers of any venue located in Oceania data centers. |
| EditJpnManagers | Allows the user to modify the managers of any venue located in the Japanese data centers. |
| OpenVenue | Allows the user to set any venue to open status. |
| OpenNaVenue | Allows the user to set venues in North American data centers to open status. |
| OpenEuVenue | Allows the user to set venues in European data centers to open status. |
| OpenOceVenue | Allows the user to set venues in Oceania data centers to open status. |
| OpenJpnVenue | Allows the user to set venues in the Japanese data centers to open status. |
| CloseVenue | Allows the user to set any venue to close status. |
| CloseNaVenue | Allows the user to set venues in North American data centers to close status. |
| CloseEuVenue | Allows the user to set venues in European data centers to close status. |
| CloseOceVenue | Allows the user to set venues in Oceania data centers to close status. |
| CloseJpnVenue | Allows the user to set venues in the Japanese data centers to close status. |
| HiatusVenue | [Currently unused] Allows the user to set any venue to closed status for a longer period time. |
| HiatusNaVenue | [Currently unused] Allows the user to set venues in North American data centers to closed status for a longer period time. |
| HiatusEuVenue | [Currently unused] Allows the user to set venues in European data centers to closed status for a longer period time. |
| HiatusOceVenue | [Currently unused] Allows the user to set venues in Oceania data centers to closed status for a longer period time. |
| HiatusJpnVenue | [Currently unused] Allows the user to set venues in the Japanese data centers to closed status for a longer period time. |
| DeleteVenue | Allows the user to delete any venue. |
| DeleteNaVenue | Allows the user to delete venues located in the North American data centers. |
| DeleteEuVenue | Allows the user to delete venues located in the European data centers. |
| DeleteOceVenue | Allows the user to delete venues located in the Oceania data centers. |
| DeleteJpnVenue | Allows the user to delete venues located in the Japanese data centers. |
| Blacklist | Allows the user to blacklist any discord user or discord server from using Veni. |
| ControlMassAudit | Allows the user to start, pause, and cancel mass venues audits, and send notices to venues we're awaiting response on mass audit.  |
| ReportMassAudit | Allows the user to get status and a detailed report on the current mass audit. |
| SetLongSchedule | Allows the user to set a schedule longer than 7 hours on any venue. |

Note: Permissions granted in the ManagerPermissions setting only apply to Venues on which the user is assigned as a manager.



