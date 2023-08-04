# Working with Document database
Mongo database

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

./deploy-mongo.sh

```

Now, ChatDbContext is setup as EF core. See extension to understand the way it set up.


# Reference

- [.Net core - mongo db - repository - unit of work pattern](https://github.com/brunobritodev/MongoDB-RepositoryUoWPatterns)