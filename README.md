# URL Shortener

Coursework project demonstrating a URL-shortening service architecture with a .NET API gateway and services, plus a Vue client.

## Components

- `ApiGateway` — Ocelot gateway.
- `UserService` — authentication and user API.
- `ShortenerService` — URL creation and redirect API.
- `vue-project` — Vue 3 / Vite client.
- PostgreSQL, Redis, RabbitMQ, and Docker Compose support.

## Status and local configuration

This is a coursework project; production readiness has not been verified. The Railway application and API health URLs previously listed here both returned HTTP 404 when checked on 2026-09-27, so no live deployment is claimed.

`docker compose config --quiet` completed on 2026-09-27 but warned that `JWT_SECRET` was unset and defaulted to blank. Set a local secret before running the stack. This audit did not start containers or validate the application end to end.
