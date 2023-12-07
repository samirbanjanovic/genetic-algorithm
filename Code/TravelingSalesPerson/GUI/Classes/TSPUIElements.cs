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
    public static class TSPUIElements
    {
        public static Tuple<Label, Path> GetLabelAndEdge(PointDetails connection, bool isOriginalCluster, SolidColorBrush edgeColor = null)
        {
            Point p1 = new Point(Canvas.GetLeft(connection.InitialEllipse),
                                 Canvas.GetTop(connection.InitialEllipse));

            Point p2 = new Point(Canvas.GetLeft(connection.DestinationEllipse),
                                 Canvas.GetTop(connection.DestinationEllipse));

            LineGeometry lg = new LineGeometry(p1, p2);

            Path tspPath = new Path();
            
            if (edgeColor == null)
                edgeColor = System.Windows.Media.Brushes.White;
            
            SolidColorBrush strokeBrush = edgeColor;
            
            if(isOriginalCluster)
                strokeBrush = System.Windows.Media.Brushes.LightGreen;

            connection.DestinationEllipse.Fill = strokeBrush;
            connection.InitialEllipse.Fill = strokeBrush;

            tspPath.Stroke = strokeBrush;

            tspPath.StrokeThickness = 2;
            lg.Transform = new ScaleTransform(2, 2);

            Label edgeLabel = new Label();
            edgeLabel.Content = string.Format("D:{0}", connection.Connection.Distance);
            edgeLabel.Margin = connection.DestinationEllipse.Margin;
            edgeLabel.FontSize = 10;

            GeometryGroup gg = new GeometryGroup();
            gg.Children.Add(lg);
            
            tspPath.Data = gg;
            
            return new Tuple<Label, Path>(edgeLabel, tspPath);
        }

        public static Tuple<Label, Ellipse> GetLabelAndEllipse(Node node, SolidColorBrush fillBrush = null, bool labelXY = true, int width_height = 10)
        {

            double width = width_height;
            double height = width_height;

            double scaledX = node.X * 7D;
            double scaledY = node.Y * 4D;

            Ellipse ellipse = new Ellipse { Width = width, Height = height };
            
            double left = scaledX - (width / 2);
            double top = scaledY - (height / 2);

            ellipse.Margin = new Thickness(left, top, 0, 0);

            if (fillBrush == null)
                fillBrush = System.Windows.Media.Brushes.White;

            SolidColorBrush strokeBrush = System.Windows.Media.Brushes.Black;

            ellipse.Fill = fillBrush;
            ellipse.Stroke = strokeBrush;

            Label vertexLabel = new Label();

            var lblString = string.Format("Id:{0}", node.Id);

            if (labelXY)
                lblString += string.Format("\r\nX:{0}\r\nY:{1}", Math.Round(node.X, 3), Math.Round(node.Y, 3));

            //vertexLabel.Content = string.Format("Id:{0}\r\nX:{1}\r\nY:{2}", node.Id, Math.Round(node.X, 3), Math.Round(node.Y, 3));
            vertexLabel.Content = lblString;
            vertexLabel.Margin = new Thickness(left + 20, top, 0, 0);
            vertexLabel.FontSize = 12;

            Canvas.SetLeft(ellipse, scaledX);
            Canvas.SetTop(ellipse, scaledY);

            Canvas.SetLeft(vertexLabel, scaledX);
            Canvas.SetTop(vertexLabel, scaledY);
           
            return new Tuple<Label, Ellipse>(vertexLabel, ellipse);
        }
    }
}
