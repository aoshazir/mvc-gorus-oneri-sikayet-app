using MVCGorusOneriSistemiApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MVCGorusOneriSistemiApp.Controllers
{
    public class YetkiliController : Controller
    {
        GorusOneriSistemiDBEntities entities = new GorusOneriSistemiDBEntities();
        // GET: Yetkili
        public ActionResult Index()
        {
            ViewBag.hataMesaj = null;
            return View();
        }

        [HttpPost]
        public ActionResult Index(string ad,string parola)
        {
            var response = Request["g-recaptcha-response"];
            const string secret = "6Ld5FAQqAAAAAIsAkEoDmdjOTwONWVc1pvALFyH6";

            var client = new WebClient();
            var reply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));

            var captchaResponse = JsonConvert.DeserializeObject<RECaptcha>(reply);

            if (captchaResponse.Success)
            {
                //TODO: Giriş işlemleri

                var yetkili = (from y in entities.Yetkililer where y.yetkiliAd == ad && y.yetkiliParola == parola select y).FirstOrDefault();

                if (yetkili != null)
                {
                    Session.Add("yetkili", yetkili);
                    return RedirectToAction("Kontrol", "Yetkili");

                }
                else
                {
                    ViewBag.hataMesaj = "Sistemde yetkili bulunamadı";
                    return View();
                }
            }
            else
            {
                //TODO: Güvenlik doğrulama işlemleri
                ViewBag.hataMesaj = "Lütfen güvenliği doğrulayınız";
                return View();
            }
        }

        public ActionResult Kontrol()
        {
            var yetkili = (Yetkililer)Session["yetkili"];
            if(yetkili == null)
            {
                return RedirectToAction("Index");
            }

            List<YetkiTalepler> listTalep=new List<YetkiTalepler>();

            foreach (var birim in yetkili.YetkiliBirimler)
            {
                var talepler = (from t in entities.Talepler where t.birimId == birim.birimId && t.durumId!=3 select t).ToList();

                foreach (var talep in talepler)
                {
                    YetkiTalepler yt = new YetkiTalepler();

                    yt.id = talep.talepId;
                    yt.talepBaslik=talep.talepBaslik;
                    yt.olusturulmaTarihi=Convert.ToDateTime(talep.olusturulmaTarihi);
                    yt.birimId = talep.birimId;
                    yt.birimAd = talep.Birimler.birimAd;
                    listTalep.Add(yt);

                }
            }

            return View(listTalep);
        }

        public ActionResult Cevaplanan()
        {
            var yetkili = (Yetkililer)Session["yetkili"];
            if (yetkili == null)
            {
                return RedirectToAction("Index");
            }

            List<YetkiTalepler> listTalep = new List<YetkiTalepler>();

            foreach (var birim in yetkili.YetkiliBirimler)
            {
                var talepler = (from t in entities.Talepler where t.birimId == birim.birimId && t.durumId == 3 select t).ToList();

                foreach (var talep in talepler)
                {
                    YetkiTalepler yt = new YetkiTalepler();

                    yt.id = talep.talepId;
                    yt.talepBaslik = talep.talepBaslik;
                    yt.olusturulmaTarihi = Convert.ToDateTime(talep.olusturulmaTarihi);
                    yt.birimId = talep.birimId;
                    yt.birimAd = talep.Birimler.birimAd;
                    listTalep.Add(yt);

                }
            }

            return View(listTalep);
        }

        public ActionResult Detay(int id)
        {
            var talep = entities.Talepler.Where(t => t.talepId == id).FirstOrDefault();
            if (talep.durumId != 3)
            {
                talep.durumId = 2;
                entities.SaveChanges();
            }
            return View(talep);
        }

        [HttpPost]
        public ActionResult Detay(string cevap,int id)
        {
            var talep = entities.Talepler.Where(t => t.talepId == id).FirstOrDefault();
            talep.durumId = 3;
            talep.talepCevap= cevap;
            entities.SaveChanges();
            return RedirectToAction("Kontrol");
        }


        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index");
        }
    }
}