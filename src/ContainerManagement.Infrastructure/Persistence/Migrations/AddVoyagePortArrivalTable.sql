-- Migration: AddVoyagePortArrivalTable
-- Date: 2026-03-27
-- Linked to TblVoyagePort via VoyagePortId (1:1 relationship)

CREATE TABLE IF NOT EXISTS "TblVoyagePortArrival" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "VoyagePortId" uuid NOT NULL,
    "InboundVoyage" varchar(50) NULL,
    "OutboundVoyage" varchar(50) NULL,
    "ActualETA" timestamp NULL,
    "ActualETB" timestamp NULL,
    "LastPortId" uuid NULL,
    "NextPortId" uuid NULL,
    "PilotOnBoard" timestamp NULL,
    "CommencedCargoOperation" timestamp NULL,
    "TugsIn" varchar(50) NULL,
    "ArrivalDraftFwdMtr" decimal(10,2) NULL,
    "ArrivalDraftAftMtr" decimal(10,2) NULL,
    "ArrivalDraftMeanMtr" decimal(10,2) NULL,
    "FuelOil" decimal(10,2) NULL,
    "DieselOil" decimal(10,2) NULL,
    "FreshWater" decimal(10,2) NULL,
    "BallastWater" decimal(10,2) NULL,
    "Remarks" varchar(2000) NULL,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "CreatedOn" timestamp NOT NULL,
    "ModifiedOn" timestamp NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "ModifiedBy" uuid NOT NULL,
    CONSTRAINT "PK_TblVoyagePortArrival" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TblVoyagePortArrival_TblVoyagePort" FOREIGN KEY ("VoyagePortId") REFERENCES "TblVoyagePort" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TblVoyagePortArrival_TblPorts_Last" FOREIGN KEY ("LastPortId") REFERENCES "TblPorts" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_TblVoyagePortArrival_TblPorts_Next" FOREIGN KEY ("NextPortId") REFERENCES "TblPorts" ("Id") ON DELETE NO ACTION
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_TblVoyagePortArrival_VoyagePortId" ON "TblVoyagePortArrival" ("VoyagePortId") WHERE "IsDeleted" = false;
