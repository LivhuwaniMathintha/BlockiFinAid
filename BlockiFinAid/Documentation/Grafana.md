# Grafana observability

The API exposes a consolidated dashboard view at `/api/grafana/overview`, dependency
checks at `/api/grafana/health`, recent timing summaries at `/api/grafana/timings`,
and Prometheus metrics at `/api/grafana/metrics`.

## Data and attribution

- “Total Number of Students Funded per Funder” counts distinct active students with an active
  funding record, matching `FundingModel.StudentId` / `FunderId` to the student and
  funder. Student-to-funder membership must also match
  `UserModel.FunderContractId` to `FunderModel.Id`.
- Seeded records are tagged with `DataSource=seed`; ordinary records default to
  `DataSource=live`. The **Live Data** dashboard filters to `live`; **Observability**
  aggregates sources so seeded/demo data is visible alongside live metrics. Its demo
  run table includes the source and run ID.
- The payment-created and failure panels use records initiated in the last 24 hours,
  based on `InitiationDate`. Failure counts include `Failed`, `FraudCheckFailed`, and
  `FraudRejected` statuses.
- Persisted payment count/status panels include all retained payment records, even
  when their initiation date is older than 24 hours. Funder/status gauges include
  zero-valued series for known funders so Grafana shows zero rather than “No data”.
- In Development, `DemoSimulation:Enabled` creates between 50 and 150 new demo-tagged
  students and active funding records at each API start, with exactly one synthetic
  payment per generated student. Configure the inclusive bounds with
  `DemoSimulation:MinStudentsPerRun` and `DemoSimulation:MaxStudentsPerRun`; both must
  be between 50 and 150. Older records are retained. The funder distribution panels
  show matching student and payment counts for the most recent run, while the separate
  payment total and status panels continue to include retained history. Each payment
  moves through `Initiated`, `Processing`, then `Fulfilled` or `Failed`; random
  amounts, funder assignments, and outcomes vary between runs.
- Demo failures such as “Simulated blockchain unavailable” and “Simulated database
  unavailable” are labels for simulated outcomes, not actual outages. Demo and seed
  payments are prevented from reaching the real RabbitMQ payment consumer, and the
  existing payment initiator is disabled while demo simulation is enabled. No ML or
  smart-contract calls are made for demo payments.
- MongoDB health pings the funds, institution, and access databases. RabbitMQ health
  establishes an AMQP connection using the configured host and credentials.
- HTTP timing uses matched ASP.NET route templates, method, and response status. The
  request total lasts for the API process lifetime; average and p95 latency use the
  most recent 500 samples per route/method/status. Background-operation timing comes
  from the existing in-process `OperationTimingTracker`; these timing values reset
  when the API restarts.

## Run locally

1. Ensure the configured MongoDB server is running. From the repository root, start
   RabbitMQ and the observability services with
   `docker compose up -d rabbitmq prometheus grafana`.
2. Start the API with its `http` launch profile so it listens on port `5017`. The
   profile binds to `0.0.0.0` so Prometheus in Docker can reach the host API through
   `host.docker.internal`; this also makes the HTTP listener reachable on the host's
   network interfaces, so use this profile only on a trusted network.
3. Open Grafana at `http://localhost:3000` and choose **BlockiFinAid Observability**
   for aggregate live/seed/demo metrics or **BlockiFinAid - Live Data** for live-only
   records in the **BlockiFinAid** folder.

Prometheus scrapes `host.docker.internal:5017/api/grafana/metrics` every second.
Both dashboards refresh every 5 seconds and use a 1-second minimum query interval;
the Observability dashboard defaults to a 5-minute range to make dense samples easier
to inspect. Grafana automatically thins x-axis tick labels to fit the panel, but the
time-series data is sampled at 1-second intervals.
Recreate Prometheus and Grafana after changing the provisioning files with
`docker compose up -d --force-recreate prometheus grafana`.
If the API uses another host or port, update `observability/prometheus.yml` to match.
Set `DemoSimulation:Enabled` to `false` to disable synthetic payment batches; keep it
disabled outside a demo environment. The API metrics endpoint is intended for the
private monitoring network; restrict network access to it when deploying outside a
trusted environment.
