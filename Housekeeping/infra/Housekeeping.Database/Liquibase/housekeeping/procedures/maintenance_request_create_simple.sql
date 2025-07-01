--liquibase formatted sql
--changeset dev_user:"create maintenance_request_create_simple procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.maintenance_request_create_simple(
    IN request_id uuid,
    IN room_id uuid,
    IN issue_type varchar(100),
    IN description text,
    IN priority varchar(20),
    IN reported_by uuid DEFAULT NULL
)
LANGUAGE SQL
BEGIN ATOMIC
    INSERT INTO housekeeping.maintenance_requests (
        request_id,
        room_id,
        issue_type,
        description,
        priority,
        status,
        reported_by
    ) 
    VALUES (
        request_id,
        room_id,
        issue_type,
        description,
        priority,
        'Pending',
        reported_by
    );
END;