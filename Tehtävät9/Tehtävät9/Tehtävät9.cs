using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author sjogr
/// @version 11.03.2026
/// <summary>
/// Tehdään silmukoita käyttämällä 8x8 shakkilauta. Vasemmassa alareunassa olevan ruudun tulee olla valkoinen.
/// </summary>
public class Tehtävät9 : PhysicsGame
{
    public override void Begin()
    {
        int ruudunKoko = 40;
        int ruudut = 8;

        // Valkoiset ruudut
        for (int i = 0; i < ruudut * ruudut; i++)
        {
            int x = i % ruudut;
            int y = i / ruudut;

            // Ruudut, joiden x+y on parillinen, ovat valkoisia
            if ((x + y) % 2 == 0)
            {
                var ruutu = new GameObject(ruudunKoko, ruudunKoko, Shape.Rectangle);
                // Asetetaan ruudun keskipiste oikeaan kohtaan
                ruutu.Position = new Vector(x * ruudunKoko + ruudunKoko / 2 - 40, y * ruudunKoko + ruudunKoko / 2 - 40);
                ruutu.Color = Color.White;
                Add(ruutu);
            }
        }

        // Mustat ruudut
        for (int i = 0; i < ruudut * ruudut; i++)
        {
            int x = i % ruudut;
            int y = i / ruudut;

            // Ruudut, joiden x+y on pariton, ovat mustia
            if ((x + y) % 2 == 1)
            {
                var ruutu = new GameObject(ruudunKoko, ruudunKoko, Shape.Rectangle);
                // Asetetaan ruudun keskipiste oikeaan kohtaan
                ruutu.Position = new Vector(x * ruudunKoko + ruudunKoko / 2 - 40, y * ruudunKoko + ruudunKoko / 2 - 40);
                ruutu.Color = Color.Black;
                Add(ruutu);
            }
        }

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
}

