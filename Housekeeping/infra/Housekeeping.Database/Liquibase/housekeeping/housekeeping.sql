--liquibase formatted sql
--changeset dev_user:"create housekeeping schema"
CREATE SCHEMA IF NOT EXISTS housekeeping;