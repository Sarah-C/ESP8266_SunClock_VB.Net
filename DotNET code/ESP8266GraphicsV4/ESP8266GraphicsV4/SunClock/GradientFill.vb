Imports System.Drawing.Drawing2D
Imports System.Drawing

Public Class GradientFill

    Public gradientDiscriptor As New GradientDescriptorClass()

    Public Class GradientDescriptorClass

        Public anchors As New List(Of Anchor)
        Public blur As Single = 0.0!
        Public startCol As Color = Nothing
        Public endCol As Color = Nothing

        Public Class Anchor

            Public location As Integer = 0
            'Public normalLocation As Single = 0
            Public col As Color = Nothing

            Public Sub New(ByVal col As Color, ByVal location As Integer)
                Me.location = location
                Me.col = col
            End Sub

        End Class
    End Class

    Public Sub addAnchor(ByVal col As Color, ByVal location As Integer)
        gradientDiscriptor.anchors.Add(New GradientDescriptorClass.Anchor(col, location))
    End Sub


    Public Function newAnchor(ByVal col As Color, ByVal location As Integer) As GradientDescriptorClass.Anchor
        Return New GradientDescriptorClass.Anchor(col, location)
    End Function

    Public Sub addAnchors(ByVal anchors As List(Of GradientDescriptorClass.Anchor))
        gradientDiscriptor.anchors.AddRange(anchors)
    End Sub

    Public Sub clearAnchors()
        gradientDiscriptor.anchors.Clear()
    End Sub

    Public Sub setBlur(ByVal blur As Single)
        gradientDiscriptor.blur = blur
    End Sub

    Public Sub setStartColor(ByVal col As Color)
        gradientDiscriptor.startCol = col
    End Sub

    Public Sub drawGradient(ByVal bmp As Bitmap)
        Dim bmpLength As Single = bmp.Width

        Dim colors As New List(Of Color)
        Dim normalPositions As New List(Of Single)

        colors.Add(gradientDiscriptor.startCol)
        normalPositions.Add(0.0!)

        Dim previousColor As Color = gradientDiscriptor.startCol
        Dim nextColor As Color = Nothing
        For index As Integer = 0 To gradientDiscriptor.anchors.Count - 1

            colors.Add(previousColor)
            normalPositions.Add(CSng(gradientDiscriptor.anchors(index).location) / bmpLength - gradientDiscriptor.blur)

            nextColor = gradientDiscriptor.anchors(index).col
            colors.Add(nextColor)
            normalPositions.Add(CSng(gradientDiscriptor.anchors(index).location) / bmpLength + gradientDiscriptor.blur)

            previousColor = nextColor
        Next

        colors.Add(previousColor)
        normalPositions.Add(1.0!)

        Dim ColorBlend As New ColorBlend()
        ColorBlend.Colors = colors.ToArray()
        ColorBlend.Positions = normalPositions.ToArray()

        Dim brsGradient As New System.Drawing.Drawing2D.LinearGradientBrush(New Point(0, 0), New Point(bmpLength, 0), Color.Red, Color.Blue)
        brsGradient.GammaCorrection = True
        brsGradient.InterpolationColors = ColorBlend

        Dim g As Graphics = Graphics.FromImage(bmp)
        g.FillRectangle(brsGradient, 0, 0, bmp.Width, bmp.Height)
        brsGradient.Dispose()
        g.Dispose()
    End Sub

End Class
