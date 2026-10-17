-- RaceDay database
-- PROG6212 Part 1
-- Thabiso Khumalo (ST10477675)

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

-- reset database
IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END;
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- organiser table
CREATE TABLE dbo.Organiser (
    Organiser_ID     INT IDENTITY(1,1) PRIMARY KEY,
    Organiser_Name   VARCHAR(100) NOT NULL,
    Email            VARCHAR(150) NOT NULL UNIQUE,
    Phone_Number     VARCHAR(20) NULL,
    Created_At       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

-- participant table
CREATE TABLE dbo.Participant (
    Participant_ID   INT IDENTITY(1,1) PRIMARY KEY,
    First_Name       VARCHAR(60) NOT NULL,
    Last_Name        VARCHAR(60) NOT NULL,
    Date_Of_Birth    DATE NOT NULL,
    Gender           VARCHAR(20) NULL,
    Email            VARCHAR(150) NOT NULL UNIQUE,
    Phone_Number     VARCHAR(20) NULL,
    Created_At       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT CK_Participant_DateOfBirth
        CHECK (Date_Of_Birth >= '1900-01-01'),
    CONSTRAINT CK_Participant_Gender
        CHECK (Gender IS NULL OR Gender IN ('Male', 'Female', 'Other', 'Prefer not to say'))
);
GO

-- event table
CREATE TABLE dbo.Event (
    Event_ID         INT IDENTITY(1,1) PRIMARY KEY,
    Organiser_ID     INT NOT NULL,
    Event_Name       VARCHAR(150) NOT NULL,
    Description      VARCHAR(500) NULL,
    Event_Date       DATE NOT NULL,
    Location         VARCHAR(200) NOT NULL,
    Event_Type       VARCHAR(20) NOT NULL,
    Status           VARCHAR(20) NOT NULL DEFAULT 'Planned',
    Closing_Date     DATE NOT NULL,
    Created_At       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Event_Organiser
        FOREIGN KEY (Organiser_ID) REFERENCES dbo.Organiser(Organiser_ID),
    CONSTRAINT CK_Event_Type
        CHECK (Event_Type IN ('Run', 'Walk', 'Cycle')),
    CONSTRAINT CK_Event_Status
        CHECK (Status IN ('Planned', 'Open', 'Closed', 'Completed', 'Cancelled')),
    CONSTRAINT CK_Event_ClosingDate
        CHECK (Closing_Date <= Event_Date)
);
GO

-- race categories
CREATE TABLE dbo.Race_Category (
    Category_ID          INT IDENTITY(1,1) PRIMARY KEY,
    Event_ID             INT NOT NULL,
    Category_Name        VARCHAR(100) NOT NULL,
    Distance_KM          DECIMAL(6,2) NOT NULL,
    Entry_Fee            DECIMAL(10,2) NOT NULL DEFAULT 0,
    Max_Participants     INT NOT NULL,
    Current_Participants INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_RaceCategory_Event
        FOREIGN KEY (Event_ID) REFERENCES dbo.Event(Event_ID),
    CONSTRAINT UQ_RaceCategory_EventName
        UNIQUE (Event_ID, Category_Name),
    CONSTRAINT UQ_RaceCategory_CategoryEvent
        UNIQUE (Category_ID, Event_ID),
    CONSTRAINT CK_RaceCategory_Distance
        CHECK (Distance_KM > 0),
    CONSTRAINT CK_RaceCategory_EntryFee
        CHECK (Entry_Fee >= 0),
    CONSTRAINT CK_RaceCategory_MaxParticipants
        CHECK (Max_Participants > 0),
    CONSTRAINT CK_RaceCategory_CurrentParticipants
        CHECK (Current_Participants >= 0 AND Current_Participants <= Max_Participants)
);
GO

-- enrolments
CREATE TABLE dbo.Enrolment (
    Enrolment_ID       INT IDENTITY(1,1) PRIMARY KEY,
    Participant_ID     INT NOT NULL,
    Event_ID           INT NOT NULL,
    Category_ID        INT NOT NULL,
    Enrolment_Date     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Race_Number        VARCHAR(20) NOT NULL UNIQUE,
    Enrolment_Status   VARCHAR(20) NOT NULL DEFAULT 'Registered',

    CONSTRAINT FK_Enrolment_Participant
        FOREIGN KEY (Participant_ID) REFERENCES dbo.Participant(Participant_ID),
    CONSTRAINT FK_Enrolment_Event
        FOREIGN KEY (Event_ID) REFERENCES dbo.Event(Event_ID),
    CONSTRAINT FK_Enrolment_CategoryEvent
        FOREIGN KEY (Category_ID, Event_ID)
        REFERENCES dbo.Race_Category(Category_ID, Event_ID),
    CONSTRAINT UQ_Enrolment_ParticipantCategory
        UNIQUE (Participant_ID, Category_ID),
    CONSTRAINT CK_Enrolment_Status
        CHECK (Enrolment_Status IN ('Registered', 'Withdrawn', 'Completed', 'Cancelled'))
);
GO

-- payments
CREATE TABLE dbo.Payment (
    Payment_ID            INT IDENTITY(1,1) PRIMARY KEY,
    Enrolment_ID          INT NOT NULL,
    Amount                DECIMAL(10,2) NOT NULL,
    Payment_Method        VARCHAR(20) NOT NULL,
    Payment_Status        VARCHAR(20) NOT NULL DEFAULT 'Pending',
    Payment_Date          DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Transaction_Reference VARCHAR(80) NULL,

    CONSTRAINT FK_Payment_Enrolment
        FOREIGN KEY (Enrolment_ID) REFERENCES dbo.Enrolment(Enrolment_ID),
    CONSTRAINT UQ_Payment_TransactionReference
        UNIQUE (Transaction_Reference),
    CONSTRAINT CK_Payment_Amount
        CHECK (Amount >= 0),
    CONSTRAINT CK_Payment_Method
        CHECK (Payment_Method IN ('Card', 'EFT', 'Cash', 'Other')),
    CONSTRAINT CK_Payment_Status
        CHECK (Payment_Status IN ('Pending', 'Paid', 'Failed', 'Refunded'))
);
GO

-- only one successful payment per enrolment
CREATE UNIQUE INDEX UX_Payment_OneSuccessfulPayment
ON dbo.Payment (Enrolment_ID)
WHERE Payment_Status = 'Paid';
GO

-- race results
CREATE TABLE dbo.Race_Result (
    Result_ID          INT IDENTITY(1,1) PRIMARY KEY,
    Enrolment_ID       INT NOT NULL UNIQUE,
    Start_Time         TIME NULL,
    Finish_Time        TIME NULL,
    Overall_Position   INT NULL,
    Result_Status      VARCHAR(20) NOT NULL DEFAULT 'Finished',
    Recorded_At        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_RaceResult_Enrolment
        FOREIGN KEY (Enrolment_ID) REFERENCES dbo.Enrolment(Enrolment_ID),
    CONSTRAINT CK_RaceResult_Position
        CHECK (Overall_Position IS NULL OR Overall_Position > 0),
    CONSTRAINT CK_RaceResult_Status
        CHECK (Result_Status IN ('Finished', 'DNS', 'DNF', 'DSQ')),
    CONSTRAINT CK_RaceResult_FinishTime
        CHECK (Finish_Time IS NULL OR Start_Time IS NULL OR Finish_Time >= Start_Time)
);
GO

-- keep participant counts updated
CREATE TRIGGER dbo.trg_Enrolment_UpdateCategoryCount
ON dbo.Enrolment
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH AffectedCategories AS (
        SELECT Category_ID FROM inserted
        UNION
        SELECT Category_ID FROM deleted
    )
    UPDATE rc
    SET Current_Participants = (
        SELECT COUNT(*)
        FROM dbo.Enrolment e
        WHERE e.Category_ID = rc.Category_ID
          AND e.Enrolment_Status = 'Registered'
    )
    FROM dbo.Race_Category rc
    INNER JOIN AffectedCategories ac
        ON ac.Category_ID = rc.Category_ID;
END;
GO

-- check payment amount against entry fee
CREATE TRIGGER dbo.trg_Payment_ValidateAmount
ON dbo.Payment
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted p
        INNER JOIN dbo.Enrolment e ON e.Enrolment_ID = p.Enrolment_ID
        INNER JOIN dbo.Race_Category rc ON rc.Category_ID = e.Category_ID
        WHERE p.Amount <> rc.Entry_Fee
    )
    BEGIN
        RAISERROR ('Payment amount must match the race category entry fee.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

-- sample data
INSERT INTO dbo.Organiser (Organiser_Name, Email, Phone_Number)
VALUES
('Durban Road Runners', 'events@durbanroadrunners.co.za', '0315550101'),
('KZN Cycling Club', 'admin@kzncycling.co.za', '0315550102');
GO

INSERT INTO dbo.Participant
    (First_Name, Last_Name, Date_Of_Birth, Gender, Email, Phone_Number)
VALUES
('Sibusiso', 'Dlamini', '2001-06-14', 'Male', 'sibusiso.dlamini@example.com', '0715551001'),
('Ayanda', 'Mthembu', '2003-11-02', 'Female', 'ayanda.mthembu@example.com', '0725551002');
GO

INSERT INTO dbo.Event
    (Organiser_ID, Event_Name, Description, Event_Date, Location, Event_Type, Status, Closing_Date)
VALUES
(1, 'Durban Beachfront 10K', 'Road running event along the Durban beachfront.', '2026-10-17', 'Durban Beachfront', 'Run', 'Open', '2026-10-10'),
(1, 'Umhlanga Charity Walk', 'Community charity walk in Umhlanga.', '2026-11-07', 'Umhlanga', 'Walk', 'Open', '2026-10-31'),
(2, 'KZN Spring Cycle', 'Road cycling event for recreational and experienced riders.', '2026-11-21', 'Durban', 'Cycle', 'Planned', '2026-11-14');
GO

INSERT INTO dbo.Race_Category
    (Event_ID, Category_Name, Distance_KM, Entry_Fee, Max_Participants)
VALUES
(1, '5 km Fun Run', 5.00, 80.00, 200),
(1, '10 km Open', 10.00, 120.00, 300),
(2, '5 km Walk', 5.00, 60.00, 250),
(2, '10 km Walk', 10.00, 90.00, 180),
(3, '40 km Cycle', 40.00, 150.00, 200),
(3, '80 km Cycle', 80.00, 220.00, 150);
GO

INSERT INTO dbo.Enrolment
    (Participant_ID, Event_ID, Category_ID, Race_Number, Enrolment_Status)
VALUES
(1, 1, 2, 'RD-1001', 'Registered'),
(2, 1, 1, 'RD-1002', 'Registered'),
(1, 2, 3, 'RD-2001', 'Registered');
GO

INSERT INTO dbo.Payment
    (Enrolment_ID, Amount, Payment_Method, Payment_Status, Transaction_Reference)
VALUES
(1, 120.00, 'EFT', 'Paid', 'PAY-RD-001'),
(2, 80.00, 'Card', 'Failed', 'PAY-RD-002A'),
(2, 80.00, 'Card', 'Paid', 'PAY-RD-002B');
GO

INSERT INTO dbo.Race_Result
    (Enrolment_ID, Start_Time, Finish_Time, Overall_Position, Result_Status)
VALUES
(1, '08:00:00', '08:52:34', 12, 'Finished');
GO

-- quick check
SELECT * FROM dbo.Organiser;
SELECT * FROM dbo.Participant;
SELECT * FROM dbo.Event;
SELECT * FROM dbo.Race_Category;
SELECT * FROM dbo.Enrolment;
SELECT * FROM dbo.Payment;
SELECT * FROM dbo.Race_Result;
GO

SELECT
    e.Event_Name,
    rc.Category_Name,
    rc.Distance_KM,
    rc.Current_Participants,
    rc.Max_Participants
FROM dbo.Race_Category rc
INNER JOIN dbo.Event e ON e.Event_ID = rc.Event_ID
ORDER BY e.Event_ID, rc.Distance_KM;
GO
