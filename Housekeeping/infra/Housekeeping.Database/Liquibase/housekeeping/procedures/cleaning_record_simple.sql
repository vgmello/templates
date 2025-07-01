--liquibase formatted sql
--changeset dev_user:"create cleaning_record_simple procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.cleaning_record_simple(
    IN cleaning_id uuid,
    IN room_id uuid,
    IN cleaner_id uuid,
    IN start_time_utc timestamp with time zone,
    IN end_time_utc timestamp with time zone,
    IN status varchar(50),
    IN notes text DEFAULT NULL
)
LANGUAGE SQL
BEGIN ATOMIC
    INSERT INTO housekeeping.cleaning_history (
        cleaning_id,
        room_id,
        cleaner_id,
        start_time_utc,
        end_time_utc,
        status,
        notes
    ) 
    VALUES (
        cleaning_id,
        room_id,
        cleaner_id,
        start_time_utc,
        end_time_utc,
        status,
        notes
    );
END;