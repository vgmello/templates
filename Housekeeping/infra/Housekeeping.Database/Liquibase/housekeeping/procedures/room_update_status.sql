--liquibase formatted sql
--changeset dev_user:"create room_update_status procedure"
CREATE OR REPLACE PROCEDURE housekeeping.room_update_status(
    room_id UUID,
    new_status VARCHAR(50),
    notes TEXT DEFAULT NULL,
    updated_by UUID DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_result RECORD;
    v_now TIMESTAMP WITH TIME ZONE := timezone('utc', now());
BEGIN
    UPDATE housekeeping.rooms_status
    SET 
        status = new_status,
        notes = COALESCE(room_update_status.notes, rooms_status.notes),
        updated_date_utc = v_now,
        version = version + 1,
        last_cleaned_date_utc = CASE 
            WHEN new_status IN ('Clean', 'Inspected') THEN v_now 
            ELSE last_cleaned_date_utc 
        END
    WHERE rooms_status.room_id = room_update_status.room_id
    RETURNING 
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
    INTO v_result;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Room not found: %', room_update_status.room_id;
    END IF;

    -- Return updated room as JSON
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