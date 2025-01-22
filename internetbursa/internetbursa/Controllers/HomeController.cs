using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using internetbursa.Models;
using System.Data.Entity;

namespace internetbursa.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ana sayfa hariç tüm sayfalar için oturum kontrolü
            if (Session["UserID"] == null &&
                !(filterContext.RouteData.Values["controller"].ToString() == "Home" &&
                  filterContext.RouteData.Values["action"].ToString() == "Index"))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "Index" }
                    });
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}

namespace internetbursa.Controllers
{
    public class HomeController : BaseController
    {
        private intDBEntities db = new intDBEntities(); // Veritabanı

        // Şehirdeki Tarihi ve Turistik Yerler
        public ActionResult TouristPlaces()
        {
            var touristPlaces = db.turistikyerler
                .Take(6)
                .Select(t => new TouristPlace
                {
                    YerAd = t.yer_ad,
                    Aciklama = t.aciklama,
                    Konum = t.konum
                })
                .ToList();

            return View(touristPlaces);
        }

        // İlçeler Listesi
        public ActionResult Districts()
        {
            var districts = db.ilcelerr
                .Select(d => new District
                {
                    IlceId = d.ilce_id,
                    IlceAd = d.ilce_ad,
                    Link = d.link,
                })
                .ToList();

            return View(districts);
        }

        // Nüfus dağılımı
        public ActionResult PopulationDistribution()
        {
            var populationData = db.nufusdagilim
                .OrderByDescending(n => n.yil)
                .Take(3)
                .Select(n => new PopulationDistribution
                {
                    Year = n.yil,
                    Population = n.nufus
                })
                .ToList();

            return View(populationData);
        }

        public ActionResult Index()
        {
            return View();
        }

        
    }
}

namespace internetbursa.Controllers
{
    public class LoginController : Controller
    {
        private intDBEntities db = new intDBEntities(); // Veritabanı bağlantısı

        // Kayıt Sayfası
        public ActionResult Register()
        {
            return View();
        }

        // Kayıt İşlemi
        [HttpPost]
        public ActionResult Register(string Username, string Password, string FullName)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(FullName))
            {
                ViewBag.ErrorMessage = "Tüm alanlar doldurulmalıdır.";
                return View();
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(Username))
            {
                ViewBag.ErrorMessage = "Geçerli bir e-posta adresi giriniz.";
                return View();
            }

            var existingUser = db.kullanicilar.FirstOrDefault(u => u.email == Username);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Bu e-posta ile daha önce kayıt olunmuş.";
                return View();
            }

            if (Password.Length < 6)
            {
                ViewBag.ErrorMessage = "Şifre en az 6 karakter olmalıdır.";
                return View();
            }

            var newUser = new kullanicilar
            {
                email = Username,
                sifre = Password,
                ad_soyad = FullName,
                rol = 2  // Varsayılan olarak kullanıcı rolü atanır
            };

            try
            {
                db.kullanicilar.Add(newUser);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyiniz.";
                Console.WriteLine(ex.Message);
                return View();
            }
        }

        // Giriş Sayfası
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string Username, string Password)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "E-posta ve şifre boş bırakılamaz.";
                return View();
            }

            var user = db.kullanicilar.FirstOrDefault(u => u.email == Username && u.sifre == Password);

            if (user != null)
            {
                Session["UserID"] = user.kullanici_id;
                Session["UserName"] = user.email;
                Session["UserRole"] = user.rol == 1 ? "Admin" : "User"; // Rol kontrolü
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "E-posta veya şifre hatalı.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

namespace internetbursa.Controllers
{
    [AdminRoleFilter]  // Bu, tüm controller üzerinde AdminRoleFilterı uygular
    public class AdminPanelController : Controller
    {
        private intDBEntities db = new intDBEntities(); // Veritabanı bağlantısı

        // Yönetim Paneli Ana Sayfası
        public ActionResult Index()
        {
            var places = db.turistikyerler.ToList();
            return View(places);
        }

        // Yeni Turistik Yer Ekleme Sayfası
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(turistikyerler place)
        {
            if (ModelState.IsValid)
            {
                db.turistikyerler.Add(place);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(place);
        }

        // Düzenleme Sayfası
        public ActionResult Edit(int id)
        {
            var place = db.turistikyerler.Find(id);
            if (place == null) return HttpNotFound();
            return View(place);
        }

        [HttpPost]
        public ActionResult Edit(turistikyerler place)
        {
            if (ModelState.IsValid)
            {
                db.Entry(place).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(place);
        }

        // Silme İşlemi
        public ActionResult Delete(int id)
        {
            var place = db.turistikyerler.Find(id);
            if (place != null)
            {
                db.turistikyerler.Remove(place);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // İlçeler Yönetimi
        public ActionResult Districts()
        {
            var districts = db.ilcelerr.ToList(); // İlçeleri alıyoruz
            return View(districts); // İlçeleri View a gönderiyoruz
        }

        // Yeni İlçe Ekleme
        public ActionResult CreateDistrict()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateDistrict(ilcelerr district)
        {
            if (ModelState.IsValid)
            {
                db.ilcelerr.Add(district);
                db.SaveChanges();
                return RedirectToAction("Districts");
            }
            return View(district);
        }

        // İlçe Düzenleme g
        public ActionResult EditDistrict(int id)
        {
            var district = db.ilcelerr.Find(id);
            if (district == null)
                return HttpNotFound();
            return View(district);
        }

        [HttpPost]
        public ActionResult EditDistrict(ilcelerr district)
        {
            if (ModelState.IsValid)
            {
                db.Entry(district).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Districts");
            }
            return View(district);
        }

        // İlçe Silme
        public ActionResult DeleteDistrict(int id)
        {
            var district = db.ilcelerr.Find(id);
            if (district != null)
            {
                db.ilcelerr.Remove(district);
                db.SaveChanges();
            }
            return RedirectToAction("Districts");
        }

        // Nüfus Dağılımı Listeleme
        public ActionResult Population()
        {
            var populations = db.nufusdagilim.ToList(); // Nüfus verilerini alıyoruz
            return View(populations); // Nüfus verilerini View'a gönderiyoruz
        }

        // Yeni Nüfus Dağılımı Ekleme
        public ActionResult CreatePopulation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreatePopulation(nufusdagilim population)
        {
            if (ModelState.IsValid)
            {
                db.nufusdagilim.Add(population); // Yeni nüfus dağılımı verisini ekliyoruz
                db.SaveChanges();
                return RedirectToAction("Population");
            }
            return View(population); // Hata durumunda aynı sayfada kalıyoruz
        }

        // Nüfus Dağılımı Düzenleme g
        public ActionResult EditPopulation(int id)
        {
            var population = db.nufusdagilim.Find(id);
            if (population == null)
                return HttpNotFound();
            return View(population);
        }

        // Nüfus Dağılımı Düzenleme p
        [HttpPost]
        public ActionResult EditPopulation(nufusdagilim population)
        {
            if (ModelState.IsValid)
            {
                db.Entry(population).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Population");
            }
            return View(population);
        }

        // Nüfus Dağılımı Silme
        public ActionResult DeletePopulation(int id)
        {
            var population = db.nufusdagilim.Find(id);
            if (population != null)
            {
                db.nufusdagilim.Remove(population);
                db.SaveChanges();
            }
            return RedirectToAction("Population");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}











