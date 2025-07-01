--liquibase formatted sql
--changeset dev_user:"create rooms_get function" runOnChange:true
CREATE OR REPLACE FUNCTION housekeeping.rooms_get(
    IN status_filter varchar(50) DEFAULT NULL,
    IN floor_filter integer DEFAULT NULL,
    IN assigned_cleaner_id_filter uuid DEFAULT NULL,
    IN page_number integer DEFAULT 1,
    IN page_size integer DEFAULT 50
)
RETURNS TABLE(
    room_id uuid,
    room_number varchar(50),
    floor integer,
    status varchar(50),
    last_cleaned_date_utc timestamp with time zone,
    assigned_cleaner_id uuid,
    mini_fridge_items jsonb,
    notes text
)
LANGUAGE SQL
BEGIN ATOMIC
    SELECT rs.room_id, rs.room_number, rs.floor, rs.status, rs.last_cleaned_date_utc,
           rs.assigned_cleaner_id, rs.mini_fridge_items, rs.notes
    FROM housekeeping.rooms_status rs
    WHERE (status_filter IS NULL OR rs.status = status_filter)
      AND (floor_filter IS NULL OR rs.floor = floor_filter)
      AND (assigned_cleaner_id_filter IS NULL OR rs.assigned_cleaner_id = assigned_cleaner_id_filter)
    ORDER BY rs.room_number
    LIMIT page_size
    OFFSET (page_number - 1) * page_size;
END;