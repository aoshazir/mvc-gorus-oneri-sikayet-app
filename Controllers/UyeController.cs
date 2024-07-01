using MVCGorusOneriSistemiApp.Filters;
using MVCGorusOneriSistemiApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCGorusOneriSistemiApp.Controllers
{
    public class UyeController : Controller
    {
        GorusOneriSistemiDBEntities entities=new GorusOneriSistemiDBEntities();
        // GET: Uye

        [UyelikKontrol]
        public ActionResult Index()
        {
            var uye = UyeGetir();

            ViewBag.guncellendi = false;

            return View(uye);
        }

        [HttpPost]
        public ActionResult Index(string adsoyad,string telefon)
        {
            var uye = UyeGetir();

            uye.uyeTelefon= telefon;
            uye.uyeAdSoyad= adsoyad;
            entities.SaveChanges();

           

            ViewBag.guncellendi = true;

            return View(uye);
        }

        [UyelikKontrol,UyeAdKontrol]
        public ActionResult TalepOlustur()
        {

            var uye = UyeGetir();
            ViewBag.birimler = BirimlerGetir();
            ViewBag.tipler = TiplerGetir();

            ViewBag.hataMesaj = null;
            ViewBag.basariliMesaj = null;
            return View();
        }

        [HttpPost]
        public ActionResult TalepOlustur(string baslik,string mesaj ,int birim,int tip)
        {
            try
            {
                var uye = UyeGetir();
                Talepler yeniTalep = new Talepler();

                yeniTalep.uyeId = uye.uyeId;
                yeniTalep.birimId = birim;
                yeniTalep.tipId = tip;
                yeniTalep.talepBaslik = baslik;
                yeniTalep.talepMesaj = mesaj;
                yeniTalep.olusturulmaTarihi=DateTime.Now;
                yeniTalep.durumId = 1;

                entities.Talepler.Add(yeniTalep);
                entities.SaveChanges();

                ViewBag.basariliMesaj = "Talep ekleme başarılı. Talepler sayfasına yönlendiriliyorsunuz";
            }
            catch (Exception)
            {

                ViewBag.hataMesaj = "Beklenmeyen bir hata oluştu";
            }

            ViewBag.birimler = BirimlerGetir();
            ViewBag.tipler = TiplerGetir();

            return View();
        }

        [UyelikKontrol,UyeAdKontrol]
        public ActionResult Talepler()
        {
            var uye = UyeGetir();
            var talepler=entities.Talepler.Where(t=>t.uyeId == uye.uyeId).OrderByDescending(t=>t.olusturulmaTarihi).ToList();
            return View(talepler);
        }

        [UyelikKontrol, UyeAdKontrol]
        public ActionResult Detay(int id)
        {
            var uye = UyeGetir();
            var talep = entities.Talepler.Where(t => t.talepId == id).FirstOrDefault();
            return View(talep);
        }


        public Uyeler UyeGetir()
        {
            int uyeId = Convert.ToInt32(Session["uyeId"]);
            var uye = entities.Uyeler.Where(u => u.uyeId == uyeId).FirstOrDefault();
            ViewBag.uyeEmail = uye.uyeEmail;

            if (uye.uyeAdSoyad != null && uye.uyeAdSoyad != "")
                ViewBag.style = "alert alert-success my-5 text-center";
            else
                ViewBag.style = "alert alert-danger my-5 text-center";

            return uye;
        }

        public List<Birimler> BirimlerGetir()
        {
            var birimler =entities.Birimler.Where(b=>b.birimAktiflik==true).ToList();
            return birimler;
        }

        public List<Tipler> TiplerGetir()
        {
            var tipler = entities.Tipler.ToList();
            return tipler;
        }


    }
}