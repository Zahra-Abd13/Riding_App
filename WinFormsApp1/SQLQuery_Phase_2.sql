CREATE TABLE Users(UserID int IDENTITY(1,1) PRIMARY KEY ,FirstName varchar(50) , LastName varchar(50) , Email varchar(100) , DateOfBirth date);

CREATE TABLE UsersPhones(UserID int  ,Phone_Number varchar(20), PRIMARY KEY (UserID,Phone_Number), FOREIGN KEY (UserID) REFERENCES Users(UserID));

CREATE TABLE Drivers(DriverID int IDENTITY(1,1) PRIMARY KEY  ,DriverName varchar(100),LicenseNumber varchar(50),PhoneNumber varchar(50));

CREATE TABLE Vehicles(VehicleID int IDENTITY(1,1) PRIMARY KEY  ,PlateNumber varchar(20),CarModel varchar(50));

CREATE TABLE Owns(VehicleID int ,DriverID int,PRIMARY KEY (DriverID,VehicleID), FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID), FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID));

CREATE TABLE Locations (LocationID int PRIMARY KEY IDENTITY(1,1), LocAddress varchar(255),Latitude decimal(9,6),Longitude decimal(9,6));

CREATE TABLE Rides(RideID int IDENTITY(1,1) PRIMARY KEY ,UserID int,DriverID int,PickupLocationID int,DropoffLocationID int,RideDate date,Fare decimal(10,2),RideDuration int
					FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID), 
					FOREIGN KEY (UserID) REFERENCES Users(UserID),
					FOREIGN KEY (PickUpLocationID) REFERENCES Locations(LocationID),
					FOREIGN KEY (DropOffLocationID) REFERENCES Locations(LocationID));

CREATE TABLE Payments (PaymentID int PRIMARY KEY IDENTITY(1,1), RideID int UNIQUE, Amount decimal(10,2), PaymentMethod varchar(50),PaymentDate date,
						FOREIGN KEY (RideID) REFERENCES Rides(RideID));

CREATE TABLE Reviews (RideID int PRIMARY KEY,Rating int,Comment varchar(255),FOREIGN KEY (RideID) REFERENCES Rides(RideID));
----------------INSERT INTO statements---------------------
INSERT INTO Users (FirstName, LastName, Email, DateOfBirth)
VALUES
('Ahmed',   'Ali',     'ahmed@gmail.com',   '2000-05-10'),
('Sara',    'Hassan',  'sara@gmail.com',    '1999-08-15'),
('Omar',    'Khaled',  'omar@gmail.com',    '2001-02-20'),
('Nour',    'Samir',   'nour@gmail.com',    '1998-03-12'),
('Youssef', 'Fathy',   'youssef@gmail.com', '2002-07-25'),
('Layla',   'Magdy',   'layla@gmail.com',   '1995-11-30');

INSERT INTO UsersPhones (UserID, Phone_Number)
VALUES
(1, '01011111111'),
(2, '01022222222'),
(3, '01033333333'),
(4, '01044444444'),
(5, '01055555555'),
(6, '01066666666'),
(1, '01011111199'), -- User 1 has 2 phones
(4, '01044444499'); -- User 4 has 2 phones

INSERT INTO Drivers (DriverName, PhoneNumber, LicenseNumber)
VALUES
('Mostafa', '01013154411', 'LIC001'),
('Hany',    '01028249320', 'LIC002'),
('Karim',   '01035555533', 'LIC003'),
('Tarek',   '01044441111', 'LIC004'),
('Bassem',  '01055552222', 'LIC005');

INSERT INTO Vehicles (CarModel, PlateNumber)
VALUES
('Toyota Corolla',  'ABC123'),
('Honda Civic',     'XYZ789'),
('Hyundai Elantra', 'LMN456'),
('Kia Sportage',    'QRS111'),
('Nissan Sunny',    'TUV222');

INSERT INTO Owns (DriverID, VehicleID)
VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 4),
(5, 5);

INSERT INTO Locations (LocAddress, Latitude, Longitude)
VALUES
('Nasr City',   30.0500, 31.3500),
('Maadi',       29.9600, 31.2600),
('Heliopolis',  30.1000, 31.3300),
('Zamalek',     30.0600, 31.2200),
('New Cairo',   30.0300, 31.4700),
('6th October', 29.9400, 30.9200);

INSERT INTO Rides (UserID, DriverID, PickupLocationID, DropoffLocationID, RideDate, Fare, RideDuration)
VALUES
(1, 1, 1, 2, '2025-01-10', 100.50, 30),  -- RideID 1
(2, 2, 2, 3, '2025-01-15', 75.00,  20),  -- RideID 2
(3, 3, 3, 4, '2025-02-01', 120.75, 40),  -- RideID 3
(4, 4, 4, 5, '2025-02-14', 90.00,  25),  -- RideID 4
(5, 5, 5, 6, '2025-03-05', 60.50,  15),  -- RideID 5
(6, 1, 6, 1, '2025-03-20', 110.00, 35),  -- RideID 6
(1, 2, 1, 3, '2025-04-01', 85.00,  22),  -- RideID 7  (User 1 second ride)
(2, 3, 2, 4, '2025-04-10', 95.25,  28),  -- RideID 8  (User 2 second ride)
(4, 5, 3, 5, '2025-04-18', 70.00,  18),  -- RideID 9  (User 4 second ride)
(3, 4, 1, 6, '2025-05-01', 130.00, 45);  -- RideID 10 (User 3 second ride)

INSERT INTO Payments (RideID, Amount, PaymentMethod, PaymentDate)
VALUES
(1,  100.50, 'Cash',   '2025-01-10'),
(2,  75.00,  'Card',   '2025-01-15'),
(3,  120.75, 'Wallet', '2025-02-01'),
(4,  90.00,  'Cash',   '2025-02-14'),
(5,  60.50,  'Card',   '2025-03-05'),
(6,  110.00, 'Wallet', '2025-03-20'),
(7,  85.00,  'Cash',   '2025-04-01'),
(8,  95.25,  'Card',   '2025-04-10');
-- RideID 9 and 10 have NO payment (tests rides without payments)

INSERT INTO Reviews (RideID, Rating, Comment)
VALUES
(1,  5, 'Excellent ride, very smooth'),
(2,  4, 'Good service'),
(3,  3, 'Average experience'),
(4,  5, 'Driver was very polite'),
(6,  2, 'Driver was late'),
(7,  4, 'Comfortable and clean car'),
(10, 1, 'Very bad experience');
----------------- for checking -----------------
select * from Drivers ;   
select * from Users ;
select * from UsersPhones ;
select * from Vehicles ;   
select * from Rides ;   
select * from Owns ;   
select * from Payments ;   
select * from Reviews ;   
-----------------Clearing to reset the data---------------
DELETE FROM Reviews;
DELETE FROM Payments;
DELETE FROM Rides;
DELETE FROM Owns;
DELETE FROM UsersPhones;
DELETE FROM Vehicles;
DELETE FROM Locations;
DELETE FROM Drivers;
DELETE FROM Users;

DBCC CHECKIDENT ('Reviews',   RESEED, 0);
DBCC CHECKIDENT ('Payments',  RESEED, 0);
DBCC CHECKIDENT ('Rides',     RESEED, 0);
DBCC CHECKIDENT ('Vehicles',  RESEED, 0);
DBCC CHECKIDENT ('Locations', RESEED, 0);
DBCC CHECKIDENT ('Drivers',   RESEED, 0);
DBCC CHECKIDENT ('Users',     RESEED, 0);