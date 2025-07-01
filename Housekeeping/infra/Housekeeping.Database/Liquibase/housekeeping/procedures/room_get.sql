--liquibase formatted sql
--changeset dev_user:"create room_get function" runOnChange:true
CREATE OR REPLACE FUNCTION housekeeping.room_get(
    IN room_id uuid
)
RETURNS TABLE(
    room_id uuid,
    room_number varchar(50),
    floor integer,
    status varchar(50),
    last_cleaned_date_utc timestamp with time zone,
    assigned_cleaner_id uuid,
    mini_fridge_items jsonb,
    notes text,
    created_date_utc timestamp with time zone,
    updated_date_utc timestamp with time zone,
    version integer
)
LANGUAGE SQL
BEGIN ATOMIC
    SELECT rs.room_id, rs.room_number, rs.floor, rs.status, rs.last_cleaned_date_utc,
           rs.assigned_cleaner_id, rs.mini_fridge_items, rs.notes,
           rs.created_date_utc, rs.updated_date_utc, rs.version
    FROM housekeeping.rooms_status rs
    WHERE rs.room_id = room_get.room_id;
END;