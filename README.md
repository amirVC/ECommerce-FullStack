# E-Commerce Platform

A full-stack e-commerce web application built with **ASP.NET Core 8, C#, Entity Framework Core, and PostgreSQL**. The project features a customer-facing storefront, a RESTful API, secure authentication, shopping cart and checkout workflows, payment processing, and a comprehensive administration dashboard.

This project demonstrates full-stack .NET development, REST API design, database management, authentication and authorization, and real-world e-commerce business logic.

## Features

### Customer Storefront

- **Product Catalog** — Browse products with categories, search, filtering, sorting, and pagination.
- **Product Details** — View product information, image galleries, pricing, and customer reviews.
- **Shopping Cart** — Add products, update quantities, remove items, and review order totals.
- **Wishlist** — Save products for future purchases.
- **Recently Viewed Products** — Keep track of previously viewed products.
- **Product Reviews** — Submit and manage product ratings and reviews.
- **User Addresses** — Manage shipping addresses associated with an account.
- **Responsive Interface** — A storefront built with ASP.NET Core MVC and Razor views.

### Pricing and Promotions

- **Sale Pricing** — Configure product sale prices.
- **Campaign Discounts** — Apply automatic discounts to eligible products or categories during scheduled campaigns.
- **Coupon System** — Support percentage-based and fixed-amount discounts.
- **Coupon Restrictions** — Support product, category, and order-level coupon rules.
- **Discount Limits** — Configure validity periods, usage limits, and maximum discount amounts.
- **Free Shipping** — Support shipping discounts through eligible coupons.
- **Transparent Pricing** — Track sale, campaign, and coupon discounts separately.

### Checkout, Payments, and Orders

- **Checkout Workflow** — Review cart items, shipping options, discounts, and final totals.
- **Shipping Costs** — Support configurable delivery methods, flat shipping rates, and free-shipping thresholds.
- **Stripe Integration** — Create payment intents and process payment events through Stripe webhooks.
- **Order Management** — Create orders and track order details and statuses.
- **Payment Management** — View and manage payment records.
- **Refund Requests** — Support refund request management.
- **Shipment Management** — Manage shipment information and tracking details.
- **Order History** — Allow customers to review their previous orders.

### Authentication and Security

- User registration and login.
- JSON Web Token (JWT) authentication for API access.
- Role-based authorization with Admin and Customer roles.
- Email One-Time Password (OTP) verification during login.
- Optional Time-Based One-Time Password (TOTP) two-factor authentication.
- Email confirmation and password reset workflows.
- Token refresh handling in the web application.
- Authentication rate limiting.
- Protected administrative endpoints.
- Centralized API exception handling and structured logging.

### Administration Dashboard

The administrative area provides tools for managing the store and monitoring its activity.

- **Dashboard Analytics** — Key performance indicators, revenue charts, recent orders, and top-selling products.
- **Product Management** — Create, edit, and manage products and product images.
- **Category Management** — Organize the product catalog.
- **Order Management** — View orders and update order statuses.
- **User Management** — View users and manage roles.
- **Coupon Management** — Create and maintain discount coupons.
- **Campaign Management** — Configure promotional campaigns.
- **Review Management** — Review customer feedback.
- **Payment Management** — Access payment administration tools.
- **Shipment Management** — Manage delivery and tracking information.
- **Refund Management** — Review refund requests.
- **Delivery Methods** — Configure available shipping options.
- **Audit Logs** — Review recorded administrative activity.

## Technology Stack

| Technology | Purpose |
|---|---|
| C# | Application development |
| .NET 8 | Application framework |
| ASP.NET Core MVC | Web application and storefront |
| ASP.NET Core Web API | RESTful backend services |
| Entity Framework Core | Object-relational mapping and database access |
| PostgreSQL | Relational database |
| JWT Bearer Authentication | API authentication |
| Role-Based Authorization | Access control |
| Stripe | Payment processing |
| MailKit / MimeKit | Email delivery |
| Swagger / OpenAPI | API documentation |
| Razor Views | Server-rendered user interface |
| Bootstrap | Responsive UI components |
| JavaScript and CSS | Client-side behavior and styling |
| Git and GitHub | Version control |

