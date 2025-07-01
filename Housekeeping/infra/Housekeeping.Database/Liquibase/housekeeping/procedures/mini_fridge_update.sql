--liquibase formatted sql
--changeset dev_user:"create mini_fridge_update procedure"
CREATE OR REPLACE PROCEDURE housekeeping.mini_fridge_update(
    p_room_id UUID,
    p_items_json TEXT,
    p_updated_by UUID DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE housekeeping.rooms_status
    SET 
        mini_fridge_items = p_items_json::jsonb,
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE room_id = p_room_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Room not found: %', p_room_id;
    END IF;
END;
$$;