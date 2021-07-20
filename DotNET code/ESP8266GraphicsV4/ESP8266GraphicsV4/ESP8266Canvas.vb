Imports System.Drawing
Imports System.IO

Public Class ESP8266Canvas

    Public bmp As Bitmap = Nothing
    Public g As Graphics = Nothing
    Public rawBmp As BitmapPixels = Nothing

    Public Class Frame
        Public bmp As Bitmap = Nothing
        Public repackedBytes(1024) As Byte

        Public Sub New(ByVal bmp As Bitmap, ByVal repackedBytes() As Byte)
            Me.bmp = bmp
            Me.repackedBytes = repackedBytes
        End Sub
    End Class

    Public frames As New List(Of Frame)

    Public Sub New()
        newFrame()
    End Sub


    Public Sub blackWhiteFrame(ByVal limitRed As Integer, ByVal limitGreen As Integer, ByVal limitBlue As Integer)
        For y = 0 To 63
            For x = 0 To 127
                If bmp.GetPixel(x, y).R > limitRed Or bmp.GetPixel(x, y).G > limitGreen Or bmp.GetPixel(x, y).B > limitBlue Then
                    bmp.SetPixel(x, y, Color.White)
                Else
                    bmp.SetPixel(x, y, Color.Black)
                End If
            Next
        Next
    End Sub

    Private Sub storeCurrentBitmap()
        If bmp IsNot Nothing Then
            blackWhiteFrame(0, 0, 0)
            Dim OLEDFormatBuffer() As Byte = createFrameSlow(bmp)
            Dim newFrame As New Frame(bmp, OLEDFormatBuffer)
            frames.Add(newFrame)
            g.Dispose()
            rawBmp = Nothing
            bmp = Nothing
        End If
    End Sub

    Public Sub newFrame()
        storeCurrentBitmap()
        bmp = New Bitmap(128, 64, Imaging.PixelFormat.Format32bppArgb)
        rawBmp = New BitmapPixels(bmp)
        g = Graphics.FromImage(bmp)
        g.Clear(Color.Black)
    End Sub

    Public Function createFrame() As Byte()
        Dim repackedBytes(1024) As Byte
        rawBmp.LockBitmap()
        Dim rotatedByte As Byte = 0
        Dim address As Integer = 0
        Dim storedAddress As Integer = 0
        For yy As Integer = 0 To 8 - 1
            address = yy * 8 * 16 * 4
            For xx As Integer = 0 To 16 - 1
                For bit As Integer = 0 To 7
                    rotatedByte = If((rawBmp.ImageBytes(address)), 1, 0)
                    rotatedByte += If((rawBmp.ImageBytes(address + (16 * 4))), 2, 0)
                    rotatedByte += If((rawBmp.ImageBytes(address + (16 * 2 * 4))), 4, 0)
                    rotatedByte += If((rawBmp.ImageBytes(address + (16 * 3 * 4))), 8, 0)
                    rotatedByte += If((rawBmp.ImageBytes(address + (16 * 4 * 4))), 16, 0)
                    rotatedByte += If((rawBmp.ImageBytes(address + (16 * 5 * 4))), 32, 0)
                    rotatedByte += If((rawBmp.ImageBytes(address + (16 * 6 * 4))), 64, 0)
                    rotatedByte += If((rawBmp.ImageBytes(address + (16 * 7 * 4))), 128, 0)
                    repackedBytes(storedAddress) = rotatedByte
                    storedAddress += 1
                    address += 4
                Next
            Next
        Next
        rawBmp.UnlockBitmap()
        Return repackedBytes
    End Function

    Public Function createFrameSlow(ByVal theBitmap As Bitmap) As Byte()
        Dim repackedBytes(1024) As Byte
        Dim rotatedByte As Byte = 0
        Dim YPos As Integer = 0
        Dim storedAddress As Integer = 0
        For yy As Integer = 0 To 8 - 1
            YPos = yy * 8
            For xx As Integer = 0 To 128 - 1
                rotatedByte = If(theBitmap.GetPixel(xx, YPos).R, 1, 0)
                rotatedByte += If(theBitmap.GetPixel(xx, YPos + 1).R, 2, 0)
                rotatedByte += If(theBitmap.GetPixel(xx, YPos + 2).R, 4, 0)
                rotatedByte += If(theBitmap.GetPixel(xx, YPos + 3).R, 8, 0)
                rotatedByte += If(theBitmap.GetPixel(xx, YPos + 4).R, 16, 0)
                rotatedByte += If(theBitmap.GetPixel(xx, YPos + 5).R, 32, 0)
                rotatedByte += If(theBitmap.GetPixel(xx, YPos + 6).R, 64, 0)
                rotatedByte += If(theBitmap.GetPixel(xx, YPos + 7).R, 128, 0)
                repackedBytes(storedAddress) = rotatedByte
                storedAddress += 1
            Next
        Next
        Return repackedBytes
    End Function

    Public Sub sendToClient(ByVal frameDelay As Integer, ByVal response As HttpResponse, ByVal asOLEDFormat As Boolean)
        storeCurrentBitmap()
        If asOLEDFormat Then
            response.Write("DataFollows:" & vbCr)
            response.Write(frameDelay & vbCr)
            For Each frame As Frame In frames
                response.BinaryWrite(frame.repackedBytes)
            Next
        Else
            response.ContentType = "image/png"
            Dim longCanvas As New Bitmap(128, 64 * frames.Count + frames.Count - 1, Imaging.PixelFormat.Format32bppArgb)
            Dim lg As Graphics = Graphics.FromImage(longCanvas)
            Dim yPosition As Integer = 0
            For Each frame As Frame In frames
                lg.DrawImageUnscaled(frame.bmp, 0, yPosition)
                lg.DrawLine(Pens.White, 0, yPosition + 65, 128, yPosition + 65)
                yPosition += 65
            Next
            lg.Dispose()
            Using ms As New MemoryStream()
                longCanvas.Save(ms, Imaging.ImageFormat.Png)
                ms.WriteTo(response.OutputStream)
            End Using
        End If
        response.End()
    End Sub

End Class
