--liquibase formatted sql

--changeset dev_user:"create invoices_mark_paid procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.invoices_mark_paid(
    IN invoice_id uuid,
    IN amount_paid decimal(18,2),
    IN payment_date timestamp with time zone
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE billing.invoices
    SET status = 'Paid',
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE invoices.invoice_id = invoices_mark_paid.invoice_id
      AND invoices.status NOT IN ('Paid', 'Cancelled')
      AND invoices.amount <= invoices_mark_paid.amount_paid;
END;