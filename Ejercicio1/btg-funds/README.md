# BTG Funds – Serverless API (.NET 8 + AWS SAM + DynamoDB)

API Serverless para gestionar suscripciones a fondos: suscribir, cancelar y consultar historial de transacciones. Implementada con AWS Lambda (.NET 8 C#), API Gateway, DynamoDB y SNS para notificaciones.

## Requisitos

- .NET 8 SDK
- AWS SAM CLI
- Docker
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

Compilar el código fuente en folder .aws-sam/build/

```bash
sam build
```

o

```bash
sam build --use-container --mount-with WRITE
```

Levantar Gateway local

```bash
sam local start-api
```

## Deploy por primera vez

Deploy del proyecto primera vez

```bash
sam deploy --guided
```

Debido a que es la primera vez hay que tener en cuenta

```bash
Stack Name [sam-app]: btg-funds-stack
AWS Region [us-east-1]: us-east-2
Confirm changes before deploy [y/N]: y
Allow SAM CLI IAM role creation [Y/n]: Y
Disable rollback [y/N]: N
SubscribeFn has no authentication. Is this okay? [y/N]: y
CancelFn has no authentication. Is this okay? [y/N]: y
HistoryFn has no authentication. Is this okay? [y/N]: y
Save arguments to configuration file [Y/n]: Y
SAM configuration file [samconfig.toml]:
SAM configuration environment [default]:
```

Para aplicar los cambios del listado mostrado en la terminal

```bash
Deploy this changeset? [y/N]: y.
```

## Deploy normal

```bash
sam deploy
```

## Cargar data semilla

En los archivos

```bash
seed/funds_seed.json
seed/users_seed.json
```

Ajustar el nombre de la tabla por el creado en el stack cambiandole el ID

```bash
btg-funds-stack-FundsTable-ID
btg-funds-stack-UsersTable-ID
```

En la raíz del proyecto para cargar la data

```bash
aws dynamodb batch-write-item --region us-east-2 --request-items file://seed/funds_seed.json
aws dynamodb batch-write-item --region us-east-2 --request-items file://seed/users_seed.json
```

Para validar la data cargada (Cambiar el ID por el asignado en la creación del stack)

```bash
aws dynamodb scan --region us-east-2 --table-name btg-funds-stack-FundsTable-ID
aws dynamodb scan --region us-east-2 --table-name btg-funds-stack-UsersTable-ID
```

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
