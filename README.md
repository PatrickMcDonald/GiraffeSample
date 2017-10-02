# Giraffe Sample

https://blogs.msdn.microsoft.com/dotnet/2017/09/26/build-a-web-service-with-f-and-net-core-2-0/

## Getting started

Clone this repo

Create a new SQLite database, and run the following script in it:

```
CREATE TABLE IF NOT EXISTS "LunchSpots" (
  "Id" integer PRIMARY KEY AUTOINCREMENT NOT NULL,
  "Name" nvarchar(128) NOT NULL,
  "Latitude" double(128) NOT NULL,
  "Longitude" double(128) NOT NULL,
  "Cuisine" nvarchar(128) NOT NULL,
  "VegetarianOptions" integer(128) NOT NULL,
  "VeganOptions" integer(128) NOT NULL
);
```

Run the following from a command prompt:

`dotnet restore`

`dotnet watch run`
