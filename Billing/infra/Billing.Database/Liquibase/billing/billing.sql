--liquibase formatted sql

--changeset dev_user:"create database" runInTransaction:false context:@setup
CREATE DATABASE billing;

--changeset dev_user:"create billing schema"
CREATE SCHEMA IF NOT EXISTS billing;
