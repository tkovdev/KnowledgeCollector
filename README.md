# Knowledge Collector
## Overview
Knowledge Collector is a .NET 10 Web API that uses Azure OpenAI to interview users about their team
and elicit processes and information that can later be used to generate quizzes assessing others'
knowledge of that team. The AI can read PDF documents from a link and use them as context when forming
questions. SharePoint links (`*.sharepoint.com/sites/...`) are read through Microsoft Graph; any other
link is downloaded directly over HTTP(S).

### Benefits
1) Authentication to Azure OpenAI and Microsoft Graph is performed with a single Azure AD (Entra ID)
service principal, so no API keys are stored.
2) The AI can call tools (currently `read_document`), so new capabilities are added by implementing `ITool`.

## Requirements
### Technical Requirements
- .NET 10 SDK
- Azure Active Directory
    - 1 Application Registration
    - 1 Enterprise Application (default with App Registration)
- Azure OpenAI resource with 1 chat model deployment
- Documents reachable by link: SharePoint documents the application is permitted to read, and/or publicly accessible document URLs (PDF)

## Setup
### Azure Setup
This setup assumes that an Azure Active Directory Tenant is already setup
and that the individual following this guide understands the basics of Azure
and its App Registration process.

1) Setup an App Registration (single tenant is sufficient). No redirect URI is required, as the API
authenticates with client credentials only.
2) Create 1 new Client Secret.
    1) Copy down the generated secret.
3) Only needed for SharePoint links: add API Permissions for Microsoft Graph (Application level):
    1) Files.Read.All (or Sites.Read.All)
    2) Grant admin consent.
4) Create (or reuse) an Azure OpenAI resource.
    1) Deploy a chat model (the default configuration uses `gpt-5-nano`). Copy the deployment name.
    2) Copy the resource endpoint (e.g. `https://<resource_name>.openai.azure.com/`).
5) On the Azure OpenAI resource, open Access control (IAM) and assign the role
`Cognitive Services OpenAI User` to the App Registration's service principal.

### Application Setup
Once Azure is setup following the instructions for Azure Setup.

#### API
The only changes needed are within `API/appsettings.json` or User Secrets. User Secrets are
recommended for the client secret (replace <> including the < & >):
```
cd API
dotnet user-secrets set "AzureAd:ClientSecret" "<client_secret_from_step_#2_from_Azure_Setup>"
```
```
"AzureAd": {
   "TenantId": "<directory_(tenant)_id_from_app_registration>",
   "ClientId": "<client_id_from_app_registration>",
   "ClientSecret": "<client_secret_from_step_#2_from_Azure_Setup>"
},
"AzureOpenAI": {
   "Endpoint": "<azure_openai_endpoint_from_step_#4.ii>",
   "DeploymentName": "<deployment_name_from_step_#4.i>"
}
```
All of the values above are required; the application throws on startup of the chat service if one is missing.

#### Run
```
dotnet run --project API
```
The API listens on `https://localhost:7191` and `http://localhost:5214` (see `API/Properties/launchSettings.json`).
In Development the OpenAPI document is available at `/openapi/v1.json`.

### Usage
Send a message to the chat endpoint:
```
curl -X POST https://localhost:7191/api/test/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "I work on the billing team."}'
```
Response: `{ "response": "<generated question>" }`

## Project Structure
- `API` - ASP.NET Core Web API (controllers, startup/DI configuration)
- `Logic` - AI chat service (`OpenAIChatService`), Azure credential service (`AzureTokenService`),
and tools the AI can call (`FileReader` / `read_document`)

## Summary
### Notes & Considerations
- Non-SharePoint links are fetched with a plain unauthenticated HTTP request, so they must be accessible without login. Currently only PDF documents are supported.
- The same service principal is used for Azure OpenAI and Microsoft Graph, so it needs both the
IAM role and the Graph permissions above.
- The `/api/test/chat` endpoint is a test endpoint and is not authenticated.
- Never commit the client secret; use User Secrets or a secure store.
