using System.Diagnostics.Metrics;
using System.Net.Mail;
using System.Threading.Channels;

abstract class Delivery
{
    internal DateTime TimeOfDelivery = DateTime.Today;
    protected static decimal DeliveryPrice(decimal Price, Book[] books)
    {
        // добавочной цены в зависимости от кол-ва товаров 
        if (books.Length == 0)
        {
            return Price += 25;
        }
        else if (books.Length == 1)
        {
            return Price += 50;
        }
        else if (books.Length > 1)
        {
            return Price += 75;
        }
        return Price;

    }
    public abstract DateTime DeliveryTime();

}

class HomeDelivery : Delivery  
{
    private Address address;
    static decimal Price = 100;
    public HomeDelivery(Address address, DateTime deliveryTime, Book[] books) //тут надо Цену вносить в скобки ? как она будет отображаться ?
    {                                                                         // отдельный метод для инфы об этом или в класс static SentIformation ?
        Price = DeliveryPrice(Price, books);
        TimeOfDelivery = DeliveryTime();
        this.address = address;
        //id = GetShortId(); как сюда добавить id из метода класса Order<>?
    }
    public override DateTime DeliveryTime()
    {
        return TimeOfDelivery = TimeOfDelivery.AddDays(3);
    }
}

class CourierDelivery : Delivery
{
    private Address address;
    static decimal Price = 175;
    public CourierDelivery(Address address, decimal price, DateTime deliveryTime, Book[] books)
    {
        Price = DeliveryPrice(Price, books);
        TimeOfDelivery = DeliveryTime();
        this.address = address;
    }
    public override DateTime DeliveryTime()
    {
        return TimeOfDelivery = TimeOfDelivery.AddDays(2);
    }
}

class PickPointDelivery : Delivery
{
    private Address address;
    static decimal Price = 100;
    public PickPointDelivery(Address address, decimal price, DateTime deliveryTime, Book[] books)
    {
        Price = DeliveryPrice(Price, books);
        TimeOfDelivery = DeliveryTime();
        this.address = address;
    }

    public override DateTime DeliveryTime()
    {
        return TimeOfDelivery = TimeOfDelivery.AddDays(4);
    }
}

class ShopDelivery : Delivery
{
    private Address address;
    public ShopDelivery(DateTime deliveryTime)
    {
        TimeOfDelivery = DeliveryTime();
        // Композиция для адресса сетевого магазина в городе
        address = new Address(city: "Воронеж", street: "ул. Новокузнецкая", building: " дом №35", postalCode: "122235");
    }

    public override DateTime DeliveryTime()
    {
        Random random = new Random();
        int randomDays = random.Next(0, 5);
        if (randomDays == 0)
        {
            return DateTime.Today;
        }
        else
        { return TimeOfDelivery = TimeOfDelivery.AddDays(randomDays); }
    }

}

internal class Order<TDelivery, TCollectionOfBooks> where TDelivery : Delivery
                                           where TCollectionOfBooks : BookCollection
{
    enum OrderStatus
    {
        inProgress,
        Complete
    }

    public TDelivery Delivery; // связь для передачи через Delivery дальше в нужную доставку (только как связь наладить?)
    public TCollectionOfBooks BookCollection; // кол-во книг для передачи в Delivery и расчет в его методе цены
    internal Guid OrderId { get; private set; }
    Customer<int> customer;
    public string GetShortId() // передать в конструкторы ? или только плясать от заказа ? или в доставка-классы 
    {
        string shortId = OrderId.ToString().Substring(0, 8);
        return shortId;
    }

    internal Order (TDelivery delivery, TCollectionOfBooks collectionOfBooks, Customer<int> customer)
    {
        Delivery = delivery;
        BookCollection = collectionOfBooks;
        OrderId = Guid.NewGuid();
        this.customer = customer;
    }

    internal void PrintInfo()
    {
        Console.WriteLine($"Заказ №{GetShortId} на получателя: {customer.Print}");
        Console.WriteLine($"Заказ был оформлен: {Delivery.TimeOfDelivery} {SentInformation.MailMessage(Delivery.TimeOfDelivery, Customer.Email)}");
    }   // как тут получить доступ к свойству Customer.Email ? 
}

