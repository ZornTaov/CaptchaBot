using System;
using System.Collections.Generic;

namespace CaptchaBot
{
    public class CaptchaCache
    {
        public CaptchaCache() : this(0)
        { }
        public CaptchaCache(long chat_ID)
        {
            captchaEnabled = false;
            chatID = chat_ID;
            welcomeMsgID = new Dictionary<long, long>();
            captchaMsgID = new Dictionary<long, long>();
            captchaAction = "kick";
            timerPreset = 3;
            testCountPreset = 3;
            pending = new Dictionary<long, string>();
            remaining = new Dictionary<long, int>();
            attempts = new Dictionary<long, int>();
            timers = new Dictionary<long, DateTime>();
        }
        public int id { get; set; } //Database placeholder.
        public bool captchaEnabled { get; set; }
        public long chatID { get; set; }

        public Dictionary<long, long> welcomeMsgID { get; set; } // userID -> welcome message ID in group
        public Dictionary<long, long> captchaMsgID { get; set; } // userID -> captcha message ID in private chat

        public string captchaAction { get; set; } // action taken when user fails captcha (mute, kick, ban)
        public int timerPreset { get; set; } // preset time until user auto-fails
        public int testCountPreset { get; set; } // preset number of attempts user has
        public Dictionary<long, string> pending { get; set; } // captcha code for user in this chat
        public Dictionary<long, int> remaining { get; set; } // userID -> number of captcha's remaining
        public Dictionary<long, int> attempts { get; set; } // userID -> number of attempts remaining
        public Dictionary<long, DateTime> timers { get; set; } // userID -> time remaining for user
    }
}
