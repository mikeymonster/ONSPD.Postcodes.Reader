CREATE PROCEDURE [dbo].[Postcode_Upsert] (
  @data [dbo].[PostcodeDataType] READONLY
)
AS
SET NOCOUNT ON;

MERGE INTO [dbo].[Postcode] AS t
USING @data AS s
ON
(
  t.[Postcode] = s.[Postcode]
)
WHEN MATCHED 
	 AND (t.[Latitude] <> s.[Latitude] 
		  OR t.[Longitude] <> s.[Longitude])
THEN UPDATE SET
  t.[Latitude]  = s.[Latitude],
  t.[Longitude] = s.[Longitude]

WHEN NOT MATCHED BY TARGET THEN INSERT
(
  [Postcode],
  [Latitude],
  [Longitude]
)
VALUES
(
  s.[Postcode],
  s.[Latitude],
  s.[Longitude]
);

--TODO: Set geometry column
GO