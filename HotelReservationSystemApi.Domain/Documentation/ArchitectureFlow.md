# Hotel Reservation System: Bounded Contexts Flow (DDD + CQRS)

This diagram illustrates the high-level architecture, showcasing DDD aggregates/value objects and CQRS commands/queries. Contexts communicate via domain events on a Message Bus for loose coupling.

```mermaid
flowchart TD
    subgraph "User/Identity Context (DDD: Domain Layer with User Aggregate)"
        UI[User Registration/Login<br/>CQRS: RegisterUserCommand, LoginUserCommand]
        UD[Domain: User Entity, Role VO<br/>Domain Service: AuthenticationService]
        UI -->|Command| UD
        UD -->|Event: UserRegistered| MB[Message Bus]
    end

    subgraph "Hotel/Inventory Context (DDD: Hotel Aggregate)"
        HI[Manage Hotels/Rooms<br/>CQRS: UpdateAvailabilityCommand]
        HD[Domain: Hotel Entity, Room Child<br/>Value Objects: Address, RoomType]
        HI -->|Command| HD
        HD -->|Event: RoomAvailabilityUpdated| MB
    end

    subgraph "Booking Context (DDD: Booking Aggregate)"
        BC[Create/Modify Bookings<br/>CQRS: CreateBookingCommand, ConfirmBookingCommand]
        BD[Domain: Booking Root, RoomReservation<br/>Value Objects: DateRange<br/>Status Lifecycle]
        BC -->|Command/Query: GetAvailabilityQuery| BD
        BD -->|Event: BookingCreated| MB
        BD -->|Event: BookingConfirmed| MB
        MB -->|Event: RoomAvailabilityUpdated| BD
    end

    subgraph "Payment Context (DDD: PaymentTransaction Aggregate)"
        PC[Process Payments<br/>CQRS: ProcessPaymentCommand]
        PD[Domain: PaymentTransaction Root<br/>Value Objects: Money, PaymentMethod]
        PC -->|Command| PD
        PD -->|Event: PaymentProcessed| MB
        MB -->|Event: BookingCreated| PC
    end

    subgraph "Notification Context (DDD: Notification Aggregate)"
        NC[Send Communications<br/>CQRS: SendNotificationCommand]
        ND[Domain: Notification Root<br/>Value Objects: MessageTemplate]
        NC -->|Command| ND
        MB -->|Event: BookingConfirmed| NC
        MB -->|Event: PaymentProcessed| NC
    end

    subgraph "Search Context (CQRS Read Model)"
        SC[Fast Queries<br/>Projections: HotelSearchView, RoomAvailabilityView]
        MB -->|Events| SC
        SC -->|Query: SearchHotelsQuery| Users[External Users/API]
    end

    UI -->|Auth Token| BC
    UI -->|Auth Token| PC
    UI -->|Auth Token| HI
    Users -->|Queries| SC
    Users -.->|Commands| BC
    Users -.->|Commands| PC

    style UI fill:#e1f5fe
    style BC fill:#f3e5f5
    style PC fill:#e8f5e8
    style NC fill:#fff3e0
    style HI fill:#fce4ec
    style SC fill:#f5f5f5
    style MB fill:#ffd700