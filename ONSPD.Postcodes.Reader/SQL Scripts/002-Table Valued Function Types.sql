CREATE TYPE [dbo].[PostcodeDataType] AS TABLE(
  Postcode varchar(10) NOT NULL,
  Latitude decimal(9, 6) NOT NULL,
  Longitude decimal(9, 6) NOT NULL
)
GO