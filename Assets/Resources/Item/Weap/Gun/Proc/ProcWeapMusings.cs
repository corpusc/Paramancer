using UnityEngine;
using System.Collections;

public class ProcWeapMusings : MonoBehaviour {
// so, i decided the game should have different weapon sets, segregated
// into different themes/settings.
// not just for immersion, but also for variety and to have a traditional
// weapon option for people who just want something familiar without having to 
//		learn or 
//		experiment with
//		or jump thru any extra binding/selection hoops involved with procedural weapons



// IceFlame wants a different procedural weapon system based on guns with floating-point parameters instead of runes.  
// and i fully support the idea if the visual design can be worked out.
// (sounds will be another challenge, but i think that will be easier, and
// i'd be more accepting of samey sounds than i would be for the visuals)
// I don't want to spend time on it myself at this point, cuz I already
// have the fantasy magic spells system design worked out.  And I think
// just that, plus a traditional present-day'ish weapon set is enough for now.




//IceFlame's weapon power formula(based on (mostly)floating-point values instead of runes):
//				bps - bullets per second
//				dmg - damage per bullet
//				auto - full auto : bool
//				bnc - the amount of times a bullet bounces from wall to wall : int
//				proj - projectiles launched at a time : int
//				vamp - part of he damage the weapon delas that you get as health, CAN BE NEGATIVE for a weapon that deals loads of damage but costs you something, but never get down to -1.0f
//				power = bps * dmg * sqrt(bnc + 1) * proj * (auto && bps > 1.0f ? sqrt(bps) : 1) * (vamp + 1.0f)
//				You'd have a limit of, say 400 power points per weapon when creating one
//				Each spawn would have a preset amount of power points that a weapon spawning on it should have
//				So that the more dangerous spawns have more powerful weapons in them
//[10:14:56 AM] Somebody (IceFlame): http://tesseractgamedev.wordpress.com/2012/11/24/procedural-weapon-generation-in-unity/}
//
//
//
//
//[7:38:05 PM | Edited 7:41:40 PM] chattanoo: i think you better work out a procedural weapon gun model to match that, otherwise.... 
//there's no way to properly convey to the user all those weapon aspects.   look at the free game LoadOut perhaps for ideas on how 
//					they put together different handles/triggers/barrels/etc to make the weapons look a bit different.  altho their 
//						models don't look different enough IMO.
//
//the mechanics are easy.  the hard part is the visuals and sounds.
//
//my rune system will be a bit easier, since magic can sound like anything, and produce any kind of particle effects 8)
//[7:39:58 PM | Edited 7:44:53 PM] chattanoo: my rune system conveys what a weapon/spell does via the rune graphics.  i don't wanna take the 
//						time to explain (with text) how you show which of the 8 of a particular runeline it is, but its basically by different 
//						arrangements of smaller graphics on the edges of the big rune that signifies the specific powerlevel/point-on-the-spectrum.  
//                      the actual weapon model 
//						could be just one staff.   or 9 staves at the most (since the staff appearance would just replace one of the runes)
//					[7:44:16 PM] chattanoo: i would LIKE it to also have unique swappable staff heads, that visually displays the projectile 
//						spew pattern, but thats not NECESSARY
//						[7:46:59 PM] chattanoo: i didn't really think about the math that you mentioned.  so i can't comment on that.  
//						IMO you need to work out the visuals of the guns at least first before proceeding with adding the mechanics.  
//						cuz IMO they are useless without that visual design.
//						[7:48:18 PM] chattanoo: how would various bounce values appear?   how would damage per bullet appear?  etc.   
//						different sizes could be used for at least one of the aspects, but most of them will probably be hard
//							[7:49:16 PM] chattanoo: ideally it would be nice to have different procedural weapon sets for sci-fi, 
//							present day, and fantasy/magic..... but i also think having a traditional gun system is a good option, 
//							cuz some people won't want to be bothered with procweapons
//[7:49:54 PM | Edited 7:53:01 PM] chattanoo: so IMO, present day stuff could be JUST traditional weapons imo.  not ALL the weapons sets 
//have to be procedural
//[7:52:31 PM] chattanoo: sci fi guns would probably be easier for an alternate proc weap system.  cuz technology is back again towards 
//being more magical seeming and unknown.
//
//but they don't even have to be all guns.  in fact, i might do NONE of them as guns, and all of them be visualized as fancy cyber/tech 
//								gloves that faciliate PSI powers and/or "nano swarms".  which is just explaining things that look/sound 
//								very similar to magic, as being created effects from swarms of nanobots (anarchy online did that, so 
//								                                                         they could make scifi EverQuest spells/abilities)
//
}