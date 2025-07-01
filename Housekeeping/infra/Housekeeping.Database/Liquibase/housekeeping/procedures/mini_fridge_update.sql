--liquibase formatted sql
--changeset dev_user:"create mini_fridge_update procedure"
CREATE OR REPLACE PROCEDURE housekeeping.mini_fridge_update(
    room_id UUID,
    items_json TEXT,
    updated_by UUID DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE housekeeping.rooms_status
    SET 
        mini_fridge_items = items_json::jsonb,
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE rooms_status.room_id = mini_fridge_update.room_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Room not found: %', mini_fridge_update.room_id;
    END IF;
END;
$$;