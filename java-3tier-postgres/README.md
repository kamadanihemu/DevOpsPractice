# Java 3-tier (Spring Boot + Nginx + Postgres)

## Run on Ubuntu
```bash
cd java-3tier-postgres
cp .env.example .env
docker compose up -d --build
docker compose ps
```

Open: http://localhost:8080

Stop:
```bash
docker compose down
```


## Docker concepts used here
- **Multi-stage builds** (Java & .NET backends): smaller runtime images
- **.env + environment variables** for connection strings
- **Docker network** (compose default network) for service discovery: `db`, `backend`, `frontend`
- **Volumes** for Postgres data persistence
- **Healthchecks** so backend waits for Postgres to become healthy
- **Port mapping** (host:container) for frontend
- **Logs**: view with `docker compose logs -f`

## Useful commands
```bash
docker compose ps
docker compose logs -f
docker compose exec db psql -U appuser -d appdb
docker compose down -v   # WARNING: deletes DB volume/data
```

