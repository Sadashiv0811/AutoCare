# AutoCare – Vehicle Service & Maintenance Management System

AutoCare is a web-based Vehicle Service & Maintenance Management System designed to simplify and manage the daily operations of an automobile service center.

The application helps service centers manage customers, vehicles, mechanics, service jobs, spare parts, invoices and maintenance records through a centralized platform.

It provides role-based access for administrators, service advisors, mechanics and customers to ensure that each user can access the features relevant to their responsibilities.

## Features

### 1. Authentication & Authorization
- User registration and login.
- JWT-based authentication.
- Role-based access control.
- Secure access to protected API endpoints.
- Support for Admin, Service Advisor, Mechanic and Customer roles.

### 2. Dashboard
- Overview of service center operations.
- Summary of service jobs and invoices.
- Revenue and payment statistics.
- Inventory overview and low-stock indicators.

### 3. Customer Management
- Add, view, update and delete customer records.
- Manage customer information.
- View customer-related vehicle details.

### 4. Vehicle Management
- Register and manage customer vehicles.
- Store vehicle registration and identification details.
- View vehicle service history.
- Track maintenance records.

### 5. Service Management
- Create and manage service jobs.
- Assign mechanics to service jobs.
- Track service job status.
- Add service notes.
- Manage spare parts used during servicing.
- Automatically record important service timestamps.

**Service Job Workflow:**

Scheduled → Received → Inspection → In Progress → Completed → Ready for Pickup → Delivered

### 6. Mechanic Management
- Add, view, update and delete mechanic records.
- Assign mechanics to service jobs.
- Restrict mechanics to their assigned service jobs.
- Provide read-only mechanic information to Service Advisors.

### 7. Spare Parts & Inventory Management
- Manage spare parts and inventory.
- Add, update, view and delete parts.
- Track available quantities.
- Monitor low-stock items.
- Automatically deduct inventory when parts are used in service jobs.
- Restore inventory when applicable parts are removed from service jobs.

### 8. Invoice Management
- Generate invoices for completed service jobs.
- Automatically generate unique invoice numbers.
- Track invoice amounts and payment status.
- View customer invoices.
- Prevent unauthorized modifications to invoiced service jobs.

### 9. Reports & Analytics
- View invoice and payment summaries.
- Monitor total invoiced, paid and unpaid amounts.
- View monthly revenue statistics.
- Analyze service job and inventory information.

### 10. Maintenance Records
- Automatically create maintenance records when a service job reaches Delivered status.
- Track vehicle maintenance history.
- Monitor upcoming maintenance based on mileage and time intervals.
- Maintenance reminders based on 5,000 km or 6 months.

### 11. Production Quality & Security
- Input validation and appropriate HTTP response codes.
- Centralized exception handling.
- Application logging.
- Pagination.
- Filtering and sorting.
- Role-based endpoint protection.
- Database transactions for data consistency.

## User Roles

| Role | Permissions |
|---|---|
| Admin | Full access to all management features. |
| Service Advisor | Manage customers, vehicles, services, parts, invoices and reports. Mechanic information is read-only. |
| Mechanic | View assigned service jobs and update service job status. |
| Customer | View own vehicles, service history, maintenance records and invoices. |

## Technology Stack

### Backend
- C#
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- RESTful APIs
- JWT Authentication
- Dependency Injection
- MySQL

### Frontend
- HTML5
- CSS3
- JavaScript
- Bootstrap

### Development & Testing Tools
- Visual Studio Code
- MySQL Workbench
- Swagger
- Git
- GitHub

## Project Architecture

The application follows a client-server architecture.

- **Frontend:** HTML, CSS, JavaScript and Bootstrap provide the user interface.
- **Backend:** ASP.NET Core Web API handles application logic, authentication, authorization and data processing.
- **Database:** MySQL stores application data.
- **ORM:** Entity Framework Core manages database operations and relationships.

The frontend communicates with the backend through REST API endpoints.

Entity Framework Core manages database relationships and schema migrations.

## Security

The application implements several security measures:

- JWT-based authentication.
- Role-based authorization.
- Protected API endpoints.
- Server-side input validation.
- Secure handling of sensitive configuration.
- Restrictions on unauthorized data access.

## Testing

The application has undergone functional and integration testing covering:

- User authentication and role-based access.
- Customer and vehicle management.
- Mechanic management and assignments.
- Service job workflow and status transitions.
- Spare parts inventory management.
- Automatic invoice generation.
- Invoice and payment tracking.
- Maintenance record automation.
- Reports and dashboard calculations.
- Pagination, filtering, and sorting.
- Invalid input handling.
- Unauthorized API access.
- Database transaction rollback.
