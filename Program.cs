using System.Diagnostics.Metrics;
using System.Net.Mail;
using System.Threading.Channels;

abstract class Delivery
{
    Address address;
    protected DateTime TimeOfDelivery = DateTime.Today;
    public static int DeliveryPrice(int Price, Book[] books)
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
    //protected abstract void  Delivery();
}

class HomeDelivery : Delivery  // домашняя почта 
{
    static int Price = 100;
    public HomeDelivery(string address, int price, DateTime deliveryTime, Book[] books)
    {
        Address = address;
        Price = DeliveryPrice(Price, books);
        TimeOfDelivery = DeliveryTime();
    }
    public override DateTime DeliveryTime()
    {
        return TimeOfDelivery = TimeOfDelivery.AddDays(3);
    }
}

class CourierDelivery : Delivery //курьер 
{
    static int Price = 175;
    public Courier courier;
    public CourierDelivery(string address, int price, DateTime deliveryTime, Book[] books, Courier courier)
    {
        Address = address;
        Price = DeliveryPrice(Price, books);
        TimeOfDelivery = DeliveryTime();
        this.courier = courier; // агреация ибо курьер может брать другие заказы и существовать отдельно от этого заказа или же нет ?
    }
    public override DateTime DeliveryTime()
    {
        return TimeOfDelivery = TimeOfDelivery.AddDays(2);
    }
}

class PickPointDelivery : Delivery // постамат 
{
    static int Price = 100;
    public PickPointDelivery(string address, int price, DateTime deliveryTime, Book[] books)
    {
        Address = address;
        Price = DeliveryPrice(Price, books);
        TimeOfDelivery = DeliveryTime();
    }

    public override DateTime DeliveryTime()
    {
        return TimeOfDelivery = TimeOfDelivery.AddDays(4);
    }
}

class ShopDelivery : Delivery // доставка в магазин сети 
{
    public ShopDelivery(string address, DateTime deliveryTime)
    {
        Address = address;
        TimeOfDelivery = DeliveryTime();
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
            return TimeOfDelivery = TimeOfDelivery.AddDays(randomDays);
    }

}

class Order<TDelivery, TCollectionOfBooks> where TDelivery : Delivery
                                           where TCollectionOfBooks : BookCollection
{
    public TDelivery Delivery;
    public TCollectionOfBooks BookCollection; // наш продукт 

    public string Description; // что можно тут описать ? передавать ли его в конструкторы ? 

    public string GetId() // передать в конструкторы ?
    {
        OrderId = Guid.NewGuid();
        string shortId = OrderId.ToString().Substring(0, 8);
        return shortId;
    }
    protected Guid OrderId { get; set; }

}

abstract class Person
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public Person(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}

class Customer : Person
{
    private Delivery Address;

    private string mobilePhone;
    public string MobilePhone
    {
        private get { return mobilePhone; }
        set
        {
            if (mobilePhone.Length != 10) // перевести в extension логику валидации 
            {
                Console.WriteLine("Введите верный номер телефона. Например: 7ХХХXXXXXXX.");
            }
            else
            {
                mobilePhone = value;
            }

        }
    }
    private string email;
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
    public Customer(string firstName, string lastName, string mobilePhone, string email, Delivery Address) : base(firstName, lastName)
    {
        Email = email;
        this.mobilePhone = mobilePhone;
        this.Address = Address;
    }
}
class Courier : Person
{
    public readonly string MobilePhone = "79876543210";
    bool availible;
    public Courier(string firstName, string lastName, string mobilePhone) : base(firstName, lastName)
    {
        MobilePhone = mobilePhone;
    }
}

public static class SentInformation
{
    public static void MailMessage(DateTime timeOfDelivery)
    {
        Console.WriteLine($"Ваша посылка будет доставлена: {timeOfDelivery}");
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

class Book
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
public static class Additions
{
    static bool IsValidPostalCode(this string postalCode)
    {
        return postalCode.Length == 6;
    }
    static bool IsValidEmail(this string email)
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
class Program
{
    static void Main(string[] args)
    {
        Book book1 = new Book();
        Address address1 = new Address();
        address1.PostalCode = "123456";
        }

}
