--liquibase formatted sql
--changeset dev_user:"create rooms_status table"
CREATE TABLE IF NOT EXISTS housekeeping.rooms_status (
    room_id UUID PRIMARY KEY,
    room_number VARCHAR(10) NOT NULL UNIQUE,
    floor INTEGER NOT NULL,
    status VARCHAR(50) NOT NULL,
    last_cleaned_date_utc TIMESTAMP WITH TIME ZONE,
    assigned_cleaner_id UUID,
    mini_fridge_items JSONB DEFAULT '{}',
    notes TEXT,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    updated_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    version INTEGER NOT NULL DEFAULT 1
);

--changeset dev_user:"create indexes on rooms_status"
CREATE INDEX IF NOT EXISTS idx_rooms_status_status ON housekeeping.rooms_status(status);
CREATE INDEX IF NOT EXISTS idx_rooms_status_floor ON housekeeping.rooms_status(floor);
CREATE INDEX IF NOT EXISTS idx_rooms_status_assigned_cleaner ON housekeeping.rooms_status(assigned_cleaner_id);
CREATE INDEX IF NOT EXISTS idx_rooms_status_room_number ON housekeeping.rooms_status(room_number);