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
                Field field = new Field(8, 8);

                unitOfWork.GetRepository<Field>().Figach();
                field.ID = unitOfWork.GetRepository<Field>().Add(field);

                for (int column = 0; column < field.Width; column++)
                {
                    for (int row = 0; row < field.Height; row++)
                    {
                        field.CoordinateField[row, column].ID =
                            unitOfWork.GetRepository<Coordinate>().Add(field.CoordinateField[row, column]);
                    }
                }

                field.SetShip(Quadrant.First, 3, 1, hybrid, Direction.Down);
                hybrid.ID = unitOfWork.GetRepository<Ship>().Add(hybrid);
                

                unitOfWork.GetRepository<Field>().Update(field);
                hybrid.FieldID = field.ID;//include
                unitOfWork.GetRepository<Ship>().Update(hybrid);
               

                unitOfWork.GetRepository<Coordinate>().Update(field[Quadrant.First, 3, 1]);

                unitOfWork.GetRepository<Ship>().Include(hybrid, field.GetType());
                Field newField = unitOfWork.GetRepository<Field>().GetItem(field.ID);

                Console.WriteLine($"{newField}");

                System.Console.Read();
                //unitOfWork.GetRepository<Ship>().Add(repairer);
                //unitOfWork.GetRepository<Ship>().Delete(1);
                // var result = unitOfWork.GetRepository<Field>().GetItem(15);
                //System.Console.WriteLine(field.FieldCondition());
                //System.Console.WriteLine(field.ToString());
            }


            System.Console.Read();
        }
    }

}