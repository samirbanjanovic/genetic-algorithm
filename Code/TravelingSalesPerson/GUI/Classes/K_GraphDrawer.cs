using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TravelingSalesPerson;

namespace GUI
{
    public static class K_GraphDrawer
    {
        public static Path GetEdge(PointDetails connection, bool isOriginalCluster, SolidColorBrush edgeColor = null)
        {
            Point p1 = new Point(Canvas.GetLeft(connection.InitialEllipse),
                                 Canvas.GetTop(connection.InitialEllipse));

            Point p2 = new Point(Canvas.GetLeft(connection.DestinationEllipse),
                                 Canvas.GetTop(connection.DestinationEllipse));

            LineGeometry lg = new LineGeometry(p1, p2);

            Path edgeLine = new Path();
            
            if (edgeColor == null)
                edgeColor = System.Windows.Media.Brushes.Black;
            
            SolidColorBrush strokeBrush = edgeColor;
            
            //connection.DestinationEllipse.Fill = strokeBrush;
            //connection.InitialEllipse.Fill = strokeBrush;

            edgeLine.Stroke = strokeBrush;

            edgeLine.StrokeThickness = 2;
            lg.Transform = new ScaleTransform(2, 2);

            GeometryGroup gg = new GeometryGroup();
            gg.Children.Add(lg);
            
            edgeLine.Data = gg;
            
            return edgeLine;
        }

        public static Tuple<Label, Ellipse> GetLabelAndEllipse(Node node)
        {

            double width = 10;
            double height = 10;

            double scaledX = node.X * 7D;
            double scaledY = node.Y * 4D;

            Ellipse ellipse = new Ellipse { Width = width, Height = height };
            
            double left = scaledX - (width / 2);
            double top = scaledY - (height / 2);

            ellipse.Margin = new Thickness(left, top, 0, 0);

            SolidColorBrush fillBrush = System.Windows.Media.Brushes.White;
            SolidColorBrush strokeBrush = System.Windows.Media.Brushes.Black;

            ellipse.Fill = fillBrush;
            ellipse.Stroke = strokeBrush;

            Label vertexLabel = new Label();
            vertexLabel.Content = string.Format("Id:{0}\r\nX:{1}\r\nY:{2}", node.Id, Math.Round(node.X, 3), Math.Round(node.Y, 3));
            vertexLabel.Margin = new Thickness(left - 8, top + 2, 0, 0);
            vertexLabel.FontSize = 10;            

            Canvas.SetLeft(ellipse, scaledX);
            Canvas.SetTop(ellipse, scaledY);

            Canvas.SetLeft(vertexLabel, scaledX);
            Canvas.SetTop(vertexLabel, scaledY);
           
            return new Tuple<Label, Ellipse>(vertexLabel, ellipse);
        }
    }
}
