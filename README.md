# ERP ↔ Finance Integration Pipeline

Simulates an automated data integration pipeline between an ERP system and a finance system (Xero format) — the kind of "glue" that keeps two enterprise systems in sync without manual CSV exports/imports.

> **Status: Week 1, Day 4** — ERP.Simulator generates fake invoice/order CSVs; Integration.Worker polls for them, parses, and transforms them into Xero-format records (with basic cleaning, covered by unit tests). Database write not built yet.

## Why this project

In most companies, the ERP system (orders, purchasing, inventory) and the finance system (invoices, receivables/payables) are two separate pieces of software that don't talk to each other automatically. The common workaround is manual export/import — slow, error-prone, and it delays things like month-end close.

This project simulates the real-world fix: an automated pipeline that polls a data source on a schedule, picks up new records, transforms them into the target system's format, and delivers them — with logging and error handling at every step.

Built as a portfolio project while transitioning from a supply chain background into software engineering — the goal is to demonstrate both an understanding of real enterprise data problems and the ability to implement the engineering solution end to end (including CI/CD and cloud deployment).

## Architecture

```mermaid
flowchart LR
    A[ERP Simulator] -->|generates CSV| B[(AWS S3)]
    B -->|poll / event trigger| C[Integration Worker]
    C -->|transform ERP → Xero format| D[(MySQL)]
    C -->|output| E[JSON file back to S3]
    F[GitHub Actions] -->|CI: build + test| F
    F -->|CD: deploy| G[AWS Lambda / EC2]
```

*(Week 1 runs this locally — a folder on disk stands in for S3, and MySQL runs in Docker. AWS wiring lands in Week 2.)*

## Tech stack

| Layer | Technology |
|---|---|
| Core language | C# / .NET 10 |
| Cloud storage | AWS S3 |
| Database | MySQL (via EF Core) |
| Scheduled job | .NET Worker Service (`BackgroundService`) |
| CI/CD | GitHub Actions |
| Cloud deploy | AWS Lambda + EventBridge (MVP) |
| Logging | Serilog |
| Testing | xUnit |

## Project structure

```
erp-finance-integration/
├── src/
│   ├── ERP.Simulator/          # generates fake ERP invoice/order data
│   ├── Integration.Worker/     # core worker service (poll + transform)
│   ├── Integration.Core/       # shared models and business logic
│   └── Integration.Data/       # EF Core data access layer
├── tests/
│   └── Integration.Tests/      # xUnit tests
├── .github/workflows/          # CI/CD pipeline definitions
├── infrastructure/aws/         # AWS configuration
└── docker-compose.yml          # local MySQL for development
```

## Running locally

Generate some fake invoices, then run the worker to pick them up:

```bash
dotnet run --project src/ERP.Simulator     # writes a CSV batch to data/incoming/
dotnet run --project src/Integration.Worker
```

Each `ERP.Simulator` run generates a batch of 5–15 fake invoice/order records and writes them as a CSV to `data/incoming/` at the repo root (a stand-in for the S3 bucket during Week 1; the folder is gitignored and created automatically). The records include a bit of realistic messiness — stray whitespace, inconsistent currency casing — that the transform step below cleans up.

`Integration.Worker` polls `data/incoming/` every 5 seconds, parses any CSV files it finds, transforms each `ErpInvoice` into a Xero-format record (trimming/title-casing customer names, uppercasing currency codes), logs the results, and moves each file into `data/processed/` so it isn't picked up again.

The database write lands Week 1, Days 5–6.

## CI/CD

Coming soon (Week 1, Day 7 for the initial build+test workflow; full deploy pipeline in Week 3).

## Scope and honest limitations

This is a portfolio MVP, not a production integration:

- Simulated data, not a real Xero API integration
- One-way sync only (ERP → Finance)
- No automatic retry — failures are logged, not retried
- No frontend/dashboard — verified via logs and database queries

## Roadmap

- [ ] Week 1: local pipeline (simulator → worker → transform → MySQL/JSON), fully working end to end
- [ ] Week 2: swap local folder for AWS S3, add EventBridge-triggered processing
- [ ] Week 3: GitHub Actions CI/CD, automated deploy to AWS
