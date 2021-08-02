CREATE TABLE dbo.Postcode(
  Postcode varchar(10) NOT NULL,
  Latitude decimal(9, 6) NOT NULL,
  Longitude decimal(9, 6) NOT NULL
 CONSTRAINT PK_Postcode PRIMARY KEY CLUSTERED 
(
  Postcode ASC
)
) 
GO