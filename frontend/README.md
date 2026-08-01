# Library Management System - Frontend

Angular 18 (standalone components, functional guards/interceptors, lazy-loaded routes).

## Setup

```bash
npm install
```

Update `src/environments/environment.ts` if your API runs on a different port than
`https://localhost:7001`.

## Run

```bash
npm start
```
Opens at `http://localhost:4200`. Requires the backend API running (see the root README).

## Build

```bash
npm run build
```
Output goes to `dist/library-frontend`.

## Structure

```
src/app/
├── core/
│   ├── auth/        # AuthService, jwtInterceptor, authGuard, roleGuard
│   ├── models/       # TypeScript interfaces mirroring backend DTOs
│   └── services/     # Typed HttpClient wrappers per module
├── shared/
│   └── components/   # navbar (role-based nav), pagination
├── features/         # One folder per module: login, dashboard, books, members,
│                      # branches, borrow-return, reservations, reports
├── app.routes.ts      # Route guards mirror the backend's authorization policies
└── app.config.ts      # HttpClient + JWT interceptor registration
```

## Login

Default seeded account (see backend README): `admin@library.local` / `Admin@123`.

## Role-based access

| Route | Guard |
|---|---|
| `/dashboard`, `/books`, `/reservations` | any authenticated user |
| `/members`, `/borrow-return` | Admin, Librarian |
| `/branches` | Admin |
| `/reports` | Admin, BranchManager |

Matches the backend's `[Authorize(Policy = "...")]` attributes exactly - a route guard
here and a policy there are two independent enforcement points, not one relying on the
other (never trust the frontend alone for authorization).
