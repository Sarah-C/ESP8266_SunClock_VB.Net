Imports System.Drawing
Imports System.Net
Imports System.Web.Script.Serialization
Imports System.Collections.Generic


Public Class _Default
    Inherits System.Web.UI.Page

    Dim c As New ESP8266Canvas()
    Dim g As Graphics = c.g

    Dim inverse As Boolean = False

    Dim backgroundPen As Pen = Nothing
    Dim backgroundBrush As Brush = Nothing
    Dim foregroundPen As Pen = Nothing
    Dim foregroundBrush As Brush = Nothing



    Dim data As New Dictionary(Of String, String)


    Public Sub addData(ByVal dictionaryNode As Dictionary(Of String, Object), ByVal path As String)
        For Each KVP As KeyValuePair(Of String, Object) In dictionaryNode

            Debug.Print("KVP processing key: " & KVP.key)

            If KVP.Value.GetType IsNot GetType(Dictionary(Of String, Object)) Then
                Debug.Print("This key is an end node, adding key and value: [" & path & KVP.Key & "] - [" & CStr(KVP.Value) & "]")
                data.Add(path & KVP.Key, CStr(KVP.Value))
            Else
                For Each v As Object In KVP.Value
                    If v.value.GetType Is GetType(Dictionary(Of String, Object)) Then
                        addData(v.value, KVP.Key)
                    Else
                        data.Add(path & "-" & KVP.Key & "-" & v.key, CStr(v.value))
                    End If
                Next
            End If


        Next
    End Sub


    Public Sub doScrapeTest()

        'http://www.timeanddate.com/weather/uk/chester/historic

        'https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20weather.forecast%20where%20woeid%20in%20(select%20woeid%20from%20geo.places(1)%20where%20text%3D%22chester%2C%20uk%22)&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys

        Dim output As String = ""
        c.requestWebPage("https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20weather.forecast%20where%20woeid%20in%20(select%20woeid%20from%20geo.places(1)%20where%20text%3D%22chester%2C%20uk%22)&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys")
        Dim dss = New JavaScriptSerializer()
        Dim json = dss.Deserialize(Of Dictionary(Of String, Object))(c.html)
        addData(json, "")


        'c.requestWebPage("https://uk.news.yahoo.com/weather/england/cheshire/chester-15829/")
        'Dim html As String = c.getWebPageText()

        'Dim tagText As String = c.getTagText("src=""/sy/os/weather/")
        'Dim attributeText As String = c.getAttribute("src", tagText)


        writeToPage(output)
    End Sub

    Public Sub writeToPage(ByVal text As String)
        text = text.Replace("<", "&lt;").Replace(">", "&gt;").Replace(vbCr, "<br>")
        Response.Write("<html><body style=""font: 10pt Arial;"">")
        Response.Write(text)
        Response.Write("</body></html>")
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        doScrapeTest()

        Return
        Dim asOLEDFormat = Not ((Debugger.IsAttached) Or (Request("debug") IsNot Nothing))

        If inverse Then
            backgroundPen = Pens.White
            backgroundBrush = Brushes.White
            foregroundPen = Pens.Black
            foregroundBrush = Brushes.Black
        Else
            backgroundPen = Pens.Black
            backgroundBrush = Brushes.Black
            foregroundPen = Pens.White
            foregroundBrush = Brushes.White
        End If

        If Application("count") Is Nothing Then Application("count") = 0
        Application("count") += 1
        If Application("count") = 31 Then Application("count") = 0

        Dim counter = Application("count")

        Dim frameDelay = 1000

        'If counter < 30 Then
        '    drawClock()
        '    frameDelay = 1000
        'ElseIf counter <= 30 Then
        '    drawWeather()
        '    frameDelay = 5000
        'End If
        drawWeather()

        c.sendToClient(frameDelay:=frameDelay, response:=Response, asOLEDFormat:=asOLEDFormat)

    End Sub

    Public Function coords(ByVal angle As Decimal) As PointF
        angle -= 90
        Dim x As Decimal = Math.Cos(angle / 180D * Math.PI)
        Dim y As Decimal = Math.Sin(angle / 180D * Math.PI)
        Return New PointF(x, y)
    End Function

    Public Sub drawWeather()

        Dim bitmap As Bitmap = c.getWebImage("http://www.sat24.com/image.ashx?country=gb&type=loop&sat=vis")

        For y = 0 To bitmap.Height - 1
            For x = 0 To bitmap.Width - 1
                If bitmap.GetPixel(x, y).R > 190 Then
                    bitmap.SetPixel(x, y, Color.White)
                Else
                    bitmap.SetPixel(x, y, Color.Black)
                End If
            Next
        Next


        g.DrawImage(bitmap, 0, 0, 128, 64)

        g.FillRectangle(backgroundBrush, 80, 42, 128, 64)
        g.DrawString("M A P", New Font("Arial", 10), foregroundBrush, 85, 46)

        'c.blackWhiteFrame(255, 130, 200)
    End Sub


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

        Dim radius As Integer = 32

        Dim sweep As Decimal = 128 - (radius * 2)
        Dim centerX As Integer = radius + ((sweep / 2.0) * (Math.Sin((s * 6.0) / 180.0 * Math.PI) + 1.0))
        Dim centerY As Integer = 32

        If inverse Then g.Clear(Color.White) Else g.Clear(Color.Black)
        g.DrawEllipse(foregroundPen, centerX - radius, centerY - radius, radius * 2, radius * 2 - 1)
        g.FillRectangle(foregroundBrush, centerX - 2, centerY - 2, 4, 4)

        Dim hour As Integer = 5
        For angle As Integer = 0 To 359 Step 360 / 12
            Dim vx As Decimal = Math.Sin(angle / 180 * Math.PI)
            Dim vy As Decimal = Math.Cos(angle / 180 * Math.PI)
            g.DrawLine(foregroundPen, centerX + vx * (radius - 1), centerY + vy * (radius - 1), centerX + vx * (radius - 4), centerY + vy * (radius - 4))
            If radius > 28 Then
                hour += 1
                g.DrawString(12 - (hour Mod 12), New Font("Arial", 6), foregroundBrush, centerX + vx * (radius - 8) - 3, centerY + vy * (radius - 8) - 4)
            End If
        Next

        g.DrawLine(foregroundPen, centerX, centerY, centerX + CInt(mins.X * (radius / 1.8)), centerY + CInt(mins.Y * (radius / 1.8)))
        g.DrawLine(foregroundPen, centerX, centerY, centerX + CInt(hours.X * (radius / 2.4)), centerY + CInt(hours.Y * (radius / 2.4)))
        g.DrawLine(foregroundPen, centerX, centerY, centerX + CInt(secs.X * (radius / 1.4)), centerY + CInt(secs.Y * (radius / 1.4)))

    End Sub
End Class