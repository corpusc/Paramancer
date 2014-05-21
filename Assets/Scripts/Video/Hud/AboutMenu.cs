using UnityEngine;
using System.Collections;

public class AboutMenu {
	bool showingFarFuture = false;

	public bool Draw(Hud hud) {
		string s = "";

		if (showingFarFuture) {
			GUILayout.Label("");
			hud.CategoryHeader("FAR FUTURE GOALS");
			GUILayout.Label("");
			hud.CategoryHeader("Multi-user map editing....AND");
			hud.CategoryHeader("FURTHER Realms & Weapon Sets", true, false);
			s += "With voxels only at first.  The easiest and fastest way to build things.  ";
			s += "Each realm/dimension/time-period will use a particular set of weapons.  ";
			s += "For immersion, the various weapon sets will be pre-selected & enforced per realm/dimension.  ";
			s += "Instanced multiplayer however would allow for mixing it up with the weapons.  ";
			s += "Here's some example weapon sets we're thinking of: \n\n";
			s += "Sci-Fi guns \nSci-Fi NanoSwarms \nPsi-Fi amplified psionic powers \n(the latter 2 being hand attachments or tech \"gloves\").  \n\n";
			s += "In order to keep most potential map creators inspired to build in the Paraverse, ";
			s += "we will half-assedly attempt to let users to build in whatever setting/theme they prefer, ";
			s += "limiting ourselves (initially) to the main 3-4 most popular settings.  ";
			s += "Some possibilities:\n\n";
			s += "Modern Day-ish (albeit BUILD-games-style), \nSci-Fi, \nCyberPunk, \nSteamPunk, \nWestern, \nVarious olden-time periods, ";
			s += "\nPurgatory/AfterLife/Ghost/Undead Realm, \nDream/Abstract Realm, \nToon Realm, \nApocalypse Du Jour\n\n";
			s += "Anything that doesn't easily reconcile within \"the real world\" of Paramancer's fiction, can be ";
			s += "represented as cyberspace/metaverse/VR.  ";
			GUILayout.Label(s);
			s = "";


			hud.CategoryHeader("Create ANY 3D Shapes");
			s += "Enabling high fidelity, high polygon, richly detailed 3D designs of any kind.  ";
			s += "Voxel style building will remain as a rapid development tool.  So you can quickly \"block out\" rough designs that can be expanded upon with more love, time & details.  ";
			GUILayout.Label(s);
			s = "";


			hud.CategoryHeader("Persistent Open World");
			s += "Introduce a very-large, fully-explorable, contiguous, chunked overworld terrain.  ";
			s += "Hand-place the best & most interesting indoor maps & dungeons (mostly built by users) ";
			s += "into select underground areas.  And outdoor maps onto the world terrain.  ";
			s += "Like in prior stages of the game, users can choose their preferred ";
			s += "scale & type of multiplayer.  ";
			s += "Users will be able to build tentative designs in the open world as well.  ";
			s += "They will be viewable by others, but remain as transparent phantom objects ";
			s += "(optionally invisible) until a jury decides that it is worthy.  ";
			s += "Otherwise, it gets stored in that players private dimension.  Which they can invite other players to visit. ";
			s += "The users will NOT be limited to building only on their own property.  They can make additions/improvements to the ENTIRE world!  ";
			s += "The open world gameplay WILL NOT have vertical (power) progression of any kind.  ";
			s += "You will collect weapons (& maybe OTHER items) which will persist, but since the ";
			s += "weapons are designed to be balanced, it will be strictly a horizontal progression.  ";
			s += "Would consider making travel through purgatory & dream realms mandatory.  The former upon death, and ";
			s += "the latter by having an exhaustion/sleep meter that needs to be managed.  Likely the duration could be brief.  ";
			GUILayout.Label(s);


			if (GUILayout.Button("NEAR-TERM GOALS")) {
				showingFarFuture = false;
				return true;
			}
		}else{
			hud.CategoryHeader("At The Moment");
			s += "...this is a fully functional, fast-paced, old-school, arena-style multiplayer FPS.  ";
			s += "That game-play foundation will never change.  ";
			s += "However, there are not many populated indie multiplayer games out there.  ";
			s += "So, I have quite a few OTHER stages/layers planned for the future.  ";
			GUILayout.Label(s);
			s = "";
			
			
			GUILayout.Label("");
			hud.CategoryHeader("NEAR-TERM GOALS");
			hud.CategoryHeader("Rogue-lite FPS");
			s += "I loved this concept since around 2006, but have been disappointed with the few that recently came onto the market.  ";
			s += "The theme/setting would be somewhere between modern/near-future and sci-fi.  ";
			s += "We will generate new maps, & item/monster-placements (in SP) for every game session.  ";
			s += "We will have single player, coop AND EVEN PVP on Roguelike maps.  Eventually we will ";
			s += "also allow dungeon invasions from others (PURELY OPTIONAL).  Choices.  Once this stage is \"done\", the game ";
			s += "will be released.  This ends the scope of what I want to promise on Kickstarter.  ";
			s += "And I might even drop a \"minor\" feature or 2.  :)";
			GUILayout.Label(s);
			s = "";
			
			
			hud.CategoryHeader("Procedural weapons");
			s += "This stage would introduce a magical/medieval-fantasy theme/setting.  ";
			s += "The new weapon set will be magical spells & staves.  ";
			s += "You will 'create' spells on-the-fly, by selecting a combination of magic runes.  The combo determines how the spell works.  ";
			s += "This will mean possibly millions of spells.  Depending on how granular the rune/component selection ends up being.  ";
			s += "However they are designed to be balanced against each other in power.  A horizontal progression.  ";
			s += "Giving you more variety & choice in how to ";
			s += "divide & deliver your (potential) damage over the desired TTK period.  ";
			s += "Not a power-level reward/advantage given for in-game grinding.  ";
			s += "The traditional 90's style guns will remain as an option.  ";
			GUILayout.Label(s);
			s = "";


			if (GUILayout.Button("FAR FUTURE GOALS")) {
				showingFarFuture = true;
				return true;
			}
		}

		return false;
	}
}
