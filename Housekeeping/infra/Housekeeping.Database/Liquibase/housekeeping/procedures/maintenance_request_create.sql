--liquibase formatted sql
--changeset dev_user:"create maintenance_request_create procedure"
CREATE OR REPLACE PROCEDURE housekeeping.maintenance_request_create(
    p_request_id UUID,
    p_room_id UUID,
    p_issue_type VARCHAR(100),
    p_description TEXT,
    p_priority VARCHAR(20),
    p_reported_by UUID DEFAULT NULL
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
        p_request_id,
        p_room_id,
        p_issue_type,
        p_description,
        p_priority,
        'Pending',
        p_reported_by
    );
    
    -- Update room status to Maintenance if priority is High or Critical
    IF p_priority IN ('High', 'Critical') THEN
        UPDATE housekeeping.rooms_status
        SET 
            status = 'Maintenance',
            updated_date_utc = timezone('utc', now()),
            version = version + 1
        WHERE room_id = p_room_id;
    END IF;
END;
$$;