public static class Additions
{
    public static bool IsValidPostalCode(this string postalCode)
    {
        return postalCode.Length == 6;
    }
    public static bool IsValidEmail(this string email)
    {
        if (!email.Contains('@'))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public static bool IsValidPhone(this string phone)
    {
        if (phone.Length != 10)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public static void Print<T>(T info)
    {
        Console.WriteLine(info.ToString());
    }
    public static string PrintAge<TAge>(TAge age)
    {
        return age.ToString();
    }
}
class Address
{
    public string flat;
    public string street;
    public string city;
    public string building;
    public string postalCode;

    public string PostalCode
    {
        get { return postalCode; }
        set
        {
            if (value.IsValidPostalCode())
            {
                postalCode = value;
            }
            else
            {
                Console.WriteLine("Введите коректный почтовый индекс!");
            }
        }
    }

    internal string Print()
    {
        return $"город: {city}, улица:{street}, здание: {building}, кв.{(String.IsNullOrWhiteSpace(flat) ? '-' : flat)}, почтовый индекс: {postalCode}";
    }
    public Address(string city = "Воронеж", string street = "проспект Московский", string building = "дом №91",
       string flat = "33", string postalCode = "123456")
    {
        this.flat = flat;
        this.street = street;
        this.city = city;
        this.building = building;
        this.postalCode = postalCode;
    }
}

class Person<TAge>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    TAge age;

    public Person(string firstName, string lastName, TAge age)
    {
        FirstName = firstName;
        LastName = lastName;
        this.age = age;
    }
    public virtual string Print()
    {
        return $"{FirstName} {LastName}" + Additions.PrintAge(age);
    }
}
class Customer<TAge> : Person<TAge>
{
    Address address;

    private string mobilePhone;
    private string email;
    public string MobilePhone
    {
        private get { return mobilePhone; }
        set
        {
            if (mobilePhone.IsValidPhone())
            {
                mobilePhone = value;
            }
            else
            {
                Console.WriteLine("Введите верный номер телефона. Например: 7ХХХXXXXXXX.");
            }

        }
    }

    public string Email
    {
        get
        {
            return email;
        }
        set
        {
            if (value.IsValidEmail())
            {

                email = value;
            }
            else
            {
                Console.WriteLine("Неверный формат адресса электронной почты");
            }
        }
    }

    public override string Print()
    {
        return base.Print() + $"телефон клиента: {mobilePhone}, email клиента: {email}  \n{address.Print()}";

    }
    public Customer(string firstName, string lastName, string mobilePhone, string email, TAge age) : base (firstName, lastName, age)
    {
        this.email = email;
        this.mobilePhone = mobilePhone;
        address = new Address(building: "75");
    }
}
class Courier <TAge>: Person<TAge>
{
    public readonly string MobilePhone = "79876543210";
    bool availible;
    public Courier(string firstName, string lastName, string mobilePhone, bool availible, TAge age) : base (firstName, lastName, age)
    {
        MobilePhone = mobilePhone;
        this.availible = availible;
    }
}

public static class SentInformation
{
    public static void MailMessage(DateTime timeOfDelivery, string email)
    {
        Console.WriteLine($"Ваша посылка будет доставлена: {timeOfDelivery}");
        //тут должна была быть рассылка дополнительно на почту клиенту со временем доставки и можно смс по номеру клиента
    }
    public static void PickPointCode()
    {
        Random randomCode = new Random();
        Random randomTerminal = new Random();
        randomTerminal.Next(0, 30);
        randomCode.Next(1000, 9999);
        Console.WriteLine($"Код доступа к постамату № {randomTerminal} : {randomCode}");
    }
}

internal class Book
{
    public string TittleOfBook;
    public string Author;
}
internal class BookCollection
{
    public Book[] collectionOfBooks;
    public BookCollection(Book[] collectionOfBooks)
    {
        this.collectionOfBooks = collectionOfBooks;
    }

    public Book this[int index]
    {
        get
        {
            if (index >= 0 && index < collectionOfBooks.Length)
            {
                return collectionOfBooks[index];
            }
            else { return null; }
        }
        set
        {
            if (index >= 0 && index < collectionOfBooks.Length)
            {
                collectionOfBooks[index] = value;
            }
        }
    }
}
class Program
{
    static void Main(string[] args)
    {
        //Ну и теперь как со всеми этими конструкторами вызвать их и создать нормальную доставку по допустим адрессу курьером ?
        //Прописать все вызовы и создания обьектов ??
    }

}
