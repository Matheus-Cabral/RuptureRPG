.PHONY: help up down restart logs build migrate test test-unit test-integration clean

# Default target
help:
	@echo ""
	@echo "  Ruptura — Available commands"
	@echo ""
	@echo "  make up              Start all containers (build if needed)"
	@echo "  make down            Stop and remove containers"
	@echo "  make restart         Restart all containers"
	@echo "  make build           Rebuild images without cache"
	@echo "  make logs            Tail logs from all containers"
	@echo "  make logs-api        Tail API logs only"
	@echo "  make migrate         Apply EF Core database migrations"
	@echo "  make test            Run all tests"
	@echo "  make test-unit       Run unit tests only"
	@echo "  make test-int        Run integration tests only"
	@echo "  make clean           Remove containers, volumes, and images"
	@echo ""

up:
	@[ -f .env ] || (cp .env.example .env && echo "Created .env from .env.example — review it before continuing." && exit 1)
	docker compose up -d --build

down:
	docker compose down

restart:
	docker compose restart

build:
	docker compose build --no-cache

logs:
	docker compose logs -f

logs-api:
	docker compose logs -f api

migrate:
	docker compose exec api dotnet ef database update \
		--project src/Ruptura.Infrastructure \
		--startup-project src/Ruptura.API

test:
	dotnet test --logger "console;verbosity=normal"

test-unit:
	dotnet test tests/Ruptura.UnitTests --logger "console;verbosity=normal"

test-int:
	dotnet test tests/Ruptura.IntegrationTests --logger "console;verbosity=normal"

clean:
	docker compose down -v --rmi local
