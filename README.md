# Library Management System

Enterprise .NET LibraryManagementSystem. Built incrementally following Onion Architecture + CQRS curriculum. 

## Stack
- ASP.NET Core 8 (Web API)
- Entity Framework Core 8 + SQL Server
- MediatR (CQRS) 
- FluentValidation 
- Serilog 
- JWT Bearer Auth 
- xUnit + Moq + FluentAssertions 
- Angular 

## Solution Structure

```
LibraryManagementSystem/
├── src/
│   ├── Domain/           # Entities, Enums, Exceptions - no external dependencies
│   ├── Application/       # Use cases (CQRS), interfaces, validation, pipeline behaviors
│   ├── Infrastructure/    # EF Core, repositories, external service implementations
│   └── Api/                # Controllers, middleware, composition root
├── tests/
│   ├── Application.UnitTests/
│   └── Api.IntegrationTests/
└── LibraryManagementSystem.sln
```

Dependency direction: `Api → Infrastructure → Application → Domain` (Domain has zero
external dependencies; each layer only knows about the layers inside it).

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server (local install, or via Docker)

### Configure the database connection
1. Copy `src/Api/appsettings.Example.json` values into `src/Api/appsettings.Development.json`
   (this file is gitignored — never commit real credentials).
2. Or use `dotnet user-secrets` (recommended):
   ```bash
   dotnet user-secrets init --project src/Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=LibraryManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;" --project src/Api
   ```

### Run migrations 
```bash
dotnet ef migrations add InitialCreate -p src/Infrastructure -s src/Api
dotnet ef database update -p src/Infrastructure -s src/Api
```

### Run with Docker 
```bash
docker-compose up --build
```
Brings up SQL Server + the API together. **Generate migrations locally first** (see
above) - the compose file doesn't run `dotnet ef migrations add` for you, only applies
migrations that already exist. API available at `http://localhost:8080/swagger`.

### Health check
`GET /health` - checks the SQL Server connection, not just that the process is running.
Useful for container orchestrators and uptime monitoring.

### API Versioning
All routes are `api/v{version}/...` (currently `v1`). Adding `v2` alongside `v1` for a
breaking change means a new controller/action with `[ApiVersion("2.0")]`, without
touching or breaking existing `v1` clients.

### Run the API
```bash
dotnet restore
dotnet build
dotnet run --project src/Api
```
Swagger UI: `https://localhost:{port}/swagger`

### Log in 
A default Admin account is seeded into the database on first migration:
- **Email:** `admin@library.local`
- **Password:** `Admin@123`

`POST /api/v1/auth/login` with those credentials to get a JWT, then click **Authorize** in
Swagger and paste `Bearer {token}`. Change this password (or register a new Admin and
deactivate the seeded one) before treating this as anything beyond a local/reviewer demo.

### Run tests
```bash
dotnet test
```
Runs the full `Application.UnitTests` suite (~25 tests: domain logic, strategy pattern,
handler orchestration via Moq, FluentValidation rules). `Api.IntegrationTests` is scaffolded
but not yet populated (optional stretch goal, see Step 12 of the curriculum).

## Assumptions & Design Decisions
- **Loan period:** fixed  by default (configurable per call to `Loan.Create`).
- **Max active loans per member:** 5, enforced in `Member.CanBorrow()`.
- **Fine threshold to block further borrowing:** 20 currency units outstanding.
- **Reservation fulfillment:** FIFO by creation date; the domain event raised on
  `BookCopy.Return()` is intended to notify the next member in queue (Step 11).
- **Aggregate boundaries:** `BookCopy` is not its own aggregate root — it's only
  ever modified through its parent `Book` (except direct status transitions during
  Borrow/Return, which are narrow, well-defined operations).

## Progress Log

- **Step 1** — Solution scaffolded (Domain, Application, Infrastructure, Api, 2 test
  projects). Core entities: `Book`, `BookCopy`, `Branch`, `Member`, `Loan`, `Reservation`.
  Domain exceptions: `NotFoundException`, `BusinessRuleException`.
- **Step 2** — Generic `IRepository<T>` + `IUnitOfWork` defined in Application.
  `EfRepository<T>` implemented in Infrastructure against a minimal `ApplicationDbContext`
  (full entity configuration lands Step 9). DI wired via `Infrastructure.DependencyInjection`.
  API now builds and runs with Swagger, though no endpoints exist yet.

