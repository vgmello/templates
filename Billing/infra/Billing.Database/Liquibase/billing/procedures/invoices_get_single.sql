--liquibase formatted sql

--changeset dev_user:"create invoices_get_single function" runOnChange:true
CREATE OR REPLACE FUNCTION billing.invoices_get_single(
    IN invoice_id uuid
)
RETURNS TABLE(
    invoice_id uuid,
    name varchar(100),
    status text,
    amount decimal(18,2),
    currency varchar(3),
    due_date timestamp with time zone,
    cashier_id uuid,
    created_date_utc timestamp with time zone,
    updated_date_utc timestamp with time zone,
    version integer
)
LANGUAGE SQL
BEGIN ATOMIC
    SELECT i.invoice_id, i.name, i.status, i.amount, i.currency, i.due_date, i.cashier_id,
           i.created_date_utc, i.updated_date_utc, i.version
    FROM billing.invoices i
    WHERE i.invoice_id = invoices_get_single.invoice_id;
END;