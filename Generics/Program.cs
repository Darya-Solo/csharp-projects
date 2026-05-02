using Generics;

var productRepo = new Repository<Product>();
productRepo.Add(new Product(1, "Laptop", 75_000m));
productRepo.Add(new Product(2, "Smartphone", 45_000m));
productRepo.Add(new Product(3, "Headphones", 3_500m));
productRepo.Add(new Product(4, "Monitor", 30_000m));
productRepo.Add(new Product(5, "Keyboard", 1_200m));

Console.WriteLine($"Всего товаров: {productRepo.Count}");
Console.WriteLine($"Поиск Id(2): {productRepo.GetById(2)}");
Console.WriteLine($"Поиск Id(99): {productRepo.GetById(99)?.ToString() ?? "null"}");

Console.WriteLine("\nДороже 10000:");
foreach (var p in productRepo.Find(p => p.Price > 10_000))
    Console.WriteLine($"  {p}");

productRepo.Remove(3);
Console.WriteLine($"\nПосле удаления Id(3), осталось: {productRepo.Count}");

try
{
    productRepo.Add(new Product(1, "Duplicate", 0));
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Ошибка дубликата: {ex.Message}");
}

var userRepo = new Repository<User>();
userRepo.Add(new User(1, "alice", "alice@example.com"));
userRepo.Add(new User(2, "bob", "bob@example.com"));

Console.WriteLine($"\nВсего пользователей: {userRepo.Count}");
foreach (var u in userRepo.GetAll())
    Console.WriteLine($"  {u}");

Console.WriteLine("\n--- Утилиты ---");

var ints = new List<int> { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
Console.WriteLine($"Уникальные числа: [{string.Join(", ", CollectionUtils.Distinct(ints))}]");

var wordList = new List<string> { "cat", "dog", "bear", "fox", "lion" };
var byLength = CollectionUtils.GroupBy(wordList, w => w.Length);
foreach (var (len, group) in byLength)
    Console.WriteLine($"Длина {len}: [{string.Join(", ", group)}]");

var c1 = new Dictionary<string, int> { ["the"] = 5, ["cat"] = 3 };
var c2 = new Dictionary<string, int> { ["the"] = 4, ["mat"] = 7 };
var merged = CollectionUtils.Merge(c1, c2, (a, b) => a + b);
Console.WriteLine($"Слияние 'the': {merged["the"]}");

var products = new List<Product> { new(10, "SSD", 8_500m), new(11, "GPU", 60_000m) };
Console.WriteLine($"Самый дорогой: {CollectionUtils.MaxBy(products, p => p.Price)}");

try
{
    CollectionUtils.MaxBy(new List<Product>(), p => p.Price);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Ошибка пустого списка: {ex.Message}");
}
