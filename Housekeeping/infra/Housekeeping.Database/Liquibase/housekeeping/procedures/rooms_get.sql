--liquibase formatted sql
--changeset dev_user:"create rooms_get procedure"
CREATE OR REPLACE PROCEDURE housekeeping.rooms_get(
    p_status VARCHAR(50) DEFAULT NULL,
    p_floor INTEGER DEFAULT NULL,
    p_assigned_cleaner_id UUID DEFAULT NULL,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 50
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_rooms JSON;
BEGIN
    v_offset := (p_page_number - 1) * p_page_size;

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
        WHERE (p_status IS NULL OR status = p_status)
          AND (p_floor IS NULL OR floor = p_floor)
          AND (p_assigned_cleaner_id IS NULL OR assigned_cleaner_id = p_assigned_cleaner_id)
        ORDER BY room_number
        LIMIT p_page_size
        OFFSET v_offset
    ) AS filtered_rooms;

    RAISE NOTICE '%', COALESCE(v_rooms, '[]'::json);
END;
$$;