--liquibase formatted sql

--changeset dev_user:"create invoices_create procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.invoices_create(
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
    VALUES (invoices_create.invoice_id, invoices_create.name, invoices_create.status, 
            invoices_create.amount, invoices_create.currency, invoices_create.due_date, invoices_create.cashier_id);
END;