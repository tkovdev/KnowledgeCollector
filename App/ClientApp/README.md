# Knowledge Collector client

The Angular client lives in `App/ClientApp`. You can run it with Angular's development server or
launch the ASP.NET Core app from VS Code.

## Run locally with `ng serve`

Requirements: Node.js and npm.

From the repository root, install the client dependencies and start the development server:

```sh
cd App/ClientApp
npm install
npx ng serve
```

Open [http://localhost:4200](http://localhost:4200). The server rebuilds the client when files
change; stop it with `Ctrl+C`.

This runs the Angular client only. It does not start the API.

## Run from VS Code

Open the repository root in VS Code, install the client dependencies as above, and open **Run and
Debug** (`F5`). Choose one of these launch configurations:

- **App** builds the Angular client and the ASP.NET Core app, then starts the app at
  [https://localhost:4200](https://localhost:4200). The app serves the built client from
  `App/ClientApp/dist/browser`. Its pre-launch build task calls `ng build`, so Angular CLI must be
  available on the task shell's `PATH`; if needed, install it with `npm install --global
  @angular/cli`.
- **API** starts only the API. It listens at `https://localhost:7191` and
  `http://localhost:5214`. Configure the required Azure settings first; see the
  [repository README](../../README.md).

The VS Code launch configurations are in `.vscode/launch.json`; their build tasks are in
`.vscode/tasks.json`.