using MyAttriubutes;

namespace GameLibrary
{
    [TableDefinition(ColumnTitle = "Coordinate")]
    [FKRelationship(typeof(Field), ColumnTitle = "Field_id")]
    public class Coordinate : EntityBase
    {
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
        public Ship Ship { get; set; }

        [ColumnDefinition(ColumnTitle = "IsHeadOfTheShip")]
        public bool IsHeadOfTheShip { get; set; }

        [ColumnDefinition(ColumnTitle = "DistanceFromShipToCenter")]
        public double DistanceFromShipToCenter { get; set; }
    }
}