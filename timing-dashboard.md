# BlockiFinAid Timing Dashboard
> Generated UTC: 2026-09-28T18:33:07.7228780Z

## Executive Summary
- Total observed operations: 250
- Average duration across all recorded steps: 10909,9 ms
- Peak p95 duration recorded: 252886 ms
- Models tracked: 6

## End-to-End by Model
| Model | Samples | Avg total processing | P95 total processing |
| --- | ---: | ---: | ---: |
| all | 1 | 926,0 ms | 926 ms |
| BankAccountModel | 68 | 5046,4 ms | 5177 ms |
| FunderModel | 30 | 534,6 ms | 1175 ms |
| FundingConditionsModel | 40 | 3031,1 ms | 3054 ms |
| FundingModel | 40 | 4048,9 ms | 4135 ms |
| UserModel | 71 | 6052,1 ms | 6203 ms |

## Slowest Average Stages
| Pipeline | Model | Step | Count | Avg (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: |
| rabbitmq | UserModel | queue_latency | 19 | 114406,8 | 252886 |
| rabbitmq | BankAccountModel | queue_latency | 18 | 93432,8 | 243829 |
| rabbitmq | FundingModel | queue_latency | 10 | 17977,4 | 35983 |
| rabbitmq | FundingConditionsModel | queue_latency | 10 | 13421,4 | 26806 |
| user_sync | UserModel | total_processing | 19 | 6052,1 | 6203 |
| bank_account_sync | BankAccountModel | total_processing | 18 | 5046,4 | 5177 |
| funding_sync | FundingModel | total_processing | 10 | 4048,9 | 4135 |
| funding_conditions_sync | FundingConditionsModel | total_processing | 10 | 3031,1 | 3054 |
| rabbitmq | FunderModel | queue_latency | 5 | 2211,2 | 4332 |
| funder_sync | FunderModel | total_processing | 5 | 1068,0 | 1175 |

## Drill-down Summary
| Pipeline | Model | Step | Count | Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| bank_account_sync | BankAccountModel | smart_contract_retry | 1 | 14,0 | 14 | 14 | 14 |
| bank_account_sync | BankAccountModel | smart_contract_write | 17 | 30,6 | 13 | 74 | 74 |
| bank_account_sync | BankAccountModel | total_processing | 18 | 5046,4 | 5021 | 5177 | 5177 |
| funder_sync | FunderModel | lookup_contract | 5 | 21,6 | 2 | 94 | 94 |
| funder_sync | FunderModel | smart_contract_write | 5 | 37,0 | 14 | 74 | 74 |
| funder_sync | FunderModel | total_processing | 5 | 1068,0 | 1023 | 1175 | 1175 |
| funding_conditions_sync | FundingConditionsModel | smart_contract_write | 10 | 22,0 | 11 | 46 | 46 |
| funding_conditions_sync | FundingConditionsModel | total_processing | 10 | 3031,1 | 3019 | 3054 | 3054 |
| funding_sync | FundingModel | smart_contract_write | 10 | 39,5 | 13 | 127 | 127 |
| funding_sync | FundingModel | total_processing | 10 | 4048,9 | 4022 | 4135 | 4135 |
| mongo_stream | BankAccountModel | publish_to_rabbit | 14 | 2,1 | 1 | 5 | 5 |
| mongo_stream | FunderModel | publish_to_rabbit | 5 | 9,0 | 1 | 40 | 40 |
| mongo_stream | FundingConditionsModel | publish_to_rabbit | 10 | 1,7 | 1 | 5 | 5 |
| mongo_stream | FundingModel | publish_to_rabbit | 10 | 1,9 | 1 | 4 | 4 |
| mongo_stream | UserModel | publish_to_rabbit | 14 | 2,0 | 1 | 6 | 6 |
| payment_creation | FunderModel | total_processing | 5 | 1,2 | 1 | 2 | 2 |
| rabbitmq | BankAccountModel | queue_latency | 18 | 93432,8 | 20015 | 243829 | 243829 |
| rabbitmq | FunderModel | queue_latency | 5 | 2211,2 | 31 | 4332 | 4332 |
| rabbitmq | FundingConditionsModel | queue_latency | 10 | 13421,4 | 7 | 26806 | 26806 |
| rabbitmq | FundingModel | queue_latency | 10 | 17977,4 | 6 | 35983 | 35983 |
| rabbitmq | UserModel | queue_latency | 19 | 114406,8 | 30075 | 252886 | 252886 |
| seed | all | total_processing | 1 | 926,0 | 926 | 926 | 926 |
| user_sync | UserModel | smart_contract_write | 19 | 33,6 | 13 | 75 | 75 |
| user_sync | UserModel | total_processing | 19 | 6052,1 | 6024 | 6203 | 6203 |
