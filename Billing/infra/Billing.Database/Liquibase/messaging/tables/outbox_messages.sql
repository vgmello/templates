--liquibase formatted sql
--changeset dev_user:"create outbox_messages table"
CREATE TABLE IF NOT EXISTS messaging.outbox_messages (
    db_event_id SERIAL PRIMARY KEY,
    event_id VARCHAR(255) NOT NULL,
    source VARCHAR(1000) NOT NULL,
    spec_version VARCHAR(20) NOT NULL,
    type VARCHAR(255) NOT NULL,
    data_content_type VARCHAR(255),
    data_schema VARCHAR(1000),
    subject VARCHAR(255),
    time TIMESTAMP WITH TIME ZONE NOT NULL,
    data JSON
);