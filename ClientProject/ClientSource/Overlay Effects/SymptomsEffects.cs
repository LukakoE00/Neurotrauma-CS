using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Data;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurotrauma.ClientSource.OverlayEffects
{
    static class SymptomsEffects
    {

        private static Texture2D bleedingTex;

        public static void InitSymptomsEffects()
        {
            bleedingTex = TextureLoader.FromFile(Path.Combine("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Barotrauma\\LocalMods\\Neurotrauma CS", "test.png"));

            var harmony = new Harmony("neurotrauma.client.symptoms");

            var original = AccessTools.Method(typeof(GameScreen), nameof(GameScreen.Draw), [typeof(double), typeof(GraphicsDevice), typeof(SpriteBatch)]);
            harmony.Patch(original, postfix: new HarmonyMethod(typeof(SymptomsEffects), nameof(SymptomsEffects.Postfix_Draw)));

        }

        // We first update the list of afflictions that should be shown on the screen, then we draw them in the Draw method.
        // Should be updated with NT Delta time update loop thingy
        public static void UpdateShowedAffList()
        {

        }

        private static void Postfix_Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            if (Character.Controlled == null || Character.Controlled.CharacterHealth == null) return;

            float t = HF.GetAfflictionStrength(Character.Controlled, "bleeding");
            if (t <= 0) return;

            HF.Print("should see bleeding effect");
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            spriteBatch.Draw(bleedingTex, new Rectangle(0,0,GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White* 0.5f);
            spriteBatch.End();

        }
    }
}
