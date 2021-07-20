Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports GeoTimeZone
Imports System.Drawing

Public Class CircularClock

    Public TZC As New TimeZoneCalculator()
    Public s As SunriseSunsetMaths.theTimes = Nothing

    Public renderedClockImage As New Bitmap(200, 250, Imaging.PixelFormat.Format32bppArgb)
    Public gRenderedClockImage As Graphics = Graphics.FromImage(renderedClockImage)

    Public clockGradientBackgroundImage As New Bitmap(200, 200, Imaging.PixelFormat.Format32bppArgb)

    Public linearGradientImage As New Bitmap(365, 5, Imaging.PixelFormat.Format32bppArgb)
    Public gLinearGradientImage As Graphics = Graphics.FromImage(linearGradientImage)

    Public currentLatitude As Double = 53.1905 '84 '53.1905
    Public currentLongitude As Double = -2.89189 '-5 '-2.89189

    Public night As New Pen(Color.FromArgb(255, 0, 0, 90))
    Public naughticalNight As New Pen(Color.FromArgb(255, 0, 0, 190))
    Public dawn As New Pen(Color.FromArgb(255, 50, 50, 250))
    Public sunrise As New Pen(Color.FromArgb(255, 90, 90, 210))
    Public goldenHour As New Pen(Color.FromArgb(255, 200, 200, 0))
    Public daytime As New Pen(Color.FromArgb(255, 170, 170, 230))

    Public oldDayNumber As Integer = 0

    Public GF As New GradientFill()
    Public timeZone As String = Nothing

    'Public Function getClockImage() As Bitmap
    '    If oldDayNumber <> Now.DayOfYear Then
    '        oldDayNumber = Now.DayOfYear
    '        clockGradientBackgroundImage = drawSunGradient(currentLatitude, currentLongitude, timeZone)
    '    End If
    '    drawFace()
    '    drawText()
    '    Return renderedClockImage
    'End Function

    'Latitudes range from -90 to 90.
    'Longitudes range from -180 to 180.
    Public Function getClockImage() As Bitmap
        getLatitudeAndLongitudeFromFile()
        setHighQuality(gRenderedClockImage)
        setHighQuality(gLinearGradientImage)
        GF.setBlur(0.005)
        GF.setStartColor(night.Color)
        timeZone = TimeZoneLookup.GetTimeZone(currentLatitude, currentLongitude).Result
        TZC.setLatLong(currentLatitude, currentLongitude)
        TZC.setTimeZone(timeZone)
        s = TZC.getTimes(Now())
        clockGradientBackgroundImage = drawSunGradient(currentLatitude, currentLongitude, timeZone)
        drawFace()
        drawText()
        Return renderedClockImage
    End Function

    Public Sub getLatitudeAndLongitudeFromFile()
        If File.Exists("latLong.txt") Then
            Dim text As String = File.ReadAllText("latLong.txt")
            Dim lines() As String = text.Split(vbCrLf)
            If lines.Count >= 2 Then
                Dim latText As String = lines(0)
                Dim longText As String = lines(1)
                If IsNumeric(latText) And IsNumeric(longText) Then
                    Dim newLatitude = CDbl(latText)
                    Dim newLongitude = CDbl(longText)
                    If newLatitude >= -90 And newLatitude <= 90 And newLongitude >= -180 And newLongitude <= 180 Then
                        currentLatitude = newLatitude
                        currentLongitude = newLongitude
                    End If
                End If
            End If
        End If
    End Sub

    Public Function minutesInClamp(ByVal d As Date, ByVal isAM As Boolean, Optional ByVal isScaled As Boolean = True) As Integer
        If d.Year = 1 Then
            If isAM Then
                Return 0
            Else
                Return 359
            End If
        End If
        If isAM And d.Hour > 12 Then Return 0
        If Not isAM And d.Hour < 12 Then Return 359
        If isScaled Then
            Return scaledMinutesIn(d)
        Else
            Return minutesIn(d)
        End If
    End Function

    Public Function scaledMinutesIn(ByVal d As Date) As Integer
        Return minutesIn(d) \ 4
    End Function

    Public Function minutesIn(ByVal d As Date) As Double
        Return (d.Hour * 60.0 + d.Minute)
    End Function

    Public Function timePeriodExists(ByVal beforeToday As Boolean, ByVal theDate As Date) As Boolean
        Return beforeToday Or theDate.Year <> 1
    End Function


    Public Function drawSunGradient(ByVal lat As Double, ByVal lon As Double, ByVal timeZone As String) As Bitmap
        Dim gradientImg As New Bitmap(200, 200, Imaging.PixelFormat.Format32bppArgb)
        Dim gGradientImg As Graphics = Graphics.FromImage(gradientImg)

        'Draw gradients.
        Dim endColors As New List(Of GradientFill.GradientDescriptorClass.Anchor)
        If timePeriodExists(s.nightEndNight_beforeToday, s.nightEnd) Then
            GF.addAnchor(naughticalNight.Color, minutesInClamp(s.nightEnd, True, True))
            If s.nightEnd.Year <> 1 Then endColors.Add(GF.newAnchor(night.Color, minutesInClamp(s.night, False, True)))
        End If
        If timePeriodExists(s.nauticalDawnnauticalDusk_beforeToday, s.nauticalDawn) Then
            GF.addAnchor(dawn.Color, minutesInClamp(s.nauticalDawn, True, True))
            If s.nauticalDawn.Year <> 1 Then endColors.Add(GF.newAnchor(naughticalNight.Color, minutesInClamp(s.nauticalDusk, False, True)))
        End If
        If timePeriodExists(s.dawnDusk_beforeToday, s.dawn) Then
            GF.addAnchor(sunrise.Color, minutesInClamp(s.dawn, True, True))
            If s.dawn.Year <> 1 Then endColors.Add(GF.newAnchor(dawn.Color, minutesInClamp(s.dusk, False, True)))
        End If
        If timePeriodExists(s.sunriseSunset_beforeToday, s.sunrise) Then
            GF.addAnchor(goldenHour.Color, minutesInClamp(s.sunrise, True, True))
            If s.sunrise.Year <> 1 Then endColors.Add(GF.newAnchor(sunrise.Color, minutesInClamp(s.sunset, False, True)))

        End If
        If timePeriodExists(s.goldenHourEndGoldenHour_beforeToday, s.goldenHourEnd) Then
            GF.addAnchor(daytime.Color, minutesInClamp(s.goldenHourEnd, True, True))
            If s.goldenHourEnd.Year <> 1 Then endColors.Add(GF.newAnchor(goldenHour.Color, minutesInClamp(s.goldenHour, False, True)))
        End If
        endColors.Reverse()
        GF.addAnchors(endColors)

        GF.drawGradient(linearGradientImage)

        endColors.Clear()
        GF.clearAnchors()

        'Curve the gradient.
        Dim co As Pen = Nothing
        Dim angleR As Double = 0
        Dim cos As Double = 0
        Dim sin As Double = 0
        gRenderedClockImage.Clear(Color.FromArgb(0, 0, 0, 0))

        For angle As Double = 0 To 360 Step 0.01 '0.01
            angleR = (angle + 90) * Math.PI / 180.0
            cos = Math.Cos(angleR)
            sin = Math.Sin(angleR)
            co = New Pen(linearGradientImage.GetPixel(CInt(angle), 0))
            gGradientImg.DrawLine(co,
                            100,
                            100,
                            100 + CInt(cos * 97.0),
                            100 + CInt(sin * 97.0))
        Next
        gGradientImg.Dispose()
        Return gradientImg
    End Function


    Public Sub drawFace()
        Dim angleR As Double = 0
        Dim cos As Double = 0
        Dim sin As Double = 0

        gRenderedClockImage.FillEllipse(Brushes.White, 2, 2, 196, 196)
        gRenderedClockImage.DrawImage(clockGradientBackgroundImage, 0, 0, 200, 200)

        'Draw hour hand
        angleR = (Now.Hour * 15) + (Now.Minute / 4.0) + 90
        angleR *= Math.PI / 180.0
        cos = Math.Cos(angleR)
        sin = Math.Sin(angleR)
        gRenderedClockImage.DrawLine(New Pen(Color.FromArgb(255, 255, 50, 50), 3),
                            100,
                            100,
                            100 + CInt(cos * 98.0),
                            100 + CInt(sin * 98.0))

        angleR = (s.noon.Hour * 15) + (s.noon.Minute / 4.0) + 90
        angleR *= Math.PI / 180.0
        cos = Math.Cos(angleR)
        sin = Math.Sin(angleR)
        gRenderedClockImage.DrawLine(New Pen(Color.FromArgb(128, 255, 255, 0), 4),
                            100 + CInt(cos * 55.0),
                            100 + CInt(sin * 55),
                            100 + CInt(cos * 98.0),
                            100 + CInt(sin * 98.0))

        angleR = (s.nadir.Hour * 15) + (s.nadir.Minute / 4.0) + 90
        angleR *= Math.PI / 180.0
        cos = Math.Cos(angleR)
        sin = Math.Sin(angleR)
        gRenderedClockImage.DrawLine(New Pen(Color.FromArgb(128, 255, 0, 255), 4),
                            100 + CInt(cos * 55.0),
                            100 + CInt(sin * 55),
                            100 + CInt(cos * 98.0),
                            100 + CInt(sin * 98.0))


        'Draw hour ticks
        Dim segmentPen As New Pen(Color.FromArgb(255, 255, 255, 255), 1)

        gRenderedClockImage.DrawLine(segmentPen, 100, 2, 100, 50)
        gRenderedClockImage.DrawLine(segmentPen, 100, 150, 100, 196)

        gRenderedClockImage.DrawLine(segmentPen, 2, 100, 50, 100)
        gRenderedClockImage.DrawLine(segmentPen, 150, 100, 196, 100)

        Dim hour As Integer = 0
        For angle As Double = 0 To 345 Step 15
            angleR = (angle + 90) * Math.PI / 180.0
            cos = Math.Cos(angleR)
            sin = Math.Sin(angleR)
            gRenderedClockImage.DrawLine(segmentPen,
                            100 + CInt(cos * 80.0),
                            100 + CInt(sin * 80.0),
                            100 + CInt(cos * 98.5),
                            100 + CInt(sin * 98.5))
            If hour <> 0 And hour <> 6 And hour <> 12 And hour <> 18 Then
                gRenderedClockImage.DrawString(hour,
                        New Font("Arial", 8),
                        New SolidBrush(Color.FromArgb(255, 255, 255, 255)),
                        96 + CInt(cos * 70),
                        92 + CInt(sin * 70))
            End If
            hour += 1
        Next
        gRenderedClockImage.DrawString("6 AM",
                             New Font("Arial", 8),
                             New SolidBrush(Color.FromArgb(128, 255, 255, 255)),
                             60, 93)
        gRenderedClockImage.DrawString("6 PM",
                             New Font("Arial", 8),
                             New SolidBrush(Color.FromArgb(128, 255, 255, 255)),
                             115, 93)
        gRenderedClockImage.DrawString("12 Noon",
                             New Font("Arial", 8),
                             New SolidBrush(Color.FromArgb(128, 255, 255, 255)),
                             78, 55)
        gRenderedClockImage.DrawString("Midnight",
                             New Font("Arial", 8),
                             New SolidBrush(Color.FromArgb(128, 255, 255, 255)),
                             80, 130)
        'Draw middle circle, and outer ring.
        gRenderedClockImage.FillEllipse(Brushes.Black, 95, 95, 10, 10)
        gRenderedClockImage.DrawEllipse(New Pen(Color.Gray, 3), 2, 2, 196, 196)


    End Sub


    Public Sub drawText()
        gRenderedClockImage.FillRectangle(Brushes.Black, 0, 201, 199, 249)
        gRenderedClockImage.DrawRectangle(Pens.DarkGray, 0, 201, 199, 48)

        Dim timeDifference As TimeSpan = Nothing
        Dim approachingTime As Date = Nothing
        Dim currentTime As Date = Now()
        Dim description As New StringBuilder()

        If s.sunrise.Year <> 1 Then

            If DateDiff(DateInterval.Second, s.sunrise, currentTime) < 0 Then
                description.AppendLine("Time before sunrise: ")
                approachingTime = s.sunrise
            Else
                description.AppendLine("Time before sunset: ")
                approachingTime = s.sunset
            End If
            timeDifference = approachingTime.Subtract(currentTime)
            description.AppendLine(timeDifference.Hours & " hours, " & timeDifference.Minutes & " minutes")
        Else
            If s.sunriseSunset_beforeToday Then
                description.AppendLine("No sunset today.")
            Else
                description.AppendLine("No sunrise today")
            End If
            description.Append("Brightest time: " & timeDifference.Hours & " hours, " & timeDifference.Minutes & " minutes")
        End If

        gRenderedClockImage.DrawString(description.ToString(),
                        New Font("Arial", 9),
                        New SolidBrush(Color.FromArgb(128, 255, 255, 255)),
                        5,
                        205)
    End Sub

    Public Sub setHighQuality(ByRef g As Graphics)
        'g.CompositingMode = Drawing2D.CompositingMode.SourceOver
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        'g.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        g.TextContrast = 12 ' The gamma correction value must be between 0 and 12. The default value is 4.
        'g.TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixel

    End Sub

End Class