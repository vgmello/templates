--liquibase formatted sql
--changeset dev_user:"create ledger_balances table"
CREATE TABLE IF NOT EXISTS accounting.ledger_balances (
    ledger_balance_id UUID NOT NULL DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    ledger_type INTEGER NOT NULL,
    balance_date DATE NOT NULL,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    updated_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    version INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY (client_id, ledger_type, balance_date)
);

--changeset dev_user:"seed ledger_balances"
INSERT INTO accounting.ledger_balances (ledger_balance_id, client_id, ledger_type, balance_date)
VALUES
    (gen_random_uuid(), gen_random_uuid(), 1, CURRENT_DATE),
    (gen_random_uuid(), gen_random_uuid(), 2, CURRENT_DATE);
