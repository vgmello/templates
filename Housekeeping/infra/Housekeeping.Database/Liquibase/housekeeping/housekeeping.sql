--liquibase formatted sql

--changeset dev_user:"create database" runInTransaction:false context:@setup
CREATE DATABASE housekeeping;

--changeset dev_user:"create housekeeping schema"
CREATE SCHEMA IF NOT EXISTS housekeeping;
