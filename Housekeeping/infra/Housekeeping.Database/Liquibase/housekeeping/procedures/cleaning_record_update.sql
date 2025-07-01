--liquibase formatted sql
--changeset dev_user:"create cleaning_record_update procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.cleaning_record_update(
    IN cleaning_id uuid,
    IN end_time_utc timestamp with time zone,
    IN status varchar(50),
    IN notes text DEFAULT NULL
)
LANGUAGE SQL
BEGIN ATOMIC
    UPDATE housekeeping.cleaning_history
    SET 
        end_time_utc = cleaning_record_update.end_time_utc,
        status = cleaning_record_update.status,
        notes = COALESCE(cleaning_record_update.notes, cleaning_history.notes)
    WHERE cleaning_history.cleaning_id = cleaning_record_update.cleaning_id;
END;