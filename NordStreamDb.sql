CREATE DATABASE NordStreamDb;
GO
USE NordStreamDb;
GO

CREATE TABLE Pipeline
(
	FirstId int NOT NULL PRIMARY KEY,
	Name nvarchar(50),
	LocationId nvarchar(50)
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
	Id int NOT NULL PRIMARY KEY,
	Name nvarchar(50),
	Country nvarchar(50)
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
