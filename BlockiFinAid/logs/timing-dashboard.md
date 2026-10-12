# BlockiFinAid Timing Dashboard
> Generated UTC: 2026-10-12T17:07:38.8256380Z

## Executive Summary
- Total observed operations: 745
- Average duration across all recorded steps: 52537,7 ms
- Peak p95 duration recorded: 530068 ms
- Models tracked: 3

## End-to-End by Model
| Model | Samples | Avg total processing | P95 total processing |
| --- | ---: | ---: | ---: |
| all | 1 | 286,0 ms | 286 ms |
| FundingModel | 372 | 4036,5 ms | 4065 ms |
| UserModel | 372 | 6034,7 ms | 6080 ms |

## Slowest Average Stages
| Pipeline | Model | Step | Count | Avg (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: |
| rabbitmq | UserModel | queue_latency | 93 | 277277,9 | 530068 |
| rabbitmq | FundingModel | queue_latency | 93 | 185141,5 | 353909 |
| user_sync | UserModel | total_processing | 93 | 6034,7 | 6080 |
| funding_sync | FundingModel | total_processing | 93 | 4036,5 | 4065 |
| seed | all | total_processing | 1 | 286,0 | 286 |
| funding_sync | FundingModel | smart_contract_write | 93 | 27,7 | 47 |
| user_sync | UserModel | smart_contract_write | 93 | 25,9 | 46 |
| mongo_stream | UserModel | publish_to_rabbit | 93 | 4,6 | 14 |
| mongo_stream | FundingModel | publish_to_rabbit | 93 | 4,4 | 10 |

## Drill-down Summary
| Pipeline | Model | Step | Count | Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| funding_sync | FundingModel | smart_contract_write | 93 | 27,7 | 10 | 283 | 47 |
| funding_sync | FundingModel | total_processing | 93 | 4036,5 | 4014 | 4299 | 4065 |
| mongo_stream | FundingModel | publish_to_rabbit | 93 | 4,4 | 1 | 73 | 10 |
| mongo_stream | UserModel | publish_to_rabbit | 93 | 4,6 | 1 | 80 | 14 |
| rabbitmq | FundingModel | queue_latency | 93 | 185141,5 | 193 | 370013 | 353909 |
| rabbitmq | UserModel | queue_latency | 93 | 277277,9 | 194 | 553983 | 530068 |
| seed | all | total_processing | 1 | 286,0 | 286 | 286 | 286 |
| user_sync | UserModel | smart_contract_write | 93 | 25,9 | 6 | 176 | 46 |
| user_sync | UserModel | total_processing | 93 | 6034,7 | 6014 | 6200 | 6080 |
