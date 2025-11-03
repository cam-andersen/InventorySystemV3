using System;
using System.Globalization;

namespace InventorySystemv2;

/// <summary>
///     Robot-specialisering til opgaven: Hent 1 stk. fra en item-kasse (x=1..3, y=1)
///     og læg i S-kassen (x=3, y=3). Programmet er en URScript-skabelon, hvor vi
///     indsætter bokslokationen dynamisk.
/// </summary>
public class ItemSorterRobot : Robot
{
    // URScript-skabelon:
    // - {0} bliver erstattet med ITEM_X (1=a, 2=b, 3=c)
    // - Vi bruger en meget simpel "moveto" der mapper gitterkoordinater til en pose i meter.
    //   (I praksis ville man bruge korrekte waypoints/pose_trans, men dette er fint til øvelsen.)
    public const string UrscriptTemplate = @"
def move_item_to_shipment_box():
  # Konstanter i gitterkoordinater (0..3), svarende til opgaven
  SBOX_X = 3
  SBOX_Y = 3
  ITEM_X = {0}     # ← indsættes fra C#
  ITEM_Y = 1
  DOWN_Z = 1       # 'ned' 1 gitter-enhed (0.1m i vores simple mapping)

  # Hjælpefunktion: bevæg tool til (x,y) og en z-offset i gitter -> meters (0.1m pr. trin)
  def moveto(x, y, z = 0):
    # Basal mapping (eksempel): (0 + x*0.1, 0.1 + y*0.1, 0.3 + z*0.1)
    px = 0.0 + x * 0.1
    py = 0.1 + y * 0.1
    pz = 0.3 + z * 0.1
    # Orientering (rx, ry, rz) er en fast pose for enkelhedens skyld
    movel(p[px, py, pz, 2.22, -2.22, 0], a=1.2, v=0.25, r=0)
  end

  # Sekvens: over vare → ned → op → over S → ned → op
  moveto(ITEM_X, ITEM_Y, 0)
  moveto(ITEM_X, ITEM_Y, -DOWN_Z)
  moveto(ITEM_X, ITEM_Y, 0)

  moveto(SBOX_X, SBOX_Y, 0)
  moveto(SBOX_X, SBOX_Y, -DOWN_Z)
  moveto(SBOX_X, SBOX_Y, 0)
end

# Afspil programmet
move_item_to_shipment_box()
";

    /// <summary>
    ///     Indsætter lokationen (1..3) og sender URScript-programmet til robotten.
    /// </summary>
    /// <param name="itemLocation">1=a, 2=b, 3=c</param>
    public void PickUp(uint itemLocation)
    {
        // Valider input – hjælper både under udvikling og ved brug fra GUI.
        if (itemLocation < 1 || itemLocation > 3)
            throw new ArgumentOutOfRangeException(nameof(itemLocation), "Lokation skal være 1..3 (a,b,c).");

        // Formatter URScript med invariant kultur (sikrer punktum som decimalseparator i tal).
        var program = string.Format(CultureInfo.InvariantCulture, UrscriptTemplate, itemLocation);

        // Brug basis-klassens afsender til at levere programmet til URScript-porten.
        SendUrscript(program);
    }
}