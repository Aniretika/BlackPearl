﻿using System;
using System.Collections.Generic;
using System.Linq;
using GameLibrary.Enums;
using MyAttriubutes;

namespace GameLibrary
{
    [MyTable(ColumnTitle = "Field")]
    public class Field : EntityBase
    {
        public Field(int width, int height)
        {
            this.DefineFieldSize(width, height);
        }

        [MyColumn(ColumnTitle = "Width")]
        public int Width { get; private set; }

        [MyColumn(ColumnTitle = "Height")]
        public int Height { get; private set; }

        [MyForeignKey(typeof(Coordinate), ColumnTitle="Coordinate_id")] 
        public Coordinate[,] CoordinateField { get; private set; }

        [MyPrimaryKey]
        public int ID { get; set; }

        public Coordinate this[Quadrant quadrant, uint coordinateXFromQuadrant, uint coordinateYFromQuadrant]
        {
            get
            {
                int shipHeadCoordinateX = this.ConvertCoordinateFromQuadrant(quadrant, coordinateXFromQuadrant, coordinateYFromQuadrant).XCoord;
                int shipHeadCoordinateY = this.ConvertCoordinateFromQuadrant(quadrant, coordinateXFromQuadrant, coordinateYFromQuadrant).YCoord;
                return this.ShipHeadCoordinate(shipHeadCoordinateX, shipHeadCoordinateY);
            }
        }

        // only for checking the field condition
        public string FieldCondition()
        {
            string stringState = "Field state:\n";
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    if (this.CoordinateField[i, j].Ship != null)
                    {
                        stringState += "* ";
                    }
                    else
                    {
                        stringState += "- ";
                    }
                }

                stringState += "\n";
            }

