--liquibase formatted sql

--changeset dev_user:"create cancel_invoice procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.cancel_invoice(
    IN invoice_id uuid
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE billing.invoices
    SET status = 'Cancelled',
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE invoices.invoice_id = cancel_invoice.invoice_id
      AND invoices.status NOT IN ('Paid', 'Cancelled');
END;