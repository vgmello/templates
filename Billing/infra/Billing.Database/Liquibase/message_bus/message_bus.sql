--liquibase formatted sql
--changeset dev_user:"create database"
SELECT 'CREATE DATABASE message_bus'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'exp_db')\gexec
