--liquibase formatted sql

--changeset dev_user:"fix duplicate cashier IDs and ensure uniqueness"
-- This migration fixes any cashiers with null or empty GUIDs
-- and ensures future cashier IDs are unique

-- Update any cashiers with null or all-zero GUIDs to have proper unique GUIDs
UPDATE billing.cashiers 
SET cashier_id = gen_random_uuid(),
    updated_date_utc = timezone('utc', now()),
    version = version + 1
WHERE cashier_id = '00000000-0000-0000-0000-000000000000' OR cashier_id IS NULL;

-- Ensure the cashier_id column has a proper unique constraint
-- (This should already exist from the PRIMARY KEY, but making it explicit)
ALTER TABLE billing.cashiers 
ADD CONSTRAINT IF NOT EXISTS uk_cashiers_cashier_id UNIQUE (cashier_id);

-- Add a check constraint to prevent null or empty GUIDs
ALTER TABLE billing.cashiers 
ADD CONSTRAINT IF NOT EXISTS chk_cashiers_cashier_id_not_empty 
CHECK (cashier_id IS NOT NULL AND cashier_id != '00000000-0000-0000-0000-000000000000');