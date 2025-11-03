namespace InventorySystemv2;

// Basisklasse for alle typer varer i systemet.
// Denne klasse indeholder navnet, prisen og nu også placeringen i lager/robot-layoutet.
public class Item
{
    // Lagerposition i robot-opsætningen:
    // 1 = box 'a', 2 = box 'b', 3 = box 'c'
    // Dette gør det muligt for robotten at vide hvilken boks den skal hente varen fra.
    public uint InventoryLocation = 0;
    public string Name = ""; // Navn på varen (vises bl.a. i ordrer og GUI)
    public decimal PricePerUnit = 0m; // Pris pr. stk / liter / kg afhængigt af varetypen

    // ToString bruges hvis varen skal vises i lister, log eller debugging.
    public override string ToString()
    {
        return $"{Name} (Pris: {PricePerUnit}, Lokation: {InventoryLocation})";
    }
}

// Bruges til varer solgt i volumener/mængder, fx olie i liter.
public class BulkItem : Item
{
    public string MeasurementUnit = ""; // F.eks. "L", "kg", "m"
}

// Bruges til varer der sælges som enkelte enheder, fx moduler, skruer og motorer.
public class UnitItem : Item
{
    public decimal Weight = 0m; // Valgfrit ekstra info (ikke brugt af robotten)
}