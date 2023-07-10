1. Install dotnet tool:

    dotnet tool install --global dotnet-ef
2. Install package Microsoft.EntityFrameworkCore.Design & Npgsql.EntityFrameworkCore.PostgreSQL

3. Run command to gen entities:

    dotnet ef dbcontext scaffold "Host=database-1.cvisbvujuezh.ap-southeast-1.rds.amazonaws.com;Port=5432;Database=langgeneral;Username=postgres;Password=t0ps3cr3tt0ps3cr3t" Npgsql.EntityFrameworkCore.PostgreSQL -o Entities


# Working with SQL database

There are many type of SQL databases such as: MySql, SQL server, Postgres .v.v.v

What will solve?:

- DB first approach/ Code first approach
- What is neccesary package
- How to connect db by .Net core

# Run project

Clone project

```bash
git clone https://github.com/nnhutan/langexchange.git
cd langexchange
```

Run on local environment

```bash
# ./langexchange

yarn install
yarn start
```

Run on docker

```bash
# ./langexchange

bin/setup.sh
```

# Reference

- [Net-6-connect-to-postgresql-database-with-entity-framework-core](https://jasonwatmore.com/post/2022/06/23/net-6-connect-to-postgresql-database-with-entity-framework-core)