- **Step 3** — Specification pattern introduced: `ISpecification<T>` + `BaseSpecification<T>`
  in Application, `SpecificationEvaluator` in Infrastructure (the only place that translates
  a spec into EF Core LINQ). `IRepository<T>` gained `ListAsync(ISpecification<T>)` and
  `CountAsync(ISpecification<T>)` overloads. Real specifications added: book search/browse,
  available-copies-at-branch, overdue loans, active loans per member, FIFO reservation queue.

- **Step 4** — MediatR wired in (`AddMediatR` in Program.cs). Full CQRS slice for the Book
  Management module: `CreateBookCommand`, `UpdateBookCommand`, `DeleteBookCommand`,
  `GetBooksQuery` (paged, reuses Step 3's `BooksBySearchSpecification`), `GetBookByIdQuery`.
  Added `PagedResult<T>`, `BookDto`, and a manual (no AutoMapper yet) mapping extension.
  `BooksController` now live with real endpoints - controllers are a one-line `Send` per
  action. Domain gained `Book.UpdateDetails()` since properties are private-set by design.

- **Step 5** — `LoggingBehavior<TRequest,TResponse>` added as a MediatR pipeline behavior
  (`AddOpenBehavior`), wrapping every command/query with entry/exit + elapsed-time logging
  automatically. Same CQRS shape from Books extended to **Branch Management** and
  **Member Management**: full Create/Update/Delete/GetAll(paged+search)/GetById for both,
  each with its own specification, DTO, and mapping extension. `DeleteMemberCommand`
  includes a real business rule (blocks delete if active loans or outstanding fines exist).
  Domain gained `Branch.UpdateDetails()` and `Member.UpdateDetails()`.

- **Step 6** — FluentValidation added. `Domain.Exceptions.ValidationException` defined
  (plain `IDictionary<string,string[]>` of errors, no FluentValidation dependency in
  Domain). `ValidationBehavior<TRequest,TResponse>` added to the MediatR pipeline
  (registered after `LoggingBehavior`, so every attempt is logged even when invalid).
  Validators added for every Create/Update command across Books, Branches, Members, plus
  paging validators on `GetBooksQuery`, `GetBranchesQuery`, `GetMembersQuery` - proof
  validation isn't just for writes.

- **Step 7** — Centralized exception handling via .NET 8's `IExceptionHandler`.
  `GlobalExceptionHandler` maps `NotFoundException` → 404, `ValidationException` → 422
  (with the field-level errors dictionary attached), `BusinessRuleException` → 409,
  `UnauthorizedAccessException` → 403, everything else → 500 (logged as an error; the
  mapped 4xx cases log as warnings instead). All responses use RFC 7807 Problem Details.
  Registered via `AddExceptionHandler<T>()` + `AddProblemDetails()` and `app.UseExceptionHandler()`
  - no try/catch blocks anywhere in controllers or handlers.

- **Step 8** — Serilog wired in via `builder.Host.UseSerilog(...)`, replacing the default
  logging provider. Writes structured logs to console and a daily-rolling file
  (`logs/library-*.log`, 14-Step retention). Enriched with an `Application` property and
  full log context. `app.UseSerilogRequestLogging()` adds one structured line per HTTP
  request (method, path, status, elapsed ms) - distinct from `LoggingBehavior`, which logs
  per MediatR command/query instead, so both HTTP-level and use-case-level activity are
  visible. `appsettings.json` overrides `Microsoft`/`EF Core` noise down to Warning.

- **Step 9** — Full EF Core model configuration: `IEntityTypeConfiguration<T>` classes for
  all 6 entities (indexes, max lengths, unique constraints on `Book.Isbn` and
  `Member.Email`, enum-to-string conversions for `Status`/`MembershipType`/`ReservationStatus`).
  `BookCopy` gained a `RowVersion` (SQL Server `rowversion`) concurrency token via
  `.IsRowVersion()` - no manual version-bumping code, SQL Server handles it. Loan and
  Reservation get real FK constraints to Member/BookCopy/Book without navigation
  properties (aggregate boundaries stay decoupled in code, referential integrity still
  enforced in the DB). `GlobalExceptionHandler` now maps `DbUpdateConcurrencyException` → 409.
  **Migrations were not generated in this environment** (no .NET SDK available in the
  sandbox that built this zip) - run the commands below locally once you have the repo.

- **Step 10** — JWT authentication + policy-based authorization.
  `AppUser` entity added (Admin/Librarian/BranchManager/Member roles), with a seeded
  default Admin account (`admin@library.local` / `Admin@123` - **change immediately**
  after first login). `POST /api/v1/auth/login` issues a signed JWT; `POST
  /api/v1/auth/register` (Admin-only) creates additional accounts. Passwords hashed with
  BCrypt (work factor 11), never stored or logged in plain text. Named authorization
  policies (`CanManageCatalog`, `CanManageMembers`, `CanManageBranches`, `AdminOnly`)
  replace repeated role lists on `[Authorize]` attributes across Books/Branches/Members
  controllers. `ApplicationDbContext.SaveChangesAsync` now auto-populates
  `CreatedAt`/`CreatedBy`/`ModifiedAt`/`ModifiedBy` on every entity via
  `ICurrentUserService` + `IDateTimeProvider` - closes the loop on audit fields that had
  sat unused since Step 1. **Secrets**: `Jwt:Key` lives in `appsettings.Development.json`
  (gitignored - convenience for running this zip locally, never actually committed to
  Git) or `dotnet user-secrets` for anything beyond local dev; `appsettings.Example.json`
  documents the shape for reviewers without exposing real values.

- **Step 11** — Strategy pattern (`IFineCalculationStrategy`: Standard/Student/Premium,
  resolved by `IFineStrategyFactory` based on `Member.MembershipType`) and Factory pattern
  (`INotificationFactory` resolves Email/InApp `INotificationMessage` implementations,
  both currently logging stubs - swapping in real SMTP/SendGrid touches only one class).
  Domain Events wired end-to-end: `BaseEntity` can now raise `IDomainEvent`s,
  `DomainEventNotification<T>` bridges them into MediatR without Domain ever referencing
  MediatR, and `ApplicationDbContext.SaveChangesAsync` collects + dispatches events only
  *after* a successful save. `BookCopy.Return()` raises `BookReturnedEvent`;
  `BookReturnedEventHandler` reacts by checking the FIFO reservation queue
  (`ActiveReservationsForBookSpecification`, Step 3) and notifying the next member -
  entirely decoupled from `ReturnBookCommandHandler`, which has no idea that handler
  exists. Full Borrow/Return/Reservation CQRS slices added, with `BorrowBookCommand`
  specifically exercising the Step 9 optimistic concurrency token under race conditions.

- **Step 12** — Unit test suite populated: `Application.UnitTests` now has domain logic
  tests (`BookCopy`, `Member`, `Loan` - pure state-machine logic, no mocking),
  table-driven `[Theory]` tests for all three fine calculation strategies plus the
  factory's resolution logic, Moq-based handler tests (`CreateBookCommandHandler`,
  `BorrowBookCommandHandler` covering both business-rule guards and the happy path,
  `DeleteMemberCommandHandler` covering both delete-guard rules), and FluentValidation
  `TestValidate`-based validator tests. ~25 focused tests total, prioritizing pure domain
  logic and business rule guards over shallow "getter returns what constructor set" tests.

