--liquibase formatted sql

--changeset dev_user:"create invoices_get function" runOnChange:true
CREATE OR REPLACE FUNCTION billing.invoices_get(
    IN "limit" integer DEFAULT 50,
    IN "offset" integer DEFAULT 0,
    IN status text DEFAULT NULL
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
    WHERE (invoices_get.status IS NULL OR i.status = invoices_get.status)
    ORDER BY i.created_date_utc DESC
    LIMIT "limit" OFFSET "offset";
END;
