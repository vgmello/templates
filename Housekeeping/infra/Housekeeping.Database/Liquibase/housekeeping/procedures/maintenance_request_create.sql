--liquibase formatted sql
--changeset dev_user:"create maintenance_request_create procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.maintenance_request_create(
    IN request_id uuid,
    IN room_id uuid,
    IN issue_type varchar(100),
    IN description text,
    IN priority varchar(20),
    IN reported_by uuid DEFAULT NULL
)
LANGUAGE plpgsql
AS $body$
BEGIN
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
    
    IF priority IN ('High', 'Critical') THEN
        UPDATE housekeeping.rooms_status
        SET 
            status = 'Maintenance',
            updated_date_utc = timezone('utc', now()),
            version = version + 1
        WHERE rooms_status.room_id = maintenance_request_create.room_id;
    END IF;
END;
$body$;