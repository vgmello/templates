--liquibase formatted sql

--changeset dev_user:"create delete_cashier procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.delete_cashier(
    IN cashier_id uuid
)
LANGUAGE SQL
BEGIN ATOMIC
    DELETE FROM billing.cashiers
    WHERE cashiers.cashier_id = delete_cashier.cashier_id;
END;