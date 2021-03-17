using DreadBot;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptchaBot
{
    public class CaptchaDatabase
    {
        private static readonly Dictionary<long, CaptchaCache> captchaCaches = new Dictionary<long, CaptchaCache>();
        private static LiteCollection<CaptchaCache> captchaDBCol;

        public static void Init()
        {
            captchaDBCol = Database.GetCollection<CaptchaCache>("captchadb");

            foreach (CaptchaCache captchaCache in captchaDBCol.FindAll())
            {
                captchaCaches.Add(captchaCache.chatID, captchaCache);
            }
        }
        internal static void Save(CaptchaCache cap)
        {
            lock (captchaDBCol)
            {
                captchaDBCol.Upsert(cap);
            }
        }

        public static Dictionary<long, CaptchaCache> GetAllCaches()
        {
            return captchaCaches;
        }

        public static IEnumerable<CaptchaCache> FindCachesByUserIDAndMsgID(long UserID, long msgID)
        {
            var caches = GetAllCaches();
            return from kvp in caches // chatID, CaptchaCache
                    from captchaMsgID in kvp.Value.captchaMsgID // UserID, MsgID
                    where captchaMsgID.Key == UserID && captchaMsgID.Value == msgID
                    select kvp.Value;
        }
        public static CaptchaCache GetCache(long ChatID)
        {
            if (captchaCaches.ContainsKey(ChatID))
            {
                return captchaCaches[ChatID];
            }
            else
            {
                CaptchaCache captchaCache = new CaptchaCache(ChatID);
                captchaCaches.Add(ChatID, captchaCache);
                Save(captchaCache);
                return captchaCache;
            }
        }
    }
}
