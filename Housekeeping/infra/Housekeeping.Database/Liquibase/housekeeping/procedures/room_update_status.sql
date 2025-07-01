--liquibase formatted sql
--changeset dev_user:"create room_update_status procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.room_update_status(
    IN room_id uuid,
    IN new_status varchar(50),
    IN notes text DEFAULT NULL,
    IN updated_by uuid DEFAULT NULL
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE housekeeping.rooms_status
    SET 
        status = new_status,
        notes = COALESCE(room_update_status.notes, rooms_status.notes),
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE rooms_status.room_id = room_update_status.room_id;
END;