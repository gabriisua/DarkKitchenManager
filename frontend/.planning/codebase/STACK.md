# Technology Stack

**Analysis Date:** 2026-06-29

## Languages

**Primary:**
- TypeScript ~5.9.2 - All application source code in `src/`

**Markup/Styles:**
- HTML (Angular templates) - Component templates
- CSS / SCSS - Component styles and global theming (`src/styles.css`, `src/material-theme.scss`)

## Runtime

**Environment:**
- Node.js 20-alpine (development and Docker build stage from `Dockerfile`)
- Browser (any modern browser — Angular SPA)

**Package Manager:**
- npm 11.12.1 (enforced via `packageManager` field in `package.json`)
- Lockfile: `package-lock.json` present

## Frameworks

**Core:**
- Angular ^21.2.0 - Standalone component architecture, no NgModules
  - `@angular/core`, `@angular/common`, `@angular/compiler`, `@angular/platform-browser`, `@angular/router`
  - Bootstrap: `bootstrapApplication()` in `src/main.ts` with standalone pattern
- Angular CDK ^21.2.11 - Component Dev Kit for overlay, a11y, etc.
- Angular Material ^21.2.11 - Material Design component library (tables, dialogs, form fields, datepicker, menus, sidenav, paginator, chips, buttons, progress spinner, icons, checkbox, cards)
- Angular Forms ^21.2.0 - Reactive forms (e.g., `FormBuilder`, `ReactiveFormsModule`)
- Angular HTTP Client - `provideHttpClient()` with functional interceptors

**Testing:**
- Vitest ^4.0.8 - Test runner (configured via `@angular/build:unit-test` builder in `angular.json`)
- jsdom ^28.0.0 - DOM environment for tests
- `vitest/globals` types referenced in `tsconfig.spec.json`

**Build/Dev:**
- Angular CLI ^21.2.8 (`@angular/cli`, `@angular/build` ^21.2.8)
- Build builder: `@angular/build:application`
- Dev server builder: `@angular/build:dev-server`
- Test builder: `@angular/build:unit-test`

## Key Dependencies

**Critical:**
- `@angular/core` ^21.2.0 - Core framework for the entire application
- `@angular/material` ^21.2.11 - All UI components (tables, dialogs, forms, datepicker, menus)
- `@angular/router` ^21.2.0 - Lazy-loaded routing with `CanMatchFn` guard
- `@angular/forms` ^21.2.0 - Reactive form validation and data binding
- `rxjs` ~7.8.0 - Reactive observable patterns used across all HTTP services

**Infrastructure:**
- `tslib` ^2.3.0 - TypeScript runtime helpers
- `typescript` ~5.9.2 - Language compiler
- `prettier` ^3.8.1 - Code formatter
- `jsdom` ^28.0.0 - Test DOM environment
- `vitest` ^4.0.8 - Test runner/framework

## Configuration

**Environment:**
- Two environment files at `src/environments/`:
  - `environment.ts` — Development config: `apiUrl: 'http://localhost:5051/api'`, `production: false`
  - `environment.staging.ts` — Staging config: `apiUrl: 'http://localhost:8080/api'`, `production: true`
- File replacement configured in `angular.json` under `configurations.staging.fileReplacements` (replaces `environment.ts` with `environment.staging.ts`)

**Build:**
- `angular.json` — Angular workspace configuration, build/serve/test targets
- `tsconfig.json` — Root TypeScript config (strict mode, ES2022 target, `module: preserve`)
- `tsconfig.app.json` — App-specific TS config (includes `src/**/*.ts`, excludes `*.spec.ts`)
- `tsconfig.spec.json` — Test-specific TS config (includes `vitest/globals` types)
- `.prettierrc` — Prettier config: 100 print width, single quotes, Angular HTML parser
- `.editorconfig` — Editor config: UTF-8, 2-space indent, single quotes for TS
- `Dockerfile` — Multi-stage Docker build using Node 20-alpine + nginx:alpine

## Platform Requirements

**Development:**
- Node.js >=20 (as per Docker build stage)
- npm >=11.12.1
- Angular CLI (`ng` commands)

**Production:**
- Docker / container environment
- nginx:alpine as web server (serves `dist/bo-configurator/browser`)
- Port 80 exposed (configured in `Dockerfile`)

---

*Stack analysis: 2026-06-29*
