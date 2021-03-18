using DreadBot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiteDB;
using System.Timers;

namespace CaptchaBot
{
    public class CaptchaMain : IDreadBotPlugin
    {
        public string PluginID { get { return "Captcha"; } }
        private string[] emotes = { "😀", "😄", "😁", "😆", "😅", "😂", "😉", "🙃", "🙂", "😇", "😊", "☺️", "🤣", "😌", "😍", "😘", "😗", "😙", "😚", "🧐", "🤨", "🤪", "😜", "😝", "😛", "😋", "🤓", "😎", "🤩", "😏", "😒", "😞", "😖", "😣", "☹️", "🙁", "😕", "😟", "😔", "😫", "😩", "😢", "😭", "😤", "😠", "😱", "😳", "🤯", "🤬", "😡", "😨", "😰", "😥", "😓", "🤗", "🤔", "🤭", "🙄", "😬", "😑", "😐", "😶", "🤥", "🤫", "😯", "😦", "😧", "😮", "😲", "😴", "🤤", "😪", "😵", "🤐", "🤢", "🤮", "😈", "🤠", "🤑", "🤕", "🤒", "😷", "🤧", "👿", "👹", "👺", "🤡", "💩", "👻", "💀", "😸", "😺", "🎃", "🤖", "👾", "👽", "☠️", "😹", "😻", "😼", "😽", "🙀", "😿", "😾", "👎", "👍🏼", "🤝", "👜", "👠", "👚", "👞", "🐨", "🐔", "🐜", "🕸", "🦐", "🦇", "🦓", "🐘", "🐈", "🐲", "🐄", "🍁", "🌒", "✨", "❄️", "☀️", "💐", "☘️", "🍏", "🍖", "🍭", "🎂", "🍰", "🥓", "🍘", "🍮", "🥠", "🍻", "🎾", "🤺", "🏑", "🏹", "⛸", "🎟", "🏵", "🎸", "🚑", "🚜", "🚍", "🛫", "🛳", "🛶", "🚥", "🗽", "🏕", "🏚", "🏨", "🏣", "🏜", "🏞", "🌉", "📱", "🖥", "📷", "📺", "💷", "🔩", "📡", "🗡", "🎪", "🎗", "⛷", "🎽", "🥅", "🚕", "🚛", "🚖", "🚋", "🛰", "🚦", "⛽️", "🗼", "🚀", "🏡", "🛣", "🌠", "🌁", "🏕", "🕹", "💿", "📠", "⌛️", "💰", "🔫", "💓", "💮", "🆎", "❌", "💯", "❗️", "♻️", "🔰", "💠", "🎦", "🆗", "⏏️", "⏪", "↘️", "🎵", "👁‍🗨", "⌚️", "📱", "📲", "💻", "⌨️", "🖥", "🖨", "💿", "💾", "💽", "🗜", "🕹", "🖲", "🖱", "📀", "📼", "📷", "📸", "📹", "🎥", "📽", "📻", "📺", "📠", "📟", "☎️", "📞", "🎞", "🎙", "🎚", "🎛", "⏱", "⏲", "⏰", "💡", "🔌", "🔋", "📡", "⏳", "⌛️", "🕰", "🔦", "🕯", "🛢", "💸", "💵", "⚖️", "💎", "💳", "💰", "💷", "💶", "💴", "🔫", "💣", "⚰️", "🚬", "🏺", "🏮", "🎁", "📨", "💌", "📤", "📦", "🏷", "📯", "📭", "🔓", "🔐", "🖇", "🗄", "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "💓", "💞", "💕", "❣️", "💔", "🤎", "🤍", "💗", "💖", "💘", "💝", "❌", "⭕️", "🛑", "⛔️", "📛", "🚫", "🚱", "🚳", "🚯", "🚷", "♨️", "💢", "💯", "🔞", "📵", "🚭", "❗️", "❕", "❓", "❔", "⁉️", "‼️", "💤", "🏧", "🚾", "♿️", "🅿️", "🈳", "🈂️", "🔈", "🔇", "🔉", "🔊", "🔔", "🔕", "📣", "📢", "💬", "💭", "🗯", "♠️", "♣️", "♥️", "♦️", "🃏", "🎴", "🀄️" };
        
        Timer captchaTimer;
        
        public void Init()
        {
            CaptchaDatabase.Init();
            Events.TextEvent += OnText;
            Events.JoinEvent += OnJoin;
            Events.CallbackEvent += OnCallbackQuery;

            Events.PluginConfigKeyboardRequest += PluginConfigKeyboard;
            // Create a timer
            captchaTimer = new Timer();
            // Tell the timer what to do when it elapses
            captchaTimer.Elapsed += new ElapsedEventHandler(CaptchaTimerEvent);
            // Set it to go off every five seconds
            captchaTimer.Interval = 5000;
            // And start it        
            captchaTimer.Enabled = true;
        }

        public void PostInit()
        {

        }

        public Func<long, CallbackButtonRequestArgs> PluginConfigKeyboard = (chatID) =>
        {
            return new CallbackButtonRequestArgs() { text = "Captcha", callback = "captcha:"+chatID };
        };
        private void CaptchaTimerEvent(object source, ElapsedEventArgs e) 
        {
            int count = 0;
            Dictionary<long, CaptchaCache> caches = CaptchaDatabase.GetAllCaches();
            foreach (KeyValuePair<long, CaptchaCache> cache in caches)
            {
                count += cache.Value.timers.Count;
                /*if (cache.Value.timers.Count > 0)
                {
                    DreadBot.Logger.LogDebug("Timer: " + cache.Value.timers.Values.ToList()[0]);
                }*/
                foreach (KeyValuePair<long, DateTime> timer in cache.Value.timers.Where((kvp) => kvp.Value <= e.SignalTime).ToList() )
                {
                    TakeAction(cache.Value, timer.Key); //chatid, userid
                    Cleanup(cache.Value, timer.Key);
                    CaptchaDatabase.Save(cache.Value);
                }
            }
            //DreadBot.Logger.LogDebug("Timers remaining: " + count);
        }

        internal void OnJoin(EventArgs args)
        {
            SystemMsgEventArgs joinArgs = args as SystemMsgEventArgs;
            CaptchaCache cache = CaptchaDatabase.GetCache(joinArgs.msg.chat.id);
            if (!cache.captchaEnabled)
            {
                User[] joinedUsers = joinArgs.msg.new_chat_members;
                foreach (User user in joinedUsers)
                {
                    if (!user.is_bot)
                    {
                        Cleanup(cache, user.id);
                        Methods.restrictChatMember(joinArgs.msg.chat.id, user.id, new ChatPermissions() { can_send_messages = false });
                    }
                }


                foreach (User user in joinedUsers)
                {
                    if (!user.is_bot)
                    {
                        InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
                        keyboard.addUrlButton("Solve Captcha", "https://telegram.me/" + escape_username(Configs.Me.username) + "?start=" + joinArgs.msg.chat.id + "_" + user.id + "_captcha", 0);
                        string name = user.first_name; //ming need escaping?
                        Result<Message> res = Methods.sendMessage(joinArgs.msg.chat.id, "Hello " + escape_username(name) + " ([" + user.id + "](tg://user?id=" + user.id + ")) \nYou must solve a captcha to remain in this group, or you will be removed.", keyboard: keyboard);
                        cache.welcomeMsgID.Add(user.id, res.result.message_id);

                        cache.remaining.Add(user.id, cache.testCountPreset);
                        cache.attempts.Add(user.id, cache.testCountPreset);

                        cache.timers.Add(user.id, DateTime.Now.AddMinutes(cache.timerPreset));
                    }
                }
            }
            CaptchaDatabase.Save(cache);
        }
        public static string escape_username(string s)
        {
            return Regex.Replace(s, @"([*_`\[\]])", @"\$1");
        }
        internal void OnText(EventArgs args)
        {
            Message msg = (args as MessageEventArgs).msg;
            if (msg.chat.type != "private") return;
            Match blocks = MatchTriggers(onTextTrigger, msg.text);
            if (blocks != null && blocks.Groups.Count > 0 && (!string.IsNullOrEmpty(blocks.Groups[0].Value)))
            {
                if (blocks.Groups[1].Value == "start")
                {
                    long chatID = long.Parse(blocks.Groups[2].Value);
                    CaptchaCache cache = CaptchaDatabase.GetCache(chatID);
                    long msgID; //get from database captcha:chatID:welmsgid + userID = msgID
                    if (cache.welcomeMsgID.TryGetValue(msg.from.id, out msgID))
                    {
                        Methods.deleteMessage(chatID, msgID);
                    }
                    else
                    {
                        Methods.sendMessage(msg.from.id, "This Captcha is not meant for you.");
                        return;
                    }
                    Captcha captcha = new Captcha();
                    cache.pending[msg.from.id] = captcha.GetCode();
                    Result<Message> res = Methods.sendPhoto(msg.chat.id, new StreamContent(captcha.GetImage()), "Captcha.png", "Please solve the above captcha by replying to this message");
                    if (res.ok)
                    {
                        cache.captchaMsgID[msg.from.id] = res.result.message_id;
                        //set "captcha:"+chat_id+":capmsgid", msg.from.id, res.result.message_id
                    }
                    CaptchaDatabase.Save(cache);
                }
            }
            else if (msg.reply_to_message != null)
            {
                CaptchaCache[] caches = CaptchaDatabase.FindCachesByUserIDAndMsgID(msg.from.id, msg.reply_to_message.message_id).ToArray();
                if (caches.Length > 0)
                {
                    if (caches.Length > 1)
                    {
                        DreadBot.Logger.LogWarn("more than one captcha cache found: " + string.Join(", ", caches.Select((c) => c.chatID)));
                    }
                    CaptchaCache cache = caches[0];
                    long chatID = cache.chatID;
                    string tryCaptcha = msg.text.ToLower();
                    string realCaptcha = cache.pending[msg.from.id].ToLower();
                    if (tryCaptcha.Equals(realCaptcha))
                    {
                        Methods.sendMessage(msg.chat.id, "Captcha correct!");
                        int tests = cache.remaining[msg.from.id];
                        if (tests == 1)
                        {
                            Methods.deleteMessage(msg.from.id, cache.captchaMsgID[msg.from.id]);
                            Methods.restrictChatMember(chatID, msg.from.id, new ChatPermissions() { can_send_messages = true });
                            Cleanup(cache, chatID);
                            Methods.sendMessage(chatID, "Welcome " + escape_username(msg.from.username));
                        }
                        else if (tests > 1)
                        {
                            Captcha captcha = new Captcha();
                            cache.pending[msg.from.id] = captcha.GetCode();
                            Result<Message> res = Methods.sendPhoto(msg.chat.id, new StreamContent(captcha.GetImage()), "Captcha.png", "Please solve the above captcha by replying to this message");
                            if (res.ok)
                            {
                                cache.captchaMsgID[msg.from.id] = res.result.message_id;
                                //set "captcha:"+chat_id+":capmsgid", msg.from.id, res.result.message_id
                                cache.remaining[msg.from.id] = tests - 1;
                            }
                        }
                    }
                    else
                    {
                        int attempts = cache.attempts[msg.from.id];
                        string text = "Incorrect.";
                        if (attempts == 2)
                        {
                            text = "Incorrect. Last attempt.";
                        }
                        Methods.sendMessage(msg.chat.id, text);
                        if (attempts == 1)
                        {
                            Methods.deleteMessage(msg.from.id, cache.captchaMsgID[msg.from.id]);
                            TakeAction(cache, msg.from.id);
                            Cleanup(cache, chatID);
                            Methods.sendMessage(msg.chat.id, "You have failed the captcha too many times.");
                        }
                        else
                        {
                            cache.attempts[msg.from.id] = attempts - 1;
                        }
                    }
                    CaptchaDatabase.Save(cache);
                }
            }
        }

        internal void OnCallbackQuery(EventArgs args)
        {
            CallbackQuery callback = (args as CallbackEventArgs).callbackQuery;
            Match blocks = MatchTriggers(onCallbackQueryTrigger, callback.data);
            if (blocks != null && blocks.Groups.Count > 0 && (!string.IsNullOrEmpty(blocks.Groups[0].Value)))
            {
                if (blocks.Groups[1].Value == "captcha")
                {
                    Methods.answerCallbackQuery(callback.id);
                    string menu_text = "*Captcha Settings*\n\n" +
                        "`Captcha Enabled`\nEnables/Disables the Captcha system.\n\n" +
                        "`Captcha Action`\nSets the action to take if a user fails to solve the captcha. Kick, ban or mute.\n\n" +
                        "`Captcha Tests`\nSets the number of tests/attempts a user has to solve. Min 1, Max 5, Default 3\n\n" +
                        "`Timer`\nSets the amount of time in minutes a user has to solve captcha.\n"+
                            "If this timer is exceeded, then the default action will be taken. Min 1, Max 60, default 3";

                    InlineKeyboardMarkup keyboard = CaptchaConfig(long.Parse(blocks.Groups[2].Value));
                    Methods.editMessageText(callback.from.id, callback.message.message_id, menu_text, "markdown", keyboard);
                }

            }

        }

        private static InlineKeyboardMarkup CaptchaConfig(long chatID)
        {
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            CaptchaCache cache = CaptchaDatabase.GetCache(chatID);
            string toggle = "☑️";//off

            if (cache.captchaEnabled) 
            {
                toggle = "✅";//on
            }
            int count = cache.testCountPreset;
            int timeout = cache.timerPreset;
            string action = cache.captchaAction;
            string actiontext;
            switch (action)
            {
                case "ban":
                    actiontext = "🔨";
                    break;
                case "mute":
                    actiontext = "👁";
                    break;
                default:
                    actiontext = "👞";
                    break;
            }
            keyboard.addCallbackButton("Captcha Enabled", "help:captcha:togglehelp", 0)
                .addCallbackButton(toggle, "captcha:toggle:" + chatID, 0);
            keyboard.addCallbackButton("Captcha Action", "help:captcha:actionhelp", 1)
                .addCallbackButton(actiontext, "captcha:actiontoggle:" + chatID, 1);
            keyboard.addCallbackButton("+", "captcha:add:" + chatID, 2)
                .addCallbackButton(count+"", "help:captcha:testcount", 2)
                .addCallbackButton("-", "captcha:minus:" + chatID, 2);
            keyboard.addCallbackButton("+10", "captcha:add10:" + chatID, 3)
                .addCallbackButton("+1", "captcha:add1:" + chatID, 3)
                .addCallbackButton(timeout+"", "help:captcha:timeout", 3)
                .addCallbackButton("-1", "captcha:minus1:" + chatID, 3)
                .addCallbackButton("-10", "captcha:minus10:" + chatID, 3);
            Menus.ConfigBackButton(ref keyboard, chatID);
            return keyboard;
        }

        private void TakeAction(CaptchaCache cache, long userID)
        {
            string text = "";
            Result<bool> result = null;
            try
            {
                //Delete the Captcha Notice msg from the group
                long capMsgID = cache.captchaMsgID[userID];
                Methods.deleteMessage(cache.chatID, capMsgID);
            }
            catch (KeyNotFoundException) { } //ignore failures
            Result<ChatMember> cm = Methods.getChatMember(cache.chatID, userID);
            if (cm == null)
            {
                //If the ChatMember is nil
                result = FireAction(cache.chatID, userID, cache.captchaAction);
                string acted = cache.captchaAction;
                if (acted == "ban")
                {
                    acted = "bann";
                }
                else if (acted == "mute")
                {
                    acted = "mut";
                }

                text = "User ([" + userID + "](tg://user?id=" + userID + "))" + acted + "ed for failing to solve the captcha.";

            }
            else
            {
                if (cm.ok)
                {
                    if (cm.result.status == "creator" || cm.result.status == "administrator")
                    {
                        text = escape_username(cm.result.user.first_name) + " ([" + userID + "](tg://user?id=" + userID + ")) is now an admin and cannot be banned by captcha.";
                        Cleanup(cache, userID);
                    }
                    else if (cm.result.status == "left" || cm.result.status == "kicked")
                    {
                        text = escape_username(cm.result.user.first_name) + " ([" + userID + "](tg://user?id=" + userID + ")) is no longer in the group.";
                        Cleanup(cache, userID);
                    }
                    else if (cm.result.status == "restricted" || cm.result.status == "member")
                    {
                        result = FireAction(cache.chatID, userID, cache.captchaAction);
                        string acted = cache.captchaAction;
                        if (acted == "ban")
                        {
                            acted = "bann";
                        }
                        else if (acted == "mute")
                        {
                            acted = "mut";
                        }

                        text = escape_username(cm.result.user.first_name) + " ([" + userID + "](tg://user?id=" + userID + "))" + acted + "ed for failing to solve the captcha.";
                    }
                }
                else
                {
                    //If getChatMember errored
                    result = FireAction(cache.chatID, userID, cache.captchaAction);
                    string acted = cache.captchaAction;
                    if (acted == "ban")
                    {
                        acted = "bann";
                    }
                    else if (acted == "mute")
                    {
                        acted = "mut";
                    }

                    text = "User ([" + userID + "](tg://user?id=" + userID + "))" + acted + "ed for failing to solve the captcha.";

                }
            }
            //If the bot doesnt have Restrict perms in the group, Report Permission error to group.
            if (result != null && !result.ok)
            {
                if (!string.IsNullOrEmpty(result.description))
                {
                    if (result.description.Equals("not enough rights to restrict/unrestrict chat member"))
                    {
                        Methods.sendMessage(cache.chatID, "I have " + result.description + "s.\nPlease give me proper permissions so that I may do my job.");
                        return;
                    }
                }
            }
            //if Silent mode is off, send message to group.
            //if (!Utilities.isSilentModeOn(cache.chatID))
            {
                if (result != null && result.ok)
                {
                    if (cache.captchaAction == "mute")
                    {
                        text += "\n\nYou need To contact an admin to reset the Captcha.";
                    }
                    Methods.sendMessage(cache.chatID, text);
                }
            }
        }

        private Result<bool> FireAction(long chatID, long userID, string action)
        {
            if (action == "kick")
            {
                return Methods.kickChatMember(chatID, userID);
            }
            else if (action == "ban")
            {
                return Methods.banChatMember(chatID, userID);
            }
            else if (action == "mute")
            {
                return Methods.restrictChatMember(chatID, userID, new ChatPermissions() { can_send_messages = false });
            }
            else
            {
                return null;
            }
        }

        private void Cleanup(CaptchaCache cache, long userID)
        {
            cache.captchaMsgID.Remove(userID);
            cache.pending.Remove(userID);
            cache.welcomeMsgID.Remove(userID);
            //cache.welmsg.Remove(userID);
            cache.remaining.Remove(userID);
            cache.attempts.Remove(userID);
            cache.timers.Remove(userID);
        }

        private static Match MatchTriggers(string[] triggers, string text)
        {
            if (!string.IsNullOrEmpty(text) && triggers != null && triggers.Length > 0)
            {
                text = text.Replace("^(/[%w_]+)@"+ Configs.Me.username, "%1");

                foreach(string trigger in triggers)
                {
                    Regex rx = new Regex(trigger,
                                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    Match match = rx.Match(text);

                    if(match.Groups.Count > 0 && !string.IsNullOrEmpty(match.Groups[0].Value))
                    {
                        return match;
                    }
                }
            }
            return null;
        }
        private static string[] onTextTrigger = { 
            @"^\/(start) (-?\d+)_(\d+)_captcha$"
        };

        private static string[] onCallbackQueryTrigger = {
		    @"^(captcha):(-?\d+)$", //main menu with chatID
            @"^captcha:(.*):(-?\d+)$", //config options, (toggle, actiontoggle, add, minus, add1, add10, minus1, minus10), chatID
            @"^(help):captcha:(.*)" //help prompt
        };
    }
}
