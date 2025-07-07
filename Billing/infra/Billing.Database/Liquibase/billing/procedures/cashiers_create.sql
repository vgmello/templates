--liquibase formatted sql

--changeset dev_user:"create cashiers_create procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE billing.cashiers_create(
    IN cashier_id uuid,
    IN name varchar(100),
    IN email varchar(100)
)
LANGUAGE SQL
BEGIN ATOMIC
    INSERT INTO billing.cashiers(cashier_id, name, email)
    VALUES (cashier_id, name, email);
END;
