# Grafana observability

The API exposes a consolidated dashboard view at `/api/grafana/overview`, dependency
checks at `/api/grafana/health`, recent timing summaries at `/api/grafana/timings`,
and Prometheus metrics at `/api/grafana/metrics`.

## Data and attribution

- “Total Students Funded per Funder” counts distinct active students with an active
  funding record, matching `FundingModel.StudentId` / `FunderId` to the student and
  funder. Student-to-funder membership must also match
  `UserModel.FunderContractId` to `FunderModel.Id`.
- Seeded records are tagged with `DataSource=seed`; ordinary records default to
  `DataSource=live`. The live dashboard filters to `live`; the demo dashboard shows
  `seed` and `demo` records and includes their source/run tags.
- Payment totals/statuses use records initiated in the last 24 hours, based on
  `InitiationDate`. Failure counts include `Failed`, `FraudCheckFailed`, and
  `FraudRejected` statuses.
- In Development, `DemoSimulation:Enabled` creates a new batch at each API start,
  with one synthetic payment per eligible student (capped by
  `DemoSimulation:MaxPaymentsPerRun`). Older batches are retained and each batch has a
  unique run ID. Records move through `Initiated`, `Processing`, then `Fulfilled` or
  `Failed`; random amounts and outcomes make the demo data vary between starts.
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
3. Open Grafana at `http://localhost:3000` and choose **BlockiFinAid - Live Data**
   or **BlockiFinAid - Demo and Seed Data** from the provisioned dashboards in the
   **BlockiFinAid** folder.

Prometheus scrapes `host.docker.internal:5017/api/grafana/metrics` every 15 seconds.
If the API uses another host or port, update `observability/prometheus.yml` to match.
Set `DemoSimulation:Enabled` to `false` to disable synthetic payment batches; keep it
disabled outside a demo environment. The API metrics endpoint is intended for the
private monitoring network; restrict network access to it when deploying outside a
trusted environment.
