CREATE DATABASE NordStreamDb;
GO
USE NordStreamDb;
GO

CREATE TABLE Pipeline
(
	Id INT NOT NULL PRIMARY KEY,
	Company NVARCHAR(50),
	MainLocation NVARCHAR(50)
);
GO

INSERT INTO Pipeline 
VALUES (46, 'GASUN', 'Brazil'), 
(47, 'Yamal', 'Europe'),
(48, 'Trans-Saharan', 'Nigeria'),
(49, 'Druzhba', 'Russia'),
(50, 'Keystone', 'United States'),
(51, 'South North', 'Korea');
GO

CREATE TABLE Society
(
	Id INT NOT NULL PRIMARY KEY,
	Company NVARCHAR(50),
	Country NVARCHAR(50)
);
GO

INSERT INTO Society 
VALUES (79, 'Gazprom', 'Russia'),
(80, 'Total', 'France'),
(81, 'Engie', 'France'),
(82, 'General Electric', 'United States'),
(83, 'Shell', 'United Kingdom'),
(84, 'ExxonMobil', 'United States'),
(85, 'Saudi Aramco', 'Saudi Arabia');
GO

CREATE TABLE Bank
(
	SerialId INT NOT NULL,
	MarketNoId INT NOT NULL,
	Company NVARCHAR(50),
	Market NVARCHAR(50),
	Country NVARCHAR(50),
	CONSTRAINT PK_Bank PRIMARY KEY (SerialId, MarketNoId)
);
GO

INSERT INTO Bank 
VALUES (118, 4958, 'MorganStanley', 'Nasdaq', 'USA'),
(120, 4668, 'SocieteGenerale', 'CAC40', 'France');
GO
