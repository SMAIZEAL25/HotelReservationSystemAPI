# User Identity Bounded Context Architecture for Hotel Revervation System

This diagram illustrates the high-level architecture, showcasing flow of this bound context and it implementation steps for user registration, showcasing DDD aggregates/value objects and CQRS commands/queries. Contexts communicate via domain events on a Message Bus for loose coupling.
```mermaid


sequenceDiagram
    participant U as External User
    participant API as API Layer (Controllers)
    participant App as Application Layer (Mediator/CQRS)
    participant Dom as Domain Layer (Services/Entities)
    participant Inf as Infrastructure Layer (Repo/DB)
    participant MB as Message Bus

    U->>API: POST /register (RegisterUserCommand)
    API->>App: Handle Command
    App->>Dom: UserRegistrationService.Validate & Create User Aggregate
    Dom->>Inf: Save User (IUserRepository)
    Inf-->>Dom: User Saved
    Dom-->>App: UserRegistered Event
    App->>MB: Publish UserRegistered
    MB-->>U: (via API) Success Response (JWT Token)
    Note over Dom: DDD: User Entity with Email VO, hashed Password