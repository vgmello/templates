--liquibase formatted sql

--changeset dev_user:"create create_invoice procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.create_invoice(
    IN invoice_id uuid,
    IN name varchar(100),
    IN status text,
    IN amount decimal(18,2),
    IN currency varchar(3),
    IN due_date timestamp with time zone,
    IN cashier_id uuid
)
LANGUAGE SQL
BEGIN ATOMIC
    INSERT INTO billing.invoices(invoice_id, name, status, amount, currency, due_date, cashier_id)
    VALUES (create_invoice.invoice_id, create_invoice.name, create_invoice.status, 
            create_invoice.amount, create_invoice.currency, create_invoice.due_date, create_invoice.cashier_id);
END;