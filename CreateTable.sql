Use Master
Go

create database PRSproto
Go

Use PRSproto
Go

Create table [User]
(
ID			int IDENTITY(1,1) PRIMARY KEY,
UserName	varchar(20) NOT NULL,
Password	varchar(10) NOT NULL,
FirstName	nvarchar(20) NOT NULL,
LastName	nvarchar (20) NOT NULL,
PhoneNumber varchar(12) NULL, 
Email		varchar(75),
Reviewer	bit Default 0,
Admin		bit Default 0,
CONSTRAINT uname UNIQUE(UserName)
);

Go

CREATE TABLE Vendor
(
ID			int IDENTITY(1,1) PRIMARY KEY,
Code		varchar(10) NOT NULL,
Name		varchar(255) NOT NULL,
Address		varchar(255) NOT NULL,
City		varchar(255) NOT NULL,
State		varchar(2) NOT NULL,
Zip			varchar(5) NOT NULL,
PhoneNumber	varchar(12) NOT NULL,
Email		varchar(100) NOT NULL,
CONSTRAINT vcode UNIQUE(Code)
);

Go
CREATE TABLE Product
(
ID			int IDENTITY(1,1) PRIMARY KEY,
VendorID	int NOT NULL REFERENCES Vendor(ID),
PartNumber	varchar(50) NOT NULL,
Name		varchar(150) NOT NULL,
Price		decimal(10,2) NOT NULL,
Unit		varchar(255) NULL,
PhotoPath	varchar(255) NULL,
CONSTRAINT vendor_part UNIQUE(VendorID, PartNumber)
);

Go

CREATE TABLE Request
(
ID			int IDENTITY(1,1) PRIMARY KEY,
UserID		int NOT NULL REFERENCES [User](ID),
RequestNumber	varchar(20) NOT NULL,
Description		varchar(100) NOT NULL,
Justification	varchar(225) NOT NULL,
DateNeeded		date NOT NULL,
DeliveryMode	varchar(25) NOT NULL,
Status			varchar(20) NOT NULL default 'NEW',
Total			decimal(10,2) default 0.0,
SubmittedDate	date NOT NULL,
ReasonForRejection	varchar(100) NULL
);

Go

CREATE TABLE LineItem
(
ID			int IDENTITY(1,1) PRIMARY KEY,
RequestID	int REFERENCES Request(ID) NOT NULL,
ProductID	int REFERENCES Product(ID) NOT NULL,
Quantity	int NOT NULL,
CONSTRAINT req_pdt UNIQUE(RequestID, ProductID)
);

Go