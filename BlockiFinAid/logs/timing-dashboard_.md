# BlockiFinAid Timing Dashboard
> Generated UTC: 2026-09-28T18:28:17.6266890Z

## Executive Summary
- Total observed operations: 186
- Average duration across all recorded steps: 4771,5 ms
- Peak p95 duration recorded: 47988 ms
- Models tracked: 6

## End-to-End by Model
| Model | Samples | Avg total processing | P95 total processing |
| --- | ---: | ---: | ---: |
| all | 1 | 950,0 ms | 950 ms |
| BankAccountModel | 44 | 5026,8 ms | 5040 ms |
| FunderModel | 20 | 1016,6 ms | 1041 ms |
| FundingConditionsModel | 40 | 3029,7 ms | 3070 ms |
| FundingModel | 40 | 4025,9 ms | 4042 ms |
| UserModel | 41 | 6027,7 ms | 6039 ms |

## Slowest Average Stages
| Pipeline | Model | Step | Count | Avg (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: |
| rabbitmq | UserModel | queue_latency | 9 | 24060,6 | 47988 |
| rabbitmq | BankAccountModel | queue_latency | 10 | 22555,8 | 44961 |
| rabbitmq | FundingModel | queue_latency | 10 | 17874,5 | 35724 |
| rabbitmq | FundingConditionsModel | queue_latency | 10 | 13413,2 | 26787 |
| user_sync | UserModel | total_processing | 9 | 6027,7 | 6039 |
| bank_account_sync | BankAccountModel | total_processing | 10 | 5026,8 | 5040 |
| funding_sync | FundingModel | total_processing | 10 | 4025,9 | 4042 |
| funding_conditions_sync | FundingConditionsModel | total_processing | 10 | 3029,7 | 3070 |
| rabbitmq | FunderModel | queue_latency | 5 | 2104,6 | 4120 |
| funder_sync | FunderModel | total_processing | 5 | 1016,6 | 1041 |

## Drill-down Summary
| Pipeline | Model | Step | Count | Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| bank_account_sync | BankAccountModel | smart_contract_write | 10 | 19,2 | 8 | 31 | 31 |
| bank_account_sync | BankAccountModel | total_processing | 10 | 5026,8 | 5014 | 5040 | 5040 |
| funder_sync | FunderModel | lookup_contract | 5 | 10,0 | 2 | 34 | 34 |
| funder_sync | FunderModel | total_processing | 5 | 1016,6 | 1008 | 1041 | 1041 |
| funding_conditions_sync | FundingConditionsModel | smart_contract_write | 10 | 22,7 | 11 | 66 | 66 |
| funding_conditions_sync | FundingConditionsModel | total_processing | 10 | 3029,7 | 3015 | 3070 | 3070 |
| funding_sync | FundingModel | smart_contract_write | 10 | 20,2 | 10 | 38 | 38 |
| funding_sync | FundingModel | total_processing | 10 | 4025,9 | 4015 | 4042 | 4042 |
| mongo_stream | BankAccountModel | publish_to_rabbit | 14 | 3,2 | 1 | 18 | 18 |
| mongo_stream | FunderModel | publish_to_rabbit | 5 | 13,8 | 1 | 61 | 61 |
| mongo_stream | FundingConditionsModel | publish_to_rabbit | 10 | 2,2 | 1 | 8 | 8 |
| mongo_stream | FundingModel | publish_to_rabbit | 10 | 2,3 | 1 | 5 | 5 |
| mongo_stream | UserModel | publish_to_rabbit | 14 | 2,4 | 1 | 7 | 7 |
| rabbitmq | BankAccountModel | queue_latency | 10 | 22555,8 | 74 | 44961 | 44961 |
| rabbitmq | FunderModel | queue_latency | 5 | 2104,6 | 95 | 4120 | 4120 |
| rabbitmq | FundingConditionsModel | queue_latency | 10 | 13413,2 | 11 | 26787 | 26787 |
| rabbitmq | FundingModel | queue_latency | 10 | 17874,5 | 6 | 35724 | 35724 |
| rabbitmq | UserModel | queue_latency | 9 | 24060,6 | 63 | 47988 | 47988 |
| seed | all | total_processing | 1 | 950,0 | 950 | 950 | 950 |
| user_sync | UserModel | smart_contract_write | 9 | 19,8 | 12 | 27 | 27 |
| user_sync | UserModel | total_processing | 9 | 6027,7 | 6021 | 6039 | 6039 |
