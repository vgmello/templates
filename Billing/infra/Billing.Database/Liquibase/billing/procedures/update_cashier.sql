--liquibase formatted sql

--changeset dev_user:"create cashier_update procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.cashier_update(
    IN cashier_id uuid,
    IN name varchar(100),
    IN email varchar(100)
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE billing.cashiers
    SET name = cashier_update.name,
        email = cashier_update.email,
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE cashiers.cashier_id = cashier_update.cashier_id;
END;