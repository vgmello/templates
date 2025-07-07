--liquibase formatted sql

--changeset dev_user:"create cashiers_delete procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.cashiers_delete(
    IN cashier_id uuid
)
LANGUAGE SQL
BEGIN ATOMIC
    DELETE FROM billing.cashiers
    WHERE cashiers.cashier_id = cashiers_delete.cashier_id;
END;