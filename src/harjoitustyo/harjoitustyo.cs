using System;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// <summary>
/// Viinaralli, jossa pelaaja kerää mietoja juomia ja välttää vahvoja juomia.
/// @author Sjogren
/// @version 1.0
/// </summary>
public class harjoitustyo : PhysicsGame
{
    //Pelissä käytettävät oliot ja muuttujat
    PhysicsObject pelaaja;
    IntMeter pisteLaskuri;
    Timer mietoAjastin;
    Timer vakevaAjastin;
    bool peliOhi = false;
    Image[] miedotKuvat;
    Image[] vakevatKuvat;
    Image pelaajanKuva;
    
    //Pelissä käytettävät vakiot
    const double MIEDON_JUOMAN_NOPEUS = -400;
    const double VAKEVAN_JUOMAN_NOPEUS = -500;
    const double JUOMAN_LEVEYS = 50;
    const double JUOMAN_KORKEUS = 90;

    public override void Begin()
    {
        miedotKuvat = new Image[]
        {
            LoadImage("ananas"),
            LoadImage("harmaa"),
            LoadImage("lemonade"),
            LoadImage("raspberry"),
            LoadImage("paaryna"),
            LoadImage("lime"),
            LoadImage("vadelma")
        };

        vakevatKuvat = new Image[]
        {
            LoadImage("tapio"),
            LoadImage("minttu"),
            LoadImage("sviina")
        };

        pelaajanKuva = LoadImage("hahmo");
        
        LuoKentta();
        LuoPelaaja();
        LuoPistelaskuri();
        LisaaOhjaimet();
        AloitaAjastimet();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    /// <summary>
    /// Luo pelin kentän, joka on 800x600 pikseliä ja taustaväriltään vaaleansininen.
    /// </summary>
    void LuoKentta()
    {
        Level.Width = 800;
        Level.Height = 600;
        Level.BackgroundColor = Color.LightBlue;
        
        //Lisätään reunat oikealle ja vasemmalle
        PhysicsObject vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 0;

        PhysicsObject oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 0;
    }

    /// <summary>
    /// Luo pelaajan kentälle.
    /// </summary>
    void LuoPelaaja()
    {
        pelaaja = new PhysicsObject(100, 150);
        pelaaja.Image = pelaajanKuva;
        pelaaja.X = 0;
        pelaaja.Y = Level.Bottom + 50;
        pelaaja.Restitution = 0;
        pelaaja.Mass = 1000;
        pelaaja.CanRotate = false;

        Add(pelaaja);
    }

    /// <summary>
    /// Lisää näppäimistöohjaimet pelaajan liikuttamiseen vasemmalle ja oikealle.
    /// Pelaajaa voi liikuttaa A- ja D-näppäimillä tai nuolinäppäimillä.
    /// </summary>
    private void LisaaOhjaimet()
    {
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, null, -300.0);
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, null, 300.0);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, null, -300.0);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, null, 300.0);
    }

    /// <summary>
    /// Liikuttaa pelaajaa vasemmalle tai oikealle määritellyllä nopeudella.
    /// </summary>
    /// <param name="nopeus">Nopeus, jolla pelaaja liikkuu. Negatiivinen nopeus liikuttaa vasemmalle, positiivinen nopeus liikuttaa oikealle.</param>
    private void Liikuta(double nopeus)
    {
        pelaaja.Velocity = new Vector(nopeus, 0);
    }

    /// <summary>
    /// Luo pistelaskurin, joka näyttää pelaajan keräämät pisteet.
    /// Pisteet kasvavat, kun pelaaja kerää mietoja juomia.
    /// Peli päättyy, kun pelaaja osuu vahvaan juomaan.
    /// </summary>
    private void LuoPistelaskuri()
    {
        //Lisätään pelin nimi ennen pistelaskuria
        Label pelinNimi = new Label("VIINARALLI");
        pelinNimi.X = Screen.Right - 100;
        pelinNimi.Y = Screen.Top - 80;
        pelinNimi.TextColor = Color.White;
        pelinNimi.Color = Color.Transparent;
        pelinNimi.Font = Font.DefaultBold;
        Add(pelinNimi);
        
        //LIsätään teksti ennen pistelaskuria
        Label pisteTeksti = new Label("PISTEET:");
        pisteTeksti.X = Screen.Right - 110;
        pisteTeksti.Y = Screen.Top - 100;
        pisteTeksti.TextColor = Color.White;
        pisteTeksti.Color = Color.Transparent;
        pisteTeksti.Font = Font.DefaultBold;
        Add(pisteTeksti);
        
        //Lisätään pistelaskuri
        pisteLaskuri = new IntMeter(0);
        
        Label pisteNaytto = new Label();
        pisteNaytto.BindTo(pisteLaskuri);
        pisteNaytto.X = Screen.Right - 55;
        pisteNaytto.Y = Screen.Top - 100;
        pisteNaytto.TextColor = Color.Green;
        pisteNaytto.Color = Color.Transparent;
        pisteNaytto.Font = Font.DefaultBold;
        Add(pisteNaytto);
    }

    /// <summary>
    /// Aloittaa kaksi ajastinta, jotka luovat mietoja ja vahvoja juomia satunnaisilla sijainneilla kentän yläreunaan.
    /// </summary>
    void AloitaAjastimet()
    {
        mietoAjastin = new Timer();
        mietoAjastin.Interval = 1.0;
        mietoAjastin.Timeout += LuoMietoJuoma;
        mietoAjastin.Start();

        vakevaAjastin = new Timer();
        vakevaAjastin.Interval = 2.0;
        vakevaAjastin.Timeout += LuoVakevaJuoma;
        vakevaAjastin.Start();
    }

    /// <summary>
    /// Luo mietoja juomia, jotka putoavat kentän yläreunasta. Pelaaja saa pisteen kerättyään miedon juoman.
    /// </summary>
    void LuoMietoJuoma()
    {
        PhysicsObject juoma = new PhysicsObject(JUOMAN_LEVEYS, JUOMAN_KORKEUS);
        juoma.Image = RandomGen.SelectOne(miedotKuvat);
        juoma.X = RandomGen.NextDouble(Level.Left, Level.Right);
        juoma.Y = Level.Top;
        juoma.Tag = "mieto";

        juoma.Hit(new Vector(0, MIEDON_JUOMAN_NOPEUS));

        Add(juoma);
        AddCollisionHandler(juoma, Osuma);
    }

    /// <summary>
    /// Luo vahvoja juomia, jotka putoavat kentän yläreunasta. Peli päättyy, jos pelaaja osuu vahvaan juomaan.
    /// </summary>
    void LuoVakevaJuoma()
    {
        PhysicsObject juoma = new PhysicsObject(JUOMAN_LEVEYS, JUOMAN_KORKEUS);
        juoma.Image = RandomGen.SelectOne(vakevatKuvat);
        juoma.X = RandomGen.NextDouble(Level.Left, Level.Right);
        juoma.Y = Level.Top;
        juoma.Tag = "vakeva";

        juoma.Hit(new Vector(0, VAKEVAN_JUOMAN_NOPEUS));

        Add(juoma);
        AddCollisionHandler(juoma, Osuma);
    }

    /// <summary>
    /// Käsittelee osuman pelaajan ja juoman välillä. Jos pelaaja osuu miedon juoman kanssa, pelaaja saa pisteen ja juoma tuhoutuu.
    /// Jos pelaaja osuu vahvan juoman kanssa, peli päättyy, pelaaja ja juoma tuhoutuvat, ja näytetään valikko, jossa pelaaja voi aloittaa uudelleen tai lopettaa pelin.
    /// </summary>
    /// <param name="juoma">Juoma, johon pelaaja osui.</param>
    /// <param name="kohde">Kohde, johon pelaaja osui (tässä tapauksessa pelaaja itse).</param>
    void Osuma(PhysicsObject juoma, PhysicsObject kohde)
    {
        if (kohde != pelaaja || peliOhi) return;

        if (juoma.Tag.ToString() == "mieto")
        {
            pisteLaskuri.Value += 1;
            juoma.Destroy();
        }
        else if (juoma.Tag.ToString() == "vakeva")
        {
            peliOhi = true;

            juoma.Destroy();
            pelaaja.Destroy();

            mietoAjastin.Stop();
            vakevaAjastin.Stop();

            NaytaValikko();
        }
    }
    
    /// <summary>
    /// Näyttää valikon pelin päätyttyä, jossa pelaaja näkee keräämänsä pisteet ja voi valita aloittaa uudelleen tai lopettaa pelin.
    /// </summary>
    void NaytaValikko()
    {
        MultiSelectWindow valikko = new MultiSelectWindow(
            
            "Peli ohi!\nPisteesi: " + pisteLaskuri.Value,
            "Aloita uudelleen",
            "Lopeta peli"
        );

        valikko.AddItemHandler(0, AloitaUudelleen);
        valikko.AddItemHandler(1, Exit);

        Add(valikko);
    }
    
    /// <summary>
    /// Aloittaa pelin uudelleen tyhjentämällä kentän, nollaamalla pisteet ja käynnistämällä pelin alusta.
    /// </summary>
    void AloitaUudelleen()
    {
        ClearAll();
        peliOhi = false;
        Begin();
    }
}