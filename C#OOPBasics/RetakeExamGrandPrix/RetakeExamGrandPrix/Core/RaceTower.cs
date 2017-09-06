﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

public class RaceTower
{
    private readonly DriverFactory driverFactory;
    private readonly TyreFactory tyreFactory;
    private readonly CarFactory carFactory;
    private Dictionary<string, Driver> drivers;
    private Dictionary<Driver, string> unfinishedDrivers;
    private int numberOfLaps;
    private int currentLap;
    private Weather weather;
    public int LenghtOfTrack { get; set; }
    public RaceTower()
    {
        this.driverFactory = new DriverFactory();
        this.tyreFactory = new TyreFactory();
        this.carFactory = new CarFactory();
        this.drivers = new Dictionary<string, Driver>();
        this.unfinishedDrivers = new Dictionary<Driver, string>();
        this.weather = Weather.Sunny;
        this.currentLap = 0;
    }
    public int NumberOfLaps
    {
        get { return this.numberOfLaps; }
        set
        {
            if (value < 0)
            {
                throw new InvalidOperationException($"There is no time! On lap {this.currentLap}.");
            }

            this.numberOfLaps = value;
        }
    }
    public void RegisterDriver(List<string> commandArgs)
    {
        try
        {
            var tyreArgs = commandArgs.Skip(4).ToList();
            var carArgs = commandArgs.Skip(2).Take(2).ToList();
            var driverArgs = commandArgs.Take(2).ToList();

            var tyre = this.tyreFactory.Create(tyreArgs);
            var car = this.carFactory.Create(carArgs, tyre);
            var driver = this.driverFactory.Create(driverArgs, car);

            this.drivers.Add(driverArgs[1], driver);
        }
        catch (Exception e)
        {
          
        }
    }

    public string CompleteLaps(List<string> commandArgs)
    {
        var result = new StringBuilder();

        var currentNumberOfLaps = int.Parse(commandArgs[0]);

        try
        {
            this.NumberOfLaps -= currentNumberOfLaps;

        }
        catch (Exception e)
        {
            return e.Message;
        }


        for (int i = 0; i < currentNumberOfLaps; i++)
        {
            foreach (var driver in this.drivers.Values)
            {
                driver.TotalTime += 60 / (this.LenghtOfTrack / driver.Speed);
            }

            this.currentLap++;
            
            foreach (var driver in this.drivers.Values)
            {
                try
                {
                    driver.ReduceFuelAmount(this.LenghtOfTrack);
                }
                catch (Exception e)
                {
                    this.unfinishedDrivers.Add(driver, e.Message);
                }
            }

            // проверка за отпаднал състезател
            foreach (var crashDriver in this.unfinishedDrivers)
            {
                if (this.drivers.ContainsKey(crashDriver.Key.Name))
                {
                    this.drivers.Remove(crashDriver.Key.Name);
                }
            }

            foreach (var driver in this.drivers.Values)
            {
                try
                {
                    driver.Car.Tyre.ReduceDegradation();
                }
                catch (Exception e)
                {
                    this.unfinishedDrivers.Add(driver, e.Message);
                }
            }

             // проверка за отпаднал състезател
            foreach (var crashDriver in this.unfinishedDrivers)
            {
                if (this.drivers.ContainsKey(crashDriver.Key.Name))
                {
                    this.drivers.Remove(crashDriver.Key.Name);
                }
            }

            var driversToOvertaken = this.drivers.Values.OrderByDescending(d => d.TotalTime).ToList();

            for (int j = 0; j < driversToOvertaken.Count - 1; j++)
            {
                var firstDriver = driversToOvertaken[j];
                var secondDriver = driversToOvertaken[j + 1];
                var timeFirstDriver = firstDriver.TotalTime;
                var timeSecondDriver = secondDriver.TotalTime;
                var difference = Math.Abs(timeFirstDriver - timeSecondDriver);

                if (firstDriver.GetType().Name == "AggressiveDriver"
                    && firstDriver.Car.Tyre.GetType().Name == "UltrasoftTyre" && difference <= 3)
                {
                    if (this.weather == Weather.Foggy)
                    {
                        this.unfinishedDrivers.Add(firstDriver, "Crashed");
                    }
                    else
                    {
                        secondDriver.TotalTime -= difference;
                        firstDriver.TotalTime += difference;
                        result.AppendLine(
                            $"{firstDriver.Name} has overtaken {secondDriver.Name} on lap {this.currentLap}.");
                    }
                }
                else if (firstDriver.GetType().Name == "EnduranceDriver"
                    && firstDriver.Car.Tyre.GetType().Name == "HardTyre" && difference <= 3)
                {
                    if (this.weather == Weather.Rainy)
                    {
                        this.unfinishedDrivers.Add(firstDriver, "Crashed");
                    }
                    else
                    {
                        secondDriver.TotalTime -= difference;
                        firstDriver.TotalTime += difference;
                        result.AppendLine(
                            $"{secondDriver.Name} has overtaken {firstDriver.Name} on lap {this.currentLap}.");
                    }
                }
                else if (difference <= 2)
                {
                    secondDriver.TotalTime -= difference;
                    firstDriver.TotalTime += difference;
                    result.AppendLine(
                        $"{secondDriver.Name} has overtaken {firstDriver.Name} on lap {this.currentLap}.");
                }
            }

            // махане на отпаднали състезатели от катастрофа
            foreach (var crashDriver in this.unfinishedDrivers)
            {
                if (this.drivers.ContainsKey(crashDriver.Key.Name))
                {
                    this.drivers.Remove(crashDriver.Key.Name);
                }
            }
        }

        if (this.NumberOfLaps == 0)
        {
            var sb = new StringBuilder();

            var winner = this.drivers.Values.OrderBy(d => d.TotalTime).First();
            sb.AppendLine($"{winner.Name} wins the race for {winner.TotalTime:F3} seconds.");

            return sb.ToString().Trim();
        }

        return result.ToString().Trim();
    }

    public void SetTrackInfo(int lapsNumber, int trackLength)
    {
        this.NumberOfLaps = lapsNumber;
        this.LenghtOfTrack = trackLength;
    }

    public void DriverBoxes(List<string> commandArgs)
    {
        var boxReasonType = commandArgs[0];
        var driversName = commandArgs[1];
        var driver = this.drivers[driversName];
        driver.TotalTime += 20;

        switch (boxReasonType)
        {
            case "Refuel":
                var fuelAmount = double.Parse(commandArgs[2]);
                driver.Car.Refuel(fuelAmount);
                break;
            case "ChangeTyres":
                var tyreArgs = commandArgs.Skip(2).ToList();
                var newTyre = this.tyreFactory.Create(tyreArgs);
                driver.Car.ChangeTyre(newTyre);
                break;
        }
    }

    public string GetLeaderboard()
    {
        var result = new StringBuilder();
        var counter = 1;
        result.AppendLine($"Lap {this.currentLap}/{this.currentLap + this.NumberOfLaps}");

        foreach (var driver in this.drivers.Values.OrderBy(d => d.TotalTime))
        {
            result.AppendLine($"{counter++} {driver.Name} {driver.TotalTime:F3}");
        }

        var crashesToPrint = this.unfinishedDrivers.Reverse();
        foreach (var driver in crashesToPrint)
        {
            result.AppendLine($"{counter++} {driver.Key.Name} {driver.Value}");
        }

        return result.ToString().Trim();
    }

    public void ChangeWeather(List<string> commandArgs)
    {
        var weatherToString = commandArgs[0];
        this.weather = (Weather)Enum.Parse(typeof(Weather), weatherToString);
    }
}
