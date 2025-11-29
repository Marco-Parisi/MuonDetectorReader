using OxyPlot;
using OxyPlot.Annotations;
using System.Linq;

public static class AnnotationExtensions
{
    public static PolygonAnnotation Clone(this PolygonAnnotation original)
    {
        if (original == null) return null;

        // Crea una nuova istanza
        var copy = new PolygonAnnotation
        {
            Fill = original.Fill,
            StrokeThickness = original.StrokeThickness,
            Layer = original.Layer,
            // Copia altre proprietà se le usi (es. Stroke, Text, etc.)
        };

        // Copia i punti (DataPoint è una struct, quindi viene copiata per valore)
        copy.Points.AddRange(original.Points);

        return copy;
    }
}