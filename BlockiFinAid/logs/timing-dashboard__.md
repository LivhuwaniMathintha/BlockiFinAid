# BlockiFinAid Timing Dashboard
> Generated UTC: 2026-09-28T18:52:40.6126650Z

## Executive Summary
- Total observed operations: 222
- Average duration across all recorded steps: 5448,8 ms
- Peak p95 duration recorded: 77986 ms
- Models tracked: 6

## End-to-End by Model
| Model | Samples | Avg total processing | P95 total processing |
| --- | ---: | ---: | ---: |
| all | 1 | 877,0 ms | 877 ms |
| BankAccountModel | 56 | 5029,6 ms | 5046 ms |
| FunderModel | 29 | 527,8 ms | 1110 ms |
| FundingConditionsModel | 40 | 3023,8 ms | 3048 ms |
| FundingModel | 40 | 4029,9 ms | 4057 ms |
| UserModel | 56 | 6034,5 ms | 6046 ms |

## Slowest Average Stages
| Pipeline | Model | Step | Count | Avg (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: |
| rabbitmq | UserModel | queue_latency | 14 | 39073,1 | 77986 |
| rabbitmq | BankAccountModel | queue_latency | 14 | 32537,1 | 64923 |
| rabbitmq | FundingModel | queue_latency | 10 | 17919,7 | 35818 |
| rabbitmq | FundingConditionsModel | queue_latency | 10 | 13391,9 | 26742 |
| user_sync | UserModel | total_processing | 14 | 6034,5 | 6046 |
| bank_account_sync | BankAccountModel | total_processing | 14 | 5029,6 | 5046 |
| funding_sync | FundingModel | total_processing | 10 | 4029,9 | 4057 |
| funding_conditions_sync | FundingConditionsModel | total_processing | 10 | 3023,8 | 3048 |
| rabbitmq | FunderModel | queue_latency | 5 | 2201,2 | 4261 |
| funder_sync | FunderModel | total_processing | 5 | 1045,4 | 1110 |

## Drill-down Summary
| Pipeline | Model | Step | Count | Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| bank_account_sync | BankAccountModel | smart_contract_write | 14 | 22,1 | 13 | 39 | 39 |
| bank_account_sync | BankAccountModel | total_processing | 14 | 5029,6 | 5019 | 5046 | 5046 |
| funder_sync | FunderModel | lookup_contract | 5 | 10,4 | 1 | 39 | 39 |
| funder_sync | FunderModel | smart_contract_write | 5 | 26,6 | 9 | 61 | 61 |
| funder_sync | FunderModel | total_processing | 5 | 1045,4 | 1017 | 1110 | 1110 |
| funding_conditions_sync | FundingConditionsModel | smart_contract_write | 10 | 17,0 | 7 | 41 | 41 |
| funding_conditions_sync | FundingConditionsModel | total_processing | 10 | 3023,8 | 3015 | 3048 | 3048 |
| funding_sync | FundingModel | smart_contract_write | 10 | 23,0 | 11 | 50 | 50 |
| funding_sync | FundingModel | total_processing | 10 | 4029,9 | 4018 | 4057 | 4057 |
| mongo_stream | BankAccountModel | publish_to_rabbit | 14 | 3,1 | 1 | 15 | 15 |
| mongo_stream | FunderModel | publish_to_rabbit | 5 | 16,4 | 2 | 66 | 66 |
| mongo_stream | FundingConditionsModel | publish_to_rabbit | 10 | 1,8 | 1 | 6 | 6 |
| mongo_stream | FundingModel | publish_to_rabbit | 10 | 1,7 | 1 | 5 | 5 |
| mongo_stream | UserModel | publish_to_rabbit | 14 | 2,3 | 1 | 7 | 7 |
| payment_creation | FunderModel | total_processing | 4 | 10,2 | 2 | 32 | 32 |
| rabbitmq | BankAccountModel | queue_latency | 14 | 32537,1 | 89 | 64923 | 64923 |
| rabbitmq | FunderModel | queue_latency | 5 | 2201,2 | 119 | 4261 | 4261 |
| rabbitmq | FundingConditionsModel | queue_latency | 10 | 13391,9 | 8 | 26742 | 26742 |
| rabbitmq | FundingModel | queue_latency | 10 | 17919,7 | 6 | 35818 | 35818 |
| rabbitmq | UserModel | queue_latency | 14 | 39073,1 | 76 | 77986 | 77986 |
| seed | all | total_processing | 1 | 877,0 | 877 | 877 | 877 |
| user_sync | UserModel | smart_contract_write | 14 | 24,4 | 13 | 38 | 38 |
| user_sync | UserModel | total_processing | 14 | 6034,5 | 6024 | 6046 | 6046 |