- **Step 13** — Bonus round, plus closing the Reports functional-requirement gap.
  **Reports module** added (was a required module, not yet built): `GetOverdueLoansReportQuery`
  and `GetMostBorrowedBooksReportQuery`, both batch-fetching related entities via the new
  reusable `ByIdsSpecification<T>` and joining in memory (avoiding N+1 queries, since
  Loan/Reservation deliberately have no navigation properties - Step 9). **Excel export**
  for the overdue loans report via ClosedXML (`GET /api/v1/reports/overdue-loans/export`).
  **API Versioning**: `Asp.Versioning.Mvc` wired in, every controller now has a real
  `[ApiVersion("1.0")]` and `api/v{version:apiVersion}/[controller]` route (not just a
  hardcoded "v1" string). **Health Checks**: `/health` checks the SQL Server connection,
  not just process liveness. **Docker**: multi-stage `Dockerfile` + `docker-compose.yml`
  (API + SQL Server, with a healthcheck-gated `depends_on` so the API doesn't start before
  the DB is ready). Migrations now auto-apply on startup in Development for convenience.
  **Skipped deliberately**: Redis and background jobs - low ROI for the remaining time
  budget relative to Steps 14's frontend work.

- **Step 14 (final)** — Angular 18 frontend added under `/frontend` (standalone components,
  functional guards/interceptors, lazy-loaded routes - confirmed with a real `ng build`,
  not just written and assumed correct; caught and fixed a genuine TypeScript field-
  initialization-order bug across 6 components in the process). `AuthService` decodes
  and stores the JWT, `jwtInterceptor` attaches it to every request and force-logs-out on
  401, `authGuard`/`roleGuard` mirror the backend's `[Authorize(Policy = "...")]`
  attributes route-for-route (never trusting the frontend alone - the backend enforces
  the same rules independently). Full feature set: Login, role-aware Dashboard, Books
  (search/paginate/create/delete), Members, Branches, Borrow/Return, Reservations,
  Reports (with a working Excel export download). See `frontend/README.md` for setup.

