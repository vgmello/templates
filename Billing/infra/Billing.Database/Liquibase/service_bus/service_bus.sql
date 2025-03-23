--liquibase formatted sql

--changeset dev_user:"create database" runInTransaction:false
CREATE DATABASE service_bus;

--changeset dev_user:"create billing schema"
CREATE SCHEMA IF NOT EXISTS queues;
