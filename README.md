# EnglishLearningPlatformApp

## About this solution

This is a layered startup solution based on [Domain Driven Design (DDD)](https://abp.io/docs/latest/framework/architecture/domain-driven-design) practises. All the fundamental ABP modules are already installed. Check the [Application Startup Template](https://abp.io/docs/latest/solution-templates/layered-web-application) documentation for more info.

### Pre-requirements

* [.NET10.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node.js 22 LTS](https://nodejs.org/en)
* [pnpm 11.7](https://pnpm.io/installation)
* Microsoft SQL Server 2022 or Docker Desktop

### Configurations

Runtime secrets and machine-specific connection strings are intentionally absent from committed configuration. Set these environment variables before migration or startup:

| Environment variable | Purpose |
| --- | --- |
| `ConnectionStrings__Default` | SQL Server connection string used by the host and DbMigrator |
| `Seed__AdminPassword` | Initial host/tenant administrator password; required by DbMigrator |
| `AuthServer__CertificatePassPhrase` | OpenIddict certificate passphrase |
| `StringEncryption__DefaultPassPhrase` | Server-side string-encryption passphrase |

Never place these values in Angular configuration or committed `appsettings*.json` files.

### Before running the application

1. From `angular`, run `pnpm install --frozen-lockfile`.
2. Set `ConnectionStrings__Default` and `Seed__AdminPassword`.
3. From the solution root, run `dotnet run --project src/EnglishLearningPlatformApp.DbMigrator`.
4. Start the API with `dotnet run --project src/EnglishLearningPlatformApp.HttpApi.Host`.
5. From `angular`, start the client with `pnpm start`.

### PWA behavior

The production Angular build is installable and online-first. Its service worker caches only the reviewed static application shell. Authentication, tokens, attempts, answers, results, writing, AI feedback, vocabulary, administration, and all other private or mutable API responses remain network-only.

When offline, the shell displays a connection-required notice. Learning operations are not queued. When a new deployment is ready, the application asks the user before reloading so active work is not silently interrupted.

For a containerized development database, set `MSSQL_SA_PASSWORD` and run:

```powershell
docker compose -f etc/docker/docker-compose.yml up -d
```

#### Generating a Signing Certificate

In the production environment, you need to use a production signing certificate. ABP Framework sets up signing and encryption certificates in your application and expects an `openiddict.pfx` file in your application.

To generate a signing certificate, you can use the following command:

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p <strong-random-passphrase>
```

Store the chosen passphrase outside source control and provide it through `AuthServer__CertificatePassPhrase`.

### Verification commands

```powershell
dotnet restore EnglishLearningPlatformApp.slnx
dotnet build EnglishLearningPlatformApp.slnx --configuration Release --no-restore
dotnet test EnglishLearningPlatformApp.slnx --configuration Release --no-build --no-restore
Set-Location angular
pnpm install --frozen-lockfile
pnpm run lint
pnpm run test -- --watch=false
pnpm run build:prod
pnpm run verify:pwa
```

It is recommended to use **two** RSA certificates, distinct from the certificate(s) used for HTTPS: one for encryption, one for signing.

For more information, please refer to: [OpenIddict Certificate Configuration](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios)

> Also, see the [Configuring OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment) documentation for more information.

### Solution structure

This is a layered monolith application that consists of the following applications:

* `angular`: Angular application.
* `EnglishLearningPlatformApp.DbMigrator`: A console application which applies the migrations and also seeds the initial data. It is useful on development as well as on production environment.
* `EnglishLearningPlatformApp.HttpApi.Host`: ASP.NET Core API application that is used to expose the APIs to the clients.

#### Test Projects

The `test` folder contains the following test projects:

* `EnglishLearningPlatformApp.Application.Tests`: Application layer tests.
* `EnglishLearningPlatformApp.Domain.Tests`: Domain layer tests.
* `EnglishLearningPlatformApp.EntityFrameworkCore.Tests`: Entity Framework Core integration tests.



## Deploying the application

Deploying an ABP application follows the same process as deploying any .NET or ASP.NET Core application. However, there are important considerations to keep in mind. For detailed guidance, refer to ABP's [deployment documentation](https://abp.io/docs/latest/Deployment/Index).

### Additional resources


#### Internal Resources

You can find detailed setup and configuration guide(s) for your solution below:

* [Angular](./angular/README.md)

#### External Resources
You can see the following resources to learn more about your solution and the ABP Framework:

* [Web Application Development Tutorial](https://abp.io/docs/latest/tutorials/book-store/part-1)
* [Application Startup Template](https://abp.io/docs/latest/startup-templates/application/index)
