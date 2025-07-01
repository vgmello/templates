--liquibase formatted sql
--changeset dev_user:"create cleaning_record procedure" runOnChange:true
CREATE OR REPLACE PROCEDURE housekeeping.cleaning_record(
    IN cleaning_id uuid,
    IN room_id uuid,
    IN cleaner_id uuid,
    IN is_complete boolean,
    IN notes text DEFAULT NULL
)
LANGUAGE plpgsql
AS $body$
BEGIN
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
        CASE WHEN is_complete THEN timezone('utc', now()) - INTERVAL '1 hour' ELSE timezone('utc', now()) END,
        CASE WHEN is_complete THEN timezone('utc', now()) ELSE NULL END,
        CASE WHEN is_complete THEN 'Completed' ELSE 'InProgress' END,
        notes
    )
    ON CONFLICT (cleaning_id) DO UPDATE
    SET 
        end_time_utc = CASE WHEN is_complete THEN timezone('utc', now()) ELSE cleaning_history.end_time_utc END,
        status = CASE WHEN is_complete THEN 'Completed' ELSE 'InProgress' END,
        notes = COALESCE(cleaning_record.notes, cleaning_history.notes);
        
    UPDATE housekeeping.rooms_status
    SET 
        status = CASE WHEN is_complete THEN 'Clean' ELSE 'Cleaning' END,
        last_cleaned_date_utc = CASE WHEN is_complete THEN timezone('utc', now()) ELSE last_cleaned_date_utc END,
        assigned_cleaner_id = cleaner_id,
        updated_date_utc = timezone('utc', now()),
        version = version + 1
    WHERE rooms_status.room_id = cleaning_record.room_id;
END;
$body$;