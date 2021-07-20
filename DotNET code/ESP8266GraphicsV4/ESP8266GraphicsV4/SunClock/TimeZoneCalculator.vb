Public Class TimeZoneCalculator

    Public offsetMinutes As Integer = 0
    Public timeZoneLongitudeChange As Double = 0
    Public timeZoneData As TimeZoneInfo = Nothing
    Public timeDifference As TimeSpan = Nothing
    Public timeDifferenceText As String = Nothing
    Public theTimes As SunriseSunsetMaths.theTimes = Nothing
    Public adjustmentRulesText As String = Nothing
    Public isDaylightSavingTime As Boolean = False

    Public latitude As Double = 0
    Public longitude As Double = 0

    Public Sub New()

    End Sub

    Public Sub setLatLong(ByVal latitude As Double, ByVal longitude As Double)
        Me.latitude = latitude
        Me.longitude = longitude
    End Sub

    Public Sub setTimeZone(ByVal timeZone As String)
        offsetMinutes = TimeZoneOffset.getInMinutes(timeZone)
        timeZoneLongitudeChange = CDbl(offsetMinutes) / 4.0  '(720 minutes maximum change, divide by 180 degrees longitude = 4 min per degree longitude)
        timeZoneData = WindowsTimeZones.OlsonTimeZoneToTimeZoneInfo(timeZone)
        timeDifference = TimeSpan.FromMinutes(Math.Abs(offsetMinutes))
        If offsetMinutes <> 0 Then
            Dim plusMinus As String = If(offsetMinutes >= 0, "+", "-")
            timeDifferenceText = plusMinus & " " & timeDifference.Hours & " hours, " & timeDifference.Minutes & " minutes"
        Else
            timeDifferenceText = "No time difference"
        End If
        adjustmentRulesText = "..."
    End Sub

    Public Sub addTimeOffset(ByRef d As Date, ByVal t As TimeSpan)
        If d.Year <> 1 Then d += t
    End Sub

    Public Function getTimes(ByVal theDate As Date) As SunriseSunsetMaths.theTimes
        Dim offsetLongitude As Double = longitude - timeZoneLongitudeChange
        theTimes = SunriseSunsetMaths.SunCalcGetTimes(theDate, latitude, offsetLongitude)
        If timeZoneData.IsDaylightSavingTime(theDate) Then
            Dim offset As TimeSpan = timeZoneData.GetUtcOffset(theDate)
            addTimeOffset(theTimes.dawn, offset)
            addTimeOffset(theTimes.dusk, offset)
            addTimeOffset(theTimes.goldenHour, offset)
            addTimeOffset(theTimes.goldenHourEnd, offset)
            addTimeOffset(theTimes.nadir, offset)
            addTimeOffset(theTimes.nauticalDawn, offset)
            addTimeOffset(theTimes.nauticalDusk, offset)
            addTimeOffset(theTimes.night, offset)
            addTimeOffset(theTimes.nightEnd, offset)
            addTimeOffset(theTimes.noon, offset)
            addTimeOffset(theTimes.sunrise, offset)
            addTimeOffset(theTimes.sunriseEnd, offset)
            addTimeOffset(theTimes.sunset, offset)
            addTimeOffset(theTimes.sunsetStart, offset)
        End If
        Return theTimes
    End Function


End Class
