dotnet ef migrations script -i -o MigrationsRunner/scripts/catalog.sql -p CatalogAPI/CatalogAPI.csproj
dotnet ef migrations script -i -o MigrationsRunner/scripts/customers.sql -p CustomersAPI/CustomersAPI.csproj
dotnet ef migrations script -i -o MigrationsRunner/scripts/documents.sql -p DocumentsAPI/DocumentsAPI.csproj
dotnet ef migrations script -i -o MigrationsRunner/scripts/orders.sql -p OrdersAPI/OrdersAPI.csproj