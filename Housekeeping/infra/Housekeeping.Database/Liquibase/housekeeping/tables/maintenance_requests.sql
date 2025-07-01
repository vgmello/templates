--liquibase formatted sql
--changeset dev_user:"create maintenance_requests table"
CREATE TABLE IF NOT EXISTS housekeeping.maintenance_requests (
    request_id UUID PRIMARY KEY,
    room_id UUID NOT NULL,
    issue_type VARCHAR(100) NOT NULL,
    description TEXT,
    priority VARCHAR(20) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    reported_by UUID,
    assigned_to UUID,
    created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    updated_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
    completed_date_utc TIMESTAMP WITH TIME ZONE
);

--changeset dev_user:"create indexes on maintenance_requests"
CREATE INDEX IF NOT EXISTS idx_maintenance_requests_room_id ON housekeeping.maintenance_requests(room_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_requests_status ON housekeeping.maintenance_requests(status);
CREATE INDEX IF NOT EXISTS idx_maintenance_requests_priority ON housekeeping.maintenance_requests(priority);
CREATE INDEX IF NOT EXISTS idx_maintenance_requests_assigned_to ON housekeeping.maintenance_requests(assigned_to);