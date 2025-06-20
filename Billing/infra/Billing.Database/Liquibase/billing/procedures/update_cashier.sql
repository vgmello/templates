--liquibase formatted sql

--changeset dev_user:"create update_cashier procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.update_cashier(
    IN cashier_id uuid,
    IN name varchar(100),
    IN email varchar(100)
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE billing.cashiers
    SET name = update_cashier.name,
        email = update_cashier.email,
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE cashiers.cashier_id = update_cashier.cashier_id;
END;