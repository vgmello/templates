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
    
    UPDATE housekeeping.rooms_status
    SET 
        status = CASE 
            WHEN priority IN ('High', 'Critical') THEN 'Maintenance'
            ELSE status
        END,
        updated_date_utc = CASE 
            WHEN priority IN ('High', 'Critical') THEN timezone('utc', now())
            ELSE updated_date_utc
        END,
        version = CASE 
            WHEN priority IN ('High', 'Critical') THEN version + 1
            ELSE version
        END
    WHERE rooms_status.room_id = maintenance_request_create.room_id;
END;