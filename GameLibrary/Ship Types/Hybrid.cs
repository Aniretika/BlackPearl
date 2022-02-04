namespace GameLibrary
{
    public class Hybrid : Ship, IFighter, IRepairer
    {
        public Hybrid(int length, int speed, int rangeOfAction)
            : base(length, speed, rangeOfAction)
        {
        }

        public Hybrid() { }

        public void Shoot()
        {
        }

        public void Repair()
        {
        }
    }
}