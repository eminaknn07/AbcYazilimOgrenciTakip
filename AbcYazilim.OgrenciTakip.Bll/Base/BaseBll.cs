using AbcYazilim.Dal.Interfaces;
using AbcYazilim.OgrenciTakip.Bll.Functions;
using AbcYazilim.OgrenciTakip.Bll.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Common.Functions;
using AbcYazilim.OgrenciTakip.Common.Message;
using AbcYazilim.OgrenciTakip.Model.Attributes;
using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace AbcYazilim.OgrenciTakip.Bll.Base
{
    /// <summary>
    /// İş mantığı katmanının (BLL) temel özelliklerini ve bağımlılıklarını barındıran jenerik temel sınıf.
    /// </summary>
    /// <typeparam name="T">İşlem yapılacak olan varlık (Entity) tipi.</typeparam>
    /// <typeparam name="TContext">Veritabanı bağlamı (DbContext) tipi.</typeparam>
    /// <remarks>
    /// Bu sınıf, UI kontrolleri ile veritabanı işlemleri arasındaki iletişimi sağlar ve 
    /// <see cref="IUnitOfWork{T}"/> üzerinden veri tutarlılığını yönetir.
    /// </remarks>
    public class BaseBll<T, TContext> : IBaseBll where T : BaseEntity where TContext : DbContext
    {
        /// <summary>
        /// Bu iş mantığının bağlı olduğu UI kontrolü (Form, TabPage vb.).
        /// </summary>
        private readonly Control _ctrl;
        /// <summary>
        /// Veri erişim işlemlerini yürüten iş birimi nesnesi.
        /// </summary>
        private IUnitOfWork<T> _uow;

        /// <summary>
        /// Kayıt işlemi öncesinde nesne üzerindeki zorunlu alanları ve mükerrer (tekrarlanan) kayıt kontrollerini dinamik olarak gerçekleştirir.
        /// </summary>
        /// <param name="islemTuru">Yapılan işlemin türü (Yeni Kayıt/Güncelleme).</param>
        /// <param name="oldEntity">Veritabanındaki orijinal veri (Güncelleme kontrolü için).</param>
        /// <param name="currentEntity">Ekrandaki güncel veri (Doğrulama yapılacak nesne).</param>
        /// <param name="filter">Mükerrer kayıt kontrolü için kullanılacak olan LINQ filtre ifadesi.</param>
        /// <returns>Doğrulama başarılı ise <c>true</c>, herhangi bir kural ihlali varsa <c>false</c> döner.</returns>
        /// <remarks>
        /// Metot şu iki ana kontrolü hiyerarşik olarak (önce hatalı giriş, sonra mükerrer kod) yürütür:
        /// <list type="number">
        /// <item><description><b>Zorunlu Alan Kontrolü:</b> <see cref="ZorunluAlan"/> niteliği taşıyan özelliklerin boş olup olmadığını denetler (Long tipleri için 0 kontrolü dahil).</description></item>
        /// <item><description><b>Mükerrer Kod Kontrolü:</b> <see cref="Kod"/> niteliği taşıyan alanların veritabanında daha önce kullanılıp kullanılmadığını <c>Count</c> metodu ile sorgular.</description></item>
        /// </list>
        /// Hata durumunda ilgili kontrol ismini döndürerek odağın (focus) o kontrole geçmesini sağlar.
        /// </remarks>
        private bool Validation(IslemTuru islemTuru, BaseEntity oldEntity, BaseEntity currentEntity, Expression<Func<T, bool>> filter)
        {
            var errorControl = GetValidationErrorControl();
            if (errorControl == null) return true;
            _ctrl.Controls[errorControl].Focus();
            return false;
            string GetValidationErrorControl()
            {
                string MukerrerKod()
                {
                    foreach (var property in typeof(T).GetPropertyAttributesFromType<Kod>())
                    {
                        if(property.Attribute==null) continue;
                        if ((islemTuru==IslemTuru.EntityInsert||oldEntity.Kod==currentEntity.Kod)&&islemTuru==IslemTuru.EntityUpdate) continue;
                        if (_uow.Rep.Count(filter)<1) continue;

                        Messages.MukerrerKayitHataMesaji(property.Attribute.Description);
                        return property.Attribute.ControlName;
                    }
                    return null;
                }
                string HataliGiris()
                {
                    foreach (var property in typeof(T).GetPropertyAttributesFromType<ZorunluAlan>())
                    {
                        if (property.Attribute == null) continue;
                        var value = property.Property.GetValue(currentEntity);
                        if(property.Property.PropertyType==typeof(long))
                            if ((long)value == 0) value=null;

                        if(!string.IsNullOrEmpty(value?.ToString())) continue;
                        Messages.HataliVeriMesaji(property.Attribute.Description);
                        return property.Attribute.ControlName;
                    }
                    return null;
                }
                return HataliGiris() ?? MukerrerKod();
            }
        }
        protected BaseBll()
        {

        }
        /// <summary>
        /// Belirtilen UI kontrolü ile <see cref="BaseBll{T, TContext}"/> sınıfının yeni bir örneğini başlatır.
        /// </summary>
        /// <param name="ctrl">İş mantığının hata mesajlarını veya verilerini bağlayacağı arayüz kontrolü.</param>
        protected BaseBll(Control ctrl)
        {
            _ctrl = ctrl;
        }
        /// <summary>
        /// Belirtilen kriterlere uygun tekil bir veriyi, iş mantığı kurallarını işleterek getirir.
        /// </summary>
        /// <typeparam name="TResult">Geri döndürülecek olan dönüştürülmüş (Projected) veri tipi.</typeparam>
        /// <param name="filter">Veriyi bulmak için uygulanacak filtreleme ifadesi.</param>
        /// <param name="selector">Varlığın hangi özelliklerinin seçileceğini belirten seçim ifadesi.</param>
        /// <returns>Filtreye uygun kayıt bulunursa <typeparamref name="TResult"/> tipinde sonuç, aksi halde null döner.</returns>
        /// <remarks>
        /// Metot çalıştırıldığında <see cref="GeneralFunctions.CreateUnitOfWork"/> aracılığıyla 
        /// birim iş yükü (<see cref="_uow"/>) oluşturulur veya mevcut olan referans alınır.
        /// </remarks>
        protected TResult BaseSingle<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector)
        {
            GeneralFunctions.CreateUnitOfWork<T, TContext>(ref _uow);
            return _uow.Rep.Find(filter, selector);
        }
        /// <summary>
        /// Belirtilen kriterlere uygun tüm kayıtları, iş mantığı katmanı üzerinden sorgulanabilir bir şekilde (<see cref="IQueryable{TResult}"/>) getirir.
        /// </summary>
        /// <typeparam name="TResult">Dönüştürülecek (Project edilecek) hedef veri tipi.</typeparam>
        /// <param name="filter">Kayıtları filtrelemek için uygulanacak ifade. Null ise tüm kayıtlar getirilir.</param>
        /// <param name="Selector">Varlıkların hangi özelliklerinin seçileceğini belirten seçim ifadesi.</param>
        /// <returns>Veritabanı seviyesinde henüz çalıştırılmamış, dönüştürülmüş sonuç sorgusu.</returns>
        /// <remarks>
        /// Metot çalışmadan önce <see cref="GeneralFunctions.CreateUnitOfWork"/> çağrılarak veri tabanı bağlantısı ve iş birimi hazır hale getirilir.
        /// </remarks>
        protected IQueryable<TResult> BaseList<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> Selector)
        {
            GeneralFunctions.CreateUnitOfWork<T, TContext>(ref _uow);
            return _uow.Rep.Select(filter, Selector);
        }
        /// <summary>
        /// Yeni bir varlığı, iş mantığı kurallarını ve doğrulamaları uygulayarak veritabanına ekler.
        /// </summary>
        /// <param name="entity">Eklenecek olan ve <see cref="BaseEntity"/>'den türetilmiş varlık nesnesi.</param>
        /// <param name="filter">Ekleme öncesi mükerrer kayıt kontrolü veya özel şartlar için kullanılacak filtre ifadesi.</param>
        /// <returns>Ekleme işlemi ve veritabanına kayıt (Save) başarılı ise true, aksi halde false döner.</returns>
        /// <remarks>
        /// Metot, gelen entity'yi <see cref="GeneralFunctions.EntityConvert{T}"/> kullanarak jenerik hedef tipe dönüştürür.
        /// Kayıt işlemi öncesinde gerekli doğrulama (validation) işlemleri burada gerçekleştirilmelidir.
        /// </remarks>
        protected bool BaseInsert(BaseEntity entity, Expression<Func<T, bool>> filter)
        {
            GeneralFunctions.CreateUnitOfWork<T, TContext>(ref _uow);
            if(!Validation(IslemTuru.EntityInsert, null, entity, filter)) return false;
            _uow.Rep.Insert(entity.EntityConvert<T>());
            return _uow.Save();
        }
        /// <summary>
        /// Mevcut bir varlığı, eski ve yeni değerlerini karşılaştırarak sadece değişen alanlar üzerinden günceller.
        /// </summary>
        /// <param name="oldEntity">Varlığın veritabanındaki veya işlem öncesindeki eski hali.</param>
        /// <param name="currentEntity">Varlığın arayüzden gelen güncel değerlerini barındıran yeni hali.</param>
        /// <param name="filter">Güncelleme öncesi yapılacak kontroller için kullanılacak filtre ifadesi.</param>
        /// <returns>Güncelleme işlemi başarılıysa veya değiştirilecek alan yoksa true, hata oluşursa false döner.</returns>
        /// <remarks>
        /// Metot, <see cref="GeneralFunctions.DegisenAlanlariGetir"/> aracılığıyla sadece farklı olan kolonları tespit eder. 
        /// Eğer hiçbir alan değişmemişse veritabanına gereksiz istek atmaz. Değişiklik varsa sadece ilgili alanları SQL UPDATE sorgusuna dahil eder.
        /// </remarks>
        protected bool BaseUpdate(BaseEntity oldEntity, BaseEntity currentEntity, Expression<Func<T, bool>> filter)
        {
            GeneralFunctions.CreateUnitOfWork<T, TContext>(ref _uow);
            if(!Validation(IslemTuru.EntityUpdate, oldEntity, currentEntity, filter)) return false;
            var degisenAlanlar = oldEntity.DegisenAlanlariGetir(currentEntity);
            if (degisenAlanlar.Count == 0) return true;
            _uow.Rep.Update(currentEntity.EntityConvert<T>(), degisenAlanlar);
            return _uow.Save();
        }
        /// <summary>
        /// Belirtilen varlığı, kullanıcı onayı ve iş mantığı kontrolleriyle birlikte veritabanından siler.
        /// </summary>
        /// <param name="entity">Silinecek olan ve <see cref="BaseEntity"/>'den türetilmiş varlık nesnesi.</param>
        /// <param name="kartTuru">Silme onay mesajında görüntülenecek olan kart türü adı.</param>
        /// <param name="mesajVer">Kullanıcıya silme onay mesajı gösterilip gösterilmeyeceğini belirler. Varsayılan değer true'dur.</param>
        /// <returns>Silme işlemi ve kayıt (Save) başarılı ise true, aksi halde veya kullanıcı "Hayır"ı seçerse false döner.</returns>
        /// <remarks>
        /// Metot, veritabanı seviyesindeki kısıtlamaları (Foreign Key) <see cref="IUnitOfWork{T}.Save"/> 
        /// metodu üzerinden kontrol eder. Eğer silinmek istenen kaydın başka tabloda hareketleri varsa, 
        /// Save metodundaki hata yönetimi devreye girer.
        /// </remarks>
        protected bool BaseDelete(BaseEntity entity, KartTuru kartTuru, bool mesajVer = true)
        {
            GeneralFunctions.CreateUnitOfWork<T, TContext>(ref _uow);
            if (mesajVer)
                if (Messages.SilMesaj(kartTuru.ToName()) != DialogResult.Yes) return false;
            _uow.Rep.Delete(entity.EntityConvert<T>());
            return _uow.Save();
        }
        /// <summary>
        /// Belirtilen kart türü ve filtreleme kriterlerine göre, veritabanındaki son kaydı inceleyerek bir sonraki artışlı kodu üretir.
        /// </summary>
        /// <param name="kartTuru">Üretilecek kodun ön ekini (prefix) belirleyen kart türü.</param>
        /// <param name="filter">Kodun okunacağı alanı (genellikle Kod alanı) belirleyen ifade.</param>
        /// <param name="where">Kod taranırken uygulanacak özel filtreleme kriteri (Örn: Şube bazlı kod üretimi için). Null ise tüm tabloyu tarar.</param>
        /// <returns>Yeni üretilen artışlı kod stringi.</returns>
        /// <remarks>
        /// Metot çalışmadan önce <see cref="GeneralFunctions.CreateUnitOfWork"/> çağrılır. 
        /// Ardından <see cref="IRepository{T}.YeniKodVer"/> metoduna yönlendirme yaparak merkezi kod üretim mantığını çalıştırır.
        /// </remarks>
        protected string BaseYeniKodVer(KartTuru kartTuru, Expression<Func<T, string>> filter, Expression<Func<T, bool>> where = null)
        {
            GeneralFunctions.CreateUnitOfWork<T, TContext>(ref _uow);
            return _uow.Rep.YeniKodVer(kartTuru, filter, where);
        }

        #region Dispose

        public void Dispose()
        {
            _ctrl?.Dispose();
            _uow?.Dispose();
        }
        #endregion
    }
}