            return stringState;
        }

        public void Move(Ship selectedShip)
        {
            throw new NotImplementedException();
        }

        public void SetShip(Quadrant quadrant, uint coordinateXFromQuadrant, uint coordinateYFromQuadrant, Ship ship, Direction direction)
        {
            int shipHeadCoordinateX = this.ConvertCoordinateFromQuadrant(quadrant, coordinateXFromQuadrant, coordinateYFromQuadrant).XCoord;
            int shipHeadCoordinateY = this.ConvertCoordinateFromQuadrant(quadrant, coordinateXFromQuadrant, coordinateYFromQuadrant).YCoord;
            this.CoordinateField[shipHeadCoordinateX, shipHeadCoordinateY].IsHeadOfTheShip = true;
            this.SetShipByDirection(shipHeadCoordinateX, shipHeadCoordinateY, direction, ship);
        }

        public override string ToString()
        {
            string stateOfField = "List of ships:\n";

            List<Coordinate> shipHeadCoordinates = new List<Coordinate>();

            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    if (this.CoordinateField[i, j].IsHeadOfTheShip)
                    {
                        shipHeadCoordinates.Add(this.CoordinateField[i, j]);
                    }
                }
            }

            foreach (var shipCoordinate in shipHeadCoordinates.OrderBy(coordinate => coordinate.DistanceFromShipToCenter))
            {
                stateOfField += $"{shipCoordinate?.Ship.ToString()}Distance from ship to center: {shipCoordinate.DistanceFromShipToCenter}\n\n";
                shipCoordinate.Ship.ToString();
            }

            return stateOfField;
        }

        private void SetShipByDirection(int shipHeadCoordinateX, int shipHeadCoordinateY, Direction direction, Ship ship)
        {
            switch (direction)
            {
                case Direction.Right:
                    {
                        if (this.CheckShipInCaseDirectionRigth(shipHeadCoordinateX, shipHeadCoordinateY, ship))
                        {
                            this.SetShipInCaseDirectionRigth(shipHeadCoordinateX, shipHeadCoordinateY, ship);
                        }

                        break;
                    }

                case Direction.Left:
                    {
                        if (this.CheckShipInCaseDirectionLeft(shipHeadCoordinateX, shipHeadCoordinateY, ship))
                        {
                            this.SetShipInCaseDirectionLeft(shipHeadCoordinateX, shipHeadCoordinateY, ship);
                        }

                        break;
                    }

                case Direction.Up:
                    {
                        if (this.CheckShipInCaseDirectionUp(shipHeadCoordinateX, shipHeadCoordinateY, ship))
                        {
                            this.SetShipInCaseDirectionUp(shipHeadCoordinateX, shipHeadCoordinateY, ship);
                        }

                        break;
                    }

                case Direction.Down:
                    {
                        if (this.CheckShipInCaseDirectionDown(shipHeadCoordinateX, shipHeadCoordinateY, ship))
                        {
                            this.SetShipInCaseDirectionDown(shipHeadCoordinateX, shipHeadCoordinateY, ship);
                        }

                        break;
                    }
            }
        }

        private void SetShipInCaseDirectionUp(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            for (int changeableCoordinateByX = shipHeadCoordinateX; changeableCoordinateByX > shipHeadCoordinateX - ship.Length; --changeableCoordinateByX)
            {
                this.CoordinateField[changeableCoordinateByX, shipHeadCoordinateY].Ship = ship;
            }
        }

        private void SetShipInCaseDirectionLeft(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            for (int changeableCoordinateByY = shipHeadCoordinateY; changeableCoordinateByY > shipHeadCoordinateY - ship.Length; --changeableCoordinateByY)
            {
                this.CoordinateField[shipHeadCoordinateX, changeableCoordinateByY].Ship = ship;
            }
        }

        private void SetShipInCaseDirectionDown(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            for (int changeableCoordinateByX = shipHeadCoordinateX; changeableCoordinateByX < shipHeadCoordinateX + ship.Length; changeableCoordinateByX++)
            {
                this.CoordinateField[changeableCoordinateByX, shipHeadCoordinateY].Ship = ship;
            }
        }

        private void SetShipInCaseDirectionRigth(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            for (int changeableCoordinateByY = shipHeadCoordinateY; changeableCoordinateByY < shipHeadCoordinateY + ship.Length; changeableCoordinateByY++)
            {
                this.CoordinateField[shipHeadCoordinateX, changeableCoordinateByY].Ship = ship;
            }
        }

        private Coordinate ShipHeadCoordinate(int shipHeadCoordinateX, int shipHeadCoordinateY)
        {
            return this.CoordinateField[shipHeadCoordinateY, shipHeadCoordinateX].Ship != null
                                ? this.CoordinateField[shipHeadCoordinateY, shipHeadCoordinateX]
                                : null;
        }

        private bool CheckShipInCaseDirectionRigth(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            if (!(this.GeneralCheckingCoordinate(shipHeadCoordinateX, shipHeadCoordinateY) &&
                shipHeadCoordinateY + ship.Length < this.Height))
            {
                return false;
            }
            else
            {
                for (int changeableCoordinateByX = shipHeadCoordinateX; changeableCoordinateByX < shipHeadCoordinateX + ship.Length; changeableCoordinateByX++)
                {
                    if (this.CoordinateField[changeableCoordinateByX, shipHeadCoordinateY].Ship != ship)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool CheckShipInCaseDirectionLeft(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            if (!(this.GeneralCheckingCoordinate(shipHeadCoordinateX, shipHeadCoordinateY) &&
                shipHeadCoordinateY - ship.Length + 1 >= 0))
            {
                return false;
            }
            else
            {
                for (int changeableCoordinateByX = shipHeadCoordinateX; changeableCoordinateByX > shipHeadCoordinateX - ship.Length; --changeableCoordinateByX)
                {
                    if (this.CoordinateField[changeableCoordinateByX, shipHeadCoordinateY].Ship != ship)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool CheckShipInCaseDirectionDown(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            if (!(this.GeneralCheckingCoordinate(shipHeadCoordinateX, shipHeadCoordinateY) &&
               shipHeadCoordinateX + ship.Length <= this.Width))
            {
                return false;
            }
            else
            {
                for (int changeableCoordinateByY = shipHeadCoordinateY; changeableCoordinateByY < shipHeadCoordinateY + ship.Length; ++changeableCoordinateByY)
                {
                    if (this.CoordinateField[shipHeadCoordinateX, changeableCoordinateByY].Ship != ship)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool CheckShipInCaseDirectionUp(int shipHeadCoordinateX, int shipHeadCoordinateY, Ship ship)
        {
            if (!(this.GeneralCheckingCoordinate(shipHeadCoordinateX, shipHeadCoordinateY) &&
                shipHeadCoordinateX - ship.Length + 1 >= 0))
            {
                return false;
            }
            else
            {
                for (int changeableCoordinateByY = shipHeadCoordinateY; changeableCoordinateByY > shipHeadCoordinateY - ship.Length; changeableCoordinateByY--)
                {
                    if (this.CoordinateField[shipHeadCoordinateX, changeableCoordinateByY].Ship != ship)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool GeneralCheckingCoordinate(int shipHeadCoordinateX, int shipHeadCoordinateY)
        {
            return shipHeadCoordinateX >= 0 && shipHeadCoordinateX <= this.Width - 1 && shipHeadCoordinateY >= 0 &&
                shipHeadCoordinateY <= this.Height - 1;
        }

        private Coordinate ConvertCoordinateFromQuadrant(Quadrant quadrant, uint coordinateXFromQuadrant, uint coordinateYFromQuadrant)
        {
            int shipHeadCoordinateY = 0;
            int shipHeadCoordinateX = 0;
            switch (quadrant)
            {
                case Quadrant.First:
                    {
                        shipHeadCoordinateY = (int)(this.CenterHeight() + coordinateXFromQuadrant);
                        shipHeadCoordinateX = (int)(this.CenterWidth() - coordinateYFromQuadrant - 1);
                        break;
                    }

                case Quadrant.Second:
                    {
                        shipHeadCoordinateY = (int)(this.CenterHeight() + coordinateXFromQuadrant);
                        shipHeadCoordinateX = (int)(this.CenterWidth() + coordinateYFromQuadrant);
                        break;
                    }

                case Quadrant.Third:
                    {
                        shipHeadCoordinateY = (int)(this.CenterHeight() - coordinateXFromQuadrant - 1);
                        shipHeadCoordinateX = (int)(this.CenterWidth() + coordinateYFromQuadrant);
                        break;
                    }

                case Quadrant.Fourth:
                    {
                        shipHeadCoordinateY = (int)(this.CenterHeight() - coordinateXFromQuadrant - 1);
                        shipHeadCoordinateX = (int)(this.CenterWidth() - coordinateYFromQuadrant - 1);
                        break;
                    }

                default:
                    {
                        return null;
                    }
            }

            return this.CoordinateField[shipHeadCoordinateX, shipHeadCoordinateY];
        }

        private void DefineFieldSize(int width, int height)
        {
            if (width % 2 == 0 && height % 2 == 0)
            {
                this.Width = width;
                this.Height = height;
                this.CoordinateField = new Coordinate[width, height];

                this.SetField(width, height);
            }
            else
            {
                Console.WriteLine("Incorrect. Width and height must be even numbers.");
                Environment.Exit(0);
            }
        }

        private void SetField(int width, int height)
        {
            for (int coordinateX = 0; coordinateX < width; ++coordinateX)
            {
                for (int coordinateY = 0; coordinateY < height; ++coordinateY)
                {
                    var distanceFromShipToCenter = this.CalculateDistanceShipToCenter(coordinateX, coordinateY);
                    this.CoordinateField[coordinateX, coordinateY] = new Coordinate(coordinateX, coordinateY, distanceFromShipToCenter);
                }
            }
        }

        private double CalculateDistanceShipToCenter(int coordinateX, int coordinateY)
        {
            return Math.Sqrt(System.Math.Pow(coordinateX - this.CenterWidth(), 2) + System.Math.Pow(coordinateY - this.CenterHeight(), 2));
        }

        private int CenterWidth()
        {
            return this.Width / 2;
        }

        private int CenterHeight()
        {
            return this.Height / 2;
        }
    }
}