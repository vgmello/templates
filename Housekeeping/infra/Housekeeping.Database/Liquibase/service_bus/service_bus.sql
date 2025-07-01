--liquibase formatted sql
--changeset dev_user:"create service bus schema"
CREATE SCHEMA IF NOT EXISTS housekeeping;

--changeset dev_user:"grant service bus permissions"
-- Wolverine will create its own tables in the housekeeping schema
GRANT ALL ON SCHEMA housekeeping TO postgres;