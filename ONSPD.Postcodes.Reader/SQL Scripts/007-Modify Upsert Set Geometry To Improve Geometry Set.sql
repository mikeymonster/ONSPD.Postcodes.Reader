ALTER PROCEDURE [dbo].[Postcode_Upsert] (
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
  t.[Longitude] = s.[Longitude],
	  t.[Location] = NULL

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

UPDATE [dbo].[Postcode]
SET [Location] = geography::Point([Latitude], [Longitude], 4326)
WHERE [Location] IS NULL
AND [Latitude] BETWEEN -90 AND 90

GO