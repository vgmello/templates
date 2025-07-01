--liquibase formatted sql
--changeset dev_user:"create rooms_get procedure"
CREATE OR REPLACE PROCEDURE housekeeping.rooms_get(
    status_filter VARCHAR(50) DEFAULT NULL,
    floor_filter INTEGER DEFAULT NULL,
    assigned_cleaner_id_filter UUID DEFAULT NULL,
    page_number INTEGER DEFAULT 1,
    page_size INTEGER DEFAULT 50
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_rooms JSON;
BEGIN
    v_offset := (page_number - 1) * page_size;

    SELECT json_agg(json_build_object(
        'RoomId', room_id,
        'RoomNumber', room_number,
        'Floor', floor,
        'Status', status,
        'LastCleanedDateUtc', last_cleaned_date_utc,
        'AssignedCleanerId', assigned_cleaner_id,
        'MiniFridgeItems', mini_fridge_items,
        'Notes', notes
    ))
    INTO v_rooms
    FROM (
        SELECT 
            room_id,
            room_number,
            floor,
            status,
            last_cleaned_date_utc,
            assigned_cleaner_id,
            mini_fridge_items,
            notes
        FROM housekeeping.rooms_status
        WHERE (status_filter IS NULL OR status = status_filter)
          AND (floor_filter IS NULL OR floor = floor_filter)
          AND (assigned_cleaner_id_filter IS NULL OR assigned_cleaner_id = assigned_cleaner_id_filter)
        ORDER BY room_number
        LIMIT page_size
        OFFSET v_offset
    ) AS filtered_rooms;

    RAISE NOTICE '%', COALESCE(v_rooms, '[]'::json);
END;
$$;