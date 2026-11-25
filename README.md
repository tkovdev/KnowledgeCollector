# Knowledge Collector

Knowledge Collector is a .NET 10 application with an Angular client. Its API uses Azure OpenAI to
interview users about their team and elicit processes and information that can later be used to
generate quizzes assessing others' knowledge of that team. The AI can read PDF documents from links
and use them as context when forming questions. SharePoint links (`*.sharepoint.com/sites/...`) are
read through Microsoft Graph; other links are downloaded directly over HTTP(S).

The API authenticates to Azure using an Entra ID application registration and a certificate-backed
service principal. It does not use an Azure client secret.

## Requirements

- .NET 10 SDK
- Node.js and npm (for the Angular client)
- A running MongoDB instance (the development configuration uses `mongodb://localhost:27017`)
- An Entra ID tenant and app registration
- An Azure OpenAI resource with a chat model deployment
- For SharePoint document links, Microsoft Graph application permissions to the relevant files or
  sites

## Azure setup

1. Create a single-tenant Entra ID app registration. No redirect URI is required; the API uses the
   client-credentials flow.
2. Create or obtain a PFX certificate for the app registration. Register its public certificate
   with the app, and keep the private PFX file out of source control.
3. If the API needs to read SharePoint links, grant the app Microsoft Graph application permission
   `Files.Read.All` or `Sites.Read.All`, then grant admin consent.
4. Create or reuse an Azure OpenAI resource and deploy a chat model. Record the endpoint and
   deployment name.
5. Assign the app registration's service principal the `Cognitive Services OpenAI User` role on the
   Azure OpenAI resource.

### GitHub Actions build certificate

The API project includes `API/Certificates/knowledgecollector.pfx` in its build output. The
certificate is intentionally excluded from source control. To let the **Build** workflow build the
API, add a repository Actions secret named `API_CERTIFICATE_PFX_BASE64` containing the
base64-encoded private PFX file. The workflow recreates the file before building and fails clearly
if the secret is not configured. Keep the unencoded PFX and its encoded secret private.

## Configure and run the API

The API reads its configuration from `API/appsettings.json`, environment variables, or .NET User
Secrets. User Secrets are recommended for local development. From the repository root, set the
Entra ID and certificate settings:

```sh
dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>" --project API/API.csproj
dotnet user-secrets set "AzureAd:ClientId" "<application-client-id>" --project API/API.csproj
dotnet user-secrets set "AzureAd:CertificateRelativePath" "<certificate-path>" --project API/API.csproj
```

`AzureAd:CertificateRelativePath` must resolve to the private PFX file relative to the API's
application output directory. Ensure the file is available there when running the API; do not
commit the private certificate or its password.

The API also requires `AzureOpenAI:Endpoint` and `AzureOpenAI:DeploymentName`. They are present in
the default application settings; use User Secrets to override them if your Azure resource differs:

```sh
dotnet user-secrets set "AzureOpenAI:Endpoint" "<azure-openai-endpoint>" --project API/API.csproj
dotnet user-secrets set "AzureOpenAI:DeploymentName" "<deployment-name>" --project API/API.csproj
```

The MongoDB connection string and database name are configured under `MongoDbSettings` in
`API/appsettings.json`. Update them for your local MongoDB instance if needed.

Start the API with the HTTPS launch profile:

```sh
dotnet run --project API/API.csproj --launch-profile https
```

The API listens on `https://localhost:7191` and `http://localhost:5214`. In Development, its OpenAPI
document is available at `/openapi/v1.json`.

## Run the Angular client

The Angular application is in `App/ClientApp`. To run it independently from the API:

```sh
cd App/ClientApp
npm install
npx ng serve
```

Open [http://localhost:4200](http://localhost:4200). This starts only the Angular development
server, not the API. See [App/ClientApp/README.md](App/ClientApp/README.md) for VS Code launch
instructions.

## Run from VS Code

Open the repository root in VS Code and use **Run and Debug** (`F5`):

- **App** builds the Angular client and ASP.NET Core host, then serves the built client at
  `https://localhost:4200`. The build task runs `ng build`; Angular CLI must be available on the
  task shell's `PATH`.
- **API** builds and starts the API. Configure Azure and MongoDB first.

Launch configurations are in `.vscode/launch.json`; build tasks are in `.vscode/tasks.json`.

## Issue and PR automation

GitHub Actions can add the `api-ready` label to issues linked from a merged PR and create a
frontend-integration issue when a qualifying backend issue closes.

### Link and label a backend issue

1. Add the `back-end` label to the backend issue.
2. In the PR's merge commit message, include both a closing reference and `api-ready`. For example:

   ```text
   Closes #12
   api-ready
   ```

   The merge workflow checks the **merge commit message** for these strings. A closing reference in
   the PR description alone may not cause this workflow to add `api-ready`.
3. Merge the PR. GitHub processes the closing reference, and the workflow adds `api-ready` to the
   linked issue.
4. When the issue is closed and has both `back-end` and `api-ready`, the workflow creates a
   frontend integration issue labeled `frontend` and `integration`. It avoids creating a duplicate
   for the same backend issue.

The workflows are `.github/workflows/label-linked-issues-on-merge.yml` and
`.github/workflows/spawn-frontend-integration.yaml`. The latter can optionally assign the generated
issue with the `FE_ASSIGNEE` repository variable and add it to a GitHub Project using the
`FE_AUTOMATION_TOKEN`, `FE_PROJECT_ID`, `FE_STATUS_FIELD_ID`, and `FE_TODO_OPTION_ID` repository
secrets.

## Project structure

- `API` — ASP.NET Core API, controllers, startup, and OpenAPI configuration
- `App` — ASP.NET Core host for the built Angular client
- `App/ClientApp` — Angular frontend
- `Command` and `Query` — chat commands and queries
- `Data` — MongoDB data access and session mapping
- `Intelligence` — Azure OpenAI, Azure authentication, and document-reading tools
- `Mediator` — command/query dispatch
