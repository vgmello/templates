--liquibase formatted sql
--changeset dev_user:"create room_status_update_simple procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.room_status_update_simple(
    IN room_id uuid,
    IN new_status varchar(50),
    IN cleaner_id uuid DEFAULT NULL
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE housekeeping.rooms_status
    SET 
        status = new_status,
        assigned_cleaner_id = COALESCE(cleaner_id, assigned_cleaner_id),
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE rooms_status.room_id = room_status_update_simple.room_id;
END;