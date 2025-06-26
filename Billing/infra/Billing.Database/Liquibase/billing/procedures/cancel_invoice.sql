--liquibase formatted sql

--changeset dev_user:"create invoice_cancel procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.invoice_cancel(
    IN invoice_id uuid
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE billing.invoices
    SET status = 'Cancelled',
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE invoices.invoice_id = invoice_cancel.invoice_id
      AND invoices.status NOT IN ('Paid', 'Cancelled');
END;