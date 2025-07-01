--liquibase formatted sql
--changeset dev_user:"create cleaning_history table"
CREATE TABLE IF NOT EXISTS housekeeping.cleaning_history (
    cleaning_id UUID PRIMARY KEY,
    room_id UUID NOT NULL,
    cleaner_id UUID NOT NULL,
    start_time_utc TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time_utc TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) NOT NULL,
    notes TEXT,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now())
);

--changeset dev_user:"create indexes on cleaning_history"
CREATE INDEX IF NOT EXISTS idx_cleaning_history_room_id ON housekeeping.cleaning_history(room_id);
CREATE INDEX IF NOT EXISTS idx_cleaning_history_cleaner_id ON housekeeping.cleaning_history(cleaner_id);
CREATE INDEX IF NOT EXISTS idx_cleaning_history_start_time ON housekeeping.cleaning_history(start_time_utc);