CREATE PROCEDURE [dbo].[Postcode_Distance_Search] (
  @fromPostcode VARCHAR(10),
  @postcodeDestinationSelector VARCHAR(10)
)
AS
SET NOCOUNT ON;

DECLARE @fromLocation GEOGRAPHY
SELECT @fromLocation = [Location] FROM [Postcode] WHERE [Postcode] = @fromPostcode

SELECT	p.[Postcode], 
		p.[Latitude],
		p.[Longitude],
		p.[Location].STDistance(@fromLocation) / 1000 AS [DistanceInKm], 
		p.[Location].STDistance(@fromLocation) / 1609.3399999999999E0 AS [Distance] 
FROM [Postcode] p
WHERE p.[Location] IS NOT NULL
  AND p.[Postcode] LIKE @postcodeDestinationSelector
ORDER BY [Distance]
GO