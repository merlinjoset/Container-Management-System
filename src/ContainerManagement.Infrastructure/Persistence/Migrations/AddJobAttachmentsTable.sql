-- Migration: AddJobAttachmentsTable
-- Date: 2026-03-27

CREATE TABLE IF NOT EXISTS "TblJobAttachment" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "JobId" uuid NOT NULL,
    "FileName" varchar(500) NOT NULL,
    "StoredFileName" varchar(500) NOT NULL,
    "ContentType" varchar(200) NOT NULL,
    "FileSize" bigint NOT NULL DEFAULT 0,
    "IsScreenshot" boolean NOT NULL DEFAULT false,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "CreatedOn" timestamp NOT NULL,
    "ModifiedOn" timestamp NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "ModifiedBy" uuid NOT NULL,
    CONSTRAINT "PK_TblJobAttachment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TblJobAttachment_TblJob" FOREIGN KEY ("JobId") REFERENCES "TblJob" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_TblJobAttachment_JobId" ON "TblJobAttachment" ("JobId") WHERE "IsDeleted" = false;
