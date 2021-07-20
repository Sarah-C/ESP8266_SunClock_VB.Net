
Imports System.Drawing
Imports System.IO
Imports System.Drawing.Drawing2D
Imports System.Net

Public Class ST7735Canvas

    Public bmp As Bitmap = Nothing
    Public g As Graphics = Nothing
    Public rawBmp As BitmapPixels = Nothing


    Public Sub New()
        bmp = New Bitmap(128, 160, Imaging.PixelFormat.Format16bppRgb565)
        rawBmp = New BitmapPixels(bmp)
        g = Graphics.FromImage(bmp)
        g.SmoothingMode = SmoothingMode.HighQuality
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        g.RenderingOrigin = New Point(0.5, 0.5)
        g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
        g.Clear(Color.Black)
    End Sub

    Public Function fitBitmapToScreen(ByVal image As Bitmap) As Image
        Return fitBitmap(image, 128, 160)
    End Function

    Public Function fitBitmap(ByVal image As Bitmap, ByVal width As Integer, ByVal height As Integer) As Image
        Dim ratio As Double = 0
        If image.Width > image.Height Then
            ratio = width / image.Width
        Else
            ratio = height / image.Height
        End If
        Dim myCallback As New System.Drawing.Image.GetThumbnailImageAbort(AddressOf ThumbnailCallback)
        Return image.GetThumbnailImage(image.Width * ratio, image.Height * ratio, myCallback, IntPtr.Zero)
    End Function

    Public Function resizeBitmap(ByVal image As Bitmap, ByVal width As Integer, ByVal height As Integer) As Image
        Dim newBitmap As New Bitmap(width, height, Imaging.PixelFormat.Format16bppRgb565)
        Dim g As Graphics = Graphics.FromImage(newBitmap)
        g.SmoothingMode = SmoothingMode.HighQuality
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        g.RenderingOrigin = New Point(0.5, 0.5)
        g.DrawImage(image, 0, 0, width, height)
        Return newBitmap
    End Function


    Public Function getURIImage(ByVal URI As String) As Bitmap
        Dim bitmapFromURI As Bitmap = Nothing
        Dim imgCall As WebRequest = WebRequest.Create(URI)

        Using webResponse As WebResponse = imgCall.GetResponse()
            Using stream As IO.Stream = webResponse.GetResponseStream()
                bitmapFromURI = Bitmap.FromStream(stream)
            End Using
        End Using
        Return bitmapFromURI
    End Function

    Public Sub drawURIImage(ByVal URI As String)
        Dim bitmapFromURI As Bitmap = getURIImage(URI)
        g.DrawImage(resizeBitmap(bitmapFromURI, 128, 160), 0, 0)
    End Sub



    Public Function ThumbnailCallback() As Boolean
        Return False
    End Function

    Public Sub sendToClient(ByVal frameDelay As Integer, ByVal response As HttpResponse, ByVal asOLEDFormat As Boolean)
        If asOLEDFormat Then
            response.Write("DataFollows:" & vbCr)
            response.Write(frameDelay & vbCr)
            rawBmp.LockBitmap()
            response.BinaryWrite(rawBmp.ImageBytes)
            rawBmp.UnlockBitmap()
        Else
            response.ContentType = "image/png"
            Using ms As New MemoryStream()
                Dim largeBmp As New Bitmap(bmp.Width * 3 + 60, bmp.Height * 3 + 60, Imaging.PixelFormat.Format16bppRgb565)
                Dim g As Graphics = Graphics.FromImage(largeBmp)
                g.CompositingQuality = Drawing2D.CompositingQuality.HighSpeed
                g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                g.Clear(Color.Gray)
                g.DrawImage(bmp, 30, 30, bmp.Width * 3, bmp.Height * 3)

                largeBmp.Save(ms, Imaging.ImageFormat.Png)
                ms.WriteTo(response.OutputStream)
            End Using
        End If
        response.End()
    End Sub

End Class

