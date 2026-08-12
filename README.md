# FiapGames.Payment

## Producao no Azure

O servico e publicado no Azure Container Apps e exposto somente pelo Azure API Management:

```text
https://apim-fiapgames-prod.azure-api.net/payment
```

As requisicoes passam pela politica JWT RS256 e pelo rate limit do APIM. O Container App permite acesso externo apenas a partir do IP do gateway. SQL, RabbitMQ e configuracoes JWT sao obtidos do Azure Key Vault por identidade gerenciada.

O workflow `.github/workflows/deploy-production.yml` faz build e push no ACR, executa migrations por Container Apps Job com `--migrate` e publica a nova revisao apenas em caso de sucesso. A mensageria possui DLX/DLQ e rejeita mensagens com falha usando `requeue: false`. Health checks: `/health/live` e `/health/ready`.

Microserviço responsável pelo processamento de pagamentos, controle de saldo e registro de movimentações financeiras da plataforma FiapGames.

Este serviço atua como o componente financeiro do ecossistema, recebendo eventos e coordenando operações como compra, cobrança, validação de saldo e histórico de transações.

> Objetivo: oferecer uma base organizada e extensível para pagamentos, seguindo princípios de Clean Architecture, separação de responsabilidades e integração com mensageria.

---

## Arquitetura do Projeto

A solução é estruturada em camadas inspiradas em Clean Architecture e DDD, com foco em manutenibilidade, testes e evolução incremental.

### Executar via Docker

Na raiz do projeto, execute:

```bash
docker compose up --build
```

### Estrutura da solução

```txt
FiapGames.PaymentService.sln

src/
├── 1-Payment.Api
├── 2-Payment.Application
├── 2-Payment.Domain
└── 3-Payment.Infrastructure

test/
├── Payment.Application.Test
└── Payment.Domain.Test
```

### Responsabilidades das camadas

#### 1-Payment.Api

Camada de exposição da API HTTP.

Responsável por:
- Endpoints REST para pagamento, saldo e compras
- Configuração de Swagger/OpenAPI
- Middleware e pipeline HTTP
- Injeção de dependências
- Exposição de endpoints financeiros

#### 2-Payment.Application

Camada de aplicação.

Responsável por:
- Regras de negócio de pagamentos
- Serviços de aplicação
- Casos de uso
- DTOs
- Interfaces de contratos

#### 2-Payment.Domain

Camada de domínio.

Responsável por:
- Entidades como compra, conta e pagamento
- Regras de domínio
- Objetos de valor
- Contratos principais
- Lógica independente de framework

#### 3-Payment.Infrastructure

Camada de infraestrutura.

Responsável por:
- Persistência de dados com Entity Framework Core
- Repositórios
- Contexto do banco
- Integrações com mensageria
- Implementações técnicas

---

## Principais Funcionalidades

Este microserviço é responsável por:
- Consulta e atualização de saldo
- Processamento de pagamentos
- Registro de compras e movimentações financeiras
- Consumo e publicação de eventos relacionados a orders e pagamentos
- Validação de disponibilidade de saldo
- Persistência do histórico financeiro

---

## Stack Tecnológica

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- RabbitMQ (integração planejada/implementada em infraestrutura)
- Swagger / OpenAPI
- xUnit (testes)

---

## Padrões Utilizados

- Clean Architecture
- SOLID
- Dependency Injection
- Repository Pattern
- Separation of Concerns
- Domain-Oriented Design
- Event-driven design

---

## Configuração do Ambiente

### Pré-requisitos

Antes de executar este projeto, certifique-se de ter instalado:
- .NET SDK 9+
- SQL Server
- Docker Desktop
- Visual Studio 2022+ ou Rider
- EF Core CLI

Instalar o CLI do Entity Framework:

```bash
dotnet tool install --global dotnet-ef
```

ou atualizar:

```bash
dotnet tool update --global dotnet-ef
```

### Exemplo de configuração

```json
{
  "ConnectionStrings": {
    "FIAPGamesConnection": "Server=localhost,1436;Database=fiapgames_payment;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False"
  }
}
```

---

## Executando o Projeto

Restaurar dependências:

```bash
dotnet restore
```

Executar a aplicação:

```bash
dotnet run --project src/1-Payment.Api
```

---

## Kubernetes (autonomia por serviço)

Manifests próprios do serviço estão em `k8s/`:

- `payment-api-configmap.yaml`
- `payment-api-secret.yaml`
- `payment-api-service.yaml`
- `payment-api-deployment.yaml`

ConfigMap contém variáveis não sensíveis (ambiente, urls e host/porta de RabbitMQ).
Secret contém variáveis sensíveis (connection string completa e credenciais de RabbitMQ).

---

## Migrations

Criar migration:

```bash
dotnet ef migrations add InitialCreate \
--project src/3-Payment.Infrastructure \
--startup-project src/1-Payment.Api
```

Aplicar migrations:

```bash
dotnet ef database update \
--project src/3-Payment.Infrastructure \
--startup-project src/1-Payment.Api
```

---

## Testes

Executar testes:

```bash
dotnet test
```

---

## Licença

Projeto desenvolvido para fins acadêmicos e evolução arquitetural da plataforma FiapGames.

#### Testes

```txt
<Serviço>.Application.Test
<Serviço>.Domain.Test
```

Essa convenção foi pensada para permitir reutilização da estrutura entre múltiplos microsserviços do ecossistema.

Exemplo:

```txt
FiapGames.Auth
FiapGames.Inventory
FiapGames.Payment
FiapGames.Matchmaking
```

---

## Roadmap

Funcionalidades previstas:

* Estorno de pagamentos
* Retry policy para mensageria
* Dead Letter Queue (DLQ)
* Observabilidade e logging
* Auditoria financeira
* Integração com novos provedores de pagamento

---

## Licença

Projeto desenvolvido para fins acadêmicos e evolução arquitetural da plataforma **FiapGames**.
