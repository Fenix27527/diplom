USE master;
GO
CREATE DATABASE PersonnelRecords;
GO
USE PersonnelRecords;
GO

CREATE TABLE Logins (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Login NVARCHAR(50) NOT NULL UNIQUE,
    FIO NVARCHAR(50) NOT NULL,
    Password NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL
);

CREATE TABLE State (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Division NVARCHAR(100),
    Post NVARCHAR(100),
    NumberOfWorkers INT,
    NumberOfHours FLOAT,
    Salary INT 
);

ALTER TABLE State
ADD CONSTRAINT UQ_State_Division_Post UNIQUE (Division, Post);

CREATE TABLE Workers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FIO NVARCHAR(100),
    DateOfBirth DateTime,
    Division NVARCHAR(100),
    Post NVARCHAR(100),
    INN NVARCHAR(50),
    Address NVARCHAR(MAX),
    DateOfReception DateTime,
    Family NVARCHAR(MAX),
    Education NVARCHAR(MAX),
    Awards NVARCHAR(MAX),
    CONSTRAINT FK_Workers_State 
        FOREIGN KEY (Division, Post) 
        REFERENCES State(Division, Post)
);

CREATE TABLE Vacancy (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Post NVARCHAR(100),       
    Conditions NVARCHAR(MAX)  
);

CREATE TABLE Resumes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FIO NVARCHAR(100),
    Post NVARCHAR(100),      
    VacancyId INT NOT NULL,            
    Link NVARCHAR(MAX) ,       
    CONSTRAINT FK_Resumes_Vacancy 
        FOREIGN KEY (VacancyId) 
        REFERENCES Vacancy(Id) 
        ON DELETE CASCADE              
);




CREATE TABLE HISTORYState (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OriginalId INT NOT NULL,            
    ChangeType NVARCHAR(10) NOT NULL,   
    ChangeDate DATETIME DEFAULT GETDATE(),

    Division NVARCHAR(100),
    Post NVARCHAR(100),
    NumberOfWorkers INT,
    NumberOfHours FLOAT,
    Salary INT 
);

CREATE TABLE HISTORYWorkers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OriginalId INT NOT NULL,            
    ChangeType NVARCHAR(10) NOT NULL,   
    ChangeDate DATETIME DEFAULT GETDATE(),

    Old_FIO NVARCHAR(100),
    Old_DateOfBirth DATETIME,
    Old_Division NVARCHAR(100),
    Old_Post NVARCHAR(100),
    Old_INN NVARCHAR(50),
    Old_Address NVARCHAR(MAX),
    Old_DateOfReception DATETIME,
    Old_Family NVARCHAR(MAX),
    Old_Education NVARCHAR(MAX),
    Old_Awards NVARCHAR(MAX),

    New_FIO NVARCHAR(100),
    New_DateOfBirth DATETIME,
    New_Division NVARCHAR(100),
    New_Post NVARCHAR(100),
    New_INN NVARCHAR(50),
    New_Address NVARCHAR(MAX),
    New_DateOfReception DATETIME,
    New_Family NVARCHAR(MAX),
    New_Education NVARCHAR(MAX),
    New_Awards NVARCHAR(MAX)
);

CREATE TABLE HISTORYVacancy (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OriginalId INT NOT NULL,            
    ChangeType NVARCHAR(10) NOT NULL,   
    ChangeDate DATETIME DEFAULT GETDATE(),

    Post NVARCHAR(100),       
    Conditions NVARCHAR(MAX)  
);

CREATE TABLE HISTORYResumes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OriginalId INT NOT NULL,            
    ChangeType NVARCHAR(10) NOT NULL,   
    ChangeDate DATETIME DEFAULT GETDATE(),

    FIO NVARCHAR(100),
    Post NVARCHAR(100),      
    VacancyId INT NOT NULL,            
    Link NVARCHAR(MAX)              
);
