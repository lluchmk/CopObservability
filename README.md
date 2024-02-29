# CoPObservability

Demo repository for the UST CoP about observabililty in dotnet with OpenTelemetry

## Running the project
There's 3 compose files to help getting the project up and running:

 - docker-compose.infra.yml: infrastucture services for the project (db, message broker, ...)
 - docker-compose.observability.yml: observability services.
 - docker-compose.yml: project services.

### Running only infra and observability services
`docker compose -f .\docker-compose.infra.yml -f .\docker-compose.observability.yml -f .\docker-compose.yml up --build`

This will run all infra and observability services, so you can run the services from you prefered way.

### Running everything
`docker compose -f .\docker-compose.infra.yml -f .\docker-compose.observability.yml -f .\docker-compose.yml up --build`

This will run everything in docker, so no need to run anything outside.

## Exposed endpoints
DocumentsAPI:
 - localhost:5192 when running from dotnet
 - localhost:5000 when running in docker

Grafana: localhost:3000\
Jaeger: localhost:16686