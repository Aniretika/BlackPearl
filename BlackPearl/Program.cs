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
            Field field = new Field(8, 8);

            Ship shooter = new Fighter(2, 2, 2);
            Ship hybrid = new Hybrid(3, 4, 3);
            Ship repairer = new Repairer(3, 1, 2);

            Ship shooter1 = new Fighter(2, 2, 3);
            Ship hybrid1 = new Hybrid(4, 4, 2);

            Ship repairer1 = new Repairer(3, 1, 2);
            field.SetShip(Quadrant.Third, 2, 0, shooter, Direction.Right);
            field.SetShip(Quadrant.First, 3, 1, hybrid, Direction.Down);
            field.SetShip(Quadrant.Fourth, 0, 2, repairer, Direction.Down);

            field.SetShip(Quadrant.First, 0, 0, shooter1, Direction.Left);
            field.SetShip(Quadrant.Second, 3, 1, hybrid1, Direction.Left);

            field.SetShip(Quadrant.Fourth, 0, 2, repairer1, Direction.Down);

            // System.Console.WriteLine(field.FieldCondition());
            // System.Console.WriteLine(field.ToString());


            //string connectionString = @"Data Source =.\SOROKASQL; " +
            //    "Initial Catalog=SeaFight;" +
            //    "User ID=SuperUser; Integrated Security=true";

            //string sqlExpression = "INSERT INTO Field(Field_id, Width, Height) VALUES(5, 10, 10)";
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();
            //    SqlCommand command = new SqlCommand(sqlExpression, connection);
            //    command.ExecuteNonQuery();
            //}
            
            using (var unitOfWork = new UnitOfWork
                (@"Data Source=.\SOROKASQL;" +
                "Initial Catalog=SeaFight;" +
                "User ID=SuperUser; Integrated Security=true"))
            {

                //unitOfWork.GetRepository<Field>().Add(field);
                //unitOfWork.GetRepository<Ship>().Add(repairer1);
                //unitOfWork.GetRepository<Ship>().Delete(1);
                var result = unitOfWork.GetRepository<Ship>().GetItem(2);
                Console.WriteLine(result.ToString());
            }


            System.Console.Read();
        }
    }

}