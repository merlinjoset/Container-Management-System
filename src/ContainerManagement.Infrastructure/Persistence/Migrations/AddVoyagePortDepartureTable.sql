-- Migration: AddVoyagePortDepartureTable
-- Date: 2026-03-27

CREATE TABLE IF NOT EXISTS "TblVoyagePortDeparture" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "VoyagePortId" uuid NOT NULL,
    "InboundVoyage" varchar(50) NULL,
    "OutboundVoyage" varchar(50) NULL,
    "CompleteCargoOperation" timestamp NULL,
    "PilotOnBoard" timestamp NULL,
    "UnberthFAOP" timestamp NULL,
    "ActualETD" timestamp NULL,
    "NextPortId" uuid NULL,
    "ETANextPort" timestamp NULL,
    "TugsOut" varchar(50) NULL,
    "DepDraftFwdMtr" decimal(10,2) NULL,
    "DepDraftAftMtr" decimal(10,2) NULL,
    "DepDraftMeanMtr" decimal(10,2) NULL,
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
    CONSTRAINT "PK_TblVoyagePortDeparture" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TblVoyagePortDeparture_TblVoyagePort" FOREIGN KEY ("VoyagePortId") REFERENCES "TblVoyagePort" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TblVoyagePortDeparture_TblPorts_Next" FOREIGN KEY ("NextPortId") REFERENCES "TblPorts" ("Id") ON DELETE NO ACTION
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_TblVoyagePortDeparture_VoyagePortId" ON "TblVoyagePortDeparture" ("VoyagePortId") WHERE "IsDeleted" = false;
