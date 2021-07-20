Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class BitmapPixels

    ' Provide public access to the picture's byte data.
    Public ImageBytes() As Byte
    Public stride As ULong
    Public total_size As Integer
    Public isLocked As Boolean
    ' A reference to the Bitmap.
    Private m_Bitmap As Bitmap

    ' Bitmap data.
    Private m_BitmapData As BitmapData
    Private bounds As Rectangle

    Public Sub New(ByRef bm As Bitmap)
        m_Bitmap = bm
        bounds = New Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height)
        m_BitmapData = m_Bitmap.LockBits(bounds, Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format16bppRgb565) 'Format1bppIndexed Format32bppPArgb
        stride = m_BitmapData.Stride
        total_size = m_BitmapData.Stride * m_BitmapData.Height
        m_Bitmap.UnlockBits(m_BitmapData)
        ReDim ImageBytes(total_size)
        isLocked = False
    End Sub

    Public Sub LockBitmap()
        If isLocked Then Return
        isLocked = True
        m_BitmapData = m_Bitmap.LockBits(bounds, Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format16bppRgb565) 'Format1bppIndexed Format32bppPArgb
        Marshal.Copy(m_BitmapData.Scan0, ImageBytes, 0, total_size)
    End Sub

    Public Sub UnlockBitmap()
        If Not isLocked Then Return
        Marshal.Copy(ImageBytes, 0, m_BitmapData.Scan0, total_size)
        m_Bitmap.UnlockBits(m_BitmapData)
        isLocked = False
    End Sub

    Public Sub kill()
        If isLocked Then UnlockBitmap()
    End Sub

End Class