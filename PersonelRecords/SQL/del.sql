USE master;
GO

-- закрываем все активные соединения с базой
ALTER DATABASE PersonnelRecords SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

DROP DATABASE PersonnelRecords;
GO