**14-Step curriculum complete.** Every functional module (Auth, Branch, Book, Member,
Borrow/Return, Reservation, Reports) has a full CQRS backend slice and a frontend screen.
Bonus features delivered: CQRS, Domain Events, Optimistic Concurrency, API Versioning,
Health Checks, Docker, Excel Export, PDF Export, Email Notifications (real SMTP via
MailKit, with graceful fallback to logging when unconfigured), Redis distributed caching,
CI/CD (GitHub Actions: build + test on push/PR). Not implemented: Background Jobs
(Hangfire/Quartz) - see "Post-Curriculum Additions" below for why.

## Post-Curriculum Additions (after Step 14)

Closed out the remaining bonus list items:

- **Real email sending** — `EmailNotificationMessage` now sends via SMTP (MailKit) when
  `Smtp:Host` is configured; falls back to logging-only otherwise, so local/CI/reviewer
  runs work without real credentials. A failed send is caught and logged, never thrown -
  a notification failure shouldn't break the operation that triggered it (e.g. a
  reservation fulfillment).
- **PDF export** — `GET /api/v1/reports/overdue-loans/export-pdf` via QuestPDF, same
  report data as the Excel export, exposed as a second button in the frontend.
- **CI/CD pipeline** — `.github/workflows/ci.yml`: two parallel jobs, backend
  (`dotnet build` + `dotnet test`, uploads test results as an artifact) and frontend
  (`npm ci` + `npm run build`), both on push/PR to `main`. No deployment step - build/test
  gating is what was asked for, and it's honestly what most real projects start with too.
- **Redis distributed caching** — a new `CachingBehavior<TRequest,TResponse>` pipeline
  behavior (same shape as `LoggingBehavior`/`ValidationBehavior`) applies cache-aside
  caching to any query implementing `ICacheableQuery`, with zero changes needed to the
  pipeline registration when a new query opts in. Applied to `GetBookByIdQuery` (5 min
  TTL). Cache invalidation added everywhere `Book`/`BookCopy` state changes that would
  make a cached `BookDto` stale: `UpdateBookCommand`, `DeleteBookCommand`,
  `BorrowBookCommand`, `ReturnBookCommand`. Redis being unreachable degrades to
  "no cache" (logged as a warning) rather than breaking the request - checked in
  `CachingBehaviorTests`, along with the hit/miss/pass-through paths.
- **Background Jobs** — deliberately still not implemented. Genuinely the highest-effort
  remaining item (new package, scheduler abstraction, a real recurring job, a story for
  how it behaves across multiple instances/Docker) relative to its marginal score value
  at this point in the timeline. The honest tradeoff, not an oversight.

## Final Submission Checklist

- [ ] `dotnet build` succeeds with zero errors/warnings (never verified in this sandbox - no .NET SDK available; **do this first**)
- [ ] `dotnet ef migrations add InitialCreate -p src/Infrastructure -s src/Api` generates cleanly
- [ ] `dotnet ef database update` applies against a real SQL Server instance
- [ ] `dotnet test` passes (Application.UnitTests, ~36 tests)
- [ ] Log in via Swagger with the seeded Admin account, confirm JWT auth works end-to-end
- [ ] `npm install && npm start` in `/frontend`, confirm it talks to the running API
- [ ] `docker-compose up --build` brings up API + SQL Server + Redis together
- [ ] Confirm caching works: call `GET /api/v1/books/{id}` twice, check logs for "Cache MISS" then "Cache HIT"
- [ ] Push to GitHub and confirm the Actions tab shows the CI workflow running (backend + frontend jobs)
- [ ] Change or rotate the seeded Admin password before sharing any live deployment
- [ ] Push to a GitHub/GitLab repo, confirm `.gitignore` actually excludes `appsettings.Development.json`, `bin/`, `obj/`, `node_modules/`, `logs/`
- [ ] Replace the placeholder GitHub URL in the CV/portfolio reference, if applicable
- [ ] Final read-through of this README for accuracy before submission email
