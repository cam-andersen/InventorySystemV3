using System;
using System.Collections.ObjectModel;

namespace InventorySystemv2;

/// <summary>
///     Én linje i en ordre: hvilken vare og hvor mange.
/// </summary>
public class OrderLine
{
    public Item Item; // Reference til varen (indeholder bl.a. InventoryLocation til robotten)
    public double Quantity; // Antal (kan være heltal, men double tillader også fx 1.5 kg for Bulk)

    public OrderLine(Item item, double quantity)
    {
        Item = item;
        Quantity = quantity;
    }

    // Disse properties bruges kun af DataGrid i GUI, så vi kan se info pænt i tabellerne.
    public string ItemName => Item.Name; // Navn på varen
    public double Qty => Quantity; // Alias for læsbarhed
    public uint Location => Item.InventoryLocation; // 1, 2, 3 → robotboks
    public decimal LineTotal => LinePrice(); // Pris for linjen


    // Linjepris = antal * pris pr. enhed
    public decimal LinePrice()
    {
        return (decimal)Quantity * Item.PricePerUnit;
    }
}

/// <summary>
///     En ordre består af flere OrderLines og har et tidspunkt (bruges i UI-sortering mv.).
/// </summary>
public class Order
{
    public ObservableCollection<OrderLine> OrderLines = new();
    public DateTime Time = DateTime.Now;

    // Samlet pris for hele ordren, beregnet ved at summere alle linjer
    public decimal TotalPrice()
    {
        var total = 0m;
        foreach (var l in OrderLines) total += l.LinePrice();
        return total;
    }
}

/// <summary>
///     Ordrebog: kø af ordrer der venter (Queued) og liste over behandlede (Processed).
///     Vi bruger ObservableCollection så DataGrid i XAML automatisk opdateres ved ændringer.
/// </summary>
public class OrderBook
{
    // Properties (med kun getter) gør det nemt at binde i XAML, og samlingerne kan stadig ændres (Add/Remove).
    public ObservableCollection<Order> QueuedOrders { get; } = new();
    public ObservableCollection<Order> ProcessedOrders { get; } = new();

    public decimal TotalRevenue { get; private set; } // total omsætning (opdateres når en ordre behandles)

    // Læg en ordre i køen (kaldes fra Customer.CreateOrder eller testdata)
    public void QueueOrder(Order order)
    {
        QueuedOrders.Add(order);
    }

    /// <summary>
    ///     Behandler næste ordre i køen:
    ///     - flytter den fra Queued → Processed
    ///     - opdaterer TotalRevenue
    ///     - returnerer den behandlede ordre, så GUI/robot kan handle på den
    ///     Returnerer null hvis køen er tom.
    /// </summary>
    public Order? ProcessNextOrder()
    {
        if (QueuedOrders.Count == 0) return null;

        var order = QueuedOrders[0]; // tag første ordre i køen (FIFO)
        QueuedOrders.RemoveAt(0); // fjern fra kø
        ProcessedOrders.Add(order); // tilføj til behandlede
        TotalRevenue += order.TotalPrice(); // læg til omsætning

        return order; // VIGTIGT: GUI skal bruge ordren til at styre robotten
    }
}

/// <summary>
///     Repræsenterer en kunde som kan afgive ordrer.
/// </summary>
public class Customer
{
    public string Name = "";
    public ObservableCollection<Order> Orders = new();

    public Customer(string name)
    {
        Name = name;
    }

    // Kunden afgiver en ordre: vi gemmer den hos kunden og lægger den i ordre-køen i OrderBook.
    public void CreateOrder(OrderBook book, Order order)
    {
        Orders.Add(order);
        book.QueueOrder(order);
    }
}