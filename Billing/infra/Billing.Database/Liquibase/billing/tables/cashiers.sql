--liquibase formatted sql
--changeset dev_user:"create cashiers table"
CREATE TABLE IF NOT EXISTS billing.cashiers (
    cashier_id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    updated_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    version INTEGER NOT NULL DEFAULT 1
);