## Architecture

The solution separates the web interface from the backend API.

```text
ECommerce.sln
│
├── ECommerce.Web/
│   ├── Areas/
│   │   └── Admin/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   │   └── Http/
│   ├── ViewModels/
│   ├── Views/
│   └── wwwroot/
│
└── ECommerceAPI/
    ├── Controllers/
    │   └── Admin/
    ├── Data/
    ├── DTOs/
    ├── Extensions/
    ├── Middleware/
    ├── Migrations/
    └── Services/
```

### Web Application — `ECommerce.Web`

The MVC application provides the customer storefront and administrative interface. It communicates with the backend through a dedicated API client and uses services, interfaces, data transfer objects (DTOs), and view models to organize application responsibilities.

### Backend API — `ECommerceAPI`

The API handles application data and business operations, including product management, authentication, carts, orders, promotions, payments, reviews, and administration. Entity Framework Core manages database access, while middleware handles cross-cutting concerns such as exception handling.

### Database

PostgreSQL stores the application's persistent data. Entity Framework Core migrations manage database schema changes, and seed data supports initial application setup.

## Getting Started

### Prerequisites

Install the following before running the application:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or another compatible .NET development environment
- [PostgreSQL](https://www.postgresql.org/download/)
- A Stripe account and API keys if you want to test online payments
- An SMTP email account or compatible email provider if you want to test email features

### 1. Clone the Repository

```bash
git clone https://github.com/amirVC/ECommerce-FullStack.git
cd ECommerce-FullStack
```

### 2. Configure the Database

Create a PostgreSQL database for the application.

Configure the database connection string for `ECommerceAPI` using User Secrets, environment variables, or your local development configuration.

Example connection string:

```text
Host=localhost;Port=5432;Database=ECommerceDb;Username=YOUR_USERNAME;Password=YOUR_PASSWORD
```

Replace the example credentials with your local PostgreSQL credentials.

### 3. Configure Application Settings

Configure the required settings for both projects, including:

- PostgreSQL connection string
- JWT authentication settings
- Web application API base URL
- Stripe API keys and webhook configuration
- SMTP credentials and sender information

Use the configuration keys expected by the application's existing code.

**Security:** Never commit real passwords, JWT signing keys, Stripe secret keys, SMTP credentials, or other sensitive configuration to GitHub. Use .NET User Secrets or environment variables for local development, and a secure secret manager for production.

### 4. Restore Dependencies

From the solution root, run:

```bash
dotnet restore ECommerce.sln
```

### 5. Apply Database Migrations

Ensure the database connection is configured, then run:

```bash
dotnet ef database update --project ECommerceAPI
```

If the Entity Framework Core command-line tool is not installed, install it with:

```bash
dotnet tool install --global dotnet-ef
```

If it is already installed, update it as appropriate for your environment.

### 6. Run the Applications

Open `ECommerce.sln` in Visual Studio and configure both `ECommerceAPI` and `ECommerce.Web` to start together.

Alternatively, open separate terminals.

**Terminal 1 — Backend API**

```bash
dotnet run --project ECommerceAPI
```

**Terminal 2 — Web Application**

```bash
dotnet run --project ECommerce.Web
```

Ensure the web application's configured API base URL matches the address at which the backend is running.

### 7. Access the Application

Use the URLs configured by your local launch profiles. For the development setup previously used in this project, the expected addresses are:

| Service | URL |
|---|---|
| Web application | https://localhost:7138 |
| Backend API | https://localhost:7230 |
| API documentation | https://localhost:7230/swagger/index.html |

These addresses may differ if your local launch settings have changed.


## Future Improvements

Potential areas for continued development include:

- Automated unit and integration tests.
- Database query and performance optimization.
- Continuous integration and deployment improvements.
- Enhanced monitoring and operational health checks.
- Additional payment and shipping integration tests.
- Production deployment documentation.



---

*Built with C#, ASP.NET Core 8, Entity Framework Core, and PostgreSQL.*
