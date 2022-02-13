using System.Text;
using System.Data.Common;
using MyAttriubutes;

namespace GameLibrary
{
    [TableDefinition(ColumnTitle = "Ship")]
    public abstract class Ship : IEntityBase
    {
        public Ship() { }

        public Ship(int length, int speed, int rangeOfAction)
        {
            this.Length = length;
            this.Speed = speed;
            this.Health = length;
            this.RangeOfAction = rangeOfAction;
        }

        [ColumnDefinition(ColumnTitle = "Speed")]
        public int Speed { get; set; }

        [ColumnDefinition(ColumnTitle = "LengthShip")]
        public int Length { get; set; }

        [ColumnDefinition(ColumnTitle = "Health")]
        public int Health { get; set; }

        [ColumnDefinition(ColumnTitle = "RangeOfAction")]
        public int RangeOfAction { get; set; }

        [PKRelationship(ColumnTitle = "Ship_id")]
        public int ID { get; set; }

        [FKRelationship(typeof(Field), ColumnTitle = "Field_id")]
        public int? FieldID { get; set; }

        public static bool operator ==(Ship ship1, Ship ship2)
        {
            if (ship1 is null)
            {
                if (ship2 is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }

            // Equals handles case of null on right side.
            return ship1.Equals(ship2);
        }

        public static bool operator !=(Ship ship1, Ship ship2) => !(ship1 == ship2);

        public override bool Equals(object obj) => this.Equals(obj as Ship);

        public bool Equals(Ship ship)
        {
            if (ship is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (ReferenceEquals(this, ship))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != ship.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (this.Length == ship.Length) && (this.Speed == ship.Speed);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(base.GetHashCode(), this.Length, this.Speed);
        }

        public override string ToString()
        {
            StringBuilder shipState = new System.Text.StringBuilder($"Type: {this.GetType().Name}\n");
            shipState.Append($"Length: {this.Length}\n");
            shipState.Append($"Speed: {this.Speed}\n");
            shipState.Append($"Health: {this.Health}\n");
            return shipState.ToString();
        }
    }
}