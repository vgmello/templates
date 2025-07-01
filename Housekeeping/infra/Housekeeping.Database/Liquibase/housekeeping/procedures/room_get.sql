--liquibase formatted sql
--changeset dev_user:"create room_get procedure"
CREATE OR REPLACE PROCEDURE housekeeping.room_get(
    room_id UUID
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_result RECORD;
BEGIN
    SELECT 
        room_id,
        room_number,
        floor,
        status,
        last_cleaned_date_utc,
        assigned_cleaner_id,
        mini_fridge_items,
        notes,
        created_date_utc,
        updated_date_utc,
        version
    INTO v_result
    FROM housekeeping.rooms_status
    WHERE rooms_status.room_id = room_get.room_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Room not found: %', room_get.room_id;
    END IF;

    -- Return result as JSON
    RAISE NOTICE '%', json_build_object(
        'RoomId', v_result.room_id,
        'RoomNumber', v_result.room_number,
        'Floor', v_result.floor,
        'Status', v_result.status,
        'LastCleanedDateUtc', v_result.last_cleaned_date_utc,
        'AssignedCleanerId', v_result.assigned_cleaner_id,
        'MiniFridgeItems', v_result.mini_fridge_items,
        'Notes', v_result.notes,
        'CreatedDateUtc', v_result.created_date_utc,
        'UpdatedDateUtc', v_result.updated_date_utc,
        'Version', v_result.version
    );
END;
$$;