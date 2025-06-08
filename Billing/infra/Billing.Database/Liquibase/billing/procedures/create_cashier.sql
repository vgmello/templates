--liquibase formatted sql
--changeset dev_user:"create create_cashier function" runOnChange:true
CREATE OR REPLACE FUNCTION billing.create_cashier(
    cashier_id uuid,
    name varchar(100),
    email varchar(100)
)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO billing.cashiers(cashier_id, name, email)
    VALUES (cashier_id, name, email);
END;
$$;
