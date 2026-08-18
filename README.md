# KidNest

[Live Demo](https://kidnest.runasp.net)

A sample ASP.NET Core MVC e-commerce platform for kids' products, with an admin
dashboard for managing products, categories, orders, contents and users.

## Stack
- ASP.NET Core 8 MVC
- ADO.NET / SQL Server (no ORM/EF Core)
- Bootstrap 5

## Getting started
1. Run `db/DDL.sql` in your SQL Server instance. It creates the `KidNest` database (if it doesn't already exist) and the schema
2. Set the `ConnectionStrings:KidNestDbConnection` value in `appsettings.Development.json` (gitignored, create locally). Check `appsettings.json` for the database connection format
3. `dotnet run --project KidNest.Web`

## Features
- Storefront: browsing, cart, checkout
- Admin area: products/categories/orders/contents/users management with [DataTables](https://datatables.net/)
- OTP-based password reset (requires an active Twilio subscription, which is not currently enabled in this deployment. You can swap in another SMS provider by adjusting the OTP-sending code in `AccountController`)

## Notes
This is a sample/learning project. There's room to make it more scalable and
maintainable (e.g. introducing an ORM, layering improvements, caching, tests).
