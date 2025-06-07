--liquibase formatted sql
--changeset dev_user:"create create_cashier procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.create_cashier(
    cashier_id uuid,
    name varchar(100),
    email varchar(100)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_number integer;
BEGIN
    INSERT INTO billing.cashiers(cashier_id, name, email)
    VALUES (cashier_id, name, email);

    SELECT COUNT(*) INTO v_number FROM billing.cashiers;
    -- return result as recordset
    SELECT v_number;
END;
$$;
