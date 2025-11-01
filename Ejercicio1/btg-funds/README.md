# BTG Funds – Serverless API (.NET 8 + AWS SAM + DynamoDB)

API Serverless para gestionar suscripciones a fondos: suscribir, cancelar y consultar historial de transacciones. Implementada con AWS Lambda (.NET 8 C#), API Gateway, DynamoDB y SNS para notificaciones.

## Endpoints

- POST `/funds/subscribe`

  - Request:
    ```json
    {
      "userId": "u123",
      "fundId": "5",
      "amount": 100000
    }
    ```
  - Response:
    ```json
    {
      "trxId": "trx#<guid>",
      "newBalance": 400000
    }
    ```

- POST `/funds/cancel`

  - Request:
    ```json
    {
      "userId": "u123",
      "fundId": "5"
    }
    ```
  - Response:
    ```json
    {
      "trxId": "trx#<guid>",
      "newBalance": 500000
    }
    ```

- GET `/funds/history/{user_id}`
  - Response:
    ```json
    [
      {
        "trx_id": "2025-10-30T14:10:00Z#trx#<guid>",
        "type": "SUBSCRIBE",
        "fund_id": "5",
        "fund_name": "FPV_BTG_PACTUAL_DINAMICA",
        "amount": 100000,
        "created_at": "2025-10-30T14:10:00Z"
      }
    ]
    ```

## Requisitos

- .NET 8 SDK
- AWS SAM CLI
- Docker (para `sam local start-api`)
- Credenciales AWS configuradas localmente (para deploy)

## Variables de entorno (definidas por SAM)

- `USERS_TABLE`
- `FUNDS_TABLE`
- `SUBS_TABLE`
- `TRX_TABLE`
- `NOTIFY_TOPIC_ARN`
- `EMAIL_SENDER`
- `AWS_REGION`

## Ejecutar local

```bash
sam build
sam local start-api
```

## Deploy

```bash
sam deploy --guided
```

## Carga de datos

```bash
aws dynamodb batch-write-item --request-items file://seed/funds_seed.json
aws dynamodb put-item --table-name <USERS_TABLE_NAME> --item file://seed/users_seed.json
```
