using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace InventorySystemv2;

public partial class MainWindow : Window
{
    // Én robot-instans genbruges (undgår at åbne/lukke TCP for hver klik)
    private readonly ItemSorterRobot _robot = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this; // gør XAML-bindings som {Binding OrderBook...} mulige

        // === Testdata til køen ===
        // Opgaven bruger "countable items" i tre kasser (a,b,c) som robotten kan plukke fra.
        // Derfor sætter vi InventoryLocation = 1,2,3 for UnitItem.
        var itemA = new UnitItem { Name = "M3 screw", PricePerUnit = 1m, InventoryLocation = 1 };
        var itemB = new UnitItem { Name = "M3 nut", PricePerUnit = 1.5m, InventoryLocation = 2 };
        var itemC = new UnitItem { Name = "Pen", PricePerUnit = 1m, InventoryLocation = 3 };

        // Eksempelordre 1: tre linjer, hvoraf en har Quantity=2 → robotten plukker 1+2+1 gange i alt
        var order1 = new Order();
        order1.OrderLines.Add(new OrderLine(itemA, 1));
        order1.OrderLines.Add(new OrderLine(itemB, 2));
        order1.OrderLines.Add(new OrderLine(itemC, 1));

        // Eksempelordre 2: kun én linje
        var order2 = new Order();
        order2.OrderLines.Add(new OrderLine(itemB, 1));

        // Tilføj til ordrebogen via kunder (så Orders-listen på Customer også udfyldes)
        var ramanda = new Customer("Ramanda");
        var totoro = new Customer("Totoro");
        ramanda.CreateOrder(OrderBook, order1);
        totoro.CreateOrder(OrderBook, order2);

        // NOTE: Din gamle testdata (hydraulic oil / servo / PLC module) var blandet
        // med BulkItem, som robotten ikke kan plukke som "stk". Vi kører nu
        // udelukkende UnitItem i testdata, så pluk-simuleringen giver mening.
    }

    // Eksponeres til XAML (to DataGrids kan binde mod QueuedOrders/ProcessedOrders)
    public OrderBook OrderBook { get; } = new();

    // Hjælpemetode: forsøg at finde statusfeltet; hvis det ikke findes i XAML, returnér null.
    // (Det gør koden robust, så du kan tilføje <TextBlock x:Name="StatusMessages" /> senere.)
    private TextBlock? StatusBlock => this.FindControl<TextBlock>("StatusMessages");

    // Lille wrapper så vi kan logge uden at null-tjekke hver gang
    private void Log(string message)
    {
        if (StatusBlock is null) return; // hvis ikke tilføjet i XAML endnu
        StatusBlock.Text += message + Environment.NewLine;
    }

    // Beholder din gamle event-handler-navn, så XAML ikke skal ændres.
    // Vi sender den videre til en ny async metode.
    public void ProcessNextOrder_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = ProcessNextOrder_OnClickAsync(sender, e);
    }

    // ASYNC handler: fryser ikke GUI. Herfra styrer vi både ordrebog og robot.
    public async Task ProcessNextOrder_OnClickAsync(object? sender, RoutedEventArgs e)
    {
        Log("Processing next order...");

        // Ny implementering i Ordering.cs returnerer selve ordren (eller null)
        var order = OrderBook.ProcessNextOrder();
        if (order is null)
        {
            Log("No queued orders.");
            return;
        }

        // For hver orderline: pluk Quantity gange hvis det er UnitItem.
        foreach (var line in order.OrderLines)
            // Robotten plukker kun "countable items" (UnitItem). BulkItem springes over.
            if (line.Item is UnitItem)
            {
                // line.Quantity er double i modellen; vi plukker heltalsgange.
                var repeats = Math.Max(0, (int)Math.Round(line.Quantity));
                for (var i = 0; i < repeats; i++)
                {
                    Log($"Picking {line.Item.Name} from box {line.Item.InventoryLocation} → S");

                    // Sender URScript til robotten (ItemSorterRobot indsætter X=1/2/3 i programmet)
                    _robot.PickUp(line.Item.InventoryLocation);

                    // Opgaven foreslår ~10s pr. bevægelse; vi bruger 9.5s for at være lidt hurtigere.
                    // Task.Delay holder UI responsivt i stedet for Thread.Sleep.
                    await Task.Delay(9500);
                }
            }
            else
            {
                // BulkItem (fx olie i liter) plukkes ikke af robotten i denne simple opgave
                Log($"Skipping bulk item '{line.Item.Name}' (not pickable as units).");
            }

        Log("Order completed. New empty S-box ready.");
        Log(string.Empty); // tom linje for luft
    }
}