namespace GameLibrary
{
    public class Repairer : Ship, IRepairer
    {
        public Repairer(int length, int speed, int rangeOfAction)
            : base(length, speed, rangeOfAction)
        {
        }
        public Repairer() { }

        public void Shoot()
        {
        }

        public void Repair()
        {
        }
    }
}