using Jypeli;

/// <summary>
/// Viinaralli, jossa pelaaja kerää mietoja juomia ja välttää vahvoja juomia.
/// @author Sjogren
/// @version 1.0
/// </summary>
public class Harjoitustyo : PhysicsGame
{
    //Pelissä käytettävät oliot ja muuttujat
    private PhysicsObject pelaaja;
    private IntMeter pisteLaskuri;
    private Timer mietoAjastin;
    private Timer vakevaAjastin;
    private bool peliOhi = false;
    private Image[] miedotKuvat;
    private Image[] vakevatKuvat;
    private Image pelaajanKuva;

    //Pelissä käytettävät vakiot
    private const double MIEDON_JUOMAN_NOPEUS = -400;
    private const double VAKEVAN_JUOMAN_NOPEUS = -500;
    private const double JUOMAN_LEVEYS = 50;
    private const double JUOMAN_KORKEUS = 90;

    //Pelissä käytettävät kuvat
    private string[] MIEDOT_JUOMAT = { "ananas", "harmaa", "lemonade", "raspberry", "paaryna", "lime", "vadelma" };
    private string[] VAKEVAT_JUOMAT = { "tapio", "minttu", "sviina" };

    public override void Begin()
    {
        miedotKuvat = new Image[MIEDOT_JUOMAT.Length];
        vakevatKuvat = new Image[VAKEVAT_JUOMAT.Length];

        //Ladataan kuvat käyttäen juomat taulukkoja
        for (int i = 0; i < MIEDOT_JUOMAT.Length; i++)
        {
            miedotKuvat[i] = LoadImage(MIEDOT_JUOMAT[i]);
        }

        for (int i = 0; i < VAKEVAT_JUOMAT.Length; i++)
        {
            vakevatKuvat[i] = LoadImage(VAKEVAT_JUOMAT[i]);
        }

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
    private void LuoKentta()
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
    private void LuoPelaaja()
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
    private void AloitaAjastimet()
    {
        mietoAjastin = new Timer();
        mietoAjastin.Interval = 1.0;
        mietoAjastin.Timeout += delegate { LuoJuoma("mieto", MIEDON_JUOMAN_NOPEUS, miedotKuvat);};
        mietoAjastin.Start();

        vakevaAjastin = new Timer();
        vakevaAjastin.Interval = 2.0;
        vakevaAjastin.Timeout += delegate { LuoJuoma("vakeva", VAKEVAN_JUOMAN_NOPEUS, vakevatKuvat); };
        vakevaAjastin.Start();
    }
    

    /// <summary>
    /// Luo uusi juoma pelikentällle
    /// </summary>
    /// <param name="tag">Juoman tagi, vakeva tai mieto</param>
    /// <param name="nopeus">Juoman putoamisnopeus</param>
    /// <param name="kuvat">Käytettävien kuvien taulukko</param>
    private void LuoJuoma(string tag, double nopeus, Image[] kuvat)
    {
        PhysicsObject juoma = new PhysicsObject(JUOMAN_LEVEYS, JUOMAN_KORKEUS);
        juoma.Image = RandomGen.SelectOne(kuvat);
        juoma.X = RandomGen.NextDouble(Level.Left, Level.Right);
        juoma.Y = Level.Top;
        juoma.Tag = tag;

        juoma.Hit(new Vector(0, nopeus));

        Add(juoma);
        AddCollisionHandler(juoma, Osuma);
    }
    

    /// <summary>
    /// Käsittelee osuman pelaajan ja juoman välillä. Jos pelaaja osuu miedon juoman kanssa, pelaaja saa pisteen ja juoma tuhoutuu.
    /// Jos pelaaja osuu vahvan juoman kanssa, peli päättyy, pelaaja ja juoma tuhoutuvat, ja näytetään valikko, jossa pelaaja voi aloittaa uudelleen tai lopettaa pelin.
    /// </summary>
    /// <param name="juoma">Juoma, johon pelaaja osui.</param>
    /// <param name="kohde">Kohde, johon pelaaja osui (tässä tapauksessa pelaaja itse).</param>
    private void Osuma(PhysicsObject juoma, PhysicsObject kohde)
    {
        if (kohde != pelaaja || peliOhi) return;

        if (juoma.Tag.ToString() == "mieto")
        {
            pisteLaskuri.Value = LaskePisteet(pisteLaskuri.Value);
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
    /// Palauttaa pelaajan uudet pisteet.
    /// </summary>
    /// <param name="pisteet">Pelaajan pisteiden nykytilanne</param>
    /// <returns>Uudet pisteet</returns>
    private static int LaskePisteet(int pisteet)
    {
        return pisteet + 1;
    }
    
    
    /// <summary>
    /// Näyttää valikon pelin päätyttyä, jossa pelaaja näkee keräämänsä pisteet ja voi valita aloittaa uudelleen tai lopettaa pelin.
    /// </summary>
    private void NaytaValikko()
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
    private void AloitaUudelleen()
    {
        ClearAll();
        peliOhi = false;
        Begin();
    }
}