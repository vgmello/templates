--liquibase formatted sql
--changeset dev_user:"create cleaning_record procedure"
CREATE OR REPLACE PROCEDURE housekeeping.cleaning_record(
    cleaning_id UUID,
    room_id UUID,
    cleaner_id UUID,
    is_complete BOOLEAN,
    notes TEXT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_status VARCHAR(50);
    v_now TIMESTAMP WITH TIME ZONE := timezone('utc', now());
BEGIN
    IF is_complete THEN
        v_status := 'Completed';
        
        -- Update or insert cleaning record
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
            v_now - INTERVAL '1 hour', -- Assume 1 hour cleaning time
            v_now,
            v_status,
            notes
        )
        ON CONFLICT (cleaning_id) DO UPDATE
        SET 
            end_time_utc = v_now,
            status = v_status,
            notes = COALESCE(cleaning_record.notes, cleaning_history.notes);
            
        -- Update room status to Clean
        UPDATE housekeeping.rooms_status
        SET 
            status = 'Clean',
            last_cleaned_date_utc = v_now,
            assigned_cleaner_id = cleaning_record.cleaner_id,
            updated_date_utc = v_now,
            version = version + 1
        WHERE rooms_status.room_id = cleaning_record.room_id;
    ELSE
        v_status := 'InProgress';
        
        -- Insert new cleaning record
        INSERT INTO housekeeping.cleaning_history (
            cleaning_id,
            room_id,
            cleaner_id,
            start_time_utc,
            status,
            notes
        ) 
        VALUES (
            cleaning_id,
            room_id,
            cleaner_id,
            v_now,
            v_status,
            notes
        );
        
        -- Update room status to Cleaning
        UPDATE housekeeping.rooms_status
        SET 
            status = 'Cleaning',
            assigned_cleaner_id = cleaning_record.cleaner_id,
            updated_date_utc = v_now,
            version = version + 1
        WHERE rooms_status.room_id = cleaning_record.room_id;
    END IF;
END;
$$;