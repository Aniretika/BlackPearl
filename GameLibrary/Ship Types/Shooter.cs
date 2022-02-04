namespace GameLibrary
{
    public class Fighter : Ship, IFighter
    {
        public Fighter(int length, int speed, int rangeOfAction)
            : base(length, speed, rangeOfAction)
        {
        }

        public Fighter() { }
        public void Shoot()
        {
        }
    }
}