--liquibase formatted sql

--changeset dev_user:"create cashier_delete procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.cashier_delete(
    IN cashier_id uuid
)
LANGUAGE SQL
BEGIN ATOMIC
    DELETE FROM billing.cashiers
    WHERE cashiers.cashier_id = cashier_delete.cashier_id;
END;