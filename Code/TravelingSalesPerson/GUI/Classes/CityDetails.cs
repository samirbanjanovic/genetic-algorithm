using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using TravelingSalesPerson;

namespace GUI
{
    public class PointDetails
    {

        public PointDetails(Ellipse initialEllipse, Ellipse destinationEllipse, ConnectionDetails connection)
        {
            this.InitialEllipse = initialEllipse;
            this.DestinationEllipse = destinationEllipse;
            this.Connection = connection;
        }

        public Ellipse InitialEllipse{ get; set; }

        public Ellipse DestinationEllipse { get; set; }
        public ConnectionDetails Connection { get; set; }
    }
}
