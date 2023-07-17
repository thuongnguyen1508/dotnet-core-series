# Working with SQL database

There are many type of SQL databases such as: MySql, SQL server, Postgres .v.v.v

What will solve?:

- DB first approach/ Code first approach
- What is neccesary package
- How to connect db by .Net core

# DB first approach => gen entity from exist database

- Install dotnet tool:

```bash
    dotnet tool install --global dotnet-ef
```
    
- Install package Microsoft.EntityFrameworkCore.Design & Npgsql.EntityFrameworkCore.{name of db such as PostgreSQL}

- Run command to gen entities:

```bash
    dotnet ef dbcontext scaffold "connection string" {Npgsql/MySql or ...}.EntityFrameworkCore.{name of db such as PostgreSQL} -o Entities
    dotnet ef dbcontext scaffold "connection string" Npgsql.EntityFrameworkCore.PostgreSQL -o Entities
```

# Code first approach => gen database from code

- Install package Microsoft.EntityFrameworkCore.Design & Npgsql.EntityFrameworkCore.{name of db such as PostgreSQL}
- Run command to create migration and execute ef migrattion:

```bash
    dotnet ef migrations add {name of migration, example: InitialCreate}
    dotnet ef database update
```


# Set up project

Clone project

```bash
git clone https://github.com/thuongnguyen1508/dotnet-core-series.git
```

Base on type of database => set up database local with docker

```bash

git clone https://github.com/thuongnguyen1508/dotnet-core-series.git
cd src/series-docker-compose

```

Run script to start db

```bash

./deploy-pgadmin.sh
./deploy-mysql.sh

```


# Reference

- [Net-6-connect-to-postgresql-database-with-entity-framework-core](https://jasonwatmore.com/post/2022/06/23/net-6-connect-to-postgresql-database-with-entity-framework-core)

- [.Net Cli](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

- [.Net Cli manage migration ](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli)