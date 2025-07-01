--liquibase formatted sql
--changeset dev_user:"create mini_fridge_update procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.mini_fridge_update(
    IN room_id uuid,
    IN items_json text,
    IN updated_by uuid DEFAULT NULL
)
LANGUAGE plpgsql
AS $body$
BEGIN
    UPDATE housekeeping.rooms_status
    SET 
        mini_fridge_items = items_json::jsonb,
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE rooms_status.room_id = mini_fridge_update.room_id;
END;
$body$;