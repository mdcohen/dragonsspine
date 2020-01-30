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
using DragonsSpine.Mail.GameMail;

namespace DragonsSpine.GameEvents
{
    [GameEventAttribute("Thanksgiving 2015 Gift", "The developers wish to thank all of the players who have aided in recreating the game we all enjoyed.",
        "11/26/2015", "11/30/2015")]
    public class Thanksgiving2015Event : IGameEventHandler
    {
        public GameEvent ReferenceGameEvent { get; set; }

        public void OnStart() { }

        public void OnFinish() { }

        public void OnTick(object sender, System.Timers.ElapsedEventArgs eventArgs)
        {
            // Given a server crash or restart doesn't occur...
            foreach (PC pc in Character.PCInGameWorld)
            {
                if (ReferenceGameEvent.ParticipantsUniqueIDList.Contains(pc.UniqueID))
                    return;

                // Now prepare a mail message since they haven't participated.
                GameMailMessage gameMail = new GameMailMessage(GameMailMessage.DEVELOPER_ID, pc.UniqueID, "Happy Thanksgiving Weekend",
                    "The developers would like to thank you for helping us to recreate the game we all enjoyed. Attached to this mail is a unique item.", true);

                Item giftItem = new Item(Item.CopyItemFromDictionary(Item.ID_HEALTH_REGENERATION_RING));

                giftItem.AttuneItem(pc);

                gameMail.MailItemAttachment = new GameMailAttachment(giftItem, 0);

                gameMail.Send();

                if(!ReferenceGameEvent.ParticipantsUniqueIDList.Contains(pc.UniqueID))
                    ReferenceGameEvent.ParticipantsUniqueIDList.Add(pc.UniqueID);

            }

            // Send mail message from developers and attach ring of regeneration item.
            // Add chr unique ID to list.
        }
    }
}
