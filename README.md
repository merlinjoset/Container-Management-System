**PaymentProcessor (Assessment)**

- **Stack:** ASP.NET Core MVC + SignalR (net8.0), Entity Framework Core (SQL Server), Hangfire for background jobs, JWT auth.
- **Projects:** `Assessment.Web` (UI/API), `Assessment.Application` (use cases, DTOs, interfaces), `Assessment.Infrastructure` (EF Core, integrations, UoW), `Assessment.Domain` (domain models), `Assessment.FakeProvider` (simulated payment provider).

**Architecture Decisions**
- **Layered/Clean separation:**
  - **Domain:** Pure models and enums (`Payment`, `PaymentStatus`, `PaymentProvider`). No framework dependencies.
  - **Application:** Use-case services and contracts only. Defines repository abstractions (`IPaymentRepository`, `IProviderRepository`, `IAuthUserRepository`, `ITokenStore`), DTOs, job orchestration (`PaymentProcessingJob`, `FailedPaymentsRetryJob`), policies/roles, and stateless services (`PaymentService`, `TokenService`).
  - **Infrastructure:** EF Core persistence (`AppDbContext`, entities, repositories, `EfUnitOfWork`, `EfTokenStore`) and external integration client (`IFakeProviderClient` via `FakeProviderClient`). Contains `DbSeeder` to create schema and seed the default provider.
  - **Web (Composition Root):** Wires dependencies, configures authentication/authorization, SignalR hub (`PaymentsHub`), Hangfire server and dashboard, and MVC controllers for UI/API.
- **Background processing with Hangfire:**
  - **Queue model:** Enqueues `PaymentProcessingJob.ProcessAsync(paymentId)` on a dedicated `payments` queue after creation and during retries.
  - **Retry policy:** Application-level max attempts = 3. A recurring job (`FailedPaymentsRetryRunner`) runs every 2 minutes to pick failed payments under the limit and re-enqueue them.
  - **Storage:** `Hangfire.SqlServer` uses the same `Default` SQL connection; schema auto-prepared.
- **Persistence via EF Core:**
  - **Tables:** Users, Roles, UserRoles, UserLogins, UserTokens, PaymentProviders, Payments. Mappings live in `AppDbContext`.
  - **Repositories:** Translate between EF entities and domain models; commands are kept small and explicit (mark processing/completed/failed, increment attempts).
  - **Unit of Work:** `EfUnitOfWork` wraps multi-step operations/commands in a database transaction for consistency.
- **Real-time updates via SignalR:** Processing jobs publish through `IPaymentEvents` implemented by `SignalRPaymentEvents`, notifying the owner (`user:{id}` group) and admins (`admins` group); UI refreshes the list on `paymentUpdated`.
- **Authentication/Authorization:**
  - **JWTs:** `TokenService` issues JWT with roles and a `jti`. Token is returned to API clients and stored in an HttpOnly cookie `access_token` for the MVC UI.
  - **Revocation model:** Single active token per user via `ITokenStore` (backed by `UserTokens` row with `jti|expiry`), validated in `JwtBearerEvents` on every request.
  - **Policies:** `Payments.Create/View` for users/admins; `Providers.Manage` and `Payments.Process` for admins only.
- **External integration:** A separate minimal service (`Assessment.FakeProvider`) simulates a PSP endpoint (`POST /fake-provider/pay`) with random success/failure. The app calls it through a typed `HttpClient` using a base URL from configuration.

**How To Run**
- **Prerequisites:**
  - .NET 8 SDK (`dotnet --version` shows 8.x)
  - SQL Server (LocalDB is fine on Windows) or a reachable SQL Server instance
  - Optional: `dotnet-ef` tool if you will add/run EF migrations (`dotnet tool install -g dotnet-ef`)

- **1) Configure development settings:**
  - File: `src/Assessment.Web/appsettings.Development.json`
  - Set `ConnectionStrings:Default` to your SQL Server. Default uses LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=AssessmentDb;Trusted_Connection=True;TrustServerCertificate=True`.
  - Ensure `FakeProvider:BaseUrl` matches where you run the fake provider. For example:
    - If you run provider on `https://localhost:7077`, keep `"https://localhost:7077"`.
    - If you use the project launch profile (defaults to `https://localhost:7165`), update the setting to `"https://localhost:7165"` or override with env var `FakeProvider__BaseUrl`.

- **2) Database schema (EF Core migrations included):**
  - Migrations are checked in under `src/Assessment.Infrastructure/Persistence/Migrations` (InitialCreate).
  - Apply to your database:
    - `dotnet restore`
    - `dotnet ef database update -p src/Assessment.Infrastructure -s src/Assessment.Web`
  - Regenerate or add new migrations when models change:
    - `dotnet ef migrations add <Name> -p src/Assessment.Infrastructure -s src/Assessment.Web -o Persistence/Migrations`
  - Design-time support: `AppDbContextFactory` reads `ConnectionStrings:Default` from Web `appsettings.*` or `ConnectionStrings__Default` env var.

