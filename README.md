# FiapGames.Payment

Microserviço responsável pelo gerenciamento de pagamentos, saldo e histórico financeiro da plataforma **FiapGames**.

Este serviço centraliza funcionalidades relacionadas ao processamento de pagamentos, gerenciamento de saldo dos usuários, consumo de eventos de pedidos e registro de movimentações financeiras da plataforma.

> **Objetivo:** fornecer uma base reutilizável e padronizada para microsserviços do ecossistema FiapGames, seguindo princípios de Clean Architecture, separação de responsabilidades e baixo acoplamento.

---

## Arquitetura do Projeto

A solução segue uma arquitetura em camadas inspirada em **Clean Architecture / DDD (Domain-Driven Design)**, visando facilitar manutenção, testes e evolução do sistema.

## Executar via Docker

Executar comando no cmd na raiz do projeto:

```bash
docker compose up --build
```

A solução segue uma arquitetura em camadas inspirada em **Clean Architecture / DDD (Domain-Driven Design)**, visando facilitar manutenção, testes e evolução do sistema.

### Estrutura da solução

```txt
FiapGames.Payment.sln

src/
├── Payment.Api
├── Payment.Application
├── Payment.Infrastructure
└── Payment.Domain

test/
├── Payment.Application.Test
└── Payment.Domain.Test
```

### Responsabilidades das camadas

#### `Payment.Api`

Camada de exposição da API.

Responsável por:

* Endpoints REST
* Swagger/OpenAPI
* Middleware e pipeline HTTP
* Configurações de DI (Dependency Injection)
* Exposição de endpoints financeiros

#### `Payment.Application`

Camada de aplicação.

Responsável por:

* Regras de negócio da aplicação
* Serviços de aplicação
* Casos de uso
* DTOs
* Interfaces de contratos

#### `Payment.Domain`

Camada de domínio.

Responsável por:

* Entidades
* Regras de domínio
* Objetos de valor
* Contratos centrais
* Regras independentes de framework

#### `Payment.Infrastructure`

Camada de infraestrutura.

Responsável por:

* Persistência de dados
* Entity Framework Core
* Contextos (`DbContext`)
* Repositórios
* Integração com mensageria
* Consumo de eventos
* Implementações técnicas

#### `Tests`

Projetos de testes automatizados.

Responsável por:

* Testes unitários
* Testes de regras de negócio
* Garantia de qualidade do domínio e aplicação

---

## Principais Funcionalidades

Este microserviço é responsável por:

* Consulta de saldo do usuário
* Adição de saldo
* Débito automático de saldo
* Registro de histórico financeiro
* Consumo do evento `OrderPlacedEvent`
* Processamento de pagamentos
* Validação de saldo disponível
* Persistência de movimentações financeiras

---

## Stack Tecnológica

* **.NET 9**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **MySQL**
* **RabbitMQ**
* **Swagger / OpenAPI**
* **xUnit** (testes)

---

## Padrões Utilizados

O projeto segue alguns princípios e padrões arquiteturais:

* Clean Architecture
* SOLID
* Dependency Injection
* Repository Pattern
* Separation of Concerns
* Domain-Oriented Design
* Event-Driven Architecture

---

## Configuração do Ambiente

### Pré-requisitos

Antes de executar o projeto, certifique-se de possuir instalado:

* .NET SDK 9+
* MySQL
* RabbitMQ
* Visual Studio 2022+ ou Rider
* EF Core CLI

Instalação do Entity Framework CLI:

```bash
dotnet tool install --global dotnet-ef
```

ou atualização:

```bash
dotnet tool update --global dotnet-ef
```

---

## Configuração do `appsettings.json`

Exemplo de configuração:

```json
{
  "ConnectionStrings": {
    "FIAPGamesConnection": "server=localhost;database=fiapgames_payment;user=root;password=sua_senha"
  },

  "RabbitMq": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
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
dotnet run --project src/Payment.Api
```

---

## Migrations

Criar uma migration:

```bash
dotnet ef migrations add InitialCreate \
--project src/Payment.Infrastructure \
--startup-project src/Payment.Api
```

Aplicar migrations:

```bash
dotnet ef database update \
--project src/Payment.Infrastructure \
--startup-project src/Payment.Api
```

---

## Testes

Executar testes:

```bash
dotnet test
```

---

## Convenções do Projeto

### Nomenclatura

#### Projetos

```txt
<Serviço>.Api
<Serviço>.Application
<Serviço>.Domain
<Serviço>.Infrastructure
```

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
