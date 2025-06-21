--liquibase formatted sql

--changeset dev_user:"create database" runInTransaction:false context:@setup
CREATE DATABASE accounting;

--changeset dev_user:"create accounting schema"
CREATE SCHEMA IF NOT EXISTS accounting;