- **3) Seed users and roles (manual, one-time):**
  - Generate a password hash using the project’s PBKDF2 format (`PasswordHasher.Hash`). Example quick C# snippet you can run in a scratch console app:
    - Create a temp console app and print the hash:
      - `dotnet new console -n PwdHash && cd PwdHash`
      - Replace `Program.cs` with:
        ```csharp
        using System.Security.Cryptography;
        static string Hash(string password, int iterations = 100_000){var salt=RandomNumberGenerator.GetBytes(16);var hash=Rfc2898DeriveBytes.Pbkdf2(password,salt,iterations,HashAlgorithmName.SHA256,32);return $"PBKDF2{\u0024}{iterations}{\u0024}{Convert.ToBase64String(salt)}{\u0024}{Convert.ToBase64String(hash)}";}
        Console.WriteLine(Hash("Passw0rd!"));
        ```
      - `dotnet run` and copy the printed hash.
  - Insert minimal rows (adjust GUIDs/emails as desired) in your database:
    - Users table:
      `INSERT INTO Users (Id, UserName, Email, EmailConfirmed, PasswordHash, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
       VALUES ('11111111-1111-1111-1111-111111111111','admin','admin@example.com',1,'PBKDF2$100000$ntpnhyjic7IxwJnhESzjbA==$LS7XfJD/qtBdgQipkfixCUX2jt6XfAinlfV9t/Xhs84=',1,0,0,0);`

      ` Password: Admin@123
    - Roles table:
      `USE [AssessmentDb]
      GO
      INSERT [dbo].[Roles] ([Id], [Name]) VALUES (N'f6987b04-a5fc-4bed-8506-02ddca71ea99', N'user')
      GO
      INSERT [dbo].[Roles] ([Id], [Name]) VALUES (N'5e4edd62-406a-4035-9d8a-d86c39bbcace', N'admin')
      GO`

    - UserRoles table:
      `INSERT INTO UserRoles (UserId, RoleId) VALUES ('11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222');`

- **4) Run the fake provider:**
  - Option A (explicit URLs): `dotnet run --project src/Assessment.FakeProvider --urls https://localhost:7077`
  - Option B (launch profile URLs): `dotnet run --project src/Assessment.FakeProvider --launch-profile https` (defaults to `https://localhost:7165` per `launchSettings.json`)

- **5) Run the web app (UI + API + jobs):**
  - `dotnet run --project src/Assessment.Web --launch-profile https`
  - Open the browser at the shown URL (e.g., `https://localhost:7035`), it redirects to `/Account/Login`.
  - Login with the seeded user (e.g., `admin@example.com` + your chosen password). The JWT is stored in an HttpOnly `access_token` cookie and bridged to the Authorization header for API calls.
  - Hangfire dashboard: browse to `/hangfire`.

- **6) Exercise the flow:**
  - Navigate to Providers to confirm/edit the seeded provider URL.
  - Create a payment from `/Payments/Create`.
  - The system enqueues processing; status updates arrive via SignalR and the page auto-refreshes on `paymentUpdated`.
  - Admins can retry failed payments (up to 3 attempts). The recurring retry job also re-enqueues eligible failures every 2 minutes.

**User Management**
- Admins can create users via UI:
  - Navigate to `/Users/Create` (requires admin role).
  - Provide Email, UserName, Password, and optionally mark as Admin.
- API creation can be added similarly; currently only the UI endpoint is provided.

**Known Limitations**
- **No user/role seed:** There is no built-in user bootstrap; a manual SQL seed is required to log in.
- **JWT key management:** The dev JWT secret lives in `appsettings.Development.json`. In production, store secrets in a secure provider (Key Vault, user secrets, or environment) and rotate regularly.
- **Single active token per user:** Logging in overwrites the stored `jti`, invalidating previous tokens. This is intentional but may not fit all clients.
- **HTTPS/auth hardening for prod:** `RequireHttpsMetadata` is disabled for dev; cookies use `SameSite=Lax`; CORS is not configured for cross-origin API use. Harden for production.
- **Fake provider port mismatch risk:** Default `FakeProvider:BaseUrl` (`https://localhost:7077`) does not match the launch profile URL (`https://localhost:7165`). Ensure they match, or override via environment.
- **Limited validation/business rules:** Currency is only length-checked; no FX validation, idempotency keys, or duplicate submission guards.
- **Concurrency/locking:** Processing relies on repository commands + Hangfire queueing without distributed locks. Duplicate enqueues are not deduplicated; jobs are idempotent-ish via status checks, but no strict exactly-once semantics.
- **Operational concerns:** No health checks/telemetry, no retry/backoff to the provider beyond the overall max attempts, and no circuit breaker.

**API Quickstart**
- Obtain a token:
  - `curl -k -X POST https://localhost:7035/api/auth/login -H "Content-Type: application/json" -d '{"Email":"admin@example.com","Password":"Passw0rd!"}'`
- Create a payment:
  - `curl -k -X POST https://localhost:7035/api/payments -H "Authorization: Bearer <TOKEN>" -H "Content-Type: application/json" -d '{"providerId":"<GUID>","amount":10.5,"currency":"USD","reference":"INV-1001"}'`
- List payments:
  - `curl -k https://localhost:7035/api/payments -H "Authorization: Bearer <TOKEN>"`

**Project Structure**
- `src/Assessment.Web`: MVC UI, API controllers, Program composition, SignalR hubs, Hangfire server.
- `src/Assessment.Application`: DTOs, services, abstractions, jobs, security.
- `src/Assessment.Infrastructure`: EF Core context/entities/repositories, token store, provider client, seed.
- `src/Assessment.Domain`: Domain models and enums.
- `src/Assessment.FakeProvider`: Minimal ASP.NET app simulating a payment provider endpoint.
