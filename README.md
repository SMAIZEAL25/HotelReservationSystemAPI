A scalable, event-driven Hotel Reservation System built with Domain-Driven Design (DDD) and CQRS patterns in NET. This project demonstrates a modular architecture using bounded contexts for managing bookings, payments, user identity, and more. Ideal for learning microservices, event sourcing, and clean architecture.
Project Overview
This system handles end-to-end hotel reservations:

## User Registration & Authentication: Secure login with JWT and RBAC (Roles: Guest, HotelAdmin, SuperAdmin).
Hotel & Room Management: Inventory tracking with availability checks.
Booking Lifecycle: Create, confirm, modify, or cancel reservations.
Payments: Idempotent processing with refunds.
Notifications: Event-triggered emails/SMS.
Search: Optimized queries for hotels and availability.

The architecture promotes loose coupling via domain events and a message bus (e.g., MediatR or MassTransit). Each bounded context can be deployed independently as microservices or modules in a monolith.
Key Features

## DDD Implementation: Aggregates (e.g., Booking as root), Value Objects (e.g., DateRange, Money), Domain Services.
CQRS: Separate commands (writes) and queries (reads) with MediatR.
Event-Driven: Domain events like BookingCreated trigger downstream actions.
Security: Password hashing (BCrypt), rate limiting, JWT tokens.
Scalability: SQL indexing, idempotency, and optional Elasticsearch for search.

## Architecture
The system is divided into 6 Bounded Contexts for high cohesion and low coupling:

User/Identity: Authentication, RBAC, user profiles.
Hotel/Inventory: Hotel/room details and availability.
Booking: Reservation management and status lifecycle.
Payment: Transaction processing and refunds.
Notification: Communication (email/SMS) on events.
Search: Denormalized read models for fast queries.

## Flow Diagram (DDD + CQRS)
HotelsQueryAuth TokenAuth TokenAuth TokenQueriesCommandsCommandsUser Registration/Login
CQRS: RegisterUserCommand, LoginUserCommandDomain: User Entity, Role VO
Domain Service: AuthenticationServiceMessage Message Bus, Manage Hotels/Rooms
CQRS: UpdateAvailabilityCommandDomain: Hotel Entity, Room Child
Value Objects: Address, RoomTypeCreate/Modify Bookings
CQRS: CreateBookingCommand, ConfirmBookingCommandDomain: Booking Root, RoomReservation
Value Objects: DateRange
Status LifecycleProcess Payments
CQRS: ProcessPaymentCommandDomain: PaymentTransaction Root
Value Objects: Money, PaymentMethod, Send Communications
CQRS: SendNotificationCommandDomain: Notification Root
Value Objects: MessageTemplateFast Queries
Projections: HotelSearchView, RoomAvailabilityViewExternal Users/API
Interactions:

## Auth tokens from User/Identity secure access to other contexts.
Booking events triggers Payment → Notification.
Search builds projections from events for efficient reads.

## For detailed plans, see docs/ArchitectureFlow.md.
Tech Stack

Backend: .NET 8+ (ASP.NET Core Web API)
Architecture: DDD, CQRS (MediatR), Clean Architecture
Database: SQL Server (Entity Framework Core)
Messaging: MediatR for in-process; optional MassTransit for distributed events
Auth: JWT Bearer, ASP.NET Identity
Search: Optional Elasticsearch/Redis for read models
Testing: xUnit, Moq
Docs: Markdown with Mermaid diagrams

## Quick Start
Prerequisites

.NET SDK 8.0+
SQL Server (local or Docker)
Git

## Setup


Clone the repo:
bashgit clone https://github.com/yourusername/HotelReservationSystem.git
cd HotelReservationSystem


Restore dependencies:
bashdotnet restore


Update appsettings.json with your DB connection string (e.g., Server=localhost;Database=HotelReservation;Trusted_Connection=true;).


## Run migrations (if using EF Core):
bashdotnet ef migrations add InitialCreate --project src/Infrastructure
dotnet ef database update --project src/Infrastructure


## Build and run:
bashdotnet build
dotnet run --project src/Api
The API will start at https://localhost:7001. Swagger UI at https://localhost:7001/swagger.



## Project Structure
textHotelReservationSystem/
├── src/
│   ├── Api/                  # Web API controllers, middleware
│   ├── Application/          # CQRS Commands/Queries, Handlers (MediatR)
│   ├── Domain/               # Entities, Value Objects, Domain Services
│   ├── Infrastructure/       # Repos, EF Core, External services
│   └── SharedKernel/         # Common interfaces, events
├── tests/                    # Unit/Integration tests
├── docs/                     # Architecture diagrams, plans (e.g., UserIdentityPlan.md)
├── README.md                 # This file
└── HotelReservationSystem.sln
## Testing
Run unit tests:
bashdotnet test
Coverage: Integrate Coverlet for reports.
## Contributing

## Fork the repo.
Create a feature branch (git checkout -b feature/AmazingFeature).
Commit changes (git commit -m 'Add some AmazingFeature').
Push to the branch (git push origin feature/AmazingFeature).
Open a Pull Request.

Follow CONTRIBUTING.md for guidelines.
## License
This project is licensed under the MIT License - see the LICENSE file for details.
## Acknowledgments

Inspired by DDD patterns from Eric Evans and Vaughn Vernon.
Thanks to the .NET community for awesome tools like MediatR and EF Core.


Current Focus: Implementing the User/Identity Bounded Context – check docs/UserIdentityPlan.md for details!
Questions? Open an issue or connect on LinkedIn: [Your LinkedIn Profile].
Last updated: October 04, 20252.3s
