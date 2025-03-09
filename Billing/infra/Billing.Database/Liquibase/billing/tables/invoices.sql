--liquibase formatted sql
--changeset dev_user:"create invoices table"
CREATE TABLE IF NOT EXISTS billing.invoices (
    invoice_id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    status TEXT NOT NULL,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    updated_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    version INTEGER NOT NULL DEFAULT 1
);
