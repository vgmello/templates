--liquibase formatted sql
--changeset dev_user:"create billing schema"
CREATE SCHEMA IF NOT EXISTS billing;