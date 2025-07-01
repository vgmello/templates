--liquibase formatted sql
--changeset dev_user:"create maintenance_request_create procedure"
CREATE OR REPLACE PROCEDURE housekeeping.maintenance_request_create(
    request_id UUID,
    room_id UUID,
    issue_type VARCHAR(100),
    description TEXT,
    priority VARCHAR(20),
    reported_by UUID DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
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
    
    -- Update room status to Maintenance if priority is High or Critical
    IF priority IN ('High', 'Critical') THEN
        UPDATE housekeeping.rooms_status
        SET 
            status = 'Maintenance',
            updated_date_utc = timezone('utc', now()),
            version = version + 1
        WHERE rooms_status.room_id = maintenance_request_create.room_id;
    END IF;
END;
$$;