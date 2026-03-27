-- Migration: AddJobsTable
-- Date: 2026-03-27

CREATE TABLE IF NOT EXISTS "TblJob" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "Title" varchar(300) NOT NULL,
    "Description" varchar(2000) NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "Tag" varchar(100) NULL,
    "TagColor" varchar(30) NULL,
    "CompletedDate" timestamp NULL,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "CreatedOn" timestamp NOT NULL,
    "ModifiedOn" timestamp NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "ModifiedBy" uuid NOT NULL,
    CONSTRAINT "PK_TblJob" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_TblJob_Status" ON "TblJob" ("Status") WHERE "IsDeleted" = false;
