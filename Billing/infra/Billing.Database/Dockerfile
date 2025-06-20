FROM liquibase/liquibase:4.24-alpine

USER root

# Install curl for healthchecks
RUN apk add --no-cache curl postgresql-client

# Copy Liquibase configuration files
COPY liquibase.properties /app/liquibase.properties
COPY liquibase.setup.properties /app/liquibase.setup.properties
COPY liquibase.service_bus.properties /app/liquibase.service_bus.properties

# Copy changelog files
COPY Liquibase/ /app/Liquibase/

WORKDIR /app

# Set default user back to liquibase
USER liquibase

ENTRYPOINT ["sh"]