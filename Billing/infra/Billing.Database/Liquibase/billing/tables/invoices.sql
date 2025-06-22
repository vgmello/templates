--liquibase formatted sql
--changeset dev_user:"create invoices table"
CREATE TABLE IF NOT EXISTS billing.invoices (
    invoice_id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    status TEXT NOT NULL,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    updated_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    version INTEGER NOT NULL DEFAULT 1,
    amount DECIMAL(18,2),
    currency VARCHAR(3),
    due_date TIMESTAMP WITH TIME ZONE,
    cashier_id UUID
);

--changeset dev_user:"add amount and other fields to invoices table"
ALTER TABLE billing.invoices
    ADD COLUMN IF NOT EXISTS amount DECIMAL(18,2),
    ADD COLUMN IF NOT EXISTS currency VARCHAR(3),
    ADD COLUMN IF NOT EXISTS due_date TIMESTAMP WITH TIME ZONE,
    ADD COLUMN IF NOT EXISTS cashier_id UUID;
