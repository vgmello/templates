--liquibase formatted sql
--changeset dev_user:"create ledger_currencies table"
CREATE TABLE IF NOT EXISTS accounting.ledger_currencies (
    ledger_balance_id UUID NOT NULL,
    currency_id UUID NOT NULL,
    effective_date_utc TIMESTAMP WITH TIME ZONE NOT NULL,
    custom_currency_code VARCHAR(10) NOT NULL,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    PRIMARY KEY (ledger_balance_id, currency_id, effective_date_utc)
);
