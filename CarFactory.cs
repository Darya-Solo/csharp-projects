using System;

namespace CarFactoryApp
{
    public enum CarType { Tesla, Toyota, BMW, Porsche, Ford, Audi }

    public interface ICar
    {
        string GetDescription();
    }

    // Интерфейсы для двигателей
    public interface IElectric { string GetEngineInfo(); }
    public interface IMechanical { string GetEngineInfo(); }

    // Интерфейсы для коробок передач
    public interface IAutomatical { string GetGearboxInfo(); }
    public interface IManualGearbox { string GetGearboxInfo(); }

    public abstract class ACar : ICar
    {
        public abstract string Brand { get; }
        public abstract int Seats { get; }
        public abstract string GetEngineInfo();
        public abstract string GetGearboxInfo();

        public virtual string GetExtra() => "стандартная комплектация";

        public string GetDescription()
        {
            return $"{Brand}: {GetEngineInfo()} с {GetGearboxInfo()} коробкой передач, {Seats} местами, {GetExtra()}";
        }
    }

    public abstract class AutomaticCar : ACar, IAutomatical
    {
        public override string GetGearboxInfo() => "автоматической";
    }

    public abstract class ManualCar : ACar, IManualGearbox
    {
        public override string GetGearboxInfo() => "механической";
    }

    public class Tesla : AutomaticCar, IElectric
    {
        public override string Brand => "Tesla";
        public override int Seats => 5;
        public override string GetEngineInfo() => "электрокар";
        public override string GetExtra() => "Андроид на борту";
    }

    public class Porsche : AutomaticCar, IElectric
    {
        public override string Brand => "Porsche";
        public override int Seats => 4;
        public override string GetEngineInfo() => "электрокар";
        public override string GetExtra() => "премиальная аудиосистема и спортивный режим";
    }

    public class Toyota : AutomaticCar, IMechanical
    {
        public override string Brand => "Toyota";
        public override int Seats => 7;
        public override string GetEngineInfo() => "бензиновый автомобиль";
        public override string GetExtra() => "холодильник в подлокотнике";
    }

    public class Audi : AutomaticCar, IMechanical
    {
        public override string Brand => "Audi";
        public override int Seats => 5;
        public override string GetEngineInfo() => "бензиновый автомобиль";
        public override string GetExtra() => "система полного привода Quattro";
    }

    public class BMW : ManualCar, IMechanical
    {
        public override string Brand => "BMW";
        public override int Seats => 4;
        public override string GetEngineInfo() => "дизельный автомобиль";
        public override string GetExtra() => "спортивный пакет M-Series";
    }

    public class Ford : ManualCar, IMechanical
    {
        public override string Brand => "Ford";
        public override int Seats => 2;
        public override string GetEngineInfo() => "автомобиль с мощным двигателем V8";
    }

    public class CarFactory
    {
        public static ICar CreateCar(CarType type)
        {
            return type switch
            {
                CarType.Tesla => new Tesla(),
                CarType.Porsche => new Porsche(),
                CarType.Toyota => new Toyota(),
                CarType.BMW => new BMW(),
                CarType.Ford => new Ford(),
                CarType.Audi => new Audi(),
                _ => throw new ArgumentException("Данная модель не производится.")
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.Write("Введите марку автомобиля или 'done' для остановки ввода:");

                    string? input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "done")
                        break;

                    if (Enum.TryParse(input, true, out CarType type))
                    {
                        ICar car = CarFactory.CreateCar(type);
                        Console.WriteLine(car.GetDescription());
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: Марка не найдена. Попробуйте: Tesla, Toyota, BMW, Porsche, Ford, Audi.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Системное уведомление: {ex.Message}");
                }
            }

            Console.WriteLine("Сессия завершена.");
        }
    }
}