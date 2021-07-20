'Property	Description
'sunrise	sunrise (top edge of the sun appears on the horizon)
'sunriseEnd	sunrise ends (bottom edge of the sun touches the horizon)
'goldenHourEnd	morning golden hour (soft light, best time for photography) ends
'solarNoon	solar noon (sun is in the highest position)
'goldenHour	evening golden hour starts
'sunsetStart	sunset starts (bottom edge of the sun touches the horizon)
'sunset	sunset (sun disappears below the horizon, evening civil twilight starts)
'dusk	dusk (evening nautical twilight starts)
'nauticalDusk	nautical dusk (evening astronomical twilight starts)
'night	night starts (dark enough for astronomical observations)
'nadir	nadir (darkest moment of the night, sun is in the lowest position)
'nightEnd	night ends (morning astronomical twilight starts)
'nauticalDawn	nautical dawn (morning nautical twilight starts)
'dawn	dawn (morning nautical twilight ends, morning civil twilight starts)



Public Class SunriseSunsetMaths

    Public Shared dayMs As Double = 1000.0 * 60.0 * 60.0 * 24.0
    Public Shared J1970 As Double = 2440588.0
    Public Shared J2000 As Double = 2451545.0

    Public Shared rad As Double = (Math.PI / 180.0)
    Public Shared e As Double = rad * 23.4397

    'The primitive value is returned as the number of millisecond since midnight January 1, 1970 UTC.
    '{ return date.valueOf() / dayMs - 0.5 + J1970; }
    Public Shared Function toJulian(ByVal d As Date) As Double
        Dim dt As Double = d.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds
        Return dt / dayMs - 0.5 + J1970
    End Function

    '{ return new Date((j + 0.5 - J1970) * dayMs); }
    Public Shared Function fromJulian(ByVal j As Double) As Date
        'Return New Date((j + 0.5 - J1970) * dayMs)
        'If Not IsNumeric(j) Then Return Now()
        Return New DateTime(1970, 1, 1) + New TimeSpan((j + 0.5 - J1970) * dayMs * 10000.0)
    End Function

    Public Shared Function toDays(ByVal d As Date) As Double
        Return toJulian(d) - J2000
    End Function

    Public Shared Function rightAscension(ByVal l As Double, ByVal b As Double) As Double
        Return Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l))
    End Function

    Public Shared Function declination(ByVal l As Double, ByVal b As Double) As Double
        Return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l))
    End Function

    Public Shared Function azimuth(ByVal H As Double, ByVal phi As Double, ByVal dec As Double) As Double
        Return Math.Atan2(Math.Sin(H), Math.Cos(H) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi))
    End Function

    Public Shared Function altitude(ByVal H As Double, ByVal phi As Double, ByVal dec As Double) As Double
        Return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(H))
    End Function

    Public Shared Function siderealTime(ByVal d As Double, ByVal lw As Double) As Double
        Return rad * (280.16 + 360.9856235 * d) - lw
    End Function

    Public Shared Function astroRefraction(ByVal h As Double) As Double
        If (h < 0) Then ' the following formula works for positive altitudes only.
            h = 0 ' If h = -0.08901179 a div/0 would occur.
        End If
        ' formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
        ' 1.02 / tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad
        Return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179))
    End Function

    Public Shared Function solarMeanAnomaly(ByVal d As Double) As Double
        Return rad * (357.5291 + 0.98560028 * d)
    End Function

    Public Shared Function eclipticLongitude(ByVal M As Double) As Double
        Dim C = rad * (1.9148 * Math.Sin(M) + 0.02 * Math.Sin(2.0 * M) + 0.0003 * Math.Sin(3.0 * M)) ' equation of center
        Dim P = rad * 102.9372 ' perihelion Of the Earth
        Return M + C + P + Math.PI
    End Function

    Public Class sunCoordsValues
        Public dec As Double = 0
        Public ra As Double = 0
        Public Sub New(ByVal dec As Double, ByVal ra As Double)
            Me.dec = dec
            Me.ra = ra
        End Sub
    End Class

    Public Shared Function sunCoords(ByVal d As Double) As sunCoordsValues
        Dim M As Double = solarMeanAnomaly(d)
        Dim L As Double = eclipticLongitude(M)
        Return New sunCoordsValues(declination(L, 0), rightAscension(L, 0))
    End Function

    Public Class sunCalcValues
        Public azimuth As Double = 0
        Public altitude As Double = 0
        Public Sub New(ByVal azimuth As Double, ByVal altitude As Double)
            Me.azimuth = azimuth
            Me.altitude = altitude
        End Sub
    End Class

    Public Shared Function SunCalcGetPosition(ByVal [date] As Date, ByVal lat As Double, ByVal lng As Double) As sunCalcValues
        Dim lw As Double = rad * -lng
        Dim phi As Double = rad * lat
        Dim d As Double = toDays([date])

        Dim c As sunCoordsValues = sunCoords(d)
        Dim H = siderealTime(d, lw) - c.ra

        Return New sunCalcValues(azimuth(H, phi, c.dec), altitude(H, phi, c.dec))
    End Function

    Public Shared J0 = 0.0009

    Public Shared Function julianCycle(ByVal d As Double, ByVal lw As Double) As Double
        Return Math.Round(d - J0 - lw / (2.0 * Math.PI))
        'Return Math.Round(d - J0 - lw / (2.0 * Math.PI))
    End Function

    Public Shared Function approxTransit(ByVal Ht As Double, ByVal lw As Double, ByVal n As Double) As Double
        Return J0 + (Ht + lw) / (2.0 * Math.PI) + n
    End Function

    Public Shared Function solarTransitJ(ByVal ds As Double, ByVal M As Double, ByVal L As Double) As Double
        Return J2000 + ds + 0.0053 * Math.Sin(M) - 0.0069 * Math.Sin(2.0 * L)
    End Function

    Public Shared Function hourAngle(ByVal h As Double, ByVal phi As Double, ByVal d As Double) As Double
        Return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)))
    End Function

    Public Class julianTimeForSunAltitude
        Public julianTimeForSunAltitude As Double = 0
        Public beforeToday As Boolean = False
        Public afterToday As Boolean = False
    End Class

    ' Returns set time for the given sun altitude
    Public Shared Function getSetJ(ByVal h As Double, ByVal lw As Double, ByVal phi As Double, ByVal dec As Double, ByVal n As Double, ByVal M As Double, ByVal L As Double) As julianTimeForSunAltitude
        Dim ST As New julianTimeForSunAltitude()
        Dim hourAngleVal As Double = (Math.Sin(h) - Math.Sin(phi) * Math.Sin(dec)) / (Math.Cos(phi) * Math.Cos(dec))
        If hourAngleVal < -1 Then
            ST.beforeToday = True
            Return ST
        ElseIf hourAngleVal > 1 Then
            ST.afterToday = True
            Return ST
        End If
        Dim w As Double = Math.Acos(hourAngleVal)
        Dim a As Double = approxTransit(w, lw, n)
        ST.julianTimeForSunAltitude = solarTransitJ(a, M, L)
        Return ST
    End Function
    'Public Shared Function getSetJ(ByVal h As Double, ByVal lw As Double, ByVal phi As Double, ByVal dec As Double, ByVal n As Double, ByVal M As Double, ByVal L As Double) As julianTimeForSunAltitude
    '    Dim ST As New julianTimeForSunAltitude()
    '    Dim hourAngleVal As Double = (Math.Sin(h) - Math.Sin(phi) * Math.Sin(dec)) / (Math.Cos(phi) * Math.Cos(dec))
    '    If hourAngleVal < -1 Then
    '        ST.beforeToday = True
    '        Return ST
    '    ElseIf hourAngleVal > 1 Then
    '        ST.afterToday = True
    '        Return ST
    '    End If
    '    Dim w As Double = Math.Acos(hourAngleVal)
    '    Dim a As Double = approxTransit(w, lw, n)
    '    ST.julianTimeForSunAltitude = solarTransitJ(a, M, L)
    '    Return ST
    'End Function

    Public Class sunCalcTimeValues
        Public solarNoon As Date = Nothing
        Public nadir As Date = Nothing
        Public Sub New(ByVal solarNoon As Date, ByVal nadir As Date)
            Me.solarNoon = solarNoon
            Me.nadir = nadir
        End Sub
    End Class

    Public Class theTimes
        Public noon As Date = Nothing
        Public nadir As Date = Nothing
        Public noonNadir_beforeToday As Boolean = False
        Public noonNadir_afterToday As Boolean = False

        Public sunrise As Date = Nothing
        Public sunset As Date = Nothing
        Public sunriseSunset_beforeToday As Boolean = False
        Public sunriseSunset_afterToday As Boolean = False

        Public sunriseEnd As Date = Nothing
        Public sunsetStart As Date = Nothing
        Public sunriseEndSunriseStart_beforeToday As Boolean = False
        Public sunriseEndSunriseStart_afterToday As Boolean = False

        Public dawn As Date = Nothing
        Public dusk As Date = Nothing
        Public dawnDusk_beforeToday As Boolean = False
        Public dawnDusk_afterToday As Boolean = False

        Public nauticalDawn As Date = Nothing
        Public nauticalDusk As Date = Nothing
        Public nauticalDawnnauticalDusk_beforeToday As Boolean = False
        Public nauticalDawnnauticalDusk_afterToday As Boolean = False

        Public nightEnd As Date = Nothing
        Public night As Date = Nothing
        Public nightEndNight_beforeToday As Boolean = False
        Public nightEndNight_afterToday As Boolean = False

        Public goldenHourEnd As Date = Nothing
        Public goldenHour As Date = Nothing
        Public goldenHourEndGoldenHour_beforeToday As Boolean = False
        Public goldenHourEndGoldenHour_afterToday As Boolean = False

    End Class

    Public Shared Function SunCalcGetTimes(ByVal [date] As Date, ByVal lat As Double, ByVal lng As Double) As theTimes
        Dim theTimes As New theTimes()

        Dim lw As Double = rad * -lng
        Dim phi As Double = rad * lat

        Dim d As Double = toDays([date])
        Dim n As Double = julianCycle(d, lw)
        Dim ds As Double = approxTransit(0, lw, n)

        Dim M As Double = solarMeanAnomaly(ds)

        Dim L As Double = eclipticLongitude(M)
        Dim dec As Double = declination(L, 0)

        Dim Jnoon As Double = solarTransitJ(ds, M, L)
        Dim result As sunCalcTimeValues = Nothing

        result = New sunCalcTimeValues(solarNoon:=fromJulian(Jnoon), nadir:=fromJulian(Jnoon - 0.5))

        theTimes.noon = result.solarNoon
        theTimes.nadir = result.nadir

        Dim ST As julianTimeForSunAltitude = Nothing
        Dim Jset As Double = 0
        Dim Jrise As Double = 0

        ST = getSetJ(-0.833 * rad, lw, phi, dec, n, M, L)
        theTimes.sunriseSunset_beforeToday = ST.beforeToday
        theTimes.sunriseSunset_afterToday = ST.afterToday
        If Not (ST.afterToday Or ST.beforeToday) Then
            Jrise = Jnoon - (ST.julianTimeForSunAltitude - Jnoon)
            theTimes.sunrise = fromJulian(Jrise)
            theTimes.sunset = fromJulian(ST.julianTimeForSunAltitude)
        End If

        ST = getSetJ(-0.3 * rad, lw, phi, dec, n, M, L)
        theTimes.sunriseEndSunriseStart_beforeToday = ST.beforeToday
        theTimes.sunriseEndSunriseStart_afterToday = ST.afterToday
        If Not (ST.afterToday Or ST.beforeToday) Then
            Jrise = Jnoon - (ST.julianTimeForSunAltitude - Jnoon)
            theTimes.sunriseEnd = fromJulian(Jrise)
            theTimes.sunsetStart = fromJulian(ST.julianTimeForSunAltitude)
        End If

        ST = getSetJ(-6 * rad, lw, phi, dec, n, M, L)
        theTimes.dawnDusk_beforeToday = ST.beforeToday
        theTimes.dawnDusk_afterToday = ST.afterToday
        If Not (ST.afterToday Or ST.beforeToday) Then
            Jrise = Jnoon - (ST.julianTimeForSunAltitude - Jnoon)
            theTimes.dawn = fromJulian(Jrise)
            theTimes.dusk = fromJulian(ST.julianTimeForSunAltitude)
        End If

        ST = getSetJ(-12.0 * rad, lw, phi, dec, n, M, L)
        theTimes.nauticalDawnnauticalDusk_beforeToday = ST.beforeToday
        theTimes.nauticalDawnnauticalDusk_afterToday = ST.afterToday
        If Not (ST.afterToday Or ST.beforeToday) Then
            Jrise = Jnoon - (ST.julianTimeForSunAltitude - Jnoon)
            theTimes.nauticalDawn = fromJulian(Jrise)
            theTimes.nauticalDusk = fromJulian(ST.julianTimeForSunAltitude)
        End If

        ST = getSetJ(-18 * rad, lw, phi, dec, n, M, L)
        theTimes.nightEndNight_beforeToday = ST.beforeToday
        theTimes.nightEndNight_afterToday = ST.afterToday
        If Not (ST.afterToday Or ST.beforeToday) Then
            Jrise = Jnoon - (ST.julianTimeForSunAltitude - Jnoon)
            theTimes.nightEnd = fromJulian(Jrise)
            theTimes.night = fromJulian(ST.julianTimeForSunAltitude)
        End If
        'Debug.Print($"beforeToday:{ST.beforeToday} afterToday:{ST.afterToday} nightEnd:{theTimes.nightEnd} night:{theTimes.night}")

        ST = getSetJ(6.0 * rad, lw, phi, dec, n, M, L)
        theTimes.goldenHourEndGoldenHour_beforeToday = ST.beforeToday
        theTimes.goldenHourEndGoldenHour_afterToday = ST.afterToday
        If Not (ST.afterToday Or ST.beforeToday) Then
            Jrise = Jnoon - (ST.julianTimeForSunAltitude - Jnoon)
            theTimes.goldenHourEnd = fromJulian(Jrise)
            theTimes.goldenHour = fromJulian(ST.julianTimeForSunAltitude)
        End If

        Return theTimes
    End Function

End Class
