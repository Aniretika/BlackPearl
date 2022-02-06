using MyAttriubutes;

namespace GameLibrary
{
    [MyTable(ColumnTitle = "Coordinate")]
    [MyForeignKey(typeof(Field), ColumnTitle = "Field_id")]
    [MyForeignKey(typeof(Ship), ColumnTitle = "Ship_id")]
    public class Coordinate : EntityBase
    {
        public Coordinate(int xCoord, int yCoord, double distanceFromShipToCenter)
        {
            this.XCoord = xCoord;
            this.YCoord = yCoord;
            this.DistanceFromShipToCenter = distanceFromShipToCenter;
            this.Ship = null;
        }

        [MyPrimaryKey(ColumnTitle = "Coordinate_id")]
        public int ID { get; set; }

        [MyColumn(ColumnTitle = "CoordX")]
        public int XCoord { get; set; }

        [MyColumn(ColumnTitle = "CoordY")]
        public int YCoord { get; set; }
        public Ship Ship { get; set; }

        [MyColumn(ColumnTitle = "IsHeadOfTheShip")]
        internal bool IsHeadOfTheShip { get; set; }

        [MyColumn(ColumnTitle = "DistanceFromShipToCenter")]
        internal double DistanceFromShipToCenter { get; set; }
    }
}