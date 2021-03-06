using GameLibrary;
using GameLibrary.Enums;
using Repository.Interfaces;
using System;
using System.Data.SqlClient;
using UnitOfWorkRepository;

namespace BlackPearl
{
    public class Program
    {
        public static void Main()
        {
            //Field field = new Field(8, 8);
            //Field field = new Field(2, 2);
            Ship shooter = new Fighter(1, 2, 2);

            Ship hybrid = new Hybrid(3, 4, 3);
            Ship repairer = new Repairer(3, 1, 2);

            Ship shooter1 = new Fighter(2, 2, 3);
            Ship hybrid1 = new Hybrid(4, 4, 2);

            Ship repairer1 = new Repairer(3, 1, 2);
            //field.SetShip(Quadrant.Fourth, 0, 0, shooter, Direction.Right);
            
            //field.SetShip(Quadrant.Fourth, 0, 2, repairer, Direction.Down);

            //field.SetShip(Quadrant.First, 0, 0, shooter1, Direction.Left);
            //field.SetShip(Quadrant.Second, 3, 1, hybrid1, Direction.Left);

            //field.SetShip(Quadrant.Fourth, 0, 2, repairer1, Direction.Down);

            using (var unitOfWork = new UnitOfWork
                (@"Data Source=.\SOROKASQL;" +
                "Initial Catalog=SeaFight;" +
                "User ID=SuperUser; Integrated Security=true; MultipleActiveResultSets=True;"))
            {
                Field field = new Field(8, 8);

                //field.ID = unitOfWork.GetRepository<Field>().Add(field);

                //for (int column = 0; column < field.Width; column++)
                //{
                //    for (int row = 0; row < field.Height; row++)
                //    {
                //        field.CoordinateField[row, column].ID =
                //            unitOfWork.GetRepository<Coordinate>().Add(field.CoordinateField[row, column]);
                //    }
                //}

                field.SetShip(Quadrant.First, 3, 1, hybrid, Direction.Down);
                //hybrid.ID = unitOfWork.GetRepository<Ship>().Add(hybrid);


                //unitOfWork.GetRepository<Field>().Update(field);
                //hybrid.FieldID = field.ID;
                //unitOfWork.GetRepository<Ship>().Update(hybrid);


                //unitOfWork.GetRepository<Coordinate>().Update(field[Quadrant.First, 3, 1]);

                //var coordinateNew = unitOfWork.GetRepository<Coordinate>().GetById(field[Quadrant.First, 1, 1].ID);
                //Field newField = unitOfWork.GetRepository<Field>().GetById(field.ID);
                //Ship newHybrid = unitOfWork.GetRepository<Ship>().GetById(hybrid.ID);

                var coordinate = unitOfWork.GetRepository<Coordinate>().Include(field[Quadrant.First, 3, 1], field.GetType());

                //Console.WriteLine($"{newField}");
                //Console.WriteLine($"{coordinate}");

                //System.Console.Read();
                //unitOfWork.GetRepository<Ship>().Add(repairer);
                //unitOfWork.GetRepository<Ship>().Delete(1);
                //System.Console.WriteLine(field.FieldCondition());
                //System.Console.WriteLine(field.ToString());
            }


            System.Console.Read();
        }
    }

}