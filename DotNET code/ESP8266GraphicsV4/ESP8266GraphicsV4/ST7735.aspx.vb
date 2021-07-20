Imports System.Drawing
Imports System.Net
Imports System.Drawing.Drawing2D

Public Class ST7735
    Inherits System.Web.UI.Page

    Dim c As New ST7735Canvas()
    Dim g As Graphics = c.g


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim asOLEDFormat = (Request.UserAgent Is Nothing)
        If Application("count") Is Nothing Then Application("count") = 0
        Application("count") += 1
        Dim counter = Application("count")
        Dim frameDelay = 1000

        'counter = 99
        If True = False Then
            Dim p As Pen = Nothing
            For x = 0 To 127
                p = New Pen(Color.FromArgb(x * 2, 0, 0))
                g.DrawLine(p, x, 0, x, 40)
                p = New Pen(Color.FromArgb(0, x * 2, 0))
                g.DrawLine(p, x, 41, x, 80)
                p = New Pen(Color.FromArgb(0, 0, x * 2))
                g.DrawLine(p, x, 81, x, 120)
                p = New Pen(Color.FromArgb(x * 2, x * 2, x * 2))
                g.DrawLine(p, x, 121, x, 160)
            Next
        End If

        If counter < 10 Then
            drawClock()
            frameDelay = 1000
        ElseIf counter <= 10 Then
            Dim bitmapFromURI As Bitmap = c.getURIImage("http://www.sat24.com/image.ashx?country=gb&type=loop&sat=vis")
            g.DrawImage(c.resizeBitmap(bitmapFromURI, 128, 160), 0, 0)
            Dim sourceR As New Rectangle(129, bitmapFromURI.Height - 21, 335, 17)
            Dim destR As New Rectangle(0, 150, 128, 10)
            g.DrawImage(bitmapFromURI, destR, sourceR, GraphicsUnit.Pixel)
            frameDelay = 5000
        ElseIf counter = 11 Then
            g.Clear(Color.Gray)
            g.DrawImage(c.getURIImage("http://www.untamed.co.uk/Calendar/todaysCalendar.aspx"), 14, 25)
            frameDelay = 5000
        ElseIf counter = 12 Then
            Dim bitmap As Bitmap = c.getURIImage("http://untamed.co.uk/sunClock/currentImage.sun")
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone)
            g.DrawImage(c.fitBitmapToScreen(bitmap), 21, 0)
            frameDelay = 5000
        ElseIf counter = 13 Then
            Dim image As Bitmap = c.getURIImage("http://umbra.nascom.nasa.gov/images/latest_hmi_igram.gif")
            Dim trim As Decimal = 15
            g.DrawImage(c.resizeBitmap(image, 128 + trim, 128 + trim), -trim / 2, 16 - (trim / 2))
            frameDelay = 5000
        ElseIf counter = 14 Then
            Dim image As Bitmap = c.getURIImage("http://sohowww.nascom.nasa.gov/data/realtime/eit_171/512/latest.jpg")
            Dim trim As Decimal = 3
            g.DrawImage(c.resizeBitmap(image, 128 + trim, 128 + trim), -trim / 2, 16 - (trim / 2))
            Dim sourceR As New Rectangle(0, image.Height - 20, 128, 20)
            Dim destR As New Rectangle(0, 140, 128, 20)
            g.DrawImage(image, destR, sourceR, GraphicsUnit.Pixel)
            frameDelay = 5000
        ElseIf counter <= 25 Then
            Dim bitmap As Bitmap = c.getURIImage("http://hbeastcam.west-cheshire.ac.uk/cgi-bin/viewer/video.jpg")
            g.DrawImage(c.resizeBitmap(bitmap, 128, 160), 0, 0)
            frameDelay = 1000
        ElseIf counter = 26 Then
            Dim sunClock As New CircularClock()
            Dim image As Bitmap = sunClock.getClockImage()
            g.DrawImage(c.fitBitmapToScreen(image), 0, 0)
            Dim sourceR As New Rectangle(5, image.Height - 45, 160, 30)
            Dim destR As New Rectangle(0, 130, 160, 30)
            g.DrawImage(image, destR, sourceR, GraphicsUnit.Pixel)
            frameDelay = 10000
            Application("count") = 0
        End If
        'drawWeather()

        c.sendToClient(frameDelay:=frameDelay, response:=Response, asOLEDFormat:=asOLEDFormat)

    End Sub

    Public Function coords(ByVal angle As Decimal) As PointF
        angle -= 90
        Dim x As Decimal = Math.Cos(angle / 180D * Math.PI)
        Dim y As Decimal = Math.Sin(angle / 180D * Math.PI)
        Return New PointF(x, y)
    End Function


    Public Sub drawClock()
        Dim dt As DateTime = Now()
        Dim h As Decimal = dt.Hour
        Dim m As Decimal = dt.Minute
        Dim s As Decimal = dt.Second
        Dim ms As Decimal = dt.Millisecond

        Dim hoursAngle As Decimal = (360D * (h / 12D)) + (30D * (m / 60D))
        Dim hours As PointF = coords(hoursAngle)
        Dim minsAngle As Decimal = (360D * (m / 60D)) + (6D * (s / 60D))
        Dim mins As PointF = coords(minsAngle)
        Dim secAngle As Decimal = (360D * (s / 60D)) + (6D * (ms / 1000D))
        Dim secs As PointF = coords(secAngle)

        Dim radius As Integer = 62

        'Dim sweep As Decimal = 128 - (radius * 2)
        Dim centerX As Integer = 64 'radius + ((sweep / 2.0) * (Math.Sin((s * 6.0) / 180.0 * Math.PI) + 1.0))
        Dim centerY As Integer = 80


        Dim backBrush As New SolidBrush(Color.FromArgb(255, 30, 30, 30))

        g.FillEllipse(backBrush, centerX - radius, centerY - radius, radius * 2, radius * 2 - 1)
        g.DrawEllipse(Pens.White, centerX - radius, centerY - radius, radius * 2, radius * 2 - 1)

        Dim hour As Integer = 5
        For angle As Integer = 0 To 359 Step 360 / 12
            Dim vx As Decimal = Math.Sin(angle / 180 * Math.PI)
            Dim vy As Decimal = Math.Cos(angle / 180 * Math.PI)
            g.DrawLine(Pens.White, centerX + vx * (radius - 1), centerY + vy * (radius - 1), centerX + vx * (radius - 4), centerY + vy * (radius - 4))
            If radius > 28 Then
                hour += 1
                g.DrawString(12 - (hour Mod 12), New Font("Arial", 11), Brushes.White, centerX + vx * (radius - 10) - 8, centerY + vy * (radius - 12) - 8)
            End If
        Next

        Dim secondHandPen As New Pen(Color.FromArgb(255, 255, 0, 0), 2)
        Dim minuteHandPen As New Pen(Color.FromArgb(255, 255, 255, 255), 4)
        Dim hourHandPen As New Pen(Color.FromArgb(255, 200, 200, 200), 6)

        secondHandPen.EndCap = LineCap.Round
        minuteHandPen.EndCap = LineCap.Round
        hourHandPen.EndCap = LineCap.Round

        g.DrawLine(hourHandPen, centerX, centerY, centerX + CInt(hours.X * (radius / 2.4)), centerY + CInt(hours.Y * (radius / 2.4)))
        g.DrawLine(minuteHandPen, centerX, centerY, centerX + CInt(mins.X * (radius / 1.8)), centerY + CInt(mins.Y * (radius / 1.8)))
        g.DrawLine(secondHandPen, centerX, centerY, centerX + CInt(secs.X * (radius / 1.4)), centerY + CInt(secs.Y * (radius / 1.4)))
        g.FillEllipse(Brushes.Yellow, centerX - 4, centerY - 4, 8, 8)

    End Sub

End Class