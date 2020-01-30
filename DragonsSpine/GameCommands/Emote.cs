#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace DragonsSpine.GameSystems.Commands
{
    class Emote
    {
        Character chr = null;

        #region Emotes
        public void applaud(string args)
        {
            if (args == null || args == "")
            {
                chr.SendToAllInSight(chr.Name + " applauds the performance.");
                chr.WriteToDisplay("You applaud the performance.");
            }
        }
        public void babble(string args)
        {
            chr.SendToAllInSight(chr.Name + " babbles incoherently.");
            chr.WriteToDisplay("You babble incoherently.");
        }
        public void bark(string args)
        {
            chr.SendToAllInSight(chr.Name + " barks like a rabid dog.");
            chr.WriteToDisplay("You bark like a rabid dog.");
        }
        public void beam(string args)
        {
            chr.SendToAllInSight(chr.Name + " beams.");
            chr.WriteToDisplay("You beam.");
        }
        public void bite(string args)
        {
            chr.SendToAllInSight(chr.Name + " bites " + Character.POSSESSIVE[(int)chr.gender].ToLower() + "lip.");
            chr.WriteToDisplay("You bite your lip.");
        }
        public void blink(string args)
        {
            chr.SendToAllInSight(chr.Name + " blinks.");
            chr.WriteToDisplay("You blink.");
        }
        public void blush(string args)
        {
            chr.SendToAllInSight(chr.Name + " blushes.");
            chr.WriteToDisplay("You blush.");
        }
        public void bounce(string args)
        {
            chr.SendToAllInSight(chr.Name + " bounces.");
            chr.WriteToDisplay("You bounce.");
        }
        public void bow(string args)
        {
            chr.SendToAllInSight(chr.Name + " bows.");
            chr.WriteToDisplay("You bow.");
        }
        public void burp(string args)
        {
            chr.SendToAllInSight(chr.Name + " belches.");
            chr.WriteToDisplay("You burp.");
        }
        public void cackle(string args)
        {
            chr.SendToAllInSight(chr.Name + " cackles.");
            chr.WriteToDisplay("You cackle.");
        }
        public void chant(string args)
        {
            chr.SendToAllInSight(chr.Name + " chants.");
            chr.WriteToDisplay("You chant.");
        }
        public void chuckle(string args)
        {
            chr.SendToAllInSight(chr.Name + " chuckles to " + Character.POSSESSIVE[(int)chr.gender].ToLower() + "self.");
            chr.WriteToDisplay("You chuckle to yourself.");
        }
        public void clap(string args)
        {
            chr.SendToAllInSight(chr.Name + " claps.");
            chr.WriteToDisplay("You clap.");
        }
        public void clench(string args)
        {
            chr.SendToAllInSight(chr.Name + " clenches every muscle in " + Character.POSSESSIVE[(int)chr.gender].ToLower() + "body.");
            chr.WriteToDisplay("You clench every muscle in your body.");
        }
        public void cough(string args)
        {
            chr.SendToAllInSight(chr.Name + " coughs.");
            chr.WriteToDisplay("You cough.");
        }
        public void cringe(string args)
        {
            chr.SendToAllInSight(chr.Name + " cringes in fear.");
            chr.WriteToDisplay("You cringe with fear.");
        }
        public void cry(string args)
        {
            chr.SendToAllInSight(chr.Name + " cries.");
            chr.WriteToDisplay("You cry.");
        }
        public void curtsy(string args)
        {
            chr.SendToAllInSight(chr.Name + " curtsies.");
            chr.WriteToDisplay("You curtsy.");
        }
        public void dance(string args)
        {
            chr.SendToAllInSight(chr.Name + " dances.");
            chr.WriteToDisplay("You dance.");
        }
        public void drool(string args)
        {
            chr.SendToAllInSight(chr.Name + " drools.");
            chr.WriteToDisplay("You drool.");
        }
        public void duck(string args)
        {
            chr.SendToAllInSight(chr.Name + " ducks.");
            chr.WriteToDisplay("You duck.");
        }
        public void fidget(string args)
        {
            chr.SendToAllInSight(chr.Name + " fidgets.");
            chr.WriteToDisplay("You fidget.");
        }
        public void flex(string args)
        {
            chr.SendToAllInSight(chr.Name + " flexes " + Character.POSSESSIVE[(int)chr.gender].ToLower() + " muscles.");
            chr.WriteToDisplay("You flex your muscles.");
        }
        public void flinch(string args)
        {
            chr.SendToAllInSight(chr.Name + " flinches.");
            chr.WriteToDisplay("You flinch.");
        }
        public void frown(string args)
        {
            chr.SendToAllInSight(chr.Name + " frowns deeply.");
            chr.WriteToDisplay("You frown deeply.");
        }
        public void gag(string args)
        {
            chr.SendToAllInSight(chr.Name + " gags.");
            chr.WriteToDisplay("You gag.");
        }
        public void gasp(string args)
        {
            chr.SendToAllInSight(chr.Name + " gasps!");
            chr.WriteToDisplay("You gasp!");
        }
        public void giggle(string args)
        {
            chr.SendToAllInSight(chr.Name + " giggles.");
            chr.WriteToDisplay("You giggle.");
        }
        public void glare(string args)
        {
            chr.SendToAllInSight(chr.Name + " glares vehemently!");
            chr.WriteToDisplay("You glare vehemently!");
        }
        public void glower(string args)
        {
            chr.SendToAllInSight(chr.Name + " glower.");
            chr.WriteToDisplay("You glower.");
        }
        public void gnash(string args)
        {
            chr.SendToAllInSight(chr.Name + " gnashes " + Character.POSSESSIVE[(int)chr.gender].ToLower() + " teeth.");
            chr.WriteToDisplay("You gnash your teeth.");
        }
        public void grin(string args)
        {
            chr.SendToAllInSight(chr.Name + " grins like a cheshire cat.");
            chr.WriteToDisplay("You grin like a cheshire cat.");
        }
        public void grit(string args)
        {
            chr.SendToAllInSight(chr.Name + " grits " + Character.POSSESSIVE[(int)chr.gender].ToLower() + " teeth.");
            chr.WriteToDisplay("You grit your teeth.");
        }
        public void grunt(string args)
        {
            chr.SendToAllInSight(chr.Name + " grunts.");
            chr.WriteToDisplay("You grunt.");
        }
        public void groan(string args)
        {
            chr.SendToAllInSight(chr.Name + " groans.");
            chr.WriteToDisplay("You groan.");
        }
        public void grumble(string args)
        {
            chr.SendToAllInSight(chr.Name + " grumbles.");
            chr.WriteToDisplay("You grumble.");
        }
        public void gulp(string args)
        {
            chr.SendToAllInSight(chr.Name + " gulps loudly.");
            chr.WriteToDisplay("You gulp loudly.");
        }
        public void hiccup(string args)
        {
            chr.SendToAllInSight(chr.Name + " hiccups.");
            chr.WriteToDisplay("You hiccup.");
        }
        public void hiss(string args)
        {
            chr.SendToAllInSight(chr.Name + " hisses.");
            chr.WriteToDisplay("You hiss.");
        }
        public void howl(string args)
        {
            chr.SendToAllInSight(chr.Name + " howls like a wolf.");
            chr.WriteToDisplay("You howl like a wolf.");
        }
        public void hug(string args)
        {
            if (args == null || args == "")
            {
                chr.SendToAllInSight(chr.Name + " hugs " + Character.POSSESSIVE[(int)chr.gender].ToLower() + "self.");
                chr.WriteToDisplay("You hug yourself.");
            }
            else
            {
                chr.SendToAllInSight(chr.Name + " hugs " + args + ".");
                chr.WriteToDisplay("You hug " + args + ".");
            }
        }
        public void kiss(string args)
        {
            if (args == null || args == "")
            {
                chr.SendToAllInSight(chr.Name + " flagrantly kisses the air.");
                chr.WriteToDisplay("You flagrantly kiss the air.");
            }
            else
            {
                chr.SendToAllInSight(chr.Name + " kisses " + args + ".");
                chr.WriteToDisplay("You kiss " + args + ".");
            }
        }
        public void laugh(string args)
        {
            chr.SendToAllInSight(chr.Name + " laughs.");
            chr.WriteToDisplay("You laugh.");
        }
        public void lick(string args)
        {
            if (args == null || args == "")
            {
                chr.SendToAllInSight(chr.Name + " licks " + Character.POSSESSIVE[(int)chr.gender].ToLower() + "lips.");
                chr.WriteToDisplay("You lick your lips.");
            }
            else
            {
                chr.SendToAllInSight(chr.Name + " licks " + args + ".");
                chr.WriteToDisplay("You lick " + args + ".");
            }
        }
        public void moan(string args)
        {
            chr.SendToAllInSight(chr.Name + " moans.");
            chr.WriteToDisplay("You moan.");
        }
        public void nod(string args)
        {
            chr.SendToAllInSight(chr.Name + " nods.");
            chr.WriteToDisplay("You nod.");
        }
        public void pinch(string args)
        {
            if (args == null || args == "")
            {
                chr.SendToAllInSight(chr.Name + " pinches nothing.");
                chr.WriteToDisplay("You pinch nothing.");
            }
            else
            {
                chr.SendToAllInSight(chr.Name + " pinches " + args + ".");
                chr.WriteToDisplay("You pinch " + args + ".");
            }
        }
        public void roar(string args)
        {
            chr.SendToAllInSight(chr.Name + " roars like a tiger.");
            chr.WriteToDisplay("You roar like a tiger.");
        }
        public void rub(string args)
        {
            if (args == null || args == "")
            {
                chr.SendToAllInSight(chr.Name + " rubs " + Character.POSSESSIVE[(int)chr.gender].ToLower() + " hands.");
                chr.WriteToDisplay("You rub your hands.");
            }
            else
            {
                chr.SendToAllInSight(chr.Name + " rubs " + args + ".");
                chr.WriteToDisplay("You rub " + args + ".");
            }
        }
        public void scowl(string args)
        {
            chr.SendToAllInSight(chr.Name + " scowls.");
            chr.WriteToDisplay("You scowl.");
        }
        public void scream(string args)
        {
            chr.SendToAllInSight(chr.Name + " screams.");
            chr.WriteToDisplay("You scream.");
        }
        public void shake(string args)
        {
            if (args == null || args == "")
            {
                chr.SendToAllInSight(chr.Name + " shakes like a bowl of gelatin.");
                chr.WriteToDisplay("You shake like a bowl of gelatin.");
            }
            else
            {
                chr.SendToAllInSight(chr.Name + " shakes " + Character.POSSESSIVE[(int)chr.gender].ToLower() + " head.");
                chr.WriteToDisplay("You shake your head.");
            }
        }
        public void shrug(string args)
        {
            chr.SendToAllInSight(chr.Name + " shrugs.");
            chr.WriteToDisplay("You shrug.");
        }
        public void shudder(string args)
        {
            chr.SendToAllInSight(chr.Name + " shudders.");
            chr.WriteToDisplay("You shudder.");
        }
        public void sigh(string args)
        {
            chr.SendToAllInSight(chr.Name + " sighs.");
            chr.WriteToDisplay("You sigh.");
        }
        public void smile(string args)
        {
            chr.SendToAllInSight(chr.Name + " smiles.");
            chr.WriteToDisplay("You smile.");
        }
        public void snarl(string args)
        {
            chr.SendToAllInSight(chr.Name + " snarls.");
            chr.WriteToDisplay("You snarl.");
        }
        public void sneer(string args)
        {
            chr.SendToAllInSight(chr.Name + " sneers.");
            chr.WriteToDisplay("You sneer.");
        }
        public void sneeze(string args)
        {
            chr.SendToAllInSight(chr.Name + " sneezes.");
            chr.WriteToDisplay("You sneeze.");
        }
        public void snicker(string args)
        {
            chr.SendToAllInSight(chr.Name + " snickers.");
            chr.WriteToDisplay("You snicker.");
        }
        public void sniff(string args)
        {
            chr.SendToAllInSight(chr.Name + " sniffs.");
            chr.WriteToDisplay("You sniff.");
        }
        public void squeal(string args)
        {
            chr.SendToAllInSight(chr.Name + " squeals like a stuck boar!");
            chr.WriteToDisplay("You squeal like a stuck boar!");
        }
        public void stretch(string args)
        {
            chr.SendToAllInSight(chr.Name + " stretches " + Character.POSSESSIVE[(int)chr.gender].ToLower() + " arms and legs.");
            chr.WriteToDisplay("You stretch your arms and legs.");
        }
        public void strut(string args)
        {
            chr.SendToAllInSight(chr.Name + " struts like a banty rooster.");
            chr.WriteToDisplay("You strut like a banty rooster.");
        }
        public void wail(string args)
        {
            chr.SendToAllInSight(chr.Name + " wails like a banshee.");
            chr.WriteToDisplay("You wail like a banshee.");
        }
        public void wave(string args)
        {
            chr.SendToAllInSight(chr.Name + " waves.");
            chr.WriteToDisplay("You wave.");
        }
        public void whimper(string args)
        {
            chr.SendToAllInSight(chr.Name + " whimpers.");
            chr.WriteToDisplay("You whimper.");
        }
        public void whine(string args)
        {
            chr.SendToAllInSight(chr.Name + " whines.");
            chr.WriteToDisplay("You whine.");
        }
        public void whistle(string args)
        {
            chr.SendToAllInSight(chr.Name + " whistles.");
            chr.WriteToDisplay("You whistle.");
        }
        public void wince(string args)
        {
            chr.SendToAllInSight(chr.Name + " winces.");
            chr.WriteToDisplay("You wince.");
        }
        public void wink(string args)
        {
            chr.SendToAllInSight(chr.Name + " winks.");
            chr.WriteToDisplay("You wink.");
        }
        public void yawn(string args)
        {
            chr.SendToAllInSight(chr.Name + " yawns.");
            chr.WriteToDisplay("You yawn.");
        }
        public void yelp(string args)
        {
            chr.SendToAllInSight(chr.Name + " yelps.");
            chr.WriteToDisplay("You yelp.");
        }
        #endregion
    }


}
