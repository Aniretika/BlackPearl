using MyAttriubutes;

namespace GameLibrary
{
    [TableDefinition(ColumnTitle = "Coordinate")]
    public class Coordinate : IEntityBase
    {
        public Coordinate()
        {
        }

        public Coordinate(int xCoord, int yCoord, double distanceFromShipToCenter)
        {
            this.XCoord = xCoord;
            this.YCoord = yCoord;
            this.DistanceFromShipToCenter = distanceFromShipToCenter;
            this.Ship = null;
        }

        [PKRelationship(ColumnTitle = "Coordinate_id")]
        public int ID { get; set; }

        [ColumnDefinition(ColumnTitle = "CoordX")]
        public int XCoord { get; set; }

        [ColumnDefinition(ColumnTitle = "CoordY")]
        public int YCoord { get; set; }

        [FKRelationship(typeof(Ship), ColumnTitle = "Ship_id")]
        public int? ShipID { get; set; }

        [FKRelationship(typeof(Field), ColumnTitle = "Field_id")]
        public int? FieldID { get; set; }
       
        public Ship Ship { get; set; }

        [ColumnDefinition(ColumnTitle = "IsHeadOfTheShip")]
        public bool IsHeadOfTheShip { get; set; }

        [ColumnDefinition(ColumnTitle = "DistanceFromShipToCenter")]
        public double DistanceFromShipToCenter { get; set; }
        
    }
}