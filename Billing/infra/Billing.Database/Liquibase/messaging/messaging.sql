--liquibase formatted sql
--changeset dev_user:"create messaging schema"
CREATE SCHEMA IF NOT EXISTS messaging;