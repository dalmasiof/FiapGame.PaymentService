# FiapGames.Payment

Microserviço responsável pelo processamento de pagamentos, controle de saldo e registro financeiro da plataforma FiapGames.

Este serviço atua como o componente financeiro do ecossistema. Ele consome solicitações de compra publicadas pelo microserviço de Catálogo, valida saldo do usuário, aprova ou recusa o pagamento, responde o resultado ao Catálogo e, quando aprovado, publica uma notificação para o microserviço de Notificação.

Objetivo: oferecer uma base organizada e extensível para pagamentos, seguindo princípios de Clean Architecture, separação de responsabilidades, persistência com Entity Framework Core e integração assíncrona via RabbitMQ.

## Arquitetura do Projeto

A solução é estruturada em camadas inspiradas em Clean Architecture e DDD, com foco em manutenibilidade, testes e evolução incremental.

```text
FiapGames.PaymentService
|-- 1-Payment.Api
|-- 2-Payment.Application
|-- 2-Payment.Domain
|-- 3-Payment.Infrastructure
|-- Payment.Application.Test
|-- Payment.Domain.Test
|-- docker-compose.yml
|-- Dockerfile
`-- FiapGames.PaymentService.slnx
```

## Responsabilidades das Camadas

### 1-Payment.Api

Camada de exposição da API HTTP.

Responsável por:

- Endpoints REST para saldo e histórico de pagamentos.
- Configuração de Swagger/OpenAPI.
- Pipeline HTTP da aplicação.
- Injeção de dependências.
- Inicialização do banco com migrations.
- Registro do worker de consumo RabbitMQ.

### 2-Payment.Application

Camada de aplicação.

Responsável por:

- Casos de uso de pagamento.
- Validação de saldo e aprovação ou recusa de compras.
- Serviços de aplicação.
- DTOs.
- Contratos de interfaces.
- Contratos de eventos de integração.
- Publicação de eventos para Catálogo e Notificação.

### 2-Payment.Domain

Camada de domínio.

Responsável por:

- Entidades como `Compra`, `Conta`, `Pagamento` e `CompraJogo`.
- Regras de domínio de saldo.
- Status de pagamento.
- Lógica independente de framework.

### 3-Payment.Infrastructure

Camada de infraestrutura.

Responsável por:

- Persistência com Entity Framework Core.
- Repositórios.
- `PaymentContext`.
- Migrations.
- Publisher RabbitMQ.
- Worker consumidor de compras solicitadas.

## Fluxo de Mensageria

### Entrada: Catálogo para Pagamento

O Payment consome solicitações de compra publicadas pelo Catálogo.

```text
Exchange: catalogo.exchange
Queue: pagamento.compra.solicitada
Routing key: catalogo.compra.solicitada
```

Contrato consumido:

```json
{
  "CompraId": 2,
  "UsuarioId": 2,
  "JogosIds": [3],
  "ValorTotal": 80.00,
  "SolicitadaEm": "2026-07-05T01:40:01.042226Z",
  "RastreioId": "b9d6b483-2f1f-4962-a6fa-37535d212355",
  "EmailUsuario": "TESTE@TESTE.COM"
}
```

O campo `EmailUsuario` deve ser extraído pelo Catálogo a partir do Bearer Token da requisição original e enviado ao Payment no evento de compra solicitada. O consumer RabbitMQ do Payment não tem acesso ao token HTTP original.

### Saída: Pagamento para Catálogo

Após processar o pagamento, o Payment publica o resultado para o Catálogo.

```text
Exchange: pagamento.exchange
Routing keys:
- pagamento.aprovado
- pagamento.recusado
```

Contrato publicado:

```json
{
  "CompraId": 2,
  "UsuarioId": 2,
  "Aprovado": true,
  "ValorTotal": 80.00,
  "Status": "CONCLUIDO",
  "ProcessadoEm": "2026-07-05T02:40:01.042226Z",
  "RastreioId": "b9d6b483-2f1f-4962-a6fa-37535d212355",
  "MotivoRecusa": null
}
```

### Saída: Pagamento para Notificação

Quando o pagamento é aprovado e há destinatário disponível, o Payment publica um evento para o microserviço de Notificação.

```text
Exchange: notificacao.exchange
Routing key: pagamento.notificacao
```

Contrato publicado:

```json
{
  "CorrelacaoId": "35afa28d-91a8-4807-b4da-9d67d82d3f43",
  "Destinatario": "TESTE@TESTE.COM",
  "Assunto": "Pagamento Aprovado",
  "CorpoMensagem": "Olá! Seu pagamento da compra 2 no valor de R$ 80,00 foi aprovado.",
  "DominioOrigem": "Pagamento",
  "RastreioId": "b9d6b483-2f1f-4962-a6fa-37535d212355"
}
```

## Idempotência

O Payment armazena o identificador da compra do Catálogo em `Compra.IdCompraCatalogo`.

Esse campo possui índice único e é usado para evitar débito duplicado em caso de reentrega da mesma mensagem pelo RabbitMQ. Se uma compra já foi processada, o Payment republica o resultado conhecido em vez de debitar novamente o saldo do usuário.

## Principais Funcionalidades

Este microserviço é responsável por:

- Consulta de saldo.
- Adição de saldo.
- Validação de saldo para compra.
- Processamento assíncrono de solicitações de compra.
- Aprovação ou recusa de pagamento.
- Débito de saldo em pagamentos aprovados.
- Registro de compras e pagamentos.
- Publicação do resultado do pagamento para o Catálogo.
- Publicação de notificação quando o pagamento é aprovado.
- Histórico de pagamentos por usuário.

## Stack Tecnológica

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- RabbitMQ
- Swagger / OpenAPI
- xUnit
- Docker

## Padrões Utilizados

- Clean Architecture
- SOLID
- Dependency Injection
- Repository Pattern
- Separation of Concerns
- Domain-Oriented Design
- Event-driven design
- Background Worker
- Idempotent Consumer

## Configuração do Ambiente

### Pré-requisitos

Antes de executar este projeto, certifique-se de ter instalado:

- .NET SDK 10+
- Docker Desktop
- SQL Server, caso execute fora do Docker
- Visual Studio 2022+, Rider ou VS Code
- EF Core CLI

Instalar o CLI do Entity Framework:

```bash
dotnet tool install --global dotnet-ef
```

Atualizar o CLI do Entity Framework:

```bash
dotnet tool update --global dotnet-ef
```

## Exemplo de Configuração

```json
{
  "ConnectionStrings": {
    "FIAPGamesConnection": "Server=localhost,1436;Database=fiapgames_payment;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False"
  },
  "RabbitMq": {
    "HostName": "127.0.0.1",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

Em Docker, o host do RabbitMQ deve ser `rabbitmq`, dentro da rede compartilhada `fiapgames-network`.

## Executar via Docker

Na raiz do projeto, execute:

```bash
docker compose up --build
```

O `docker-compose.yml` do Payment configura:

- API de pagamento.
- SQL Server do Payment.
- Rede compartilhada `fiapgames-network`.
- Variáveis de ambiente para conexão com SQL Server e RabbitMQ.

Observação: o RabbitMQ deve estar disponível na mesma rede Docker com o nome `rabbitmq`.

## Executar Localmente

Restaurar dependências:

```bash
dotnet restore
```

Executar a aplicação:

```bash
dotnet run --project 1-Payment.Api/1-Payment.Api.csproj
```

Build da solução:

```bash
dotnet build FiapGames.PaymentService.slnx
```

## Migrations

Criar migration:

```bash
dotnet ef migrations add NomeDaMigration --project 3-Payment.Infrastructure/3-Payment.Infrastructure.csproj --startup-project 1-Payment.Api/1-Payment.Api.csproj
```

Aplicar migrations:

```bash
dotnet ef database update --project 3-Payment.Infrastructure/3-Payment.Infrastructure.csproj --startup-project 1-Payment.Api/1-Payment.Api.csproj
```

Ao iniciar, a API executa `MigrateAsync()` para aplicar migrations pendentes.

## Testes

Executar testes:

```bash
dotnet test FiapGames.PaymentService.slnx
```

Projetos de teste:

- `Payment.Application.Test`
- `Payment.Domain.Test`

Essa convenção permite reutilização da estrutura entre múltiplos microsserviços do ecossistema FiapGames.

Exemplos:

- `FiapGames.Auth`
- `FiapGames.Catalog`
- `FiapGames.Payment`
- `FiapGames.Notification`

## Roadmap

Funcionalidades previstas:

- Estorno de pagamentos.
- Outbox pattern para publicação transacional de eventos.
- Retry policy mais explícita para mensageria.
- Dead Letter Queue no consumidor do Payment.
- Observabilidade e tracing distribuído.
- Auditoria financeira.
- Integração com provedores externos de pagamento.
- Testes automatizados para workers RabbitMQ.

## Licença

Projeto desenvolvido para fins acadêmicos e evolução arquitetural da plataforma FiapGames.
