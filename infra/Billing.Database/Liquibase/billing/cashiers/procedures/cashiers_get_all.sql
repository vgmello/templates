--liquibase formatted sql

--changeset dev_user:"create cashiers_get_all function" runOnChange:true splitStatements:false
CREATE OR REPLACE FUNCTION billing.cashiers_get_all(
    IN p_tenant_id uuid,
    IN p_limit integer DEFAULT 1000,
    IN p_offset integer DEFAULT 0
)
    RETURNS SETOF billing.cashiers
    LANGUAGE SQL
AS $$
    SELECT *
    FROM billing.cashiers c
    WHERE c.tenant_id = p_tenant_id
    ORDER BY c.name
    LIMIT p_limit OFFSET p_offset;
$$;
