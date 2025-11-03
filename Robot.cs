using System;
using System.Net.Sockets;
using System.Text;

namespace InventorySystemv2;

/// <summary>
///     Simpel basis-robot der kan sende strenge/URScript til en UR-controller (eller URSim).
///     Indkapsler TCP-kommunikationen, så GUI og forretningslogik ikke blandes med netværkskode.
/// </summary>
public class Robot
{
    // Som i opgaven: to porte på UR/URSim – Dashboard og URScript.
    // Dashboard bruges bl.a. til "brake release", mens URScript-porten afspiller programmer.
    public const int urscriptPort = 30002;
    public const int dashboardPort = 29999;

    // Standard-IP: "localhost" (UDSIM på samme maskine).
    // Skift denne til robotens rigtige IP-adresse, når du kører i lab.
    public string IpAddress = "localhost";

    /// <summary>
    ///     Sender en rå tekststreng til en vilkårlig port på robotten (genbruges af SendUrscript).
    /// </summary>
    /// <param name="port">TCP-port (fx 29999 eller 30002)</param>
    /// <param name="message">Den præcise streng der skal sendes (inkl. newline hvis nødvendigt)</param>
    public void SendString(int port, string message)
    {
        // Brug "using" så forbindelsen lukkes deterministisk, også ved fejl.
        using var client = new TcpClient();

        // (Valgfrit) Sæt en kort timeout, så vi ikke hænger i lang tid hvis IP/port er forkert.
        client.ReceiveTimeout = 3000;
        client.SendTimeout = 3000;

        // Opret TCP-forbindelse til robotten.
        client.Connect(IpAddress, port);

        // Hent netværksstrøm og send bytes i ASCII (UR forventer ASCII-kommandoer).
        using var stream = client.GetStream();
        var bytes = Encoding.ASCII.GetBytes(message);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();
        // Bemærk: Vi læser ikke svar – i denne opgave kører vi "fire-and-forget".
    }

    /// <summary>
    ///     Sender et helt URScript-program til robotten.
    ///     (Overskriver det der måtte køre i forvejen.)
    /// </summary>
    /// <param name="urscript">Gyldigt URScript-program som tekst</param>
    public void SendUrscript(string urscript)
    {
        // Dashboard-kommando for at frigive bremserne. Vigtigt at den slutter med newline.
        SendString(dashboardPort, "brake release\n");

        // URScript-porten modtager selve programmet. URScript må gerne slutte med newline.
        if (!urscript.EndsWith("\n", StringComparison.Ordinal))
            urscript += "\n";

        SendString(urscriptPort, urscript);
    }